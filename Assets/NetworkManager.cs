﻿
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

  void Start()
  {
    PhotonNetwork.ConnectUsingSettings("v0.0.20");
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
    
    
    Instantiate(level, new Vector3(0, 0, 0), Quaternion.identity); //level
  }
}
