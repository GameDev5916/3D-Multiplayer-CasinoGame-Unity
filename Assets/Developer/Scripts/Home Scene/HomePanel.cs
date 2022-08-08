using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using DG.Tweening;
using System;

public class HomePanel : MonoBehaviour
{
    public static HomePanel Instance;

    public Image BG;
    public List<Sprite> allBGSprites = new List<Sprite>();

    public Text TimeText;
    public bool IsTimeComplete;

    public DOTweenAnimation BonusSpinAnimation;

    public GameObject ScrollView;

    public Sprite BgForSlotSelection;

    public int bgnumber;

    public GameObject FreeBonusGameObject;
    public GameObject FreeBonus_WhenTimerCompletedObject;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance == this) Destroy(gameObject);
    }

    private void OnEnable()
    {
        FreeBonusTimerAndShop.OnTimerUpdate += DisplayTime;
        FreeBonusTimerAndShop.OnTimerCompleted += TimerCompleted;

        Debug.LogWarning("FreeSpinTime1 " + Constants.FREE_BONUS_SPIN_TIME);
        UserInfoUpdate();
        BG.sprite = allBGSprites[bgnumber];
        Debug.LogWarning("FreeSpinTime2 " + Constants.FREE_BONUS_SPIN_TIME);

        if (Constants.FREE_BONUS_SPIN_TIME <= 0)
        {
            FreeBonusGameObject.SetActive(true);
            FreeBonus_WhenTimerCompletedObject.SetActive(false);
        }
        else
        {
            FreeBonusGameObject.SetActive(false);
            FreeBonus_WhenTimerCompletedObject.SetActive(true);
        }

        //Constants.instance.Chips_Gold_Update();
    }

    private void OnDisable()
    {
        FreeBonusTimerAndShop.OnTimerUpdate -= DisplayTime;
        FreeBonusTimerAndShop.OnTimerCompleted -= TimerCompleted;
    }

    void Start()
    {
        if (!Constants.TimerCompletedForBonus)  //(Constants.FREE_BONUS_SPIN_TIME > 0 && !Constants.IS_FREESPIN_BOOSTER_ON)
        {
            //FreeBonusTimerAndShop.Instance.StartTimerForFreeBonus();
            FreeBonusGameObject.SetActive(true);
            FreeBonus_WhenTimerCompletedObject.SetActive(false);
        }
        else
        {
            FreeBonusGameObject.SetActive(false);
            FreeBonus_WhenTimerCompletedObject.SetActive(true);
        }

        if (Constants.ShowSelectSlot)
            Sloat20ButtonClick(0);

        bgnumber = UnityEngine.Random.Range(0, allBGSprites.Count);
        BG.sprite = allBGSprites[bgnumber];

        if (Constants.TimerCompletedForBonus)
            BonusSpinAnimation.DORestart();
    }

    public void FreeSpinButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Constants.GotoScene("FreeSpin");
    }

    public void FreeBonusButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        //if (Constants.TimerCompletedForBonus)
            Constants.GotoScene("Bonus Spins");
    }

    void DisplayTime(float timeToDisplay)
    {
        TimeSpan interval = TimeSpan.FromSeconds(timeToDisplay);
        //float days = interval.Days;
        float hours = interval.Hours;
        float minutes = interval.Minutes;
        float seconds = interval.Seconds;

        TimeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    private void TimerCompleted()
    {
        DisplayTime(0);
        FreeBonus_WhenTimerCompletedObject.SetActive(true);
        FreeBonusGameObject.SetActive(false);
        BonusSpinAnimation.DOPlay();
    }

    public void PokerGamePlayButtonClick()
    {
        ScrollView.SetActive(false);
        HomeScreenUIManager.Instance.PokerSelection.SetActive(true);
        //TopPanel.Instance.BG.enabled = true;
    }

    public void BlackJackGamePlayButtonClick()
    {
        //    HomeScreenUIManager.Instance.HomePanel.SetActive(false);
        //    HomeScreenUIManager.Instance.TopPanel.SetActive(false);
        //    Constants.GotoScene("BlackJack");
    }

    public void Sloat20ButtonClick(int n)
    {
        //SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        //ScrollView.SetActive(false);
        //HomeScreenUIManager.Instance.SlotSelectionPanel.SetActive(true);
        //BG.sprite = BgForSlotSelection;
        //TopPanel.Instance.BG.enabled = true;
    }

    public void GetAllPublicButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        JSONNode jsonnode = new JSONObject
        {
            ["playerId"] = Constants.PLAYER_ID,
        };

        Debug.Log("GetAllPublicButton " + jsonnode.ToString());
        MainNetworkManager.Instance.MainSocket?.Emit("getAllPublicPlayer", jsonnode.ToString());
    }

    public void WatchAdsButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        AdsManager.Instance.ShowRewardedAd("Chips");
    }

    public void WatchAdsForFreeSpin()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        AdsManager.Instance.ShowRewardedAd("FreeBonus");
    }

    public void InviteFrindButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();
        Debug.LogError(Constants.REFER_CODE);
        new NativeShare()
            .SetSubject("Casino").SetText($"Use This Referral Code : { Constants.REFER_CODE }").SetUrl("https://play.google.com/store/apps/details?id=com.vasu.casino.test")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
    }

    public void IncreaseDay()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_IncreaseDay, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.SetPlayerData(jsonNode);
                Debug.LogError("Day Increase");
            }
            else
            {
                Debug.LogError("Faild");
            }
        }));
    }


    void UserInfoUpdate()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.SetPlayerData(jsonNode);
                Debug.LogError("USER_INFO " + jsonNode.ToString());
            }
            else
                Constants.Logout();
        }));
    }
}
