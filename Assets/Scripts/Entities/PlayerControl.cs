using UnityEngine;
using System.Collections;

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
  //public bool isAttacking;

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
  }

  void Update()
  {
    //Debug.Log(isAttacking);
    UpdateHUD();

    attackCooldown -= Time.deltaTime;
    //status updating
    canMove = !(attackCooldown > 0);
    //Debug.Log(PhotonNetwork.GetPing());
    if (photonView.isMine)
    {

      camera.transform.position = GetComponent<Transform>().position + new Vector3(0, -2, -12); //camera follows player default -12, -30 for more zoom
      InputMovement();
      if (Input.GetMouseButtonDown(0) && attackCooldown < 0)
      {
        photonView.RPC("Attack", PhotonTargets.All);
        PhotonNetwork.SendOutgoingCommands();
        //Attack();
      }
      
      
    }
    else
    {
      SyncedMovement();
    }
  }

  void UpdateHUD()
  {
    healthText.text = playManager.playerDisplayName + ": " + health;
  }

  void InputMovement()
  {
    
    

    movement = new Vector2(0,0);
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
    if (canMove)
    {
      rigidbody.AddForce(movement * speed * Time.deltaTime);
      //animator.SetFloat("moveX", movement.x);
      //animator.SetFloat("moveY", movement.y);


      Vector3 mousePos = Input.mousePosition;
      mousePos.z = 12;
      mousePos = Camera.main.ScreenToWorldPoint(mousePos);
      transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - transform.position);
     
      Debug.Log(upperTorso);
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

  void LateUpdate()
  {
    if (movement != Vector2.zero)
    {
      //lowerTorso.localRotation = Quaternion.LookRotation(lowerTorso.worldToLocalMatrix * movement, lowerTorso.right * -1);
      //lowerTorso.rotation = Quaternion.LookRotation(lowerTorso.right * -1, lowerTorso.worldToLocalMatrix * movement);
      //lowerTorso.rot//Rotate(0, 0, 90);
    }
    
  }
  void OnMouseDown()
  {
    if (photonView.isMine)
    {
      photonView.RPC("Attack", PhotonTargets.All);
      PhotonNetwork.SendOutgoingCommands();
    }
  }

  [PunRPC]
  void CutsceneEnded()
  {
    canMove = true;
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

  [PunRPC]
  void Attack()
  {
    //animation code goes here
    animator.Play("Attack1_1");

    //step:
    DelayedStep(0, 500);
    //create tracer:
    GameObject tracer = (GameObject)Instantiate(Resources.Load("AttackTracer"), transform.position + (transform.rotation * Vector3.up), transform.rotation);
    tracer.transform.parent = transform;
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
