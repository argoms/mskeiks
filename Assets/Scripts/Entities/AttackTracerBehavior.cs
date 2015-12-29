using UnityEngine;
using System.Collections;

public class AttackTracerBehavior : MonoBehaviour {

  public bool friendly = true;
  public float lifetime = 0.25f;
  private PolygonCollider2D myCollider;
  // Use this for initialization
  void Start () {
    StartCoroutine(DieAfterTime());
    this.gameObject.layer = friendly ? 10 : 12; //12 is enemy 10 is friendly (projectile)
    myCollider = GetComponent<PolygonCollider2D>();
    //Tracer_PlayerSword1_1();
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

  public void SetTracer(string type)
  {
    myCollider = GetComponent<PolygonCollider2D>();
    switch (type)
    {
      case "PlayerSword1_1":
        Tracer_PlayerSword1_1();
        break;
      case "SpiderWolf_Pounce":
        Tracer_SpiderWolf_Pounce();
        break;
    }
  }
  void Tracer_PlayerSword1_1()
  {
    myCollider.points = new[] {
      new Vector2(0, 1),
      new Vector2(1.22f, 1.27f),
      new Vector2(2, 0.12f),
      new Vector2(1.15f, -0.77f),
      new Vector2(0.59f, -0.81f),
      new Vector2(-0.59f, -0.81f),
      new Vector2(-0.57f, -0.8f)
    };

    myCollider.SetPath(0, new[] {
      new Vector2(0, 1),
      new Vector2(1.22f, 1.27f),
      new Vector2(2, 0.12f),
      new Vector2(1.15f, -0.77f),
      new Vector2(0.59f, -0.81f),
      new Vector2(-0.59f, -0.81f),
      new Vector2(-0.57f, -0.8f)
    });
  }

  void Tracer_SpiderWolf_Pounce()
  {
    myCollider.points = new[] {
      new Vector2(0, 0.22f),
      new Vector2(0.4f, 0),
      new Vector2(-0.3f, 0)
    };

    myCollider.SetPath(0, new[] {
      new Vector2(0, 0.22f),
      new Vector2(0.4f, 0),
      new Vector2(-0.3f, 0)
    });
  }
}
