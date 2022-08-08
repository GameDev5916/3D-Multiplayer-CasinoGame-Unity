using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

#if UNITY_ANDROID
    //[SerializeField] string BannerAdsId = "ca-app-pub-3940256099942544/6300978111";
    //[SerializeField] string InterstitialAdsId = "ca-app-pub-3940256099942544/1033173712";
    [SerializeField] string RewardAdsId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    [SerializeField] string BannerAdsId = "ca-app-pub-3940256099942544/2934735716";
    [SerializeField] string InterstitialAdsId = "ca-app-pub-3940256099942544/4411468910";
    [SerializeField] string RewardAdsId = "ca-app-pub-3940256099942544/1712485313";
#else
    string adUnitId = "unexpected_platform";
#endif

    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;

    private string RewardType;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this) Destroy(gameObject);
    }

    public void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(false);
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        //RequestInterstitial();
        RequestReward();
    }

    //#region IntertisialAds
    //private void RequestInterstitial()
    //{
    //    // Initialize an InterstitialAd.
    //    interstitial = new InterstitialAd(InterstitialAdsId);
    //    // Create an empty ad request.
    //    AdRequest request = new AdRequest.Builder().Build();
    //    // Load the interstitial with the request.
    //    interstitial.LoadAd(request);
    //}

    //public void ShowInterstitialAd()
    //{
    //    if (interstitial.IsLoaded())
    //    {
    //        interstitial.Show();
    //        RequestInterstitial();
    //    }
    //    else
    //    {
    //        RequestInterstitial();
    //    }
    //}
    //#endregion

    #region RewardAds
    private void RequestReward()
    {
#if !PLATFORM_STANDALONE
        this.rewardedAd = new RewardedAd(RewardAdsId);

        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);
#endif
    }

    private void HandleUserEarnedReward(object sender, GoogleMobileAds.Api.Reward e)
    {
        if (RewardType == "Chips")
        {
            Constants.CHIPS += 800000;
            Constants.instance.Chips_Gold_Update();
            Constants.ForAds = true;
            Constants.ShowWarning("You Recive 800,000 Chips");
            Debug.Log("successfully rewarded ");
        }
        else if (RewardType == "FreeBonus")
        {
            FreeBonusTimerAndShop.Instance.StopFreeSpinBonusTimer();
        }
    }

    private void HandleRewardedAdClosed(object sender, EventArgs e)
    {
        RequestReward();
    }

    public void ShowRewardedAd(string RewardFor)
    {
        if (!rewardedAd.IsLoaded())
        {
            Constants.ShowWarning("Ads Not Loaded");
            RequestReward();
            return;
        }
        RewardType = RewardFor;
        rewardedAd.Show();
    }
    #endregion
}
