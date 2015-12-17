using UnityEngine;
using System.Collections;

public class DamageTextBehavior : MonoBehaviour {

	// Use this for initialization
	void Start () {
    StartCoroutine(DieAfterTime());

  }
	
	// Update is called once per frame
	void Update () {
    this.transform.Translate(Vector3.up * Time.deltaTime);
	}

  IEnumerator DieAfterTime()
  {
    yield return new WaitForSeconds(1f);
    if (PhotonNetwork.isMasterClient)
    {
      PhotonNetwork.Destroy(this.gameObject);
    }
    //PhotonNetwork.Destroy(this.gameObject);
  }
}
