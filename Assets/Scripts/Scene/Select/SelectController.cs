using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimension.Player;

namespace Dimension
{
    public class SelectController : MonoBehaviour
    {
        public SelectObject[] selectObjects;
        public TestPlayer player;

        //-----------------------------------------------------
        //  プレイヤーからの入力を受け取る
        //-----------------------------------------------------
        public void SelectStageDecision()
        {
            if (!IsSelect()) return;
            player.IsStop = true;
            SendStageGameManager();
            TransitionGameScene();
        }
        //-----------------------------------------------------
        //  ステージが選択されているかどうか
        //-----------------------------------------------------
        bool IsSelect()
        {
            foreach(SelectObject obj in selectObjects) {
                if (obj.IsTouch) return true;
            }
            return false;
        }
        //-----------------------------------------------------
        //  選択しているステージをゲームマネージェーへ
        //-----------------------------------------------------
        void SendStageGameManager()
        {
            foreach(SelectObject obj in selectObjects) {
                if(obj.IsTouch) {
                    obj.SendNextStageData();
                    break;
                }
            }
        }
        //-----------------------------------------------------
        //  ゲームシーンへ
        //-----------------------------------------------------
        void TransitionGameScene()
        {
            FadeManagerController.Instance.FadeScene(FadeManagerController.Scene.GameScene);
        }
    }
}