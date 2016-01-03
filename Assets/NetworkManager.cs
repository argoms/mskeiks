
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
  private bool autojoin;
  private int roomToJoinType;
  

  /*
  map numbers
    
  0: hub (always named hub)
  1: forest
  */
  void Start()
  {
    PhotonNetwork.ConnectUsingSettings(version);
    PhotonNetwork.sendRate = 30;
    PhotonNetwork.sendRateOnSerialize = 30;
    
    //PhotonNetwork.ConnectToBestCloudServer(version);
    //PhotonNetwork.ConnectToMaster("3.4.9.2931", 8088, "fed2732f-682e-45a2-947a-cd784755636a", version);
    //50.170.122.211"
    

  }

  void Update()
  {
    //Debug.Log(PhotonNetwork.GetPing());
    //Debug.Log(PhotonNetwork.GetPing()); 
  }

  void OnGUI()
  {
    if (!PhotonNetwork.connected ||PhotonNetwork.connecting)
    {
      GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
    else if (PhotonNetwork.room == null)
    {

      hubExists = false;
      // Join Room
      if (roomsList != null && roomsList.Length > 0)
      {
        int j = 0;//secondary index for spacing of buttons since hub doesn't take up a space 
        for (int i = 0; i < roomsList.Length; i++)
        {
          
          if ((int)roomsList[i].customProperties["map"] == 0) //special behavior for finding a room with a hub
          {
            hubExists = true;
          }
          /*
          else if (GUI.Button(new Rect(100, 250 + (110 * j), 250, 100), "Join " + roomsList[i].name))
          {
            PhotonNetwork.JoinRoom(roomsList[i].name);
            
            j++;
          }*/
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

          string[] roomPropsInLobby = { "map" };

          //PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);

          RoomOptions options = new RoomOptions();
          options.maxPlayers = 16; 
          options.isOpen = true;
          options.isVisible = true;
          options.customRoomProperties = customRoomProperties;
          options.customRoomPropertiesForLobby = roomPropsInLobby;
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

  public void JoinRoom(int roomType)
  {
    PhotonNetwork.LeaveRoom();
    PhotonNetwork.JoinLobby();
    roomToJoinType = roomType;
    autojoin = true;

  }
  void OnConnectedToMaster()
  {
    PhotonNetwork.JoinLobby(lobbyName);
  }

  void OnReceivedRoomListUpdate()
  {
    
    
    roomsList = PhotonNetwork.GetRoomList();
    Debug.Log("Room list was updated (" + roomsList.Length + " rooms)");
    //print rooms
    for (int i = 0; i < roomsList.Length; i++)
    {
      Debug.Log("room" + i + ":" + roomsList[i].name + "|" + roomsList[i].customProperties.ToStringFull());
    }
  }

  void OnJoinedLobby()
  {
    Debug.Log("Joined Lobby");
    roomsList = PhotonNetwork.GetRoomList();
    if (autojoin)
    {
      
      autojoin = false;
      Debug.Log("joining forest now");

      string[] roomPropsInLobby = { "map" };
      Hashtable customRoomProperties = new Hashtable();
      customRoomProperties.Add("map", 1);
      customRoomProperties.Add("seed", Random.Range(0, 256));

      //PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);

      RoomOptions options = new RoomOptions();
      options.maxPlayers = 4;
      options.isOpen = true;
      options.isVisible = true;
      options.customRoomPropertiesForLobby = roomPropsInLobby;
      options.customRoomProperties = customRoomProperties;
      PhotonNetwork.JoinOrCreateRoom("Dungeon", options, null);
      //PhotonNetwork.CreateRoom("Dungeon" + "|" + customRoomProperties["seed"], options, null);
      /*
      bool found = false;
      if (roomsList != null)
      {
        for (int i = 0; i < roomsList.Length; i++)
        {
          if ((int)roomsList[i].customProperties["map"] == 1) //special behavior for finding a room with a hub
          {
            PhotonNetwork.JoinRoom(roomsList[i].name);
            found = true;
          }
        }
      }
      else
      {
        found = false;
      }
      if (!found)
      {
        string[] roomPropsInLobby = { "map" };
        Hashtable customRoomProperties = new Hashtable();
        customRoomProperties.Add("map", 1);
        customRoomProperties.Add("seed", Random.Range(0, 256));

        //PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);

        RoomOptions options = new RoomOptions();
        options.maxPlayers = 4;
        options.isOpen = true;
        options.isVisible = true;
        options.customRoomPropertiesForLobby = roomPropsInLobby;
        options.customRoomProperties = customRoomProperties;
        PhotonNetwork.CreateRoom("Dungeon" + "|" + customRoomProperties["seed"], options, null);
      }*/
    }
    
  }

  void OnJoinedRoom()
  {
    Debug.Log("Connected to Room");
    
    Instantiate(level, new Vector3(0, 0, 0), Quaternion.identity); //level
  }

  void OnLeftRoom()
  {
    Destroy(FindObjectOfType<LevelManager>().gameObject);
    Debug.Log("Disconnected from room");
  }

  void OnPhotonPlayerConnected(PhotonPlayer otherPlayer)
  {
    FindObjectOfType<LevelManager>().UpdateList();
    Debug.Log(otherPlayer.name);
  }

  void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
  {
    FindObjectOfType<LevelManager>().UpdateList();
    Debug.Log(otherPlayer.name);
  }

  //host migration? removed for now since there was a more elegant solution
  void OnApplicationQuit()
  {
    /*
    if (PhotonNetwork.isMasterClient && (PhotonNetwork.playerList.Length > 0))
    {
      PhotonPlayer newHost = PhotonNetwork.playerList[0];

      PhotonView[] objects = FindObjectsOfType<PhotonView>();
      for (int i = 0; i < objects.Length; i++)
      {
        PhotonView currentView = objects[i];
        if (currentView.ownerId == PhotonNetwork.masterClient.ID)
        {
          currentView.TransferOwnership(newHost.ID);
        }
      }
    }*/
    Debug.Log("i ded");
  }

  //this shouldn't actually ever get called at the moment but hey what the hell :L
  public void OnOwnershipRequest(object[] viewAndPlayer)
  {
    PhotonView view = viewAndPlayer[0] as PhotonView;
    PhotonPlayer requestingPlayer = viewAndPlayer[1] as PhotonPlayer;

    Debug.Log("OnOwnershipRequest(): Player " + requestingPlayer + " requests ownership of: " + view + ".");
    if (true)
    {
      view.TransferOwnership(requestingPlayer.ID);
    }
  }
}
