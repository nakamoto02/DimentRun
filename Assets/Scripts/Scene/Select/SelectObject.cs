using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimension.Player;

namespace Dimension
{
    public class SelectObject : MonoBehaviour
    {
        const float TOUCH_LENGTH = 2.0f;
        const float ACTIVE_HEIGHT   = 4.0f;
        const float INACTIVE_HEIGHT = 2.5f;
        Vector3 ACTIVE_SIZE   = new Vector3(0.05f, 0.05f, 0.05f);
        Vector3 INACTIVE_SIZE = new Vector3(0.02f, 0.02f, 0.02f);

        public StageData stageData;
        public TestPlayer player;
        public MeshSetter meshSetter;

        Transform transformCache;
        
        float lerpTimer;
        bool beforeFlg;
        //-------------------------------------------------
        //  プロパティ
        //-------------------------------------------------
        public bool IsTouch { get; set; }
        //=================================================
        void Awake()
        {
            transformCache = transform;
            transformCache.localScale = INACTIVE_SIZE;
            meshSetter.SetMesh(stageData.renderMesh, stageData.colliderMesh);
            lerpTimer = 1;
            beforeFlg = IsTouch = false;
            fromHeight = nextHeight = INACTIVE_HEIGHT;
            fromSize = nextSize = INACTIVE_SIZE;
        }
        void FixedUpdate()
        {
            TouchCheck();
            ActiveAction();

            if (IsTouch) MoveAction();

            beforeFlg = IsTouch;
        }
        //-------------------------------------------------
        //  触れているか判定
        //-------------------------------------------------
        void TouchCheck()
        {
            Vector3 pPos = player.LocalPosition;
            Vector3 myPos = transformCache.position;
            pPos.y = myPos.y = 0;
            IsTouch = (pPos - myPos).magnitude < TOUCH_LENGTH;
        }
        //-------------------------------------------------
        //  有効時非有効時の動き
        //-------------------------------------------------
        float fromHeight, nextHeight;
        Vector3 fromSize, nextSize;
        void ActiveAction()
        {
            if(beforeFlg != IsTouch) {
                lerpTimer = 0;
                fromHeight = (IsTouch) ? INACTIVE_HEIGHT : ACTIVE_HEIGHT;
                nextHeight = (IsTouch) ? ACTIVE_HEIGHT : INACTIVE_HEIGHT;
                fromSize = (IsTouch) ? INACTIVE_SIZE : ACTIVE_SIZE;
                nextSize = (IsTouch) ? ACTIVE_SIZE : INACTIVE_SIZE;
            }
            // タイマー
            lerpTimer = Mathf.Min(lerpTimer + Time.deltaTime * 2, 1);
            // 位置
            Vector3 movePos = transform.localPosition;
            movePos.y = Mathf.Lerp(fromHeight, nextHeight, lerpTimer);
            transform.localPosition = movePos;
            // スケール
            transform.localScale = Vector3.Lerp(fromSize, nextSize, lerpTimer);
        }
        //-------------------------------------------------
        //  動作
        //-------------------------------------------------
        void MoveAction()
        {

            transformCache.Rotate(Vector3.up * 30 * Time.deltaTime);
        }
        //-------------------------------------------------
        //  ステージデータをゲームマネージャーへ
        //-------------------------------------------------
        public void SendNextStageData()
        {
            GameManager.instance.NextStage = stageData;
        }
    }
}