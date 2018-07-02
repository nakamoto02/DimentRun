using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimension.Stage
{
    public class StageGoal : GoalObject
    {
        public GameController gController;

        void Start()
        {
            gController = GameObject.Find("GameController").GetComponent<GameController>();
        }

        protected override void HitEvent()
        {
            gController.StageClear();
            //FadeManagerController.Instance.FadeScene(FadeManagerController.Scene.StageSelect);
        }
    }
}