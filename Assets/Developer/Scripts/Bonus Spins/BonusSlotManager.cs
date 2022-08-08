using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;

[System.Serializable]
public class ChipsAndRowValue
{
    public int Row_Y_Value;
    public int Chips_Value;
}

[System.Serializable]
public class ItemValues
{
    public int Chips;
    public int Multiplayer;
}

public class BonusSlotManager : MonoBehaviour
{
    public static BonusSlotManager Instance;

    public static Action StartRolling;

    [Header("Panels")]
    [SerializeField] private GameObject LossPanel;
    [SerializeField] private GameObject WinPanel;

    [SerializeField] private GameObject SpinButtonObject;
    [SerializeField] private GameObject TimerShowObject;
    [SerializeField] private GameObject InviteFriendShowObject;

    [Space]
    public TextMeshProUGUI ChipsText;
    public TextMeshProUGUI MultiplayText;
    public TextMeshProUGUI TimerText;

    public Sprite NotOpenDay, CurrentDay, OpendDay;

    int count;
    int row1value, row2value, row3value;

    int WinAmount;
    int WinAmountAfterMultiply;

    public Button SpinButton;

    [Header("List")]
    public List<GameObject> DayShowObject;
    //public int[] Chips;
    //public List<ItemValues> ItemValues2;
    //public List<ItemValues> ItemValues3;

    //public ScrollSnap SS;
    public RectTransform RT;

    public TextMeshProUGUI ToalWinMoneyText;
    public TextMeshProUGUI TotalwinwithMultiplayerText;
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI TotalWinWithBooster;

    public TextMeshProUGUI BonusText;

    int[] multipliers = new int[] { 5, 7, 10, 15, 20, 25, 30, };
    private int DailyMultiplier()
    {
        if (Constants.CurrentDay >= multipliers.Length)
            return multipliers[multipliers.Length - 1];
        return multipliers[Constants.CurrentDay - 1];
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        if (Constants.CurrentDay > 7)
            BonusText.text = "Your 7 day is completed \nNow you can spin with only 30x";
        else
            BonusText.text = "You'll Receive A Special Bonus on 7 day!";

        if (Constants.CurrentDay == 2)
            RT.anchoredPosition = new Vector3(-43, 0, 0);
        else if (Constants.CurrentDay > 2)
            RT.anchoredPosition = new Vector3(-244 - (200 * (Constants.CurrentDay - 3)), 0, 0);

        //SS.SnapToIndex(2);
        for (int i = 0; i < DayShowObject.Count; i++)
        {
            if (i == Constants.CurrentDay - 1)
            {
                //DayShowObject[i].transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                DayShowObject[i].transform.GetChild(0).gameObject.SetActive(true);
                DayShowObject[i].transform.GetChild(1).GetComponent<Image>().sprite = CurrentDay;
            }
            else if (i < Constants.CurrentDay - 1)
            {
                DayShowObject[i].transform.GetChild(0).gameObject.SetActive(false);
                DayShowObject[i].transform.GetChild(1).GetComponent<Image>().sprite = OpendDay;
            }
            else
            {
                DayShowObject[i].transform.GetChild(0).gameObject.SetActive(false);
                DayShowObject[i].transform.GetChild(1).GetComponent<Image>().sprite = NotOpenDay;
            }
        }

        MultiplayText.text = $"{DailyMultiplier()}x";

        bool bSpinEnabled = Constants.TimerCompletedForBonus;
        SpinButtonObject.SetActive(bSpinEnabled);
        TimerShowObject.SetActive(!bSpinEnabled);
        InviteFriendShowObject.SetActive(!bSpinEnabled);
    }

    private void OnEnable()
    {
        FreeBonusTimerAndShop.OnTimerUpdate += DisplayTime;
        FreeBonusTimerAndShop.OnTimerCompleted += TimerCompleted;
    }

    private void OnDisable()
    {
        FreeBonusTimerAndShop.OnTimerUpdate -= DisplayTime;
        FreeBonusTimerAndShop.OnTimerCompleted -= TimerCompleted;
    }

    void DisplayTime(float timeToDisplay)
    {
        TimeSpan interval = TimeSpan.FromSeconds(timeToDisplay);
        //float days = interval.Days;
        float hours = interval.Hours;
        float minutes = interval.Minutes;
        float seconds = interval.Seconds;

        TimerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    private void TimerCompleted()
    {
        DisplayTime(0);
        SpinButtonObject.SetActive(true);
        TimerShowObject.SetActive(false);
        InviteFriendShowObject.SetActive(false);
        SpinButton.enabled = true;
    }

    public void SpinButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        StartRolling?.Invoke();
        SpinButton.enabled = false;
    }

    public void BackButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Constants.GotoScene("Home");
    }

    public void HomeButtonClick()
    {
        Debug.Log("=-=-= HomeButtonClick - FreeBonus =-=-=");
        FreeBonusTimerAndShop.Instance.RestartTimer(FreeBonusTimerAndShop.Instance.IsFromGift);
        FreeBonusTimerAndShop.Instance.IsFromGift = false;
        Constants.TimerCompletedForBonus = false;
        //BackButtonClick();
        WinPanel.SetActive(false);
        SpinButtonObject.SetActive(false);
        TimerShowObject.SetActive(true);
        InviteFriendShowObject.SetActive(true);
    }

    public void FindWin(int row, int value)
    {
        switch (row)
        {
            case 1:
                row1value = value;
                break;
            case 2:
                row2value = value;
                break;
            case 3:
                row3value = value;
                break;
            default:
                break;
        }
        count++;
        if (count == 3)
        {
            CalculaterWinAmount();
            StartCoroutine(ShowWinOrLoss());
        }
    }

    private void CalculaterWinAmount()
    {
        int itemHeight = 210;
        int TotalWin = 0;

        int row1Index = row1value / itemHeight;
        int row2Index = row2value / itemHeight;
        int row3Index = row3value / itemHeight;

        TotalWin = (5 + row1Index) * ((20000 + 1000 * row2Index) + (21000 + 1000 * row3Index));
        //TotalWin *= DailyMultiplier();

        WinAmount = TotalWin;
        Debug.LogError("Not Same Value $" + WinAmount);

        ChipsText.text = $"{Constants.NumberShow(WinAmount)} Chips";

        count = 0;
    }

    IEnumerator ShowWinOrLoss()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.GameWin);

        WinAmountAfterMultiply = WinAmount * DailyMultiplier();

        yield return new WaitForSeconds(1);

        WinPanel.SetActive(true);
        TotalwinwithMultiplayerText.text = $"{ Constants.NumberShow(WinAmount) } x {DailyMultiplier()}x : {Constants.NumberShow(WinAmountAfterMultiply)}";

        DayText.text = $"{ Constants.CurrentDay} Day Bonus";
        if (Constants.IS_FREESPIN_BOOSTER_ON)
        {
            long WinAmountwithBooster = WinAmountAfterMultiply * 2;
            TotalWinWithBooster.gameObject.SetActive(true);
            TotalWinWithBooster.text = $"{Constants.NumberShow(WinAmountAfterMultiply)} x {2}x : {Constants.NumberShow(WinAmountwithBooster)}";

            ToalWinMoneyText.text = $" You win <color=#FDEF3C>{ Constants.NumberShow(WinAmountwithBooster)} Chips</color>";
            Constants.CHIPS += WinAmountwithBooster;
        }
        else
        {
            ToalWinMoneyText.text = $" You win <color=#FDEF3C>{ Constants.NumberShow(WinAmountAfterMultiply)} Chips</color>";

            TotalWinWithBooster.gameObject.SetActive(false);
            Constants.CHIPS += WinAmountAfterMultiply;
        }
        Constants.instance.Chips_Gold_Update();
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

    //public void LeftButtonClick()
    //{
    //    SS.SnapToNext();
    //}

    //public void RightButtonClick()
    //{
    //    SS.SnapToPrev();
    //}
}
