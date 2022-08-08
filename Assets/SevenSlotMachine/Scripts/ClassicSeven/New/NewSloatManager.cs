using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BE;
using UnityEngine.UI;
using System.Linq;

public class NewSloatManager : MonoBehaviour
{
    public static NewSloatManager Instance;

    public CSBottomPanel CSBottomPanel;
    public SceneSlotGame SceneSlotGame;
    //public TMP_InputField TotalBetAmountINput;
    public TextMeshProUGUI TotalBetAmount;
    public TextMeshProUGUI Chips;
    public TextMeshProUGUI WinAmountText;
    public long TotalBetAMount;
    public long WinAmount;

    public Image Line;
    public Image LineImage3X5;
    public Image LineImage4X6;

    public GameObject ParticalPreFab;
    public GameObject Slot3X5,Slot4X6;

    [Header("Buttons")]
    public Button SpinButton;
    public Button PluseButton;
    public Button MinusButton;

    [Header("Lists")]
    public List<Sprite> LineImagesFor_3X5_Slot;
    public List<Sprite> LineImagesFor_4X6_Slot;

    public List<SlotGame> ListOfSlotGame;

    public List<MiniMaxList> MinMaxList;

    public List<GameObject> SlotPrefab;

    int Totalcolums;

    long MinBet;
    long MaxBet;
    long Increment;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);

        //Constants.SelectedSlot = 4;
        //Constants.CHIPS = 1000000;

        SetPreFabSprite();

        for (int i = 0; i < ListOfSlotGame.Count; i++)
        {
            ListOfSlotGame[i].gameObject.SetActive(false);
        }
        ListOfSlotGame[Constants.SelectedSlot - 1].gameObject.SetActive(true);

        SceneSlotGame.Slot = ListOfSlotGame[Constants.SelectedSlot-1];
        Totalcolums = SceneSlotGame.Slot.Reels.Count;

        if (Totalcolums == 5)
        {
            Slot3X5.SetActive(true);
            Slot4X6.SetActive(false);
            Line = LineImage3X5;
        }
        else if (Totalcolums == 6)
        {
            Slot3X5.SetActive(false);
            Slot4X6.SetActive(true);
            Line = LineImage4X6;
        }
    }

    public void SetPreFabSprite()
    {
        string path = "SlotElement/Slot" + Constants.SelectedSlot;

        //var textFile = Resources.Load<Sprite>(path);
        var textFile1 = Resources.LoadAll(path, typeof(Sprite)).Cast<Sprite>().ToArray();
        //Debug.LogError(textFile1.Length);
        for (int i = 0; i < textFile1.Length; i++)
        {
            if(i<10)
                SlotPrefab[i].GetComponent<Image>().sprite = textFile1[i];
        }
    }

    private void OnEnable()
    {
        SetMinMaxBets();
        ShowChips();
        Constants.On_Chips_Gold_Update += ShowChips;
    }

    private void OnDisable()
    {
        Constants.On_Chips_Gold_Update -= ShowChips;
    }

    private void Start()
    {
        TotalBetAMount = MinBet;
        UpdateTotalBetAmount();
    }

    private void SetMinMaxBets()
    {
        MinBet = MinMaxList[Constants.SelectedSlot-1].Min;
        MaxBet = MinMaxList[Constants.SelectedSlot-1].Min * MinMaxList[Constants.SelectedSlot-1].HowmanyTimesIncement;
        Increment = MinMaxList[Constants.SelectedSlot-1].Min;
    }

    void ShowChips()
    {
        Chips.text = Constants.NumberShow(Constants.CHIPS);
    }

    public void UpdateTotalBetAmount()
    {
        //TotalBetAMount = long.Parse(TotalBetAmountINput.text);
        //TotalBetAmountINput.text = Constants.NumberShow(TotalBetAMount);

        TotalBetAmount.text = Constants.NumberShow(TotalBetAMount);
        SlotGame.instance.TotalBet = TotalBetAMount;

        //CSBottomPanel.totalBet = TotalBetAMount;
        //Debug.LogError(SlotGame.instance.TotalBet);
    }

    public void UpdateWinAmount(long Amount)
    {
        Debug.LogError(Amount);
        if (Amount > 0)
        {
            WinAmount = Amount * TotalBetAMount;
            //WinAmount = (Amount * 10000) + TotalBetAMount + (int)(TotalBetAMount / 10);
            //WinAmount += (int)(TotalBetAMount / 10);
            Constants.CHIPS += WinAmount;
            Constants.instance.Chips_Gold_Update();
        }

        WinAmountText.text = Constants.NumberShow(WinAmount);
    }

    public void BackButtonClick()
    {
        Constants.ShowSelectSlot = true;

        Constants.GotoScene("Home");
    }

    public void SetLine(int number)
    {
        //Debug.LogError(number);
        Line.enabled = true;
        if(Totalcolums == 5)
            Line.sprite = LineImagesFor_3X5_Slot[number];
        else if(Totalcolums == 6)
            Line.sprite = LineImagesFor_4X6_Slot[number];
    }

    public void DisableLine()
    {
        //Debug.LogError("Disable");
        Line.enabled = false;
    }

    public void ShowPartical(Transform parant)
    {
        Instantiate(ParticalPreFab,parant);
    }

    public void PluseMinusButtonClick(int n)
    {
        if(n == 0)
        {
            if(TotalBetAMount < MaxBet)
            {
                TotalBetAMount += Increment;
                UpdateTotalBetAmount();
            }
        }
        else if(n != 0)
        {
            if (TotalBetAMount > MinBet)
            {
                TotalBetAMount -= Increment;
                UpdateTotalBetAmount();
            }
        }
    }

    public void DisableAllButton()
    {
        SpinButton.enabled = false;
        PluseButton.enabled = false;
        MinusButton.enabled = false;
    }

    public void EnableAllButton()
    {
        SpinButton.enabled = true;
        PluseButton.enabled = true;
        MinusButton.enabled = true;
    }
}

[System.Serializable]
public class MiniMaxList
{
    public long Min;
    public long HowmanyTimesIncement;
}
