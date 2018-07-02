using UnityEngine;

namespace Dimension.Camera2D3D
{
    public class CameraWorkNull : CameraWork
    {
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        public override void Initialize() { return; }
        //-----------------------------------------------------
        //  行動
        //-----------------------------------------------------
        public override void Move() { return; }
        //-----------------------------------------------------
        //  移動制限
        //-----------------------------------------------------
        protected override Vector3 MoveRestriction(Vector3 point) { return point; }
    }
}
