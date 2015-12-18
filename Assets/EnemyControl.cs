using UnityEngine;
using System.Collections;

public class EnemyControl : Photon.MonoBehaviour
{

  public float speed = 10f;
  Rigidbody2D rigidbody;

  private float lastSynchronizationTime = 0f;
  private float syncDelay = 0f;
  private float syncTime = 0f;
  private Vector3 syncStartPosition = Vector3.zero;
  private Vector3 syncEndPosition = Vector3.zero;

  public int health = 10;
  //private GameObject camera;
  public Vector2 movement;

  private TextMesh healthText;
  private Quaternion rotationEnd;
  private Quaternion rotationStart;
  public GameObject level;
  void Start()
  {
    level = FindObjectOfType<MapGeneration>().gameObject;
    movement = Vector2.up;
    rigidbody = GetComponent<Rigidbody2D>();
    healthText = transform.Find("HealthDisplay").gameObject.GetComponent<TextMesh>();
  }
  void Update()
  {
    //Debug.Log(PhotonNetwork.GetPing());
    healthText.text = ""+health;
    if (photonView.isMine)
    {
      InputMovement();
    }
    else
    {
      SyncedMovement();
    }

    //CircleCollider2D circle = GetComponent<CircleCollider2D>();
  }

  void InputMovement()
  {

    rigidbody.AddForce(movement * speed * Time.deltaTime);
    //Debug.Log(rigidbody.velocity);
  }

  void SyncedMovement()
  {

    syncTime += Time.deltaTime;
    rigidbody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    transform.rotation = rotationEnd;//Quaternion.Lerp(rotationStart, rotationEnd, syncTime / syncDelay); //rotation;
  }

  [PunRPC]
  void Attack(Vector3 direction, float attackLifetime)
  {
    //Debug.Log("lol");
    
    //animation code goes here

    //create tracer:
    GameObject tracer = (GameObject)Instantiate(Resources.Load("AttackTracer"), transform.position + (transform.rotation * direction), Quaternion.LookRotation(Vector3.forward, direction));
    tracer.transform.parent = transform;
    tracer.GetComponent<AttackTracerBehavior>().lifetime = attackLifetime;
    tracer.GetComponent<AttackTracerBehavior>().friendly = false;

    PhotonNetwork.SendOutgoingCommands();
  }

  public void Hit(int damage) //called when entity is hit by an attack
  {
    //animation code goes here

    if (photonView.isMine)
    {
      health -= damage;
      if (health < 1)
      {
        PhotonNetwork.Destroy(this.gameObject);
      }
    }
  }


  void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
  {
    if (stream.isWriting)
    {
      stream.SendNext(rigidbody.position);
      stream.SendNext(rigidbody.velocity);
      stream.SendNext(transform.rotation);
      stream.SendNext(health);
    }
    else
    {
      //rigidbody.position = (Vector3)stream.ReceiveNext();

      //position
      Vector2 syncPosition;
      syncPosition = (Vector2)stream.ReceiveNext();

      //velocity
      Vector2 syncVelocity;
      syncVelocity = (Vector2)stream.ReceiveNext();

      //syncEndPosition = (Vector2)stream.ReceiveNext();
      //syncStartPosition = rigidbody.position;

      //rotation
      rotationEnd = (Quaternion)stream.ReceiveNext();

      //health
      health = (int)stream.ReceiveNext();

      //sync timing
      syncTime = 0f;
      syncDelay = Time.time - lastSynchronizationTime;
      lastSynchronizationTime = Time.time;


      //client side prediction:
      syncEndPosition = syncPosition + syncVelocity * syncDelay;
      syncStartPosition = rigidbody.position;


    }
  }
}
