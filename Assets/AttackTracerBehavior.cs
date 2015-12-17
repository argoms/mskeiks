using UnityEngine;
using System.Collections;

public class AttackTracerBehavior : MonoBehaviour {

  public bool friendly = true;
  // Use this for initialization
  void Start () {
    StartCoroutine(DieAfterTime());
    this.gameObject.layer = friendly ? 10 : 12; //12 is enemy 10 is friendly (projectile)
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


  void OnTriggerEnter2D(Collider2D coll)
  {
    Debug.Log(coll.gameObject.tag);
    if (coll.gameObject.layer == 11)
    {
      if (PhotonNetwork.isMasterClient)
      {
        PhotonNetwork.Instantiate("DamageText", this.transform.position, Quaternion.identity, 0);
        PhotonNetwork.Destroy(coll.gameObject);
      }
    }
  }
}
