using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.Globalization;
using Facebook.Unity;

public class Constants : MonoBehaviour
{
    public static Constants instance;

    public static Action ProfilePicUpadate, On_Level_Or_Percentage_Update, On_Chips_Gold_Update;

    //public static readonly string SERVER_URL = "https://casino.vasundharaapps.com/";      //LiveURL

    public static readonly string SOCKET_URL = "socket.io/";
#if UNITY_EDITOR
    //public static readonly string SERVER_URL = "http://10.10.10.146:8888/";      //LocalURL
    //private static readonly string Main_URL = "http://10.10.10.146:8888/";       //LocalURL
    public static readonly string SERVER_URL = "http://gac.eqg.mybluehost.me:8080/";    //LiveURL
    private static readonly string Main_URL = "http://gac.eqg.mybluehost.me:8080/";       //LiveURL
#else
     public static readonly string SERVER_URL = "http://gac.eqg.mybluehost.me:8080/";    //LiveURL
     private static readonly string Main_URL = "http://gac.eqg.mybluehost.me:8080/";       //LiveURL
     //public static readonly string SERVER_URL = "http://10.10.10.146:8888/";    //LiveURL
     //private static readonly string Main_URL = "http://10.10.10.146:8888/";       //LiveURL
//#else
//    public static readonly string SERVER_URL = "http://139.59.86.248:8520/";      // LiveURL
//    //public static readonly string SERVER_URL = "http://192.168.10.84:7777/";    //LocalURL

//    //private static readonly string Main_URL = "https://casino.vasundharaapps.com/";      //LiveURL

//    private static readonly string Main_URL = "http://139.59.86.248:8520/";      //LiveURL
//    //private static readonly string Main_URL = "http://192.168.10.84:7777/";       //LocalURL
#endif

    public static readonly string API_GUEST_REGISTRATION = Main_URL + "guest_register";
    public static readonly string API_GUEST_LOGIN = Main_URL + "guest_login";
    public static readonly string API_FB_GOOGLE_LOGIN = Main_URL + "login";
    public static readonly string API_PROFILE_UPDATE = Main_URL + "profile_update";
    public static readonly string API_SHOP = Main_URL + "shop";
    public static readonly string API_LevelUpdate = Main_URL + "level_update";
    public static readonly string API_Coin_Gold_Update = Main_URL + "chipsgold_update";
    public static readonly string API_User_Info = Main_URL + "user_info";
    public static readonly string API_FreeSpin_Restart = Main_URL + "freespinrestart";
    public static readonly string API_Stop_FreeSpin = Main_URL + "stopfreespin";
    public static readonly string API_Enter_Refer_Code = Main_URL + "enterinvitecode";
    public static readonly string API_Booster_Start = Main_URL + "boosterstart";
    public static readonly string API_IncreaseDay = Main_URL + "increaseday";
    public static readonly string API_After_SavenDay_Dift_Collect = Main_URL + "aftersevendaygiftuse";
    public static readonly string API_Forgot_Password = Main_URL + "forgotpassword";
    public static readonly string API_Reset_Password = Main_URL + "resetpassword";
    public static readonly string API_FreeSpin_Booster = Main_URL + "freespinbooster";
    public static readonly string API_LevelUP_Booster = Main_URL + "levelupbooster";
    public static readonly string API_VIP_Data = Main_URL + "vip";
    public static readonly string API_Get_MostPlayedSlot = Main_URL + "mostplayedslot";
    public static readonly string API_Updateslot = Main_URL + "updateslot";

#region SOCKETS EMITS STRINGS
    public static readonly string CREATE_ROOM = SERVER_URL + "/createPublicRoom";
    public static readonly string JOINROOM = SERVER_URL + "/joinRoom";
    public static readonly string SEARCH_ROOM = SERVER_URL + "/searchRoom";
    public static readonly string STARTGAME = SERVER_URL + "/startGame";
    public static readonly string DISCONNECTEDMANULLY = SERVER_URL + "/disconnectManually";
#endregion

    public static int CurrentDay;
    public static bool TimerCompletedForBonus;
    public static bool PokerJoinRoom;
    public static bool buyChipButtonClicked = false;

    public static string RoomName;
    public static string RoomType;

    public static string PokerGiftReceiverID;
    public static string PokerGiftSenderID;

    public static string BlackJackGiftReceiverID;
    public static string BlackJackGiftSenderID;

    public static long PokerMaxAmount;
    public static bool BuyMaxOn;
    public static int blackJackPosition;
    public static string blackJackTimer;

    public GameObject LoadingPanel;
    public GameObject LoadingScreen;
    public GameObject WorningPanel;
    public GameObject CheckIntenetPanel;

    public List<Sprite> Flags;
    public List<string> fbFriendList = new List<string>();

    //public JSONNode jsonNodeFriendList;
    public JSONNode FrinedInvitationJsonData;
    public JSONNode AddFriendRequestJsonData;

    public static int pokerMinMaxIndex;
    public static int blackJackMinMaxIndex;

    public static bool ForAds;
    public static int SelectedSlot;
    public static bool ShowSelectSlot;
    public static bool isJoinByInvitation = false;
    public static bool isJoinByStandUp = false;
    public static int SelectedSeat = 0;
    public static int SelectedInvite = 0;
    public static bool AutoReBuy = false;
    public static long AutoBuyAmount = 0;

    private bool isPlayerDataLoaded = false;

#region PLAYERPREFS
    public static int SOUND
    {
        get { return PlayerPrefs.GetInt(nameof(SOUND), 0); }    // 1 = OFF , 0 = ON
        set { PlayerPrefs.SetInt(nameof(SOUND), value); }
    }

    public static int MUSIC
    {
        get { return PlayerPrefs.GetInt(nameof(MUSIC), 0); }    // 1 = OFF , 0 = ON
        set { PlayerPrefs.SetInt(nameof(MUSIC), value); }
    }

    public static bool ISLOGIN
    {
        get { return PlayerPrefs.GetInt(nameof(ISLOGIN), 0) != 0; }
        set { PlayerPrefs.SetInt(nameof(ISLOGIN), value ? 1 : 0); }
    }

    public static int THEAM
    {
        get { return PlayerPrefs.GetInt(nameof(THEAM)); }
        set { PlayerPrefs.SetInt(nameof(THEAM), value); }
    }

    public static int FIRST_TIME_OPEN
    {
        get { return PlayerPrefs.GetInt(nameof(FIRST_TIME_OPEN)); }
        set { PlayerPrefs.SetInt(nameof(FIRST_TIME_OPEN), value); }
    }

#region PLAYERDATA

    public static string PlayerId;
    public static string PLAYER_ID
    {
        get { return PlayerPrefs.GetString(nameof(PLAYER_ID)); }
        set { PlayerPrefs.SetString(nameof(PLAYER_ID), value); }
    }

    public static string NAME
    {
        get { return PlayerPrefs.GetString(nameof(NAME)); }
        set { PlayerPrefs.SetString(nameof(NAME), value); }
    }

    public static string EMAIL_ID
    {
        get { return PlayerPrefs.GetString(nameof(EMAIL_ID)); }
        set { PlayerPrefs.SetString(nameof(EMAIL_ID), value); }
    }

#region ProfilePic64String
    public static string PLAYER_PHOTO_64STRING = "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";
#endregion

    public static string PLAYER_PHOTO_URL
    {
        get { return PlayerPrefs.GetString(nameof(PLAYER_PHOTO_URL)); }
        set { PlayerPrefs.SetString(nameof(PLAYER_PHOTO_URL), value); }
    }

    public static string LOGIN_TYPE
    {
        get { return PlayerPrefs.GetString(nameof(LOGIN_TYPE)); }
        set { PlayerPrefs.SetString(nameof(LOGIN_TYPE), value); }
    }

    //public static int CHIPS
    //{
    //    get { return PlayerPrefs.GetInt(nameof(CHIPS)); }
    //    set { PlayerPrefs.SetInt(nameof(CHIPS), value); instance.StartCoroutine(instance.Chips_Gold_Update()); }
    //}

    //public static int GOLDS
    //{
    //    get { return PlayerPrefs.GetInt(nameof(GOLDS)); }
    //    set { PlayerPrefs.SetInt(nameof(GOLDS), value); instance.StartCoroutine(instance.Chips_Gold_Update()); }
    //}

    static long chips, gold, vipPoints;

    public static long CHIPS
    {
        get { return chips; }
        set
        {
            chips = value;
            //instance.Chips_Gold_Update();
            Debug.LogWarning($"ChipsSet==={CHIPS}");
        }
    }


    public static long GOLDS
    {
        get { return gold; }
        set
        {
            gold = value;
            //instance.Chips_Gold_Update();
            Debug.LogWarning($"GoldSet==={GOLDS}");

        }
    }

    public static long VIP_POINTS
    {
        get { return vipPoints; }
        set { vipPoints = value; }
    }

    public static int LEVEL
    {
        get { return PlayerPrefs.GetInt(nameof(LEVEL), 0); }
        set { PlayerPrefs.SetInt(nameof(LEVEL), value); }
    }

    public static string COUNTRY
    {
        get { return PlayerPrefs.GetString(nameof(COUNTRY)); }
        set { PlayerPrefs.SetString(nameof(COUNTRY), value); }
    }

    public static int LEVEL_PERCENTAGE
    {
        get { return PlayerPrefs.GetInt(nameof(LEVEL_PERCENTAGE)); }
        set { PlayerPrefs.SetInt(nameof(LEVEL_PERCENTAGE), value); instance.StartCoroutine(instance.LevelUpdate()); }
    }

    public static int VIP_TIER_LEVEL
    {
        get { return PlayerPrefs.GetInt(nameof(VIP_TIER_LEVEL)); }
        set { PlayerPrefs.SetInt(nameof(VIP_TIER_LEVEL), value); }
    }

    public static int VIP_TIER_PERCENTAGE
    {
        get { return PlayerPrefs.GetInt(nameof(VIP_TIER_PERCENTAGE)); }
        set { PlayerPrefs.SetInt(nameof(VIP_TIER_PERCENTAGE), value); }
    }

    public static int FREE_BONUS_SPIN_TIME
    {
        get { return PlayerPrefs.GetInt(nameof(FREE_BONUS_SPIN_TIME)); }
        set
        {
            PlayerPrefs.SetInt(nameof(FREE_BONUS_SPIN_TIME), value);
            //Debug.LogWarning("FreeSpinTime=== " + FREE_BONUS_SPIN_TIME);
        }
    }

    public static int FREE_BONUS_SPIN_IN_GIFT
    {
        get { return PlayerPrefs.GetInt(nameof(FREE_BONUS_SPIN_IN_GIFT)); }
        set { PlayerPrefs.SetInt(nameof(FREE_BONUS_SPIN_IN_GIFT), value); }
    }

    public static string REFER_CODE
    {
        get { return PlayerPrefs.GetString(nameof(REFER_CODE)); }
        set { PlayerPrefs.SetString(nameof(REFER_CODE), value); }
    }

    public static string IS_REFER_CODE_USED
    {
        get { return PlayerPrefs.GetString(nameof(IS_REFER_CODE_USED), "True"); }
        set { PlayerPrefs.SetString(nameof(IS_REFER_CODE_USED), value); }
    }

    public static bool IS_FREESPIN_BOOSTER_ON
    {
        get { return PlayerPrefs.GetString(nameof(IS_FREESPIN_BOOSTER_ON)) == "True"; }
        set { PlayerPrefs.SetString(nameof(IS_FREESPIN_BOOSTER_ON), value ? "True" : "False"); }
    }

    public static int FREESPIN_BOOSTER_TIME
    {
        get { return PlayerPrefs.GetInt(nameof(FREESPIN_BOOSTER_TIME)); }
        set { PlayerPrefs.SetInt(nameof(FREESPIN_BOOSTER_TIME), value); }
    }

    public static bool IS_LEVELUP_BOOSTER_ON
    {
        get { return PlayerPrefs.GetString(nameof(IS_LEVELUP_BOOSTER_ON)) == "True"; }
        set { PlayerPrefs.SetString(nameof(IS_LEVELUP_BOOSTER_ON), value ? "True" : "False"); }
    }

    public static int LEVELUP_BOOSTER_TIME
    {
        get { return PlayerPrefs.GetInt(nameof(LEVELUP_BOOSTER_TIME)); }
        set { PlayerPrefs.SetInt(nameof(LEVELUP_BOOSTER_TIME), value); }
    }

    public static bool Is_7_Day_Completed
    {
        get { return PlayerPrefs.GetString(nameof(Is_7_Day_Completed)) == "True"; }
        set { PlayerPrefs.SetString(nameof(Is_7_Day_Completed), value ? "True" : "False"); }
    }

    public static bool Is_After_7_day_Gift_Collect
    {
        get { return PlayerPrefs.GetString(nameof(Is_After_7_day_Gift_Collect)) == "True"; }
        set { PlayerPrefs.SetString(nameof(Is_After_7_day_Gift_Collect), value ? "True" : "False"); }
    }

    public static Image PROFILE_PIC;

    private static Texture profile_Pic_texture;
    public static Texture PROFILE_PIC_TEXTURE
    {
        get { return profile_Pic_texture; }
        set { profile_Pic_texture = value; ProfilePicUpadate?.Invoke(); }
    }

#endregion PLAYERDATA

    public static int TIMER_POKER
    {
        get { return PlayerPrefs.GetInt(nameof(TIMER_POKER)); }
        set { PlayerPrefs.SetInt(nameof(TIMER_POKER), value); }
    }

#endregion PLAYERPREFS

#region Class

    [System.Serializable]
    public class CardThems
    {
        public Sprite CardBackSprite;
        public List<CardSuit> Cards = new List<CardSuit>();
    }

    [System.Serializable]
    public class CardSuit
    {
        public SuitEnum Suit;
        public Sprite[] CardsSprites = new Sprite[13];
    }

    [System.Serializable]
    public enum SuitEnum
    {
        hearts = 1,
        clubs = 2,
        diamonds = 3,
        spades = 4,
    }

#endregion

#region VIPPoints
    public static readonly int ChipVIPPoint_499 = 5;
    public static readonly int ChipVIPPoint_999 = 10;
    public static readonly int ChipVIPPoint_1999 = 15;
    public static readonly int ChipVIPPoint_3999 = 20;
    public static readonly int ChipVIPPoint_7999 = 25;
    public static readonly int ChipVIPPoint_120 = 30;

    public static readonly int GoldVIPPoint_499 = 5;
    public static readonly int GoldVIPPoint_999 = 10;
    public static readonly int GoldVIPPoint_1999 = 15;
    public static readonly int GoldVIPPoint_3999 = 20;
    public static readonly int GoldVIPPoint_7999 = 25;

    public static readonly int Booster_3_day_VIPPoint = 5;
    public static readonly int Booster_7_day_VIPPoint = 10;

    public static readonly int LevelUP = 5;
#endregion VIPPoints

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            InternetConnection();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GetProfileImage();
    }

    public static void GetProfileImage()
    {
        //Debug.Log("");
        //if (PROFILE_PIC_TEXTURE == null)
        {
            if (PLAYER_PHOTO_URL != "" && PLAYER_PHOTO_URL != "null")
            {
                //StartCoroutine(GetImageFromUrl(PLAYER_PHOTO_URL, (Texture image, bool success) =>
                //{
                //    if (success)
                //    {
                //        PROFILE_PIC_TEXTURE = image;
                //        ProfilePicUpadate?.Invoke();
                //    }
                //    else
                //    {
                //        //GetImage();
                //        Debug.Log("");
                //    }
                //}));
                Debug.Log("&&&&&&& PhotoURL: " + PLAYER_PHOTO_URL);
                GetImageFrom64String(PLAYER_PHOTO_URL, (Texture tex) =>
                {
                    PROFILE_PIC_TEXTURE = tex;
                });
            }
            else
            {
                GetImageFrom64String(PLAYER_PHOTO_64STRING, (Texture tex) =>
                {
                    PROFILE_PIC_TEXTURE = tex;
                });
            }
        }
    }

    public static IEnumerator GetImageFromUrl(string Url, Action<Texture, bool> action)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(Url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Network Error");
            action?.Invoke(null, false);

        }
        else
        {
            action?.Invoke(DownloadHandlerTexture.GetContent(www), true);
        }
    }

    public static void GetImageFrom64String(string str, Action<Texture> action)
    {
        var temp = new Texture2D(500, 500);
        temp.LoadImage(Convert.FromBase64String(str));
        action?.Invoke(temp);
    }

    public IEnumerator LevelUpdate()
    {
        if (LEVEL_PERCENTAGE >= 100)
        {
            LEVEL++;
            LEVEL_PERCENTAGE -= 100;
        }

        JSONNode data = new JSONObject
        {
            ["unique_id"] = PLAYER_ID,
            ["level"] = LEVEL,
            ["level_percentage"] = LEVEL_PERCENTAGE,
            ["vip_tier_level"] = LEVEL,
            ["vip_tier_percentage"] = LEVEL_PERCENTAGE,
        };

        UnityWebRequest result = UnityWebRequest.Put(Constants.API_LevelUpdate, data.ToString());
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        yield return result.SendWebRequest();

        if (result.result != UnityWebRequest.Result.Success)
        {
            print("Error downloading: " + result.downloadHandler.text);
            StartCoroutine(LevelUpdate());
        }
        else
        {
            On_Level_Or_Percentage_Update?.Invoke();
        }
    }

    public void Chips_Gold_Update()
    {
        if (isPlayerDataLoaded)
        {
            JSONNode data = new JSONObject
            {
                ["unique_id"] = PLAYER_ID,
                ["chips"] = CHIPS,
                ["gold"] = GOLDS,
            };

            Debug.LogError("ChipsGoldUpdate " + data.ToString());

            StartCoroutine(ApiCall(API_Coin_Gold_Update, data.ToString(), (bool IsSuccess, string result) =>
            {
                if (IsSuccess)
                {
                    On_Chips_Gold_Update?.Invoke();
                }
                else
                {
                    print("Error downloading: " + result);
                    Chips_Gold_Update();
                }
            }));
        }
        else
            On_Chips_Gold_Update?.Invoke();

        //UnityWebRequest result = UnityWebRequest.Put(Constants.API_Coin_Gold_Update, data.ToString());
        //result.method = UnityWebRequest.kHttpVerbPOST;
        //result.SetRequestHeader("Content-Type", "application/json");
        //result.SetRequestHeader("Accept", "application/json");

        //yield return result.SendWebRequest();

        //if (result.isNetworkError || result.isHttpError)
        //{
        //    print("Error downloading: " + result.downloadHandler.text);
        //    StartCoroutine(Chips_Gold_Update());
        //}
        //else
        //{
        //    On_Chips_Gold_Update?.Invoke();
        //}
    }

    public static string NumberShow(long digit)
    {
        //string[] names = { "", "K", "M", "B", "T" };
        //int n = 0;
        //while (n + 1 < names.Length && digit >= 1000m)
        //{
        //    digit /= 1000m;
        //    n++;
        //}
        //return string.Format("{0}{1}", digit, names[n]);

        //return digit.ToString();

        //return string.Format("#,##0.00", digit);
        return digit.ToString("C0", CultureInfo.CreateSpecificCulture("en-US")).Remove(0, 1);    // For USA = "en-US"
        //return string.Format("{0:0,0}", digit);
    }

    public static void GotoScene(string SceneName)
    {
        Instantiate(instance.LoadingPanel).GetComponent<LoadingScreen>().loadSceneName = SceneName;
    }

    public static void ShowWarning(string msg)
    {
        instance.StartCoroutine(instance.ShowWarningMsg(msg));
    }

    public IEnumerator ShowWarningMsg(string msg)
    {
        GameObject WarningPanel = Instantiate(instance.WorningPanel);
        WarningPanel.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = msg;
        yield return new WaitForSeconds(2f);
        Destroy(WarningPanel);
    }

    public static IEnumerator ApiCall(string Api, string JsonData, Action<bool, string> OnComplete)
    {
        UnityWebRequest result = UnityWebRequest.Put(Api, JsonData);
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        yield return result.SendWebRequest();

        if (result.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(result.downloadHandler.text);
            OnComplete?.Invoke(false, result.downloadHandler.text);
        }
        else
        {
            OnComplete?.Invoke(true, result.downloadHandler.text);
        }
    }

    public static void SetPlayerData(JSONNode jsonNode)
    {
        PLAYER_ID = jsonNode["unique_id"].Value;
        NAME = jsonNode["name"].Value;
        EMAIL_ID = jsonNode["email"].Value;
        PLAYER_PHOTO_URL = jsonNode["profile_pic"].Value;
        LOGIN_TYPE = jsonNode["login_type"].Value;

        GOLDS = jsonNode["gold"].AsLong;
        CHIPS = jsonNode["chips"].AsLong;
        COUNTRY = jsonNode["country"].Value;
        LEVEL = jsonNode["level"].AsInt;
        LEVEL_PERCENTAGE = jsonNode["level_percentage"].AsInt;
        VIP_TIER_LEVEL = jsonNode["vip_tier_level"].AsInt;
        VIP_TIER_PERCENTAGE = jsonNode["vip_tier_percentage"].AsInt;

        FREE_BONUS_SPIN_TIME = jsonNode["freespin_timer"].AsInt;

        CurrentDay = jsonNode["login_day_count"].AsInt;
        FREE_BONUS_SPIN_IN_GIFT = jsonNode["giftspin_count"].AsInt;

        REFER_CODE = jsonNode["refercode"].Value;
        IS_REFER_CODE_USED = jsonNode["is_refer_code_use"].Value;

        IS_FREESPIN_BOOSTER_ON = jsonNode["freespinboosterOn"].AsBool;
        FREESPIN_BOOSTER_TIME = jsonNode["freespinbooster_timer"].AsInt;

        IS_LEVELUP_BOOSTER_ON = jsonNode["levelupboosterOn"].AsBool;
        LEVELUP_BOOSTER_TIME = jsonNode["levelupbooster_timer"].AsInt;

        Is_7_Day_Completed = jsonNode["afterSevenDayGift"].AsBool;
        Is_After_7_day_Gift_Collect = jsonNode["afterSevenDailyGiftUse"].AsBool;

        GetProfileImage();

        instance.Chips_Gold_Update();
        instance.isPlayerDataLoaded = true;

        Debug.LogWarning("SetplayerData: " + jsonNode.ToString());
    }

    static bool Onetime;
    GameObject CheckInternatePanel;
    public void InternetConnection()
    {
        StartCoroutine(CheckInternetConnection((bool isconected) =>
        {
            if (isconected == false)
            {
                if (Onetime == false)
                {
                    Debug.Log("Is Not Connected");
                    CheckInternatePanel = Instantiate(CheckIntenetPanel);
                    Onetime = true;
                }
            }
            else
            {
                if (Onetime)
                {
                    Destroy(CheckInternatePanel);
                    Debug.Log("Is Connected");
                    Onetime = false;
                }
            }
        }
        ));
    }

    IEnumerator CheckInternetConnection(Action<bool> action)
    {
        while (true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.google.com/"))
            {
                // Request and wait for the desired page.
                // yield return webRequest.SendWebRequest();

                // if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                // {
                //     action(false);
                // }
                // else
                // {
                //     action(true);
                // }
                 action(true);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    public static bool CheckImageGallaryPermission()
    {
        NativeGallery.Permission permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read);
        if (permission == NativeGallery.Permission.Granted)
            return true;
        else
        {
            NativeGallery.OpenSettings();
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);
            permission = NativeGallery.CheckPermission(NativeGallery.PermissionType.Read);
            return (permission == NativeGallery.Permission.Granted);
        }
    }

    public static void Logout()
    {
        PlayerPrefs.DeleteAll();
        if (FB.IsLoggedIn) FB.LogOut();
        GotoScene("Login");
    }
}
