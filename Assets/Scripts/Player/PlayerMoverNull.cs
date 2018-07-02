using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Player
{
    public class PlayerMoverNull : PlayerMover
    {
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            rigidbodyCache.useGravity = false;
            rigidbodyCache.velocity = new Vector3(0, 0, 0);
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move(KeyState key) { return; }
        //-----------------------------------------------------
        //  復帰
        //-----------------------------------------------------
        public override void ReSpawn(Vector3 position) { return; }
        
    }
}