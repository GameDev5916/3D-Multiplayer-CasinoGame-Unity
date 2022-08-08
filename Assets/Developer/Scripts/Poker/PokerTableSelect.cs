using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Casino_Poker
{
    public class PokerTableSelect : MonoBehaviour
    {
        #region Singleton
        public static PokerTableSelect instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        public Slider AmountSlider;
        public TextMeshProUGUI TimerText;
        public TextMeshProUGUI MinMaxBuyinText;
        public TextMeshProUGUI StackText;

        [SerializeField] private GameObject FastTimer;
        [SerializeField] private GameObject NormalTimer;

        public Toggle Auto_ReBuy;
        public Toggle Buy_Max;

        public GameObject GameManager;
        public GameObject PlayButton, BuyChipButton;

        public Button playButton;

        private void OnEnable()
        {
            if (GameManager_Poker.Instance)
                Plus_Minus_ButtonClick(0);

            playButton.interactable = false;
            TimerButtonClick();
        }

        public void Start()
        {
            Plus_Minus_ButtonClick(0);
        }

        public void Plus_Minus_ButtonClick(int n)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            AmountSlider.value += n;
            ChangeMinMaxAmountText();
        }

        public void ChangeMinMaxAmountText()
        {
            int value = (int)AmountSlider.value;
            MinMaxBuyinText.text = $"${ Constants.NumberShow(GameManager_Poker.Instance.MinMaxBuyinAmounts[value].Min) } - ${Constants.NumberShow(GameManager_Poker.Instance.MinMaxBuyinAmounts[value].Max)}";
            StackText.text = $"${ Constants.NumberShow(GameManager_Poker.Instance.MinMaxStakesAmounts[value].Min) } - ${Constants.NumberShow(GameManager_Poker.Instance.MinMaxStakesAmounts[value].Max)}";

            PlayBuyButtonSwitch(value);
        }

        private void PlayBuyButtonSwitch(int value)
        {
            if (GameManager_Poker.Instance.MinMaxBuyinAmounts[value].Max <= Constants.CHIPS)
            {
                PlayButton.SetActive(true);
                BuyChipButton.SetActive(false);
            }
            else if (GameManager_Poker.Instance.MinMaxBuyinAmounts[value].Max > Constants.CHIPS)
            {
                PlayButton.SetActive(false);
                BuyChipButton.SetActive(true);
            }
        }

        public void BuyChipsButton()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            Debug.Log("Buy Chips Button Pressed - Poker");
            Constants.buyChipButtonClicked = true;
            Constants.GotoScene("Home");
        }

        public void PlayButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            Constants.isJoinByStandUp = true;
            Constants.SelectedSeat = 1;

            if (!Buy_Max.isOn && Auto_ReBuy.isOn)
            {
                Constants.RoomName = "";
                Constants.pokerMinMaxIndex = (int)AmountSlider.value;
                Constants.PokerMaxAmount = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Max;

                Constants.AutoReBuy = Auto_ReBuy.isOn;
                GameManager_Poker.Instance.TableSelectPanel.SetActive(false);
                //GameManager_Poker.Instance.BuyInPanel.SetActive(true);
            }
            else
                SendTableSelectData();

            //GameManager_Poker.Instance.NetworkManagerPoker.SetActive(true);
            //StartCoroutine(SendTableSelectionData());
            //PlayButton.GetComponent<Button>().interactable = false;
        }

        //IEnumerator SendTableSelectionData()
        //{
        //    yield return new WaitForSeconds(2f);
        //    string timer;

        //    if (Constants.TIMER_POKER == 0)
        //        timer = "normal";
        //    else
        //        timer = "fast";

        //    JSONNode jsonnode = new JSONObject
        //    {
        //        ["playerId"] = Constants.PLAYER_ID,
        //        ["minAmount"] = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Min,
        //        ["maxAmount"] = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Max,
        //        ["timer"] = timer,
        //        ["stake"] = GameManager_Poker.Instance.MinMaxStakesAmounts[(int)AmountSlider.value].Min,
        //        //["maxstackAmount"] = GameManager_Poker.Instance.MinMaxStakesAmounts[(int)AmountSlider.value].Max,
        //        ["autobuy"] = Auto_ReBuy.isOn,
        //        ["buyatmax"] = Buy_Max.isOn,
        //        ["tablelimit"] = 5,

        //    };
        //    Debug.LogError("PlayButton********* " + jsonnode.ToString());

        //    Constants.pokerMinMaxIndex = (int)AmountSlider.value;

        //    //NetworkManager_Poker.Instance.CreateRoom();
        //    NetworkManager_Poker.Instance.PokerSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
        //    GameManager_Poker.Instance.TableSelecPanel.SetActive(false);
        //    GameManager_Poker.Instance.GamePlayPanel.SetActive(true);
        //}

        private void SendTableSelectData()
        {
            string timer;
            if (Constants.TIMER_POKER == 0)
                timer = "normal";
            else
                timer = "fast";

            Constants.AutoBuyAmount = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Max;
            Constants.BuyMaxOn = Buy_Max.isOn;

            //JSONNode jsonnode = new JSONObject
            //{
            //    ["playerId"] = Constants.PLAYER_ID,
            //    ["minAmount"] = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Min,
            //    ["maxAmount"] = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Max,
            //    ["timer"] = timer,
            //    ["stake"] = GameManager_Poker.Instance.MinMaxStakesAmounts[(int)AmountSlider.value].Min,
            //    //["maxstackAmount"] = GameManager_Poker.Instance.MinMaxStakesAmounts[(int)AmountSlider.value].Max,
            //    ["autobuy"] = Auto_ReBuy.isOn,
            //    ["buyatmax"] = Buy_Max.isOn,
            //    ["tablelimit"] = 5,
            //    ["isStandUp"] = Constants.isJoinByStandUp,
            //};
            //Debug.LogError("PlayButton********* " + jsonnode.ToString());
            Debug.LogError("PlayButton********* " + Constants.BuyMaxOn);

            Constants.isJoinByStandUp = true;
            Constants.pokerMinMaxIndex = (int)AmountSlider.value;
            Constants.PokerMaxAmount = GameManager_Poker.Instance.MinMaxBuyinAmounts[(int)AmountSlider.value].Max;
            //NetworkManager_Poker.Instance.CreateRoom();
            //NetworkManager_Poker.Instance.PokerSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
            GameManager_Poker.Instance.TableSelectPanel.SetActive(false);
            GameManager_Poker.Instance.GamePlayPanel.SetActive(true);
        }

        public void TimerButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            FastTimer.SetActive(!(Constants.TIMER_POKER == 0));
            NormalTimer.SetActive(Constants.TIMER_POKER == 0);
        }

        public void SetTimer()
        {
            Constants.TIMER_POKER = Constants.TIMER_POKER == 0 ? 1 : 0;
            TimerButtonClick();
        }

        public void TableSelectionCloseButton()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            Constants.GotoScene("Home");
        }
    }
}