using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using SimpleJSON;

public class FreeBonusTimerAndShop : MonoBehaviour
{
    public static FreeBonusTimerAndShop Instance;

    public static Action<float> OnTimerUpdate, On_FreeSpin_BoosterTimerUpdate, On_LevelUp_BoosterTimerUpdate;
    public static Action OnTimerCompleted, On_FreeSpin_BoosterTimerCompleted, On_LevelUp_BoosterTimerCompleted;
    public static float timer = 14400;

    public bool IsFromGift;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        StartTimerForFreeBonus();

        if (Constants.IS_FREESPIN_BOOSTER_ON)
            StartCoroutine(TimerForFreeSpinBooster());
        if (Constants.IS_LEVELUP_BOOSTER_ON)
            StartCoroutine(TimerForLevelUPBooster());
    }

    public void StartTimerForFreeBonus()
    {
        Debug.Log("StartTimerForFreeSpinCoroutine");
        StartCoroutine(TimerForFreeBonus());
    }

    IEnumerator TimerForFreeBonus()
    {
        OnTimerUpdate?.Invoke(Constants.FREE_BONUS_SPIN_TIME);
        while (Constants.FREE_BONUS_SPIN_TIME > 0)
        {
            yield return new WaitForSeconds(1);
            Constants.FREE_BONUS_SPIN_TIME--;
            OnTimerUpdate?.Invoke(Constants.FREE_BONUS_SPIN_TIME);
            //Debug.Log("Timer: " + Constants.FREE_BONUS_SPIN_TIME);
        }

        Constants.TimerCompletedForBonus = true;
        OnTimerCompleted?.Invoke();
    }

    IEnumerator TimerForFreeSpinBooster()
    {
        On_FreeSpin_BoosterTimerUpdate?.Invoke(Constants.FREESPIN_BOOSTER_TIME);
        while (Constants.FREESPIN_BOOSTER_TIME > 0)
        {
            yield return new WaitForSeconds(1);
            Constants.FREESPIN_BOOSTER_TIME--;
            On_FreeSpin_BoosterTimerUpdate?.Invoke(Constants.FREESPIN_BOOSTER_TIME);
        }

        Debug.LogError("Booster FreeSpin Stop:");

        Constants.IS_FREESPIN_BOOSTER_ON = false;
        On_FreeSpin_BoosterTimerCompleted?.Invoke();
        StopCoroutine(TimerForFreeSpinBooster());
    }

    IEnumerator TimerForLevelUPBooster()
    {
        On_LevelUp_BoosterTimerUpdate?.Invoke(Constants.LEVELUP_BOOSTER_TIME);
        while (Constants.LEVELUP_BOOSTER_TIME > 0)
        {
            yield return new WaitForSeconds(1);
            Constants.LEVELUP_BOOSTER_TIME--;
            On_LevelUp_BoosterTimerUpdate?.Invoke(Constants.LEVELUP_BOOSTER_TIME);
        }

        Debug.LogError("Booster LevelUP Stop:");

        Constants.FREESPIN_BOOSTER_TIME = 0;
        On_LevelUp_BoosterTimerCompleted?.Invoke();
        StopCoroutine(TimerForFreeSpinBooster());
    }

    public void StopFreeSpinBonusTimer()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_Stop_FreeSpin, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.FREE_BONUS_SPIN_TIME = jsonNode["freespin_timer"].AsInt;
                Constants.TimerCompletedForBonus = true;
                Debug.Log("Stop Free Spin");
            }
            else
            {
                Debug.LogError("Faild");
            }
        }));

        Debug.LogError("Stop:");
    }

    public void StartFreeSpinBoosterTimer()
    {
        StartCoroutine(TimerForFreeSpinBooster());
    }

    public void StartLevelUpBoosterTimer()
    {
        StartCoroutine(TimerForLevelUPBooster());
    }

    public void RestartTimer(bool isfromgiftspeen)
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
            ["isfromGift"] = isfromgiftspeen,
        };
        Debug.LogError(data.ToString());

        StartCoroutine(Constants.ApiCall(Constants.API_FreeSpin_Restart, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Debug.LogError(jsonNode);
                Constants.FREE_BONUS_SPIN_TIME = jsonNode["freespin_timer"].AsInt;
                Constants.FREE_BONUS_SPIN_IN_GIFT = jsonNode["giftspin_count"].AsInt;

                if (isfromgiftspeen == false)
                {
                    StartCoroutine(TimerForFreeBonus());
                    Constants.TimerCompletedForBonus = false;
                }
            }
            else
            {
                Debug.LogError(result);
            }
        }));
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause == false)
        {
            JSONNode data = new JSONObject
            {
                ["unique_id"] = Constants.PLAYER_ID,
            };

            StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
            {
                if (IsSuccess)
                {
                    if (Constants.ForAds)
                    {
                        Constants.ForAds = false;
                        Debug.LogError("NOT UPDATE");
                    }
                    else
                    {
                        Debug.LogError("UPDATE");
                        JSONNode jsonNode = JSON.Parse(result)["data"];
                        Constants.SetPlayerData(jsonNode);
                    }
                }
                else
                    Constants.Logout();
            }));
        }
    }
}
