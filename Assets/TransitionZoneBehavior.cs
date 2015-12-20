using UnityEngine;
using System.Collections;

public class TransitionZoneBehavior : Photon.MonoBehaviour {
  public bool active = true;
  public int type = 1;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  void OnGUI()
  {
    if (active)
    {
      string text = "uninitialized";
      switch (type)
      {
        case 0:
          text = "To Hub";
          break;
        case 1:
          text = "To Forest";
          break;
        default:
          text = "shit's broke D:";
          break;
      }
      if (GUI.Button(new Rect(300, 300, 200, 100), text))
      {
        FindObjectOfType<NetworkManager>().JoinRoom(type);
      }
    }
  }
}
