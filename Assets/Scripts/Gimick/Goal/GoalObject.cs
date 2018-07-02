using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Stage
{
    public abstract class GoalObject : MonoBehaviour
    {
        // 当たった際の処理
        protected abstract void HitEvent();
        //-----------------------------------------------------
        // 当たり判定
        //-----------------------------------------------------
        void OnTriggerEnter(Collider coll)
        {
            if (coll.tag != "Player") return;

            HitEvent();
        }
    }
}