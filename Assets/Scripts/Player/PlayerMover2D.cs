using UnityEngine;

namespace Dimension.Player
{
    public class PlayerMover2D : PlayerMover
    {
        const float ACCEL_MAX = 2.0f;

        float moveAccel;
        //-----------------------------------------------------
        Vector3 ForwardAxis { get { return new Vector3(Mathf.Abs(StageForward.x), 0, Mathf.Abs(StageForward.z)); } }
        Vector3 RightAxis { get { return new Vector3(Mathf.Abs(StageRight.x), 0, Mathf.Abs(StageRight.z)); } }
        Vector3 StageRight { get { return PController.SController.StageRight; } }
        float StageWidth { get { return PController.SController.StageWidth; } }
        bool IsRight { get { return PController.IsRight; } }
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            rigidbodyCache.useGravity = true;   // 重力を有効
            moveAccel = PController.SaveAccel * 0.5f;

            // 向き
            //transformCache.forward = StageForward;
            // 位置
            int sign = (PController.IsRight) ? 1 : -1;
            transform.localPosition = (sign * StageRight * StageWidth * 2) + Vector3.Scale(ForwardAxis + Vector3.up, transformCache.localPosition);
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move(KeyState key)
        {
            // 入力された移動方向
            Vector3 inputVec = new Vector3(0, 0, 0);

            // 移動
            if (Mathf.Abs(key.Axis.x) > 0.5f)  {
                inputVec = Camera.main.transform.right * key.Axis.x;
            }

            // 加速度
            if (inputVec != new Vector3(0, 0, 0) && IsGround)
                moveAccel = Mathf.Min(moveAccel + 3 * Time.deltaTime, ACCEL_MAX);
            else if (IsGround)
                moveAccel = Mathf.Max(moveAccel - 3 * Time.deltaTime, 0);
                
                

            // 向き
            if(inputVec.x != 0) {
                animator.SetBool("Walk", true);
                transformCache.forward = inputVec;
            } else
            {
                animator.SetBool("Walk", false);
            }

            // ジャンプ
            if (PController.IsGround && key.Jump) {
                rigidbodyCache.AddForce(Vector3.up * 250);
                animator.SetTrigger("Jump");
                animator.SetBool("Walk", false);
            } else
            {
                animator.SetBool("IsGround", IsGround);
            }

            // 更新
            //transformCache.localPosition += transformCache.forward * DEFAULT_SPEED * moveAccel * Time.deltaTime;
            transformCache.localPosition += inputVec * DEFAULT_SPEED * Time.deltaTime;

            // モード切替
            if (key.Action) {
                // 位置
                transformCache.localPosition = PController.SController.GetBackPoint(transformCache.localPosition, IsRight);

                PController.GController.ChangeDimension();
            }
        }
        //-----------------------------------------------------
        //  復帰
        //-----------------------------------------------------
        public override void ReSpawn(Vector3 position)
        {
            rigidbodyCache.velocity = new Vector3(0, 0, 0);
            PController.SaveAccel = moveAccel = 0;

            Vector3 spawnPoint = PController.SController.GetReSpawrnPoint(position);
            transformCache.localPosition = new Vector3(
                transformCache.localPosition.x,
                PController.HeightMax + 1,
                spawnPoint.z
                );
        }
    }
}