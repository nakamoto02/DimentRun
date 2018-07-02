using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCharaAnimation : MonoBehaviour
{
    private Animator anim;

	// Use this for initialization
	void Start ()
    {
        anim = gameObject.GetComponent<Animator>();
        anim.SetBool("Title", true);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
