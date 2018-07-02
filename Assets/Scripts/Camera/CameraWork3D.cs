using UnityEngine;

namespace Dimension.Camera2D3D
{
    public class CameraWork3D : CameraWork
    {
        public const float LOOK_HEIHGT = 2.0f;  // 見る位置の高さ

        const float DEFALUT_LENGTH = 5.0f;      // ターゲットの距離

        Vector3 nextPos;    // 移動先
        Vector3 beforePos;  // 前回の位置
        float lerpTime;
        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        private Vector3 StageCenter { get; set; }   // ステージの中心
        private float StageWidth { get; set; }      // ステージの幅
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            MyCamera.orthographic = false;

            StageCenter = CController.SController.StageCenter;
            StageWidth  = CController.SController.StageWidth;
            beforePos   = transformCache.localPosition;
            lerpTime = 0;
        }
        //-------------------------------------------------
        //  行動
        //-------------------------------------------------
        public override void Move()
        {
            NextMovePosition(); // 移動先を計算
            UpdatePosition();   // 位置
            UpdateRotation();   // 向き
        }
        //-----------------------------------------------------
        //  移動制限
        //-----------------------------------------------------
        protected override Vector3 MoveRestriction(Vector3 point)
        {
            return new Vector3(
                point.x,
                Mathf.Clamp(point.y, CameraRange.y + 7.0f, CameraRange.y + CameraRange.height),
                point.z
                );
        }
        //-----------------------------------------------------
        //  移動先の計算
        //-----------------------------------------------------
        void NextMovePosition()
        {
            

            nextPos = GetNextMovePos(Target.LocalPosition, StageForward, StageRight, StageCenter, StageWidth);
        }
        //-----------------------------------------------------
        //  移動先の取得
        //-----------------------------------------------------
        public static Vector3 GetNextMovePos(Vector3 target, Vector3 forward, Vector3 right,Vector3 center, float stageW)
        {
            // 中心からの距離
            float pCenterLength = Conversion.Vector3ToFloat(Vector3.Scale(right, target) - Vector3.Scale(right, center));
            // 移動量
            float pValue = (pCenterLength / stageW * 0.5f) * Mathf.PI;
            pValue = Mathf.Clamp(pValue, -Mathf.PI / 4, Mathf.PI / 4);

            return target -
                forward    * DEFALUT_LENGTH * Mathf.Cos(pValue / 2) +
                right      * DEFALUT_LENGTH * Mathf.Sin(pValue) +
                Vector3.up * DEFALUT_LENGTH * Mathf.Cos(pValue);
        }
        //-----------------------------------------------------
        //  位置を更新
        //-----------------------------------------------------
        void UpdatePosition()
        {
            transformCache.localPosition = nextPos;
        }
        //-----------------------------------------------------
        //  向きを更新
        //-----------------------------------------------------
        void UpdateRotation()
        {
            // 向く方向
            Vector3 lookVec = (Target.LocalPosition + Vector3.up * LOOK_HEIHGT) - transformCache.localPosition;
            transformCache.localRotation = Quaternion.LookRotation(lookVec);
        }
    }
}