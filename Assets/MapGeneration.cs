using UnityEngine;
using System.Collections;

public class MapGeneration : Photon.MonoBehaviour
{
  private bool isFirst;
  // Use this for initialization
  void Start()
  {
    isFirst = PhotonNetwork.isMasterClient;
    Debug.Log("generating map with seed " + PhotonNetwork.room.customProperties["seed"] + (isFirst ? " as master" : " as client"));
    
	}
	
  /*
	// Update is called once per frame
	void Update ()
  {
	
	}*/
}
