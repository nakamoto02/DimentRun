using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Player
{
    public class PlayerMoverSelect : PlayerMover
    {
        public SelectController seController;

        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move(KeyState key)
        {
            Vector3 inputVec = Vector3.forward * key.Axis.y + Vector3.right * key.Axis.x;

            animator.SetBool("IsGround", IsGround);

            // 向き
            if (key.Axis != new Vector2(0, 0)) {
                animator.SetBool("Walk", true);
                transformCache.forward = inputVec;
            } else
            {
                animator.SetBool("Walk", false);
            }

            transformCache.localPosition += inputVec * DEFAULT_SPEED * Time.deltaTime;

            // 決定ボタン
            if(key.Action || key.Jump) {
                seController.SelectStageDecision();
            }
        }
        //-----------------------------------------------------
        //  リスポーン
        //-----------------------------------------------------
        public override void ReSpawn(Vector3 position) { return; }
    }
}