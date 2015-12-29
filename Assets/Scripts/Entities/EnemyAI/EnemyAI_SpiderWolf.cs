using UnityEngine;
using System.Collections;

[RequireComponent (typeof(EnemyControl))]
public class EnemyAI_SpiderWolf : MonoBehaviour {

  private GameObject level; //level object (containing tile objects as children & levelmanager component)
  private EnemyControl control; //base control behavior
  private LevelManager levelManager; //the levelmanager on GameObject level
  private float timer; //internal timer for pauses between decisions

  private GameObject target; //object that the spiderwolf wants to kill

  private bool awake; //waking up makes the AI pay more attention to things (longer aggro range etc.)

  //0 = not attacking
  //1 = winding up
  //2 = dangerous
  //3 = cooling down
  private int attackPhase = 0;
  
  //0: none
  //1: lunge
  private int attackType = 0;


  //distance which lunge starts
  public float lungeDist = 3;

  private Vector3 attackDirection;
  private Animator animator;

  private float skittering;
  void Start ()
  {
    //initialize vars/find components to reference later
    control = GetComponent<EnemyControl>();
    level = control.level;
    levelManager = level.GetComponent<LevelManager>();
    awake = true;

    animator = gameObject.GetComponentInChildren<Animator>();
    animator.SetBool("walking", false);
    skittering = 1;
  }
	
	void Update ()
  {
    timer -= Time.deltaTime;
    skittering -= Time.deltaTime;
    switch(attackPhase)
    {
      case 0: //0: not attacking
        if (timer < 0) //does this every second 
        {
          timer = 1;
          target = FindClosestPlayer(awake ? 16 : 4); //search for a nearby player, distance to search depends on whether or not awake
        }
        if (target != null)
        {
          awake = true; //wakes up upon finding target
          
          if (DistToTargetSQ() < lungeDist * lungeDist)
          {
            animator.SetTrigger("beginLunge");
            attackPhase = 1;
            timer = 1;
            attackType = 1;
            //Debug.Log("winding up attack");
            control.movement = Vector2.zero;
            attackDirection =  Vector3.Normalize(target.transform.position - transform.position); 
          }

          if (skittering > 0)
          {
            Vector3 movement = Vector3.Normalize(target.transform.position - this.transform.position);
            control.movement = movement; //set movement direction for control behavior
            animator.SetBool("walking", true);

          }
          else
          {
            control.movement = Vector2.zero; //tell control to stop moving
            animator.SetBool("walking", false);
            if (skittering < -0.5f)
            {
              skittering = 0.5f;
            }
          }
          transform.rotation = Quaternion.LookRotation(Vector3.forward, target.transform.position - transform.position);
          //control.movement = Vector2.up;
        }
        else
        {
          control.movement = Vector2.zero; //tell control to stop moving
          animator.SetBool("walking", false);
        }
        break;

      case 1: //1: winding up attack
        control.movement = Vector2.zero;
        if (timer < 0)
        {
          if (control.photonView.isMine)
          {
            control.photonView.RPC("Attack", PhotonTargets.All, attackDirection, 0.7f, "SpiderWolf_Pounce"); //tells control to attack on all clients
            PhotonNetwork.SendOutgoingCommands();
            control.GetComponent<Rigidbody2D>().AddForce(attackDirection * 2400);
          }
          
          control.GetComponent<Rigidbody2D>().drag = 8f;
          attackPhase = 2;
          timer = 0.2f;
          animator.SetTrigger("pounce");
        }
        break;

      case 2: //2: attacking
        if (timer < 0)
        {
          attackPhase = 3;
          timer = 0.5f;
          animator.SetTrigger("land");
          
        }
        break;

      case 3: //3: after attack cooldown period
        if (timer < 0)
        {
          attackPhase = 0;
          animator.SetTrigger("attackCooled");
          control.GetComponent<Rigidbody2D>().drag = 16f;
        }
        break;
    }

  }

  /*
  void FixedUpdate()
  {
    if (attackPhase == 2)
    {
      if (attackType == 1)
      {
        control.GetComponent<Rigidbody2D>().AddForce(attackDirection * 100); //adds a bit more velocity non-instantaneously, allowing for a longer 'leap' with slower starting speed
      }
    }
  }*/

  GameObject FindClosestPlayer(float maxDist) //returns closest player, maxDist defines furthest away that a player will be recognized
  {
    
    int i = 0;
    if (levelManager != null)
    {
      GameObject finalPlayer = null;//(GameObject)levelManager.playerList[i];
      float finalDistance = 999;

      while (i < levelManager.playerList.Count) //fuck, it's zilch all over again, all I want is if(thing) instead of if(thing != null) D:
      {
        if ((GameObject)levelManager.playerList[i] != null)
        {
          GameObject selectedPlayer = (GameObject)levelManager.playerList[i];

          //doesn't actually get distance (gets the square of dist for faster calculation) , but works for relative measurements, square of actual distance
          float distanceIsh = DistanceSQ(this.transform.position.x - selectedPlayer.transform.position.x, this.transform.position.y - selectedPlayer.transform.position.y);
          distanceIsh = distanceIsh < 0 ? distanceIsh * -1 : distanceIsh; //get absolute value

          if (finalDistance > distanceIsh) //overwrites existing closest player if closer one is found
          {
            finalDistance = distanceIsh;
            finalPlayer = selectedPlayer;
          }
        }
        i++;
      }
      return maxDist * maxDist > finalDistance ? finalPlayer : null; //only returns new player if within given distance
    }
    else
    {
      return null;
    }
  }

  float DistanceSQ(float dx, float dy) //returns the square of the distance defined by vector (dx,dy)
  {
    return dx * dx + dy * dy;
  }

  float DistToTargetSQ() //shortcut for calling distance to target object
  {
    return DistanceSQ(this.transform.position.x - target.transform.position.x, this.transform.position.y - target.transform.position.y);
  }
}
