using UnityEngine;
using System.Collections;

public class MapGeneration : MonoBehaviour
{

  // Use this for initialization
  void Start()
  {
    Debug.Log("generating map with seed " + PhotonNetwork.room.customProperties["seed"]);
	}
	
	// Update is called once per frame
	void Update ()
  {
	
	}
}
