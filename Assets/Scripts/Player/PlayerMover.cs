using UnityEngine;

namespace Dimension.Player
{
    public abstract class PlayerMover : MonoBehaviour
    {
        protected const float DEFAULT_SPEED = 3.0f;   // 移動速度

        protected Transform transformCache;
        protected Rigidbody rigidbodyCache;
        protected Animator animator;

        protected KeyState beforeKey;
        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        protected TestPlayer PController { get; private set; }
        protected Mode GameMode { get { return PController.GController.GameMode; } }
        protected Vector3 StageForward { get { return PController.StageForward; } }
        protected bool IsGround { get { return PController.IsGround; } }
        protected bool IsForward { get { return PController.IsForward; } }
        //=====================================================
        void Awake()
        {
            transformCache  = transform;
            rigidbodyCache  = GetComponent<Rigidbody>();
            animator        = GetComponent<Animator>();
            PController     = GetComponent<TestPlayer>();
            beforeKey       = new KeyState();
            Initialize();
        }
        //-----------------------------------------------------
        //  抽象メソッド
        //-----------------------------------------------------
        public abstract void Initialize();      // 初期化
        
        public abstract void Move(KeyState key);// 行動
        public abstract void ReSpawn(Vector3 position);        // リスポーン
    }
}