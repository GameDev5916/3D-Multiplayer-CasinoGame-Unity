using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public static Login instance;

    public string Prifile_Image_string;

    public Image profilePic;
    private LoginAndRegistrationPanel login_and_registration;

    //public GameObject loadingPanel;
    private GameObject LoadingScreen;
    private string Profile_photo_Base64_string;

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        Initialization();

        login_and_registration = FindObjectOfType<LoginAndRegistrationPanel>();

        Debug.Log("Login Awake Free Spin: " + Constants.TimerCompletedForBonus);
    }

    public IEnumerator ConverProfileUrlTo64String(string url, Action action)
    {
        if (url != "")
        {
            UnityWebRequest result = UnityWebRequestTexture.GetTexture(url);
            yield return result.SendWebRequest();

            if (result.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network Error");
                Prifile_Image_string = "";
                action?.Invoke();
            }
            else
            {
                Prifile_Image_string = Convert.ToBase64String(DownloadHandlerTexture.GetContent(result).EncodeToJPG());// .EncodeToPNG());
                action?.Invoke();
            }

            //UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            //yield return www.SendWebRequest();

            //if (www.isNetworkError || www.isHttpError)
            //{
            //    Debug.LogError("Network Error");
            //    Prifile_Image_string = "";
            //    action?.Invoke();
            //}
            //else
            //{
            //    Prifile_Image_string = Convert.ToBase64String(DownloadHandlerTexture.GetContent(www).EncodeToPNG());
            //    action?.Invoke();
            //}
        }
    }

    public IEnumerator GetImageFromURL(string url, Action action)
    {
        if (url != "")
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Network Error");
                action?.Invoke();
            }
            else
            {
                Constants.PROFILE_PIC_TEXTURE = DownloadHandlerTexture.GetContent(www);
                //Constants.PLAYER_PHOTO_URL = Convert.ToBase64String(DownloadHandlerTexture.GetContent(www).EncodeToPNG());
                action?.Invoke();
            }
        }
    }

    //private void GetImge(string image_string)
    //{
    //    Texture2D temp = new Texture2D(2, 2);
    //    temp.LoadImage(Convert.FromBase64String(image_string));
    //    Sprite a = Sprite.Create(temp, new Rect(0.0f, 0.0f, temp.width, temp.height), new Vector2(0.5f, 0.5f), 100.0f);
    //    profilePic.sprite = a;
    //    Constants.PROFILE_PIC.sprite = a;
    //}

    #region GOOGLE LOGIN

    public void GoogleLoginButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
#if PLATFORM_WEBGL && !UNITY_EDITOR
            Application.ExternalCall("GoogleSingIn");
#else
        SignInSample.GoogleSignInScript.instance.OnSignIn();
        LoadingScreen = Instantiate(Constants.instance.LoadingScreen);
#endif
    }

    public void GoogleLoginCancal()
    {
        Debug.LogError("Candle");
        Destroy(LoadingScreen);
    }

    public void OnLoginCompleted(string json)
    {
        Destroy(LoadingScreen);

        StartCoroutine(ShowMsg("Login complete from google"));

        Debug.LogError(json);

        JSONNode jsonNode = JSON.Parse(json);

        Constants.PLAYER_ID = jsonNode["player_Id"].Value;
        Constants.EMAIL_ID = jsonNode["Email"].Value;
        Constants.PLAYER_PHOTO_URL = jsonNode["profile_Url"].Value;
        Constants.NAME = jsonNode["Full_Name"].Value;
        Constants.LOGIN_TYPE = "google";

        StartRegister();
    }

    #endregion GOOGLE LOGIN

    #region FACEBOOK LOGIN

    private void Initialization()
    {
        Debug.Log("======initialization=====");

        if (!FB.IsInitialized)
            FB.Init(InitCallback, OnHideUnity);
        else
            FB.ActivateApp();
    }

    private void InitCallback()
    {
        Debug.Log("======init call back=====");


        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            GetMyProfile();
        }
        else
            Debug.Log("Failed to Initialize the Facebook SDK");
    }

    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("======on hide unity=====");

        if (!isGameShown)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void FB_LoginButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        if (FB.IsInitialized)
        {
            var perms = new List<string>() { "public_profile", "email", "user_friends" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
            LoadingScreen = Instantiate(Constants.instance.LoadingScreen);
        }
        else
        {
            Debug.Log("Fb Not Initialized.. ");
        }
    }

    private void AuthCallback(ILoginResult result)
    {
        Debug.Log("======auth callback=====");


        if (FB.IsLoggedIn)
        {
            GetMyProfile();
        }
        else
        {
            Destroy(LoadingScreen);
            Debug.Log("User cancelled login");
        }
    }

    private void ProfilePhotoCallback(IGraphResult result)
    {
        Debug.Log("======profile photo callback=====");


        if (String.IsNullOrEmpty(result.Error) && !result.Cancelled)
        {
            Destroy(LoadingScreen);
            //login_and_registration.ShowWarningMsg("Login complete from facebook");
            StartCoroutine(ShowMsg("Login complete from facebook"));
            IDictionary data = result.ResultDictionary["data"] as IDictionary; //create a new data dictionary
            string photoURL = data["url"] as String; //add a URL field to the dictionary

            Constants.PLAYER_PHOTO_URL = photoURL;
            Debug.Log("PhotoURL: " + photoURL);

            FB.API("me?fields=id,email,name", HttpMethod.GET, GetFacebookData);
        }
    }

    private void GetMyProfile()
    {
        var aToken = AccessToken.CurrentAccessToken;
        FB.API("/me/picture?redirect=false", HttpMethod.GET, ProfilePhotoCallback);
    }

    void GetFacebookData(IGraphResult result)
    {
        Debug.Log("======get facebook data===== : " + result.RawResult);

        if (result.ResultDictionary.ContainsKey("id"))
            Constants.PLAYER_ID = result.ResultDictionary["id"].ToString();
        if (result.ResultDictionary.ContainsKey("email"))
            Constants.EMAIL_ID = result.ResultDictionary["email"].ToString();
        if (result.ResultDictionary.ContainsKey("name"))
            Constants.NAME = result.ResultDictionary["name"].ToString();
        Constants.LOGIN_TYPE = "facebook";

        //Debug.LogError($"Player_ID : {Player_ID}");
        //Debug.LogError($"Email : {Email}");
        //Debug.LogError($"Profile_Photo_URL : {Profile_Photo_URL}");
        //Debug.LogError($" Name : {Name}");

        StartRegister();
        GetFriendsPlayingThisGame();
    }

    void GetFriendsPlayingThisGame()
    {
        Debug.Log("======get friends1=====");

        string query = "/me/friends";

        FB.API(query, HttpMethod.GET, result =>
        {
            Debug.Log("the raw login " + result.RawResult);
            var dictionary = (Dictionary<string, object>)Facebook.MiniJSON.Json.Deserialize(result.RawResult);
            var friendsList = (List<object>)dictionary["data"];
            //friendsText.text = string.Empty;
            foreach (var dict in friendsList)
            {
                Dictionary<string, object> friend = (Dictionary<string, object>)dict;
                if (friend.ContainsKey("id"))
                {
                    string frndID = friend["id"].ToString();
                    if (!Constants.instance.fbFriendList.Contains(frndID))
                        Constants.instance.fbFriendList.Add(frndID);
                }
            }
        });
        //Debug.Log("======get friends2=====");
    }

    #endregion FACEBOOK LOGIN

    #region SEND DATA FOR FACEBOOK AND GOOGLE LOGIN

    public void StartRegister()
    {
        StartCoroutine(ConverProfileUrlTo64String(Constants.PLAYER_PHOTO_URL, () =>
        {
            if (Prifile_Image_string == "")
                StartRegister();
            else
                StartCoroutine(Register());
        }));
    }

    //private void StartRegister()
    //{
    //    StartCoroutine(GetImageFromURL(Profile_Photo_URL, () =>
    //    {
    //        if (Constants.PROFILE_PIC_TEXTURE == null)
    //            StartRegister();
    //        else
    //            StartCoroutine(Register());
    //    }));
    //}

    IEnumerator Register()
    {
        //Debug.Log(" ===== URL : " + Constants.PLAYER_PHOTO_URL);
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
            ["name"] = Constants.NAME,
            ["email"] = Constants.EMAIL_ID,
            ["profile_pic"] = Prifile_Image_string,
            ["login_type"] = Constants.LOGIN_TYPE,
        };
        UnityWebRequest result = UnityWebRequest.Put(Constants.API_FB_GOOGLE_LOGIN, data.ToString());
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        Debug.LogWarning("RegisterData " + data.ToString());
        yield return result.SendWebRequest();

        //if (result.isNetworkError || result.isHttpError)
        if (result.result != UnityWebRequest.Result.Success)
        {
            JSONNode jsonNode = JSON.Parse(result.downloadHandler.text);
            Debug.LogError("RegisterFail " + jsonNode["message"]);
            _ = login_and_registration.ShowWarningMsg(jsonNode["message"]);
        }
        else
        {
            Debug.Log("Registration Success");
            SetPlayerData(result.downloadHandler.text);
        }
    }

    void SetPlayerData(string result)
    {
        Debug.Log("Result SetPlaeyrData : " + result);

        JSONNode jsonNode = JSON.Parse(result)["data"];

        Constants.ISLOGIN = true;
        Constants.SetPlayerData(jsonNode);

        GameObject obj = Instantiate(Constants.instance.LoadingPanel);
        obj.GetComponent<LoadingScreen>().loadSceneName = "Home";
    }

    #endregion SEND DATA FOR FB AND GOOGLE LOGIN

    #region GUEST LOGIN AND RIGISTRATION

    public void GustLoginButtonClick(string email, string password)
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        LoadingScreen = Instantiate(Constants.instance.LoadingScreen);
        StartCoroutine(Guestlogin(email, password));
    }

    IEnumerator Guestlogin(string email, string password)
    {
        JSONNode data = new JSONObject
        {
            ["email"] = email,
            ["password"] = password,
        };

        UnityWebRequest result = UnityWebRequest.Put(Constants.API_GUEST_LOGIN, data.ToString());
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        yield return result.SendWebRequest();

        if (result.isNetworkError || result.isHttpError)
        {
            print("Error downloading: " + result.downloadHandler.text);
            JSONNode jsonNode = JSON.Parse(result.downloadHandler.text);
            Debug.LogError(jsonNode["message"]);
            //login_and_registration.ShowWarningMsg(jsonNode["message"]);
            StartCoroutine(ShowMsg(jsonNode["message"]));
            Destroy(LoadingScreen);
        }
        else
        {
            SetPlayerDataForGuest(result.downloadHandler.text);
        }
    }

    public void GustResitration(string name, string email, string password, string country)
    {
        Debug.LogError(country);
        StartCoroutine(GuestResitration(name, email, password, country));
        LoadingScreen = Instantiate(Constants.instance.LoadingScreen);
    }

    IEnumerator GuestResitration(string name, string email, string password, string country)
    {
        JSONNode data = new JSONObject
        {
            ["name"] = name,
            ["email"] = email,
            ["password"] = password,
            ["unique_id"] = SystemInfo.deviceUniqueIdentifier,
            ["login_type"] = "guest",
            ["country"] = country,
        };
        Debug.Log("Register Data : " + data);

        UnityWebRequest result = UnityWebRequest.Put(Constants.API_GUEST_REGISTRATION, data.ToString());
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        yield return result.SendWebRequest();

        if (result.isNetworkError || result.isHttpError)
        {
            print("Error downloading: " + result.downloadHandler.text);
            JSONNode jsonNode = JSON.Parse(result.downloadHandler.text);
            Debug.LogError(jsonNode["message"]);
            Destroy(LoadingScreen);
            ShowMsg(jsonNode["message"]);
        }
        else
        {

            JSONNode jsonNode = JSON.Parse(result.downloadHandler.text);
            if (jsonNode["status"] == 0)
            {
                //login_and_registration.ShowWarningMsg(jsonNode["message"]);
                StartCoroutine(ShowMsg(jsonNode["message"]));
                Destroy(LoadingScreen);
            }
            else
            {
                Debug.Log("Login Success");
                SetPlayerDataForGuest(result.downloadHandler.text);
            }
        }
    }

    void SetPlayerDataForGuest(string result)
    {
        Debug.Log("Login Result : " + result);

        JSONNode jsonNode = JSON.Parse(result)["data"];

        Constants.ISLOGIN = true;

        Constants.SetPlayerData(jsonNode);
        Destroy(LoadingScreen);
        //Constants.LoadScene("Home");
        GameObject obj = Instantiate(Constants.instance.LoadingPanel);
        obj.GetComponent<LoadingScreen>().loadSceneName = "Home";
    }

    #endregion GUEST LOGIN

    IEnumerator ShowMsg(string msg)
    {
        login_and_registration.StartCoroutine(login_and_registration.ShowWarningMsg(msg));
        yield return new WaitForSeconds(2);
        login_and_registration.warningMsg.SetActive(false);
    }
}
