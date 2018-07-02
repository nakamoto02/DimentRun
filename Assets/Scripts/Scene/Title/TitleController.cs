using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour {

    bool fadeFlg = true;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(FadeManagerController.Instance.WhiteFadeOut());
        Invoke("FadeTime", 1.5f);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (fadeFlg) return;

		if(Input.anyKeyDown)
        {
            FadeManagerController.Instance.FadeScene(FadeManagerController.Scene.StageSelect);
            AudioMnagerController.SE1.Play();
        }
	}

    void FadeTime()
    {
        fadeFlg = false;
    }
}
