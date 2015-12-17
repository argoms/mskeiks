using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

  public ArrayList playerList;

  private float timer = 0;
	// Use this for initialization
	void Start () {
    playerList = new ArrayList();
	
	}

  public void UpdateList()
  {
    playerList = new ArrayList();
    
    Object[] players = Object.FindObjectsOfType<PlayerControl>();
    for (int i = 0; i < players.Length; i++)
    {
      playerList.Add(((PlayerControl)players[i]).gameObject);
    }
  }
}
