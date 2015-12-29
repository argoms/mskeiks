using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoginMenuBehavior : MonoBehaviour {

  //GameObject usernameField;
  //GameObject passwordField;
  GameObject gameManager;
  public InputField usernameField;
  public InputField passwordField;
  public InputField emailField;
  private InputField highlight; //inputfield currently selected

  
  string username;
  string password;
  PlayFabManager manager;

  
	// Use this for initialization
	void Start () {
    //Debug.Log("potatoeasdasdass all of you");
    //usernameField = GameObject.Find("Username");
    //passwordField = GameObject.Find("Password");
    gameManager = GameObject.Find("GameManager");
    manager = gameManager.GetComponent<PlayFabManager>();

    usernameField.ActivateInputField();


  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Tab)) //tab between fields
    {
      if (usernameField.IsActive())
      {
        passwordField.ActivateInputField();
        usernameField.DeactivateInputField();
        emailField.DeactivateInputField();
      }
      else if (passwordField.IsActive())
      {
        emailField.ActivateInputField();
        passwordField.DeactivateInputField();
        usernameField.DeactivateInputField();
      }
      else if (emailField.IsActive())
      {
        usernameField.ActivateInputField();
        emailField.DeactivateInputField();
        passwordField.DeactivateInputField();
      }
    }

    if (Input.GetKeyDown(KeyCode.Return)) //login on enter press
    {
      LoginExisting();
    }
  }
  public void LoginExisting()
  {
    //    Debug.Log("Username: " + usernameField.text);
    //    Debug.Log("Password: " + passwordField.text);
    manager.Login(usernameField.text, passwordField.text, this);
    
    //    username = usernameField.GetComponentInChildren<Text>
    //Debug.Log("potatoes all of you");
  }

  public void CreateAccount()
  {
    manager.CreateNewUser(usernameField.text, passwordField.text, emailField.text);
    //Debug.Log("Nope.");
  }

  public void SuccessfulLogin()
  {
    if (manager.playerDisplayName != "")
    {
      //Debug.Log("Loginworked");
      gameManager.GetComponent<NetworkManager>().enabled = true;
      Destroy(transform.parent.gameObject);
      //gameManager.GetComponent<MapGeneration>().enabled = true;
    }
  }
}
