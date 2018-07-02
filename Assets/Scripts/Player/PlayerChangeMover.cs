using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Player
{
    public class PlayerChangeMover : PlayerMover
    {
        Transform targetCamera;
        bool isRight;

        // 保存
        Vector3 saveVelocity;

        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            // 動きを止める
            rigidbodyCache.useGravity = false;
            saveVelocity = rigidbodyCache.velocity;
            rigidbodyCache.velocity = new Vector3(0, 0, 0);

            targetCamera = PController.GController.cController.TransformCache;
            isRight = PController.IsRight;
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move(KeyState key)
        {
            //// プレイヤーからカメラへの方向
            //Vector3 directionPtoC = targetCamera.position - transformCache.position;
            //// // 高さは無視
            //directionPtoC.y = 0;

            //transformCache.localRotation = Quaternion.LookRotation(directionPtoC, Vector3.up);
            float speed = 5f;
            float step;
            Quaternion PlayerRotation = transform.rotation;
            step = speed * Time.deltaTime;
            //ステージの進行方向に回転
            if(IsForward)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, (float)StageForward.y, 0), step);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, (float)StageForward.y - 180.0f, 0), step);
            }
        }
        //-----------------------------------------------------
        //  復帰
        //-----------------------------------------------------
        public override void ReSpawn(Vector3 position){ return; }
        //-----------------------------------------------------
        //  消去される時
        //-----------------------------------------------------
        void OnDestroy()
        {
            rigidbodyCache.useGravity = true;
            rigidbodyCache.velocity = saveVelocity;
        }
    }
}