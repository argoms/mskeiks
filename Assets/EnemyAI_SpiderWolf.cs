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
	// Use this for initialization
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
    timer += Time.deltaTime;
    if (timer > 1)
    {
      timer = 0;
      target = FindClosestPlayer(awake ? 3 : 2);
    }
    if (target != null)
    {
      awake = true;
      control.movement = Vector3.Normalize(target.transform.position - this.transform.position);
      //control.movement = Vector2.up;
    }
    else
    {
      control.movement = Vector2.zero;
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
          float distanceIsh = ((this.transform.position.x - selectedPlayer.transform.position.x) * (this.transform.position.x - selectedPlayer.transform.position.x))
            + ((this.transform.position.y - selectedPlayer.transform.position.y) * (this.transform.position.y - selectedPlayer.transform.position.y));
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
}
