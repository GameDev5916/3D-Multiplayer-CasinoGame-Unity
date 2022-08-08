using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

public class ShopItems : MonoBehaviour
{
    public Text AmountText;
    public Text PriceText;

    public long Amount;
    public float Price;

    public string Type;

    public void BuyButtonClick()
    {
        Debug.LogError($"Type: {Type} Gold: {Amount} Price: {Price}");
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (Type == "gold")
        {
            //Debug.LogError(Constants.GOLDS+Amount);
            Constants.GOLDS += Amount;

            var points = Price switch
            {
                4.99f => Constants.GoldVIPPoint_499,
                9.99f => Constants.GoldVIPPoint_999,
                19.99f => Constants.GoldVIPPoint_1999,
                39.99f => Constants.GoldVIPPoint_3999,
                79.99f => Constants.GoldVIPPoint_7999,
                _ => 0,
            };
            Constants.VIP_POINTS += points;
        }
        else if(Type == "chips")
        {
            //Debug.LogError(Constants.CHIPS + Amount);
            Constants.CHIPS += Amount;
            var points = Price switch
            {
                4.99f => Constants.ChipVIPPoint_499,
                9.99f => Constants.ChipVIPPoint_999,
                19.99f => Constants.ChipVIPPoint_1999,
                39.99f => Constants.ChipVIPPoint_3999,
                79.99f => Constants.ChipVIPPoint_7999,
                120f => Constants.ChipVIPPoint_120,
                _ => 0,
            };
            Constants.VIP_POINTS += points;
        }
        else if(Type == "booster")
        {
            //Debug.LogError("Booster Started:");
            //JSONNode data = new JSONObject
            //{
            //    ["unique_id"] = Constants.PLAYER_ID,
            //};
            //Debug.LogError(data.ToString());

            //StartCoroutine(Constants.ApiCall(Constants.API_Booster_Start, data.ToString(), (bool IsSuccess, string result) =>
            //{
            //    if (IsSuccess)
            //    {
            //        JSONNode jsonNode = JSON.Parse(result)["data"];
            //        Debug.LogError(jsonNode.ToString());
            //        Constants.SetPlayerData(jsonNode);
            //        FreeBonusTimerAndShop.Instance.StartBoosterTimer();

            //        ShopPanel.Instance.WhenBoosterNotON.SetActive(false);
            //        ShopPanel.Instance.WhenBoosterON.SetActive(true);
            //    }
            //    else
            //    {
            //        Debug.LogError(result);
            //    }
            //}));
        }
        Debug.LogError(Constants.VIP_POINTS);

        Constants.instance.Chips_Gold_Update();
    }

    public void BuyBoosterForFreeSpin(int n)
    {
        gameObject.GetComponent<Button>().enabled = false;
        JSONNode userinfo = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_User_Info, userinfo.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode data = new JSONObject
                {
                    ["unique_id"] = Constants.PLAYER_ID,
                    ["forDay"] = n
                };
                Debug.LogError(data.ToString());

                StartCoroutine(Constants.ApiCall(Constants.API_FreeSpin_Booster, data.ToString(), (bool IsSuccess, string result) =>
                {
                    if (IsSuccess)
                    {
                        gameObject.GetComponent<Button>().enabled = true;

                        Debug.LogError("Free Spin Booster Started:");

                        JSONNode jsonNode = JSON.Parse(result)["data"];
                        Debug.LogError(jsonNode.ToString());
                        //Constants.SetPlayerData(jsonNode);
                        Constants.IS_FREESPIN_BOOSTER_ON = jsonNode["freespinboosterOn"].AsBool;
                        Constants.FREESPIN_BOOSTER_TIME = jsonNode["freespinbooster_timer"].AsInt;
                        FreeBonusTimerAndShop.Instance.StartFreeSpinBoosterTimer();

                        ShopPanel.Instance.WhenBoosterNotON_ForFreeSpin.SetActive(false);
                        ShopPanel.Instance.WhenBoosterON_ForFreeSpin.SetActive(true);
                    }
                    else
                    {
                        gameObject.GetComponent<Button>().enabled = true;

                        Debug.LogError("Booster Not Started:");
                        Debug.LogError(result);
                    }
                }));
            }
        }));

        var points = Price switch
        {
            3.00f => Constants.Booster_3_day_VIPPoint,
            5.00f => Constants.Booster_7_day_VIPPoint,
            _ => 0,
        };
        Constants.VIP_POINTS += points;
        Debug.LogError(Constants.VIP_POINTS);
    }

    public void BuyBoosterForLevelUP(int n)
    {
        gameObject.GetComponent<Button>().enabled = false;

        JSONNode userinfo = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_User_Info, userinfo.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode data = new JSONObject
                {
                    ["unique_id"] = Constants.PLAYER_ID,
                    ["forDay"] = n
                };

                Debug.LogError(data.ToString());

                StartCoroutine(Constants.ApiCall(Constants.API_LevelUP_Booster, data.ToString(), (bool IsSuccess, string result) =>
                {
                    if (IsSuccess)
                    {
                        gameObject.GetComponent<Button>().enabled = true;

                        Debug.LogError("LevelUP Booster Started:");

                        JSONNode jsonNode = JSON.Parse(result)["data"];
                        Debug.LogError(jsonNode.ToString());
                        //Constants.SetPlayerData(jsonNode);
                        Constants.IS_LEVELUP_BOOSTER_ON = jsonNode["levelupboosterOn"].AsBool;
                        Constants.LEVELUP_BOOSTER_TIME = jsonNode["levelupbooster_timer"].AsInt;
                        FreeBonusTimerAndShop.Instance.StartLevelUpBoosterTimer();

                        ShopPanel.Instance.WhenBoosterNotON_ForLevelUP.SetActive(false);
                        ShopPanel.Instance.WhenBoosterON_ForLevelUP.SetActive(true);
                    }
                    else
                    {
                        gameObject.GetComponent<Button>().enabled = true;

                        Debug.LogError("Booster Not Started:");
                        Debug.LogError(result);
                    }
                }));
            }
        }));

        var points = Price switch
        {
            3.00f => Constants.Booster_3_day_VIPPoint,
            5.00f => Constants.Booster_7_day_VIPPoint,
            _ => 0,
        };
        Constants.VIP_POINTS += points;
        Debug.LogError(Constants.VIP_POINTS);
    }

    public void ShowInfo()
    {
        ShopPanel.Instance.BoosterInfoText.text = $"You will Receive\n{Constants.NumberShow(Amount)} {Type} \n After Purchasing this";
        ShopPanel.Instance.BoosterInfo.SetActive(true);
        ShopPanel.Instance.BoosterInfoBG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }
}
