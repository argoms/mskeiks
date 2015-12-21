using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour {
  Animator animator;
  PlayerControl playerControl;
	// Use this for initialization
	void Start () {
    animator = this.GetComponent<Animator>();
    playerControl = this.gameObject.GetComponentInParent<PlayerControl>();
    animator.Play("Walk");
  }
	
	// Update is called once per frame
	void Update () {
    if (playerControl.movement == Vector2.zero)
    {
      animator.Play("Idle");
    }
    else
    {
      animator.Play("Walk");
    }
	}
}
