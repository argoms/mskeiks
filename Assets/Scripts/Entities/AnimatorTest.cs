using UnityEngine;
using System.Collections;

public class AnimatorTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
    Animator anim = GetComponent<Animator>();
    anim.Play("Idle");
    //Debug.Log(anim.);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
