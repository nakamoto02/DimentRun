using UnityEngine;
using System.Collections;

namespace Dimension.Camera2D3D
{
    public class CameraChangeWork : CameraWork
    {
        public const float CHANGE_TIME = 0.5f;  // かかる時間

        const float FIELD_VIEW_MIN = 10.0f; // 最小画角
        const float FIELD_VIEW_MAX = 60.0f; // 最大画角
        const float BACK_LENGTH    = 41.0f; // ドリーアウト距離

        public Vector3 fromPoint;   // 元の位置
        public Vector3 nextPoint;   // 次の位置

        float fromView;             // 元の画角
        float nextView;             // 次の画角
        float changeValue;          // 変化値

        float fromLookY;
        float nextLookY;

        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            fromPoint = LocalPosition;
            changeValue = 0;

            //-------------------------------------------------
            Vector3 stageCenter = CController.SController.StageCenter;
            float   stageWidth  = CController.SController.StageWidth;

            if(GameMode == Mode.Third)
            {   // 3D → 2D
                fromView = FIELD_VIEW_MAX;      // 元の画角に最大画角を設定
                nextView = FIELD_VIEW_MIN;      // 次の画角に最小画角を設定
                // 位置
                Vector3 nextVec = (Target.IsRight) ? StageRight : -StageRight;
                nextPoint = CameraWork2D.RestrictionPositon(Target.LocalPosition + nextVec * BACK_LENGTH, CameraRange);
                // 向く位置の高さ
                fromLookY = CameraWork3D.LOOK_HEIHGT;
                nextLookY = 0;
            } else
            if (GameMode == Mode.Second)
            {   // 2D → 3D
                MyCamera.orthographic = false;  // Cameraを透視投影に変換
                fromView = FIELD_VIEW_MIN;      // 元の画角に最小画角を設定
                nextView = FIELD_VIEW_MAX;      // 次の画角に最大画角を設定
                // 位置
                nextPoint = CameraWork3D.GetNextMovePos(Target.LocalPosition, StageForward, StageRight, stageCenter, stageWidth);
                // 向く位置の高さ
                fromLookY = 0;
                nextLookY = CameraWork3D.LOOK_HEIHGT;
            }
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move()
        {
            if (IsFinish()) return;

            changeValue = Mathf.Min(changeValue + Time.deltaTime / CHANGE_TIME, 1);

            MyCamera.fieldOfView = Mathf.Lerp(fromView, nextView, changeValue);
            LocalPosition    = Vector3.Lerp(fromPoint, nextPoint, changeValue);

            // 位置

            // 向き
            UpdateRotation();

            if(IsFinish()) {
                FinishWork();
            }
        }
        //-----------------------------------------------------
        //  向きを更新
        //-----------------------------------------------------
        void UpdateRotation()
        {
            float lookHeight = Mathf.Lerp(fromLookY, nextLookY, changeValue);
            Vector3 lookVec = (Target.LocalPosition + Vector3.up * lookHeight) - transformCache.localPosition;
            transformCache.localRotation = Quaternion.LookRotation(lookVec);
        }
        //-----------------------------------------------------
        //  移動制限
        //-----------------------------------------------------
        protected override Vector3 MoveRestriction(Vector3 point)
        {
            return point;
        }
        //-----------------------------------------------------
        //  終了判断
        //-----------------------------------------------------
        bool IsFinish()
        {
            return changeValue >= 1;
        }
        //-----------------------------------------------------
        //  終了処理
        //-----------------------------------------------------
        void FinishWork()
        {
            if (GameMode == Mode.Third && !IsDisplayPlayer())
            {   // プレイヤーが映っていないなら失敗
                CController.GController.GameMode = Mode.Second;
                CController.GController.ChangeDimension();
                return;
            } else
            if(GameMode == Mode.Third)
            {   // 3D → 2D
                MyCamera.orthographic = true;
                CController.GController.ChangeMode2D();
                return;
            } 

            CController.GController.ChangeMode3D();
        }
        //-----------------------------------------------------
        //  プレイヤーが映っているか
        //-----------------------------------------------------
        bool IsDisplayPlayer()
        {
            Vector3 origin = new Vector3(LocalPosition.x, Target.LocalPosition.y, LocalPosition.z);
            Ray ray = new Ray(origin, (Target.Center - origin).normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100.0f)) {
                if (hit.collider.tag == "Player") return true;
            }
            return false;
        }
    }
}