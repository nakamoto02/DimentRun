using UnityEngine;

namespace Dimension.Player
{
    public class PlayerMover3D : PlayerMover
    {
        const float ACCEL_PER = 1.0f;   // 1秒間に増加する加速度
        const float ACCEL_MAX = 2.0f;   // 最大加速度

        Vector3 moveVector;     // 移動方向
        Vector3 accelVector;    // 加速方向

        float moveAccel;    // 加速度

        Vector3 StageRight { get { return PController.SController.StageRight; } }
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            moveVector  = new Vector3(0, 0, 0);
            accelVector = new Vector3(0, 0, 0);
            moveAccel   = PController.SaveAccel * 0.5f;
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move(KeyState key)
        {
            // 入力された移動方向
            Vector3 inputVec = StageRight * key.Axis.x + StageForward * key.Axis.y;

            // 加速度
            if (inputVec != new Vector3(0, 0, 0) && IsGround)
                moveAccel = Mathf.Min(moveAccel + 3 * Time.deltaTime, ACCEL_MAX);
            else if(IsGround)
                moveAccel = Mathf.Max(moveAccel - 3 * Time.deltaTime, 0);


            // ジャンプ
            if (PController.IsGround  && key.Jump) {
                rigidbodyCache.AddForce(Vector3.up * 250);
                animator.SetTrigger("Jump");
                animator.SetBool("Walk", false);
            }else
            {
                animator.SetBool("IsGround", IsGround);
            }
            

            // 向き
            if(key.Axis != new Vector2(0, 0)) {
                animator.SetBool("Walk", true);
                transformCache.forward = inputVec;
            }
            else
            {
                animator.SetBool("Walk", false);
            }

            // ダッシュ
            if (key.Dash) {
                rigidbodyCache.AddForce(transformCache.forward * 250);
            }

            // 更新
            //transformCache.localPosition += transformCache.forward * DEFAULT_SPEED * moveAccel * Time.deltaTime;
            transformCache.localPosition += inputVec * DEFAULT_SPEED * Time.deltaTime;

            // モード切替
            if (key.Action) {
                PController.SaveAccel = moveAccel;
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
                spawnPoint.x,
                PController.HeightMax + 1,
                spawnPoint.z
                );
        }
    }
}