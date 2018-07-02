using System.Collections;
using UnityEngine;
using Dimension.Camera2D3D;
using Dimension.Player;
using Dimension.Stage;

namespace Dimension
{
    public enum Mode
    {
        Second,
        Third
    }
    public class GameController : MonoBehaviour
    {
        // ステージ情報
        public StageData stageData;

        [Space]
        public CameraController cController;
        public TestPlayer pController;
        public StageController sController;

        public GameObject clearLabel;

        //-----------------------------------------------------
        //  プロパティ
        //-----------------------------------------------------
        public Mode GameMode { get; set; }
        //=====================================================
        void Awake()
        {
            if (GameManager.instance.NextStage != null)
                stageData = GameManager.instance.NextStage;

            sController.SetStageData(stageData);
            pController.SetGameController(this);
            cController.SetGameController(this);

            // 初期化
            Initialize();
        }
        //-----------------------------------------------------
        //  初期化
        //-----------------------------------------------------
        void Initialize()
        {
            // 初期位置
            pController.Position = sController.StartPoint;
            cController.Position = sController.StartPoint - sController.StageForward * 10.0f;
            cController.Forward = sController.StageForward;

            ChangeMode3D();
        }
        //-----------------------------------------------------
        //  3D2Dの切り替え
        //-----------------------------------------------------
        public void ChangeDimension()
        {
            ChangeMode<PlayerChangeMover, CameraChangeWork>();
        }
        //-----------------------------------------------------
        //  3DModeへ切り替え
        //-----------------------------------------------------
        public void ChangeMode3D()
        {
            ChangeMode<PlayerMover3D, CameraWork3D>();
            GameMode = Mode.Third;
        }
        //-----------------------------------------------------
        //  2DModeへ切り替え
        //-----------------------------------------------------
        public void ChangeMode2D()
        {
            ChangeMode<PlayerMover2D, CameraWork2D>();
            GameMode = Mode.Second;
        }
        //-----------------------------------------------------
        //  Modeの変更
        //-----------------------------------------------------
        void ChangeMode<PM, CW>() where PM : PlayerMover where CW : CameraWork
        {
            pController.ChangeMover<PM>();
            cController.ChangeWork<CW>();
        }
        //-----------------------------------------------------
        //  クリア
        //-----------------------------------------------------
        public void StageClear()
        {
            clearLabel.SetActive(true);
            Invoke("TransitionSelect", 3.0f);
        }
        void TransitionSelect()
        {
            FadeManagerController.Instance.FadeScene(FadeManagerController.Scene.StageSelect);
        }
    }
}