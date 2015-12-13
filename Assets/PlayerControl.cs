using UnityEngine;
using System.Collections;

public class PlayerControl : Photon.MonoBehaviour
{

  public float speed = 10f;
  Rigidbody rigidbody;

  private float lastSynchronizationTime = 0f;
  private float syncDelay = 0f;
  private float syncTime = 0f;
  private Vector3 syncStartPosition = Vector3.zero;
  private Vector3 syncEndPosition = Vector3.zero;

  void Start()
  {
    rigidbody = GetComponent<Rigidbody>();
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
  }

  void InputMovement()
  {
    

    //shitty testing movement
    if (Input.GetKey(KeyCode.W))
      rigidbody.MovePosition(rigidbody.position + Vector3.forward * speed * Time.deltaTime);

    if (Input.GetKey(KeyCode.S))
      rigidbody.MovePosition(rigidbody.position - Vector3.forward * speed * Time.deltaTime);

    if (Input.GetKey(KeyCode.D))
      rigidbody.MovePosition(rigidbody.position + Vector3.right * speed * Time.deltaTime);

    if (Input.GetKey(KeyCode.A))
      rigidbody.MovePosition(rigidbody.position - Vector3.right * speed * Time.deltaTime);
  }

  void SyncedMovement()
  {

    syncTime += Time.deltaTime;
    rigidbody.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
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
      
      syncEndPosition = (Vector3)stream.ReceiveNext();
      syncStartPosition = rigidbody.position;

      syncTime = 0f;
      syncDelay = Time.time - lastSynchronizationTime;
      lastSynchronizationTime = Time.time;
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
