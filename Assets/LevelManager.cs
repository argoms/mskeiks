using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

  public ArrayList playerList;
  public ArrayList teamSpawnPoints;

  private float timer = 0;
  // Use this for initialization

  private struct PlayerSpawnPoint
  {
    public PlayerSpawnPoint(int _team, Vector3 _pos)
    {
      team = _team;
      position = _pos;
    }
    public int team;
    public Vector3 position;
  }
	void Start () {
    playerList = new ArrayList();
    teamSpawnPoints = new ArrayList();
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

  public void Update()
  {
    if (Input.GetKeyDown(KeyCode.P))
    {
      SpawnPlayer(1);
    }
    if (Input.GetKeyDown(KeyCode.O))
    {
      SpawnPlayer(0);
    }
  }

  public void SpawnPlayer(int team)
  {
    Vector3 spawnPosition = ((PlayerSpawnPoint)teamSpawnPoints[team]).position;
    GameObject newPlayer = PhotonNetwork.Instantiate("Player", spawnPosition, Quaternion.identity, 0);
    newPlayer.GetComponent<PlayerControl>().team = team;
    UpdateList();
    
  }

  public void AddSpawnPoint(Vector3 loc)
  {
    PlayerSpawnPoint newP = new PlayerSpawnPoint(teamSpawnPoints.Count, loc);
    teamSpawnPoints.Add(newP);
  }
}
