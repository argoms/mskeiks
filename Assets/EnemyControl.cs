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

  private Quaternion rotationEnd;
  private Quaternion rotationStart;
  public GameObject level;
  void Start()
  {
    level = FindObjectOfType<MapGeneration>().gameObject;
    movement = Vector2.up;
    rigidbody = GetComponent<Rigidbody2D>();
  }
  void Update()
  {
    //Debug.Log(PhotonNetwork.GetPing());
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
  void Attack()
  {
    rigidbody.AddForce((transform.rotation * Vector3.up * 10));
    //animation code goes here

    //create tracer:
    GameObject tracer = (GameObject)Instantiate(Resources.Load("AttackTracer"), transform.position + (transform.rotation * Vector3.up), transform.rotation);
    tracer.transform.parent = transform;
    tracer.GetComponent<AttackTracerBehavior>().friendly = false;
  }

  void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
  {
    if (stream.isWriting)
    {
      stream.SendNext(rigidbody.position);
      stream.SendNext(transform.rotation);
      //stream.SendNext(rigidbody.velocity);
    }
    else
    {
      //rigidbody.position = (Vector3)stream.ReceiveNext();

      syncEndPosition = (Vector2)stream.ReceiveNext();
      syncStartPosition = rigidbody.position;

      rotationEnd = (Quaternion)stream.ReceiveNext();

      syncTime = 0f;
      syncDelay = Time.time - lastSynchronizationTime;
      lastSynchronizationTime = Time.time;

      //client side prediction:
      /*
      Vector3 syncPosition = (Vector3)stream.ReceiveNext();
      Vector3 syncVelocity = (Vector3)stream.ReceiveNext();

      syncTime = 0f;
      syncDelay = Time.time - lastSynchronizationTime;
      lastSynchronizationTime = Time.time;*/

      //syncEndPosition = syncPosition + syncVelocity * syncDelay;
      //syncStartPosition = rigidbody.position;


    }
  }
}
