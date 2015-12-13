using UnityEngine;
using System.Collections;

public class NetworkManager :Photon. MonoBehaviour
{
  private const string roomName = "RoomName";
  private TypedLobby lobbyName = new TypedLobby("New_Lobby", LobbyType.Default);
  private RoomInfo[] roomsList;
  public GameObject player;

  void Start()
  {
    PhotonNetwork.ConnectUsingSettings("v4.2");
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
      // Create Room
      if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
      {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2, isOpen = true, isVisible = true }, lobbyName);
      }

      // Join Room
      if (roomsList != null)
      {
        for (int i = 0; i < roomsList.Length; i++)
        {
          if (GUI.Button(new Rect(100, 250 + (110 * i), 250, 100), "Join " + roomsList[i].name))
          {
            PhotonNetwork.JoinRoom(roomsList[i].name);
          }
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
    PhotonNetwork.Instantiate(player.name, Vector3.up * 5, Quaternion.identity, 0);
  }

  /*
  // Use this for initialization
  void Start()
  {
    PhotonNetwork.ConnectUsingSettings("0.1");
    Debug.Log("potato");
    TextMesh myText = GetComponent<TextMesh>();
    myText.text = "Aaa";
  }
  
  private const string roomName = "RoomName";
  private RoomInfo[] roomsList;

  void OnGUI()
  {
    if (!PhotonNetwork.connected)
    {
      GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

    }
    else if (PhotonNetwork.room == null)
    {
      // Create Room
      if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
        //PhotonNetwork.CreateRoom(roomName + System.Guid.NewGuid().ToString("N"), true, true, 5);
        PhotonNetwork.CreateRoom(roomName + System.Guid.NewGuid().ToString("N"));//, true, true, 5);
      // Join Room
      if (roomsList != null)
      {
        for (int i = 0; i < roomsList.Length; i++)
        {
          if (GUI.Button(new Rect(100, 250 + (110 * i), 250, 100), "Join " + roomsList[i].name))
            PhotonNetwork.JoinRoom(roomsList[i].name);
        }
      }
    }
  }

  void OnReceivedRoomListUpdate()
  {
    roomsList = PhotonNetwork.GetRoomList();
  }
  void OnJoinedRoom()
  {
    Debug.Log("Connected to Room");
    Debug.Log("room: " + PhotonNetwork.room.name);
  }

  
	// Update is called once per frame
	void Update () {
	  TextMesh myText = GetComponent<TextMesh>();
    myText.text = "Aaa";
    Debug.Log(R
	}*/
}
