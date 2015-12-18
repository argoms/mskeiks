using UnityEngine;
using System.Collections;

[RequireComponent (typeof(EnemyControl))]
public class EnemyAI_SpiderWolf : MonoBehaviour {

  private GameObject level;
  private EnemyControl control;
  private LevelManager levelManager;
  private float timer;

  private GameObject target;

  private bool awake;

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
	void Start ()
  {
    control = GetComponent<EnemyControl>();
    level = control.level;
    levelManager = level.GetComponent<LevelManager>();
    awake = true;
	}
	
	// Update is called once per frame
	void Update ()
  {
    timer -= Time.deltaTime;
    switch(attackPhase)
    {
      case 0:
        if (timer < 0)
        {
          timer = 1;
          target = FindClosestPlayer(awake ? 8 : 4);
        }
        if (target != null)
        {
          awake = true;
          if (DistToTargetSQ() < lungeDist * lungeDist)
          {

            attackPhase = 1;
            timer = 1;
            attackType = 1;
            //Debug.Log("winding up attack");
            control.movement = Vector2.zero;
            attackDirection = transform.rotation * Vector3.Normalize(target.transform.position - this.transform.position);
          } 
          control.movement = Vector3.Normalize(target.transform.position - this.transform.position);
          //control.movement = Vector2.up;
        }
        else
        {
          control.movement = Vector2.zero;
        }
        break;
      case 1:
        control.movement = Vector2.zero;
        if (timer < 0)
        {
          if (control.photonView.isMine)
          {
            control.photonView.RPC("Attack", PhotonTargets.All, attackDirection, 0.2f);
            PhotonNetwork.SendOutgoingCommands();
          }
          control.GetComponent<Rigidbody2D>().AddForce(attackDirection * 800);
          attackPhase = 2;
          timer = 0.2f;
        }
        break;
      case 2:
        if (timer < 0)
        {
          attackPhase = 3;
          timer = 0.5f;
        }
        break;
      case 3:
        if (timer < 0)
        {
          attackPhase = 0;
        }
        break;
    }

  }

  void FixedUpdate()
  {
    if (attackPhase == 2)
    {
      if (attackType == 1)
      {
        control.GetComponent<Rigidbody2D>().AddForce(attackDirection * 100);
      }
    }
  }

  GameObject FindClosestPlayer(float maxDist)
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

          //doesn't actually get distance, but works for relative measurements, square of actual distance
          float distanceIsh = DistanceSQ(this.transform.position.x - selectedPlayer.transform.position.x, this.transform.position.y - selectedPlayer.transform.position.y);
          distanceIsh = distanceIsh < 0 ? distanceIsh * -1 : distanceIsh;

          if (finalDistance > distanceIsh)
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

  float DistanceSQ(float dx, float dy)
  {
    return dx * dx + dy * dy;
  }

  float DistToTargetSQ()
  {
    return DistanceSQ(this.transform.position.x - target.transform.position.x, this.transform.position.y - target.transform.position.y);
  }
}
