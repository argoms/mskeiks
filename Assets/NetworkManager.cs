
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager :Photon. MonoBehaviour
{
  private const string roomName = "RoomName";
  private TypedLobby lobbyName = new TypedLobby("New_Lobby", LobbyType.Default);
  private RoomInfo[] roomsList;
  public GameObject playerPrefab;
  public GameObject level;
  public string version;

  private bool hubExists; //whether or not a room at the hub is open at the moment
  

  /*
  map numbers
    
  0: hub (always named hub)
  1: forest
  */
  void Start()
  {
    PhotonNetwork.ConnectUsingSettings(version);
    
  }

  void Update()
  {
    
  }

  void OnGUI()
  {
    if (!PhotonNetwork.connected)
    {
      GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
    else if (PhotonNetwork.room == null)
    {
      hubExists = false;
      // Join Room
      if (roomsList != null)
      {
        int j = 0;//secondary index for spacing of buttons since hub doesn't take up a space 
        for (int i = 0; i < roomsList.Length; i++)
        {
          if ((int)roomsList[i].customProperties["map"] == 0) //special behavior for finding a room with a hub
          {
            hubExists = true;
          }
          else if (GUI.Button(new Rect(100, 250 + (110 * j), 250, 100), "Join " + roomsList[i].name))
          {
            PhotonNetwork.JoinRoom(roomsList[i].name);
            
            j++;
          }
        }
      }

      // Create Room
      //if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
      if (GUI.Button(new Rect(100, 100, 250, 100), "go to hub"))
      {

        if (hubExists)
        {
          PhotonNetwork.JoinRoom("hub0");
        }
        else
        {
          //properties:
          Hashtable customRoomProperties = new Hashtable();
          customRoomProperties.Add("map", 0);
          customRoomProperties.Add("seed", 0);
          //PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);

          RoomOptions options = new RoomOptions();
          options.maxPlayers = 32; //heh, more than max allowed ccu
          options.isOpen = true;
          options.isVisible = true;
          options.customRoomProperties = customRoomProperties;
          PhotonNetwork.CreateRoom("hub" + customRoomProperties["seed"], options, null);

          //forest room creation code, commented out for now:
          /*
          //properties:
          Hashtable customRoomProperties = new Hashtable();
          customRoomProperties.Add("map", 1);
          customRoomProperties.Add("seed", Random.Range(0, 256));
          //PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);

          RoomOptions options = new RoomOptions();
          options.maxPlayers = 4;
          options.isOpen = true;
          options.isVisible = true;
          options.customRoomProperties = customRoomProperties;
          PhotonNetwork.CreateRoom("Dungeon" + "|" + customRoomProperties["seed"], options, null);
          //PhotonNetwork.room.customProperties.Add("plist", new ArrayList());
          */
        }
      }

      


    }
  }

  void OnConnectedToMaster()
  {
    PhotonNetwork.JoinLobby(lobbyName);
  }

  void OnReceivedRoomListUpdate()
  {
    Debug.Log("Room was created");
    roomsList = PhotonNetwork.GetRoomList();
  }

  void OnJoinedLobby()
  {
    Debug.Log("Joined Lobby");
  }

  void OnJoinedRoom()
  {
    Debug.Log("Connected to Room");
    
    
    Instantiate(level, new Vector3(0, 0, 0), Quaternion.identity); //level
  }
}
