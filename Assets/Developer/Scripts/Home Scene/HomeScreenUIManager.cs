using System.Collections;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;

public class HomeScreenUIManager : MonoBehaviour
{
    public static HomeScreenUIManager Instance;

    public GameObject HomePanel, ShopPanel, ProfilePanel, TopPanel, ManuPanel, EnterCodePanel, GiftPanel, ReciveChipsPanel, WarningPanel, PokerSelection,
                        VIPPanel, VIPFQAPanel, VIPFQAInfoPanel, SlotSelectionPanel, AllPlayersPanel, InvitationRequestPanel, AddFriendRequestPanel;

    public JSONNode GoldPrices, ChipsPrices, BooterPrices, VIPData, SoltData;

    TopPanel topPanel;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        MainNetworkManager.OnReciveBonusGift += OnBounucRecive;
        MainNetworkManager.JoinInvitation += OnPokerInvitation;
        MainNetworkManager.AllPlayerListAction += OpenAllPlayerPanel;
        MainNetworkManager.AcceptFriendRequest += OpenFriendRequestPanel;

        //StartCoroutine(GetShopData());
        StartCoroutine(GetShopData1());
        StartCoroutine(GetVIPData());
        StartCoroutine(GetSlotData());
        if (Constants.FIRST_TIME_OPEN == 0 && Constants.IS_REFER_CODE_USED == "False")
        {
            EnterCodePanel.SetActive(true);
            Constants.FIRST_TIME_OPEN = 1;
        }
        Debug.Log("Current Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        MainNetworkManager.OnReciveBonusGift -= OnBounucRecive;
        MainNetworkManager.JoinInvitation -= OnPokerInvitation;
        MainNetworkManager.AllPlayerListAction -= OpenAllPlayerPanel;
        MainNetworkManager.AcceptFriendRequest -= OpenFriendRequestPanel;
    }

    private void Start()
    {
        if (Constants.Is_7_Day_Completed && Constants.Is_After_7_day_Gift_Collect == false)
            ReciveChipsPanel.SetActive(true);

        if (Constants.buyChipButtonClicked == true)
        {
            //ShopPanel.SetActive(true);
            StartCoroutine(OpenShop());
        }
    }

    IEnumerator OpenShop()
    {
        yield return new WaitForSeconds(0.1f);
        topPanel = FindObjectOfType<TopPanel>();
        topPanel.ShopButtonClick();
        Constants.buyChipButtonClicked = false;
    }

    IEnumerator GetShopData1(bool retry = false)
    {
        if (retry)
            yield return new WaitForSeconds(3);

        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_SHOP, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode ShopData = JSON.Parse(result);
                Debug.LogError(ShopData.ToString());
                if (!ShopData.HasKey("data"))
                    return;
                for (int i = 0; i < ShopData["data"].Count; i++)
                {
                    if (ShopData["data"][i]["shop_type"] == "gold")
                    {
                        GoldPrices = ShopData["data"][i]["price"];
                    }
                    else if (ShopData["data"][i]["shop_type"] == "chips")
                    {
                        ChipsPrices = ShopData["data"][i]["price"];
                    }
                    else if (ShopData["data"][i]["shop_type"] == "booster")
                    {
                        BooterPrices = ShopData["data"][i]["price"];
                    }
                }
            }
            else
            {
                StartCoroutine(GetShopData1(true));
                Debug.LogError(result);
            }
        }));
    }

    IEnumerator GetVIPData(bool retry = false)
    {
        if (retry)
            yield return new WaitForSeconds(3);

        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_VIP_Data, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode vipdata = JSON.Parse(result);
                if (vipdata == null)
                    return;
                Debug.Log(vipdata);
                VIPData = vipdata;
            }
            else
            {
                StartCoroutine(GetVIPData(true));
                Debug.Log(result);
            }
        }));
    }

    IEnumerator GetSlotData()
    {
        UnityWebRequest result = UnityWebRequest.Get(Constants.API_Get_MostPlayedSlot);
        yield return result.SendWebRequest();

        if (result.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(result.downloadHandler.text);
        }
        else
        {
            Debug.Log(result.downloadHandler.text);
            SoltData = JSON.Parse(result.downloadHandler.text)["slot"];
        }
    }

    private void OnBounucRecive(JSONNode jsonNode)
    {
        Constants.FREE_BONUS_SPIN_IN_GIFT = jsonNode["spinInGift"].AsInt;
        Debug.Log("Bouce Recive : " + jsonNode.ToString());
        GiftPanel.SetActive(true);
        Constants.TimerCompletedForBonus = true;
        Debug.Log("FreeSpin---OnBonusReceive " + Constants.TimerCompletedForBonus);
        FreeBonusTimerAndShop.OnTimerCompleted?.Invoke();
    }

    private void OnPokerInvitation(JSONNode jsonNode)
    {
        if (jsonNode["friendPlayerId"] == Constants.PLAYER_ID)
        {
            Debug.Log("Invitaion-Home");
            InvitationRequestPanel.SetActive(true);
            MainNetworkManager.SetInvitePanel?.Invoke(jsonNode);
        }
    }

    private void OpenAllPlayerPanel(JSONNode jsonNode)
    {
        AllPlayersPanel.SetActive(true);
        MainNetworkManager.SetAllPlayerPanel?.Invoke(jsonNode);
        //MainNetworkManager.DisplayFriendsPanel?.Invoke(jsonNode);
    }

    private void OpenFriendRequestPanel(JSONNode jsonNode)
    {
        if (jsonNode["friendPlayerId"].Value == Constants.PLAYER_ID)
        {
            AddFriendRequestPanel.SetActive(true);
            MainNetworkManager.SetFriendRequestPanel?.Invoke(jsonNode);
        }
        //Debug.Log("OpenFriendRequestPanel~~~Action");
    }
}
