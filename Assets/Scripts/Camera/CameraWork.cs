using UnityEngine;
using Dimension.Player;

namespace Dimension.Camera2D3D
{
    public abstract class CameraWork : MonoBehaviour
    {
        protected Transform transformCache;
        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        protected CameraController CController { get; private set; }

        // ステージ情報
        protected Vector3 StageForward { get { return CController.StageForward; } }
        protected Vector3 StageRight { get { return CController.StageRight; } }
        protected Rect CameraRange { get { return CController.CameraRange; } }
        // Transform
        protected Vector3 Position {
            get { return transformCache.position; }
            set { transformCache.position = MoveRestriction(value); }
        }
        protected Vector3 LocalPosition {
            get { return transformCache.localPosition; }
            set { transformCache.localPosition = MoveRestriction(value); }
        }
        protected Vector3 EulerAngles {
            get { return transformCache.eulerAngles; }
            set { transformCache.eulerAngles = value; }
        }
        protected Vector3 LocalEulerAngles {
            get { return transformCache.localEulerAngles; }
            set { transformCache.localEulerAngles = value; }
        }

        protected Mode GameMode { get { return CController.GetGameMode(); } }
        protected Camera MyCamera   { get; private set; }
        protected TestPlayer Target { get; private set; }
        //=====================================================
        void Awake()
        {
            transformCache = transform;
            MyCamera       = GetComponent<Camera>();
            CController    = GetComponent<CameraController>();
            Target         = CController.player;
            Initialize();
        }
        //-----------------------------------------------------
        //  抽象メソッド
        //-----------------------------------------------------
        public abstract void Initialize();  // 初期化
        public abstract void Move();        // 行動
        // 移動制限
        protected abstract Vector3 MoveRestriction(Vector3 point);
    }
}