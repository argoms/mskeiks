using UnityEngine;
using System.Collections;

public class EnemyControl : Photon.MonoBehaviour
{

  public float speed = 10f; //movement speed, should actually be several orders of magnitude higher (player's speed is 4096 or so)
  Rigidbody2D rigidbody; 

  private float lastSynchronizationTime = 0f; //time since last sync from network
  private float syncDelay = 0f;
  private float syncTime = 0f;

  //position storages for interpolation:
  private Vector3 syncStartPosition = Vector3.zero;
  private Vector3 syncEndPosition = Vector3.zero;

  public int health = 10;
  //private GameObject camera;
  public Vector2 movement; //movement direction vector (0 = stationary)

  private TextMesh healthText; 

  //maybe some day this'll be used for rotation interp, but right now rotationEnd is the only used one for directly syncing
  private Quaternion rotationEnd;
  private Quaternion rotationStart;


  public GameObject level;

  public int seed;

  //test stuff
  public double m_InterpolationBackTime = 0.15; //0.15 = 150ms
  public double m_ExtrapolationLimit = 0.5;
  internal struct State
  {
    internal double timestamp;
    internal Vector3 pos;
    internal Vector2 velocity;
    internal Quaternion rot;
  }
  // We store twenty states with "playback" information
  State[] m_BufferedState = new State[20];
  // Keep track of what slots are used
  int m_TimestampCount;

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

    healthText.text = ""+health; //update hud

    if (photonView.isMine) //master client controls movement etc., others just grab info over network and interpolate
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

    /*
    syncTime += Time.deltaTime;
    rigidbody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    transform.rotation = rotationEnd;//Quaternion.Lerp(rotationStart, rotationEnd, syncTime / syncDelay); //rotation;
    */
    //TEST
    // This is the target playback time of the rigid body
    double interpolationTime = PhotonNetwork.time - m_InterpolationBackTime;

    // Use interpolation if the target playback time is present in the buffer
    if (m_BufferedState[0].timestamp > interpolationTime)
    {
      // Go through buffer and find correct state to play back
      for (int i = 0; i < m_TimestampCount; i++)
      {
        if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
        {
          // The state one slot newer (<100ms) than the best playback state
          State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];
          // The best playback state (closest to 100 ms old (default time))
          State lhs = m_BufferedState[i];

          // Use the time between the two slots to determine if interpolation is necessary
          double length = rhs.timestamp - lhs.timestamp;
          float t = 0.0F;
          // As the time difference gets closer to 100 ms t gets closer to 1 in
          // which case rhs is only used
          // Example:
          // Time is 10.000, so sampleTime is 9.900
          // lhs.time is 9.910 rhs.time is 9.980 length is 0.070
          // t is 9.900 - 9.910 / 0.070 = 0.14. So it uses 14% of rhs, 86% of lhs
          if (length > 0.0001)
          {
            t = (float)((interpolationTime - lhs.timestamp) / length);
          }
          // if t=0 => lhs is used directly
          transform.localPosition = Vector3.Lerp(lhs.pos, rhs.pos, t);

          transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
          //animator.SetBool("walking", (rhs.velocity.magnitude > 0.1));
          //Debug.Log(lhs.pos + "||" + rhs.pos + "VVV" + rhs.velocity);
          return;
        }
      }
    }
    // Use extrapolation
    else
    {
      State latest = m_BufferedState[0];

      float extrapolationLength = (float)(interpolationTime - latest.timestamp);
      // Don't extrapolation for more than 500 ms, you would need to do that carefully
      if (extrapolationLength < m_ExtrapolationLimit)
      {
        //float axisLength = extrapolationLength * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
        //Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

        transform.position = latest.pos + (Vector3)(latest.velocity * extrapolationLength);
        //transform.rotation = angularRotation * latest.rot;
        rigidbody.velocity = latest.velocity;
        //animator.SetBool("walking", (latest.velocity.magnitude > 0.1));
      }
    }
  }

  [PunRPC]
  void Attack(Vector3 direction, float attackLifetime, string attackType)
  {
    //Debug.Log("lol");
    
    //animation code goes here

    //create tracer:
    GameObject tracer = (GameObject)Instantiate(Resources.Load("AttackTracer"), transform.position + (transform.rotation * Vector3.up * 0.5f), Quaternion.LookRotation(Vector3.forward, direction));
    tracer.transform.parent = transform;
    AttackTracerBehavior behavior = tracer.GetComponent<AttackTracerBehavior>();
    behavior.lifetime = attackLifetime;
    behavior.friendly = false;
    behavior.SetTracer(attackType);

    PhotonNetwork.SendOutgoingCommands();
  }

  [PunRPC]
  void AnimTrigger(string anim)
  {
    Animator animator = gameObject.GetComponentInChildren<Animator>();
    animator.SetTrigger(anim);
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
    /*
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
      //Debug.Log(syncDelay);
      lastSynchronizationTime = Time.time;


      //client side prediction:
      syncEndPosition = syncPosition + syncVelocity * syncDelay;
      syncStartPosition = rigidbody.position;


    }*/

    // Send data to server
    if (stream.isWriting)
    {
      Vector3 pos = transform.position;
      Quaternion rot = transform.rotation;
      Vector3 velocity = rigidbody.velocity;

      stream.Serialize(ref pos);
      stream.Serialize(ref velocity);
      stream.Serialize(ref rot);
      stream.SendNext(health);
    }
    // Read data from remote client
    else
    {
      Vector3 pos = Vector3.zero;
      Vector3 velocity = Vector3.zero;
      Quaternion rot = Quaternion.identity;
      stream.Serialize(ref pos);
      stream.Serialize(ref velocity);
      stream.Serialize(ref rot);
      health = (int)stream.ReceiveNext();

      // Shift the buffer sideways, deleting state 20
      for (int i = m_BufferedState.Length - 1; i >= 1; i--)
      {
        m_BufferedState[i - 1] = m_BufferedState[i];
      }

      // Record current state in slot 0
      State state;
      state.timestamp = info.timestamp;
      state.pos = pos;
      state.velocity = velocity;
      state.rot = rot;
      m_BufferedState[0] = state;

      // Update used slot count, however never exceed the buffer size
      // Slots aren't actually freed so this just makes sure the buffer is
      // filled up and that uninitalized slots aren't used.
      m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);

      // Check if states are in order, if it is inconsistent you could reshuffel or
      // drop the out-of-order state. Nothing is done here
      for (int i = 0; i < m_TimestampCount - 1; i++)
      {
        if (m_BufferedState[i].timestamp < m_BufferedState[i + 1].timestamp)
          Debug.Log("State inconsistent");
      }
    }
  }


}
