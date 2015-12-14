using UnityEngine;
using System.Collections;

public class PlayerControl : Photon.MonoBehaviour
{

  public float speed = 10f;
  Rigidbody2D rigidbody;

  private float lastSynchronizationTime = 0f;
  private float syncDelay = 0f;
  private float syncTime = 0f;
  private Vector3 syncStartPosition = Vector3.zero;
  private Vector3 syncEndPosition = Vector3.zero;

  private GameObject camera;

  void Start()
  {
    rigidbody = GetComponent<Rigidbody2D>();
    camera = GameObject.Find("Camera");
  }
  void Update()
  {
    //Debug.Log(PhotonNetwork.GetPing());
    if (photonView.isMine)
    {

      camera.transform.position = GetComponent<Transform>().position + new Vector3(0, -2, -12); //camera follows player default -12, -30 for more zoom
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
    Vector2 movement = new Vector2(0,0);
    if (Input.GetKey(KeyCode.W))
    {
      movement.y += 1;
    }

    if (Input.GetKey(KeyCode.S))
    {
      movement.y -= 1;
    }

    if (Input.GetKey(KeyCode.D))
    {
      movement.x += 1;
    }

    if (Input.GetKey(KeyCode.A))
    {
      movement.x -= 1;
    }
    movement.Normalize();
    rigidbody.AddForce(movement * speed * Time.deltaTime) ;
  }

  void SyncedMovement()
  {

    syncTime += Time.deltaTime;
    rigidbody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
  }
  void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
  {
    if (stream.isWriting)
    {
      stream.SendNext(rigidbody.position);
      //stream.SendNext(rigidbody.velocity);
    }
    else
    {
      //rigidbody.position = (Vector3)stream.ReceiveNext();
      
      syncEndPosition = (Vector2)stream.ReceiveNext();
      syncStartPosition = rigidbody.position;

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
