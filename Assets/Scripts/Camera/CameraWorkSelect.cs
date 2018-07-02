using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Camera2D3D
{
    public class CameraWorkSelect : CameraWork
    {
        const float PLAYER_DISTANCE = 5.0f;
        const float HEIGHT_DISTANCE = 4.0f;

        Vector3 _cameraForward;

        //=====================================================
        Vector3 CameraForward { get { return _cameraForward; } }
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize()
        {
            _cameraForward = Vector3.forward;   // 正面方向
        }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move()
        {
            transformCache.localPosition = Target.LocalPosition -
                CameraForward * PLAYER_DISTANCE +
                Vector3.up    * HEIGHT_DISTANCE;
        }
        //-----------------------------------------------------
        //  移動制限
        //-----------------------------------------------------
        protected override Vector3 MoveRestriction(Vector3 point) { return point; }
    }
}