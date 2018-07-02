using System.Collections;
using UnityEngine;
using Dimension.Player;
using Dimension.Stage;

namespace Dimension.Camera2D3D
{
    public class CameraController : MonoBehaviour
    {
        Transform _transformCache;  // キャッシュ

        public TestPlayer player;   // プレイヤー
        CameraWork cWork;           // カメラワーク

        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        public GameController  GController { get; private set; }
        public StageController SController { get; private set; }
        // Transform
        public Transform TransformCache {
            get {
                if (_transformCache == null) _transformCache = transform;
                return _transformCache;
            }
        }
        public Vector3 Position {
            get { return TransformCache.position; }
            set { TransformCache.position = value; }
        }
        public Vector3 LocalPosition {
            get { return TransformCache.localPosition; }
            set { TransformCache.localPosition = value; }
        }
        public Vector3 Forward {
            get { return TransformCache.forward; }
            set { TransformCache.forward = value; }
        }

        // ステージから受けとる情報
        public Vector3 StageForward { get; private set; }   // 進行方向
        public Vector3 StageRight { get; private set; }     // 右方向
        public Vector3 StageCenter { get; private set; }
        public Rect CameraRange { get; private set; }       // 描画制限範囲(位置は左下)

        // 状態
        public bool IsStop { get; set; }    // 停止
        //=====================================================
        void Start()
        {
            IsStop = false;
            cWork = GetComponent<CameraWork>();
            if (cWork == null) ChangeWork<CameraWork3D>();
        }
        void LateUpdate()
        {
            if (IsStop) return;
            cWork.Move();
        }
        //-----------------------------------------------------
        //  カメラワークを変更
        //-----------------------------------------------------
        public void ChangeWork<CW>() where CW : CameraWork
        {
            Destroy(cWork);
            cWork = gameObject.AddComponent<CW>();
        }
        //-----------------------------------------------------
        //  ゲームコントローラー受け取り
        //-----------------------------------------------------
        const float RANGE_WIDTH_MIN = 18.0f;    // 描画範囲：幅の最低値
        const float RANGE_HEIGHT_MIN = 10.0f;   // 描画範囲：高さの最低値
        public void SetGameController(GameController gCon)
        {
            if (GController != null) return;
            GController = gCon;
            SController = GController.sController;

            StageForward = SController.StageForward;    // 正面
            StageRight = SController.StageRight;        // 右
            StageCenter = SController.StageCenter;      // 中心
            CameraRange = new Rect(
                SController.StageCenter.z - SController.StageDepth * 0.5f - 1,
                SController.StageCenter.y - SController.StageHeight * 0.5f - 1,
                Mathf.Max(SController.StageDepth + 2, RANGE_WIDTH_MIN),
                Mathf.Max(SController.StageHeight + 5, RANGE_HEIGHT_MIN)
                );
        }
        //-----------------------------------------------------
        //  ゲームモード取得
        //-----------------------------------------------------
        public Mode GetGameMode() {
            return GController.GameMode;
        }
    }
}