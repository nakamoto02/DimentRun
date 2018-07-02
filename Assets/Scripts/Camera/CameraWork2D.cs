using UnityEngine;

namespace Dimension.Camera2D3D
{
    public class CameraWork2D : CameraWork
    {
        const float VIEW_WIDTH  = 18.0f;
        const float VIEW_HEIGHT = 10.0f;

        float ViewWidth { get; set; }
        float ViewHeight { get; set; }

        //---------------------------------------------------------------
        //  初期化
        //---------------------------------------------------------------
        public override void Initialize()
        {
            MyCamera.orthographic = true;

            Vector3 leftDown = MyCamera.ViewportToWorldPoint(new Vector2(0, 0));
            Vector3 rightUp  = MyCamera.ViewportToWorldPoint(new Vector2(1, 1));

            ViewWidth = Mathf.Round(Mathf.Abs(rightUp.z - leftDown.z));
            ViewHeight = rightUp.y - leftDown.y;

            // 向き
            transformCache.forward = (Target.IsRight) ? -StageRight : StageRight;
        }
        //---------------------------------------------------------------
        //  行動
        //---------------------------------------------------------------
        public override void Move()
        {
            if (Target == null) return;

            LocalPosition = new Vector3(
                LocalPosition.x,
                Target.Position.y,
                Target.Position.z
                );
        }
        //-----------------------------------------------------
        //  移動制限
        //-----------------------------------------------------
        protected override Vector3 MoveRestriction(Vector3 point)
        {
            return RestrictionPositon(point, CameraRange);
        }
        //-----------------------------------------------------
        //  制限をかけて値を返す
        //-----------------------------------------------------
        public static Vector3 RestrictionPositon(Vector3 cPos, Rect range)
        {
            return new Vector3(
                cPos.x,
                Mathf.Clamp(cPos.y, range.y + VIEW_HEIGHT * 0.5f, (range.y + range.height) - VIEW_HEIGHT * 0.5f),
                cPos.z
                );
        }
    }
}