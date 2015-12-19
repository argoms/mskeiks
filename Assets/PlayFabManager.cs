#if UNITY_EDITOR

#elif UNITY_ANDROID
#define PLAYFAB_ANDROID
#elif UNITY_IOS
#define PLAYFAB_IOS
#endif

using UnityEngine;

using PlayFab;
using PlayFab.ClientModels;

using System.Collections;
using System.Collections.Generic;


public class PlayFabManager : MonoBehaviour {
  public string PlayFabId;
	public string TitleId;
	public string callStatus = "** PlayFab Console ** \n Run on an Android or iOS device to see automatic device ID authentication.";

  string playerID;
  public string playerUsername { get; private set; }
  public string playerDisplayName { get; private set; }
  public string playerPhotonToken { get; private set; }

  public string AppId = "c3ea583d-92ca-47b3-919d-efc4ef3494dc"; //MAY HAVE TO CHANGE THIS TO THE PLAYFAB INSTEAD OF PHOTON ONE

  ProjectDelegates.PlayFabLoginCallback OnLoginCompletedCallback;

  void Awake () {
		PlayFabSettings.TitleId = TitleId;
	}

	void Start () {
		//Debug.Log("Starting Auto-login Process");
    //Login(TitleId);
    //peasant stuff:
    /*
		#if PLAYFAB_IOS
		PlayFabClientAPI.LoginWithIOSDeviceID (new LoginWithIOSDeviceIDRequest
		{
			DeviceId = SystemInfo.deviceUniqueIdentifier,
			OS = SystemInfo.operatingSystem,
			DeviceModel = SystemInfo.deviceModel,
			CreateAccount = true
		}, onLoginSuccess, null);
		#elif PLAYFAB_ANDROID
		PlayFabClientAPI.LoginWithAndroidDeviceID (new LoginWithAndroidDeviceIDRequest
		{
			AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
			OS = SystemInfo.operatingSystem,
			AndroidDevice = SystemInfo.deviceModel,
			CreateAccount = true
		}, onLoginSuccess, null);
		#endif*/
	}
	
	private void onLoginSuccess(LoginResult result)
	{
	    string advertisingIdType, advertisingIdValue;
	    bool disableAdvertising = false;
        PlayFabClientAPI.GetAdvertisingId(out advertisingIdType, out advertisingIdValue, ref disableAdvertising);
	    this.callStatus = string.Format("PlayFab Authentication Successful!\n"
	        + "Player ID: " + result.PlayFabId) + "\n"
	        + "advertisingIdType: " + advertisingIdType + "\n"
	        + "advertisingIdValue: " + advertisingIdValue + "\n"
	        + "disableAdvertising: " + disableAdvertising;
        Debug.Log(callStatus);
	}
	
	private void onLoginError(PlayFabError error)
	{
		this.callStatus = string.Format("Error {0}: {1}", error.HttpCode, error.ErrorMessage);
		Debug.Log(callStatus);
	}

  public void Login(string username, string password)
  {
    //this.OnLoginCompletedCallback = OnLoginCompletedCallback;

    PlayFabSettings.TitleId = TitleId;



    LoginWithPlayFabRequest request = new LoginWithPlayFabRequest();
    request.Username = username;
    request.Password = password;
    request.TitleId = TitleId;

    PlayFabClientAPI.LoginWithPlayFab(request, OnLoginCompleted, OnLoginError);

    playerDisplayName = username;
    /*
    string titleId = TitleId;
    LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
    {
      TitleId = titleId,
      CreateAccount = true,
      CustomId = SystemInfo.deviceUniqueIdentifier
    };

    PlayFabClientAPI.LoginWithCustomID(request, (result) => {
      PlayFabId = result.PlayFabId;
      
      
      Debug.Log("Got PlayFabID: " + PlayFabId);

      if (result.NewlyCreated)
      {
        Debug.Log("(new account)");
      }
      else
      {
        Debug.Log("(existing account)");
      }
    },
    (error) => {
      Debug.Log("Error logging in player with custom ID:");
      Debug.Log(error.ErrorMessage);
    });*/
  }
  //public void CreateNewUser(string username, string password, string email, ProjectDelegates.PlayFabLoginCallback OnUserCreatedCallback)
  public void CreateNewUser(string username, string password, string email)
  {

    PlayFabSettings.TitleId = TitleId;

    RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest();
    request.Username = username;
    request.Password = password;
    request.Email = email;
    request.TitleId = TitleId;
    PlayFabClientAPI.RegisterPlayFabUser(request, OnRegistrationCompleted, OnLoginError);
    
  }

  void OnUserCreated(ProjectDelegates.PlayFabLoginCallback result)
  {
  }

  void OnRegistrationCompleted(RegisterPlayFabUserResult result)
  {
    Debug.Log(result.Username + "registered successfully");
    playerID = result.PlayFabId;
    playerUsername = result.Username;
    
    Dictionary<string, string> playerData = new Dictionary<string, string>();
    playerData.Add("a", "0");

    UpdateUserDataRequest request = new UpdateUserDataRequest();
    request.Data = playerData;
    request.Permission = UserDataPermission.Public;

    PlayFabClientAPI.UpdateUserData(request, OnAddDataSuccess, OnAddDataError);
  }

  void OnLoginError(PlayFabError error)
  {
    playerDisplayName = null;
    Debug.Log("Login error: " + error.Error + " " + error.ErrorMessage);
    
  }

  public void Logout()
  {
    playerID = "";
    playerUsername = "";
  }

  void OnAddDataSuccess(UpdateUserDataResult result)
  {
    //Everything related to login completed. Time to go back to the manager.
    //OnLoginCompletedCallback(0, 0);

  }

  void OnAddDataError(PlayFabError error)
  {
    Debug.Log("Add data error: " + error.Error + " " + error.ErrorMessage);
  }

  void OnLoginCompleted(LoginResult result)
  {
    Debug.Log(playerDisplayName);
    playerID = result.PlayFabId;

    //PushNotificationsManager.instance.Register();

    //this.LoadTitleData();


//photon stuff remember to re-enable!
    //GetPhotonAuthenticationTokenRequest tokenrequest = new GetPhotonAuthenticationTokenRequest();
    //tokenrequest.PhotonApplicationId = AppId;

    //PlayFabClientAPI.GetPhotonAuthenticationToken(tokenrequest, OnPhotonAuthenticationSuccess, OnPlayFabError);

    /*
    if (result.NewlyCreated)
    {

      Dictionary<string, string> playerData = new Dictionary<string, string>();
      playerData.Add("a", "0");

      UpdateUserDataRequest request = new UpdateUserDataRequest();
      request.Data = playerData;
      request.Permission = UserDataPermission.Public;

      PlayFabClientAPI.UpdateUserData(request, OnAddDataSuccess, OnAddDataError);
    }
    else
    {
      Dictionary<string, string> playerData = new Dictionary<string, string>();

      UpdateUserDataRequest request = new UpdateUserDataRequest();
      request.Data = playerData;
      request.Permission = UserDataPermission.Public;

      PlayFabClientAPI.UpdateUserData(request, OnAddDataSuccess, OnAddDataError);
    }*/
  }

  void OnPhotonAuthenticationSuccess(GetPhotonAuthenticationTokenResult result)
  {
    Debug.Log("token!");
    playerPhotonToken = result.PhotonCustomAuthenticationToken;
  }

  void OnPlayFabError(PlayFabError error)
  {
    Debug.Log("Got an error: " + error.ErrorMessage);
  }
}
