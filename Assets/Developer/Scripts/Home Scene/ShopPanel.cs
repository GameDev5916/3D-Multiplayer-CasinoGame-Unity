using System;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopPanel : MonoBehaviour
{
    public static ShopPanel Instance;

    public GameObject BG;

    public GameObject ButtonParent;
    public GameObject BottomParent;
    public GameObject Bottom;

    public RectTransform ContantOfChip;
    public Button NextButton;
    public Button PrivousButton;

    public List<Sprite> ButtonSelectSpirte;
    public List<Sprite> ButtonNotSelectSpirte;

    [Header("For Boosters")]
    public Sprite CurrentSelectedDay;
    public Sprite NotSelectedDay;
    public GameObject BoosterInfo;
    public GameObject BoosterInfoBG;
    public TextMeshProUGUI BoosterInfoText;

    [Header("Booster For Free Spin")]
    public TextMeshProUGUI BoosterTimerFreeSpin;
    public GameObject WhenBoosterON_ForFreeSpin;
    public GameObject WhenBoosterNotON_ForFreeSpin;
    public GameObject For3dayForFreeSpin;
    public GameObject For7dayForFreeSpin;
    public Image ThreeDayForFreeSpin, SevenDayForFreeSpin;
    int CurrentSelectedDayForBooster;

    [Header("Booster For Level UP")]
    public TextMeshProUGUI BoosterTimerLevelUP;
    public GameObject WhenBoosterON_ForLevelUP;
    public GameObject WhenBoosterNotON_ForLevelUP;
    public GameObject For3dayForLevelUP, For7dayForLevelUP;
    public Image ThreeDayForLevelUP, SevenDayForLevelUP;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
        NextButton.interactable = true;
        PrivousButton.interactable = false;
    }

    private void OnEnable()
    {
        SetShopData();

        FreeBonusTimerAndShop.On_FreeSpin_BoosterTimerUpdate += DisplayTimeForFreeSpinBooster;
        FreeBonusTimerAndShop.On_FreeSpin_BoosterTimerCompleted += OnFreeSpinBoosterTimerCompleted;
        FreeBonusTimerAndShop.On_LevelUp_BoosterTimerUpdate += DisplayTimeForLevelupBooster;
        FreeBonusTimerAndShop.On_LevelUp_BoosterTimerCompleted += OnLevelUPBoosterTimerCompleted;

        BG.GetComponent<RectTransform>().DOAnchorPosX(0, .5f).From(new Vector2(-2200, 0)).SetEase(Ease.InOutBack);

        //DaySelect(0);
        Day3Select(0);
        Day3Select(1);
    }

    private void OnDisable()
    {
        FreeBonusTimerAndShop.On_FreeSpin_BoosterTimerUpdate -= DisplayTimeForFreeSpinBooster;
        FreeBonusTimerAndShop.On_FreeSpin_BoosterTimerCompleted -= OnFreeSpinBoosterTimerCompleted;
        FreeBonusTimerAndShop.On_LevelUp_BoosterTimerUpdate -= DisplayTimeForLevelupBooster;
        FreeBonusTimerAndShop.On_LevelUp_BoosterTimerCompleted -= OnLevelUPBoosterTimerCompleted;
    }

    public void SetShopData()
    {
        for (int i = 0; i < BottomParent.transform.childCount; i++)
        {
            Transform a;
            if (i == 1)
                a = BottomParent.transform.GetChild(i).GetChild(0).GetChild(0).transform;
            else
                a = BottomParent.transform.GetChild(i).transform;

            JSONNode price = "";
            string amount = "";

            switch (i)
            {
                case 0:
                    price = HomeScreenUIManager.Instance.GoldPrices;
                    amount = "gold";
                    break;
                case 1:
                    price = HomeScreenUIManager.Instance.ChipsPrices;
                    amount = "chips";
                    break;
                case 2:
                    price = HomeScreenUIManager.Instance.BooterPrices;
                    amount = "booster";
                    break;

                default:
                    break;
            }

            if (price == null)
                break;

            if (i == 2)
            {
                //Debug.LogError(price[0]["freespinbooster"][0]["price"]);
                //Debug.LogError(price[0]["freespinbooster"][0]["booster"]);
                For3dayForFreeSpin.GetComponent<ShopItems>().PriceText.text = $"$ {price[0]["freespinbooster"][0]["price"]}";
                For3dayForFreeSpin.GetComponent<ShopItems>().Price = price[0]["freespinbooster"][0]["price"].AsFloat;
                For7dayForFreeSpin.GetComponent<ShopItems>().PriceText.text = $"$ {price[0]["freespinbooster"][1]["price"]}";
                For7dayForFreeSpin.GetComponent<ShopItems>().Price = price[0]["freespinbooster"][1]["price"].AsFloat;

                For3dayForLevelUP.GetComponent<ShopItems>().PriceText.text = $"$ {price[1]["levelupbooster"][0]["price"]}";
                For3dayForLevelUP.GetComponent<ShopItems>().Price = price[1]["levelupbooster"][0]["price"].AsFloat;
                For7dayForLevelUP.GetComponent<ShopItems>().PriceText.text = $"$ {price[1]["levelupbooster"][1]["price"]}";
                For7dayForLevelUP.GetComponent<ShopItems>().Price = price[1]["levelupbooster"][1]["price"].AsFloat;
                return;
            }

            for (int j = 0; j < a.childCount; j++)
            {
                ShopItems shopitems= a.GetChild(j).GetComponent<ShopItems>();

                shopitems.Type = amount;

                shopitems.AmountText.text = $"{Constants.NumberShow(price[j][amount].AsLong)}";
                shopitems.Amount = price[j][amount].AsLong;

                shopitems.PriceText.text = $"$ {price[j]["price"]}";
                shopitems.Price = price[j]["price"].AsFloat;
            }
        }
    }

    public void SelectButtonCLick(int number)
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        for (int i = 0; i < ButtonParent.transform.childCount; i++)
        {
            if(i == number)
            {
                ButtonParent.transform.GetChild(i).GetComponent<Image>().sprite = ButtonSelectSpirte[i];
                ButtonParent.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                ButtonParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);

                BottomParent.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ButtonParent.transform.GetChild(i).GetComponent<Image>().sprite = ButtonNotSelectSpirte[i];
                ButtonParent.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                ButtonParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);

                BottomParent.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (number == 2)
        {
            Bottom.SetActive(true);

            WhenBoosterON_ForFreeSpin.SetActive(Constants.IS_FREESPIN_BOOSTER_ON);
            WhenBoosterNotON_ForFreeSpin.SetActive(!Constants.IS_FREESPIN_BOOSTER_ON);
            WhenBoosterON_ForLevelUP.SetActive(Constants.IS_LEVELUP_BOOSTER_ON);
            WhenBoosterNotON_ForLevelUP.SetActive(!Constants.IS_LEVELUP_BOOSTER_ON);

            //if (Constants.IS_FREESPIN_BOOSTER_ON)
            //{
            //    WhenBoosterON_ForFreeSpin.SetActive(true);
            //    WhenBoosterNotON_ForFreeSpin.SetActive(false);
            //}
            //else
            //{
            //    WhenBoosterON_ForFreeSpin.SetActive(false);
            //    WhenBoosterNotON_ForFreeSpin.SetActive(true);
            //}

            //if (Constants.IS_LEVELUP_BOOSTER_ON)
            //{
            //    WhenBoosterON_ForLevelUP.SetActive(true);
            //    WhenBoosterNotON_ForLevelUP.SetActive(false);
            //}
            //else
            //{
            //    WhenBoosterON_ForLevelUP.SetActive(false);
            //    WhenBoosterNotON_ForLevelUP.SetActive(true);
            //}
        }
        else Bottom.SetActive(false);
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosX(2200, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                HomeScreenUIManager.Instance.ShopPanel.SetActive(false);
                Debug.LogError(SceneManager.GetActiveScene().name);
                if (SceneManager.GetActiveScene().name == "Home")
                {
                    HomeScreenUIManager.Instance.HomePanel.SetActive(true);
                    HomeScreenUIManager.Instance.TopPanel.SetActive(true);
                    HomePanel.Instance.ScrollView.SetActive(true);
                    //TopPanel.Instance.BG.enabled = false;
                }
            });
    }

    void DisplayTimeForFreeSpinBooster(float timeToDisplay)
    {
        TimeSpan interval = TimeSpan.FromSeconds(timeToDisplay);
        float days = interval.Days;
        float hours = interval.Hours;
        float minutes = interval.Minutes;
        float seconds = interval.Seconds;

        BoosterTimerFreeSpin.text = string.Format("{0:00} : {1:00} : {2:00} : {3:00}",days, hours, minutes, seconds);
    }

    public void OnFreeSpinBoosterTimerCompleted()
    {
        Constants.IS_FREESPIN_BOOSTER_ON = false;
        WhenBoosterON_ForFreeSpin.SetActive(false);
        WhenBoosterNotON_ForFreeSpin.SetActive(true);
    }

    void DisplayTimeForLevelupBooster(float timeToDisplay)
    {
        TimeSpan interval = TimeSpan.FromSeconds(timeToDisplay);
        float days = interval.Days;
        float hours = interval.Hours;
        float minutes = interval.Minutes;
        float seconds = interval.Seconds;

        BoosterTimerLevelUP.text = string.Format("{0:00} : {1:00} : {2:00} : {3:00}", days, hours, minutes, seconds);
    }

    public void OnLevelUPBoosterTimerCompleted()
    {
        Constants.IS_LEVELUP_BOOSTER_ON = false;
        WhenBoosterON_ForLevelUP.SetActive(false);
        WhenBoosterNotON_ForLevelUP.SetActive(true);
    }

    public void DaySelect(int n)
    {
        //CurrentSelectedDayForBooster = n;
        if (n == 0)
        {
            ThreeDayForFreeSpin.sprite = CurrentSelectedDay;
            SevenDayForFreeSpin.sprite = NotSelectedDay;
            For3dayForFreeSpin.SetActive(true);
            For7dayForFreeSpin.SetActive(false);
        }
        else
        {
            ThreeDayForFreeSpin.sprite = NotSelectedDay;
            SevenDayForFreeSpin.sprite = CurrentSelectedDay;
            For3dayForFreeSpin.SetActive(false);
            For7dayForFreeSpin.SetActive(true);
        }
    }

    public void Day3Select(int n)
    {
        if (n == 0)
        {
            ThreeDayForFreeSpin.sprite = CurrentSelectedDay;
            SevenDayForFreeSpin.sprite = NotSelectedDay;
            For3dayForFreeSpin.SetActive(true);
            For7dayForFreeSpin.SetActive(false);
        }
        else
        {
            ThreeDayForLevelUP.sprite = CurrentSelectedDay;
            SevenDayForLevelUP.sprite = NotSelectedDay;
            For3dayForLevelUP.SetActive(true);
            For7dayForLevelUP.SetActive(false);
        }
    }

    public void Day7Select(int n)
    {
        if (n == 0)
        {
            ThreeDayForFreeSpin.sprite = NotSelectedDay;
            SevenDayForFreeSpin.sprite = CurrentSelectedDay;
            For3dayForFreeSpin.SetActive(false);
            For7dayForFreeSpin.SetActive(true);
        }
        else
        {
            ThreeDayForLevelUP.sprite = NotSelectedDay;
            SevenDayForLevelUP.sprite = CurrentSelectedDay;
            For3dayForLevelUP.SetActive(false);
            For7dayForLevelUP.SetActive(true);
        }
    }

    public void BoosterInfoButtonClick(int n)
    {
        if(n == 0)
        {
            //BoosterInfoText.text = "Booster give you 2x \n amount of chips that you \n win in free bonus spin ";
            BoosterInfoText.text = "This Booster will give you 2x the amount you win on your 4 hour Bonus Spin";
        }
        else if(n == 1)
        {
            //BoosterInfoText.text = "Booster give you 2x \namount of Xp and gift\nFor Level Reward";
            BoosterInfoText.text = "This Booster will give you 2x experience (XP) and will count towards your Level Rewards and for Level Rewards Gold";
        }
        BoosterInfo.SetActive(true);
        BoosterInfoBG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }

    public void DoneButtonClickInBoosterInfo()
    {
        BoosterInfoBG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                BoosterInfo.SetActive(false);
            });
    }

    public void NextButtonClick()
    {
        NextButton.interactable = false;
        PrivousButton.interactable = true;
        ContantOfChip.DOAnchorPosX(-333f, .2f).From(new Vector2(ContantOfChip.anchoredPosition.x, 0)).SetEase(Ease.Linear);
    }

    public void PriviousButtonClick()
    {
        NextButton.interactable = true;
        PrivousButton.interactable = false;
        ContantOfChip.DOAnchorPosX(0f, .2f).From(new Vector2(ContantOfChip.anchoredPosition.x, 0)).SetEase(Ease.Linear);
    }

    public void ChipScrollChange()
    {
        if(ContantOfChip.anchoredPosition.x > -166)
        {
            NextButton.interactable = true;
            PrivousButton.interactable = false;
        }
        else
        {
            NextButton.interactable = false;
            PrivousButton.interactable = true;
        }
    }
}
