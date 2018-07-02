using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeManagerController : SingletonMonoBehaviour<FadeManagerController>
{
    //黒の画像
    public Image BlackImage;
    //白の画像
    public Image WhiteImage;
    //アルファチャンネル
    public static float Whitealfa = 1.0f;
    public static float Blackalfa = 1.0f;

    public enum Scene
    {
        Title,
        StageSelect,
        GameScene
    }

    public void FadeScene(Scene nextScene)
    {
        switch (nextScene)
        {
            case Scene.Title: StartCoroutine(TransitionWhiteFade(nextScene.ToString())); break;
            case Scene.StageSelect: StartCoroutine(TransitionWhiteFade(nextScene.ToString())); break;
            case Scene.GameScene: StartCoroutine(TransitionBlackFade(nextScene.ToString())); break;
        }
    }

    public IEnumerator TransitionBlackFade(string next)
    {
        yield return StartCoroutine(BlackFadeIn());

        SceaneManagerController.Instance.TransitionScene(next);

        yield return StartCoroutine(BlackFadeOut());
    }

    public IEnumerator TransitionWhiteFade(string next)
    {
        Debug.Log("a");

        yield return StartCoroutine(WhiteFadeIn());

        Debug.Log("b");

        SceaneManagerController.Instance.TransitionScene(next);

        yield return StartCoroutine(WhiteFadeOut());
    }

    //白のフェードアウト
    public IEnumerator  WhiteFadeOut()
    {
        while (true)
        {
            Whitealfa = Mathf.Max(Whitealfa - 1.0f * Time.deltaTime, 0);
            WhiteImage.color = new Color(255, 255, 255, Whitealfa);
            if (Whitealfa == 0) break;

            yield return null;
        }
    }

    //黒のフェードアウト
    public IEnumerator BlackFadeOut()
    {
        while (true)
        {
            Blackalfa = Mathf.Max(Blackalfa - 1.0f * Time.deltaTime, 0);
            BlackImage.color = new Color(255, 255, 255, Blackalfa);
            if (Blackalfa == 0) break;
            yield return null;
        }
    }

    //白のフェードイン
    public IEnumerator WhiteFadeIn()
    {
        while (true)
        {
            Whitealfa = Mathf.Min(Whitealfa + 1.0f * Time.deltaTime, 1);
            WhiteImage.color = new Color(255, 255, 255, Whitealfa);
            if (Whitealfa == 1) break;
            yield return null;
        }
    }

    //黒のフェードイン
    public IEnumerator BlackFadeIn()
    {
        while (true)
        {
            Blackalfa = Mathf.Min(Blackalfa + 1.0f * Time.deltaTime, 1);
            BlackImage.color = new Color(255, 255, 255, Blackalfa);
            if (Blackalfa == 1) break;
            yield return null;
        }
    }
}
