using UnityEngine;
using System.Collections;

public class AttackTracerBehavior : MonoBehaviour {

  public bool friendly = true;
  public float lifetime = 0.25f;
  // Use this for initialization
  void Start () {
    StartCoroutine(DieAfterTime());
    this.gameObject.layer = friendly ? 10 : 12; //12 is enemy 10 is friendly (projectile)
  }


  IEnumerator DieAfterTime()
  {
    yield return new WaitForSeconds(lifetime);
    Destroy(this.gameObject);
    //PhotonNetwork.Destroy(this.gameObject);
  }


  void OnTriggerEnter2D(Collider2D coll)
  {
    if (coll.gameObject.layer == 11) //enemy entity layer num
    {
      if (PhotonNetwork.isMasterClient) //only master client creates damage text
      {
        PhotonNetwork.Instantiate("DamageText", coll.gameObject.transform.position, Quaternion.identity, 0);
        coll.GetComponent<EnemyControl>().Hit(1);
      }
    }
    if (coll.gameObject.layer == 9) //friendly entity layer num
    {
      if (PhotonNetwork.isMasterClient)
      {
        PhotonNetwork.Instantiate("DamageText", coll.gameObject.transform.position, Quaternion.identity, 0);
        coll.GetComponent<PlayerControl>().photonView.RPC("Hit", PhotonTargets.All, 1);
      }
    }
  }
}
