using UnityEngine;
using System.Collections;

public class AttackTracerBehavior : MonoBehaviour {

  // Use this for initialization
  void Start () {
    StartCoroutine(DieAfterTime());
  }
	
	// Update is called once per frame
	void Update () {
	}


  IEnumerator DieAfterTime()
  {
    yield return new WaitForSeconds(0.25f);
    Destroy(this.gameObject);
    //PhotonNetwork.Destroy(this.gameObject);
  }
}
