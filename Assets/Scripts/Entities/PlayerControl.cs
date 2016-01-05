using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerControl : Photon.MonoBehaviour
{

  public float speed = 10f;
  Rigidbody2D rigidbody;
  Transform transform;

  private float lastSynchronizationTime = 0f;
  private float syncDelay = 0f;
  private float syncTime = 0f;
  private Vector3 syncStartPosition = Vector3.zero;
  private Vector3 syncEndPosition = Vector3.zero;

  private GameObject camera;

  public bool canMove;

  private Quaternion rotationEnd;
  private Quaternion rotationStart;
  private TextMesh healthText;
  public int health = 10;
  private PlayFabManager playManager;
  public Vector2 movement;

  private Animator animator;
  public float attackCooldown;

  [SerializeField]
  private Transform lowerTorso;

  [SerializeField]
  private Transform upperTorso;

  [SerializeField]
  private Transform leftLeg;

  [SerializeField]
  private Transform rightLeg;

  [SerializeField]
  private Transform leftLowerLeg;

  [SerializeField]
  private Transform rightLowerLeg;
  //public bool isAttacking;

  private float lastRotation;

  private bool zoomOut; //DEBUG

  private Vector2 syncVelocity;

  private string action;
  private Vector3 walkTarget;
  private bool walking;
  /*
  actions:
    -walk
   
     
  */

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

  //no mroe test


  public struct ActionInfo
  {
    public Vector3 targetPos;
  }

  void Start()
  {
    FindObjectOfType<LevelManager>().UpdateList(); //update playerlist when a player connects
    

    canMove = true;
    rigidbody = GetComponent<Rigidbody2D>();
    transform = GetComponent<Transform>();
    camera = GameObject.Find("Camera");
 

    healthText = transform.Find("HealthDisplay").gameObject.GetComponent<TextMesh>();
    playManager = Object.FindObjectOfType<PlayFabManager>();

    animator = gameObject.GetComponentInChildren<Animator>();//this.GetComponent<Animator>();
    animator.Play("Walk");

    PhotonNetwork.sendRate = 30;

    PhotonNetwork.sendRateOnSerialize = 30;
  }

  void Update()
  {
    //Debug.Log(PhotonNetwork.ServerTimestamp + "||" + PhotonNetwork.time);
    //Debug.Log(isAttacking);
    UpdateHUD();

    attackCooldown -= Time.deltaTime;
    //status updating
    canMove = !(attackCooldown > 0);
    //Debug.Log(PhotonNetwork.GetPing());
    if (photonView.isMine)
    {

      camera.transform.position = GetComponent<Transform>().position + new Vector3(0, -2, zoomOut ? -120 : -6); //camera follows player default -12, -30 for more zoom
      InputMovement();
      Hashtable actionInfo = new Hashtable();
      if (Input.GetMouseButtonDown(0))
      {
        switch (action)
        {
          case "walk":
            Vector3 walkTargeta = Input.mousePosition;
            walkTargeta.z = 6;
            walkTargeta = Camera.main.ScreenToWorldPoint(walkTargeta);
            walkTargeta.z = 0;
            Debug.Log("walking");

            
            actionInfo.Add("target", walkTargeta);
            photonView.RPC("QueueAction", PhotonTargets.AllViaServer, PhotonNetwork.GetPing(), "walk", actionInfo);
            PhotonNetwork.SendOutgoingCommands();
            
            break;

          case "melee":
            Vector3 dir = Input.mousePosition;
            dir.z = 6;
            dir = Camera.main.ScreenToWorldPoint(dir);
            dir.z = 0;
            dir = dir - transform.position;
            Debug.Log("attacking");

            actionInfo.Add("direction", dir);
            
            photonView.RPC("QueueAction", PhotonTargets.AllViaServer, PhotonNetwork.GetPing(), "melee", actionInfo);
            break;
        }
      }
      
      
    }
    else
    {
      InputMovement();
      //SyncedMovement();
    }
  }

  void UpdateHUD()
  {
    healthText.text = playManager.playerDisplayName + ": " + health;
  }

  void InputMovement()
  {
    if (Input.GetKeyDown(KeyCode.Q)) //DEBUG FOR ZOOMING MAP OUT
    {
      zoomOut = !zoomOut;
    }
    if (Input.GetKeyDown(KeyCode.W))
    {
      action = "walk";
      Debug.Log("walkcrosshairtoggle");
    }
    else if (Input.GetKeyDown(KeyCode.Alpha1))
    {
      action = "melee";
      Debug.Log("meleecrosshairtoggle");
    }

    if (walking)
    {
      movement = walkTarget - transform.position;
      //Debug.Log((walkTarget - transform.position));
      transform.rotation = Quaternion.LookRotation(Vector3.forward, movement);
      if ((walkTarget - transform.position).magnitude < 0.1)
      {
        walking = false;
      }
    }
    else
    {
      movement = new Vector2(0, 0);
    }
    
    

    movement.Normalize();
    if (canMove)
    {
      rigidbody.AddForce(movement * speed * Time.deltaTime);
      //animator.SetFloat("moveX", movement.x);
      //animator.SetFloat("moveY", movement.y);


      Vector3 mousePos = Input.mousePosition;
      mousePos.z = 12;
      mousePos = Camera.main.ScreenToWorldPoint(mousePos);
      //transform.rotation = Quaternion.LookRotation(Vector3.forward, (mousePos - transform.position) + Vector3.down * 2);

      //Debug.Log(upperTorso);
      if (movement == Vector2.zero)
      {
        //animator.Play("Idle");
        animator.SetBool("walking", false);
      }
      else
      {
        //animator.Play("Walk");
        animator.SetBool("walking", true);

      }


    }

    
  }

/*  void LateUpdate()
  {
    if (movement != Vector2.zero)
    {
      lastRotation = (Mathf.Atan2(movement.y, movement.x) * 180 / Mathf.PI - 90) + Quaternion.Angle(Quaternion.identity, transform.rotation);
      //lowerTorso.Rotate(Mathf.Atan2(movement.y, movement.x) * 180/Mathf.PI - 90, 0, 0);

      lowerTorso.Rotate(lastRotation, 0, 0);
      upperTorso.Rotate(lastRotation * -1, 0, 0);
    }
    
  }*/

  [PunRPC]
  public void QueueAction(int senderPing, string action,  Hashtable actionInfo)
  {
    double masterPing = PhotonNetwork.GetPing(); //DEBUG SINCE YOu'RE TESTING EVERYTHING ON ONE COMPUTER REMEMBER TO ACTUALLY QUERY PING ONCE DEPLOYED

    double msDelay = 300 - senderPing - masterPing - PhotonNetwork.GetPing();
    if (photonView.isMine)
    {
      msDelay += PhotonNetwork.GetPing();
    }

    //debug check, not sure if best way to do this atm
    if (msDelay > 0)
    {
      Debug.Log("action sent");
    }
    else
    {
      Debug.Log("action send error");
      return;
    }

    double timeDelay = (msDelay * 0.001);
    Debug.Log(action + "||" + timeDelay + "||" + "|a|" + msDelay + "|" + PhotonNetwork.GetPing());


    switch (action)
    {
      case "walk":
        StartCoroutine(BeginWalking(timeDelay, (Vector3)actionInfo["target"]));
        break;

      case "melee":
        StartCoroutine(BeginMelee(timeDelay, (Vector3)actionInfo["direction"]));
        break;
    }
  }

  IEnumerator BeginMelee(double ping, Vector3 direction)
  {
    yield return new WaitForSeconds((float)ping);
    Debug.Log("a");
    transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
    Attack();

  }

  IEnumerator BeginWalking(double ping, Vector3 target)
  {
    yield return new WaitForSeconds((float)ping);
    Debug.Log("a");
    walking = true;
    walkTarget = target;
    //PhotonNetwork.Destroy(this.gameObject);
  }


  [PunRPC]
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
  
  void Attack()
  {
    //animation code goes here
    animator.Play("Attack1_1");

    //step:
    DelayedStep(0, 250);

    //create tracer:
    GameObject tracer = (GameObject)Instantiate(Resources.Load("AttackTracer"), transform.position + (transform.rotation * Vector3.up * 0.5f), transform.rotation);
    tracer.transform.parent = transform;
    tracer.GetComponent<AttackTracerBehavior>().SetTracer("PlayerSword1_1");
    /*
    if (PhotonNetwork.isMasterClient)
    {

    }*/
    //this.rigidbody.AddForce(new Vector2(0, 1000));
  }
  void SyncedMovement()
  {
    
    syncTime += Time.deltaTime;
    rigidbody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    
    transform.rotation = rotationEnd;//Quaternion.Lerp(rotationStart, rotationEnd, syncTime / syncDelay); //rotation;
    
    animator.SetBool("walking", (walking) );
    
    /*
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
          animator.SetBool("walking", (rhs.velocity.magnitude > 0.5));
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
        animator.SetBool("walking", (latest.velocity.magnitude > 0.1));
      }
    }*/

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

    /*
    //TEST
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
    }*/
  }

  //collisions with pickups/zones etc.:
  void OnTriggerEnter2D(Collider2D coll)
  {
   // Debug.Log("hitathing");
    if (coll.gameObject.layer == 14) 
    {
      if (photonView.isMine)
      {
        if (coll.GetComponent<TransitionZoneBehavior>() != null)
        {
          coll.GetComponent<TransitionZoneBehavior>().enabled = true;
        }
      }
    }
  }

  void OnTriggerExit2D(Collider2D coll)
  {
    //Debug.Log("Leftathing");
    if (coll.gameObject.layer == 14)
    {
      if (photonView.isMine)
      {
        if (coll.GetComponent<TransitionZoneBehavior>() != null)
        {
          coll.GetComponent<TransitionZoneBehavior>().enabled = false;
        }
      }
    }
  }

  void DelayedStep(float delayTime, float magnitude)
  {
    if (delayTime != 0)
    {
      StartCoroutine(ImpulseAfterTime(delayTime, magnitude));
    }
    else
    {
      rigidbody.AddForce(transform.rotation * Vector3.up * magnitude);
    }
  }

  IEnumerator ImpulseAfterTime(float delayTime, float magnitude)
  {
    yield return new WaitForSeconds(delayTime);
    rigidbody.AddForce(transform.rotation * Vector3.up * magnitude);
    //control.GetComponent<Rigidbody2D>().AddForce(attackDirection * 100);
  }
}
