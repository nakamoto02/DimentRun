using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
public class SceaneManagerController : SingletonMonoBehaviour<SceaneManagerController>
{
    //現在のシーン名
    private string NowScene;
    //タイトルでの遷移フラグ
    private bool sceaneflg;
    //Time
    private float TIME = 0.0f;

    void Awake()
    {
        TIME = 0.0f;
        sceaneflg = false;
        FadeManagerController.Blackalfa = 0.0f;
        //SelectStagePlayer.Load = false;
    }

    public void NowSceneManagement()
    {

        //現在のシーン
        NowScene = SceneManager.GetActiveScene().name;
        //タイトルなら
        if (NowScene == "Title")
        {
            //何かボタンを押したら
            if (Input.anyKeyDown)
            {
                sceaneflg = true;
                AudioMnagerController.SE1.Play();
            }

            //初めにWhiteImageのアルファを小さくする
            StartCoroutine(FadeManagerController.Instance.WhiteFadeOut());
            //ボタンを押したら
            if (sceaneflg)
            {
                //BlackImageのアルファを多きくする
                StartCoroutine(FadeManagerController.Instance.BlackFadeIn());
                //アルファが1以上になったら
                if (FadeManagerController.Blackalfa >= 1.0f)
                {
                    sceaneflg = false;
                    //ステージセレクトへ
                    SceneManager.LoadScene("StageSelect");
                }
            }
        }

        //セレクト画面なら
        if (NowScene == "StageSelect")
        {
            if (FadeManagerController.Blackalfa >= 0.0f)
            {
                //初めにBlackImageのアルファを小さくする
                //StartCoroutine(FadeManagerController.Instance.BlackFadeOut());
            }
            //セレクト画面でプレイヤーがステージに触ったら
            //if (SelectStagePlayer.Load)
            //{
            //    StageSelectFade();
            //}
            //クリアしているときにセレクト画面に来たら白のフェード
            //if (PlayerMoveController.Clear >= 1)
            //{
            //    FadeManagerController.Blackalfa = 0.0f;
            //    StartCoroutine(FadeManagerController.Instance.WhiteFadeOut());
            //}
        }

        //ゲームシーンなら
        if (NowScene == "GameScene")
        {
            //ここが何回も呼ばれてるよ
            //AudioMnagerController.BGM2.Play();
            //初めにBlackImageのアルファを小さくする
            StartCoroutine(FadeManagerController.Instance.BlackFadeOut());

            //clear時
            //if (PlayerMoveController.Clear >= 1)
            //{
            //    if (FadeManagerController.Whitealfa <= 1.0f)
            //    {
            //        StartCoroutine(FadeManagerController.Instance.WhiteFadeIn());
            //    }
            //}
        }
    }

    public void StageSelectFade()
    {
        StartCoroutine(FadeManagerController.Instance.BlackFadeOut());
        //if (AudioMnagerController.BGM1.volume >= 0.0f)
        //{
        //    AudioMnagerController.Instance.BGMFadeOut();
        //}
        //if (TIME >= 3.0f)
        //{
        if(FadeManagerController.Blackalfa <= 0)
        {
            AudioMnagerController.BGM1.Pause();
            AudioMnagerController.SE2.Play();
            SceneManager.LoadScene("GameScene");
        }
            TIME = 0.0f;
        //}
    }

    public void TransitionScene(string nextScene)
    {
        SceneManager.LoadScene(nextScene);
    }
}
