
using UnityEngine;
using System.Collections;

public class NetworkManager :Photon. MonoBehaviour
{
  private const string roomName = "RoomName";
  private TypedLobby lobbyName = new TypedLobby("New_Lobby", LobbyType.Default);
  private RoomInfo[] roomsList;
  public GameObject playerPrefab;

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
    GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity, 0);
  }
}
