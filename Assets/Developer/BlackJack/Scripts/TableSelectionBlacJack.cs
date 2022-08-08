using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleJSON;

namespace BalckJack
{
    public class TableSelectionBlacJack : MonoBehaviour
    {
        #region Singleton
        public static TableSelectionBlacJack instance;

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
        public Text TimerText;
        public Text MinMaxText;
        public Button PlayButton;

        [Space]
        public GameObject playGameButton;
        public GameObject buyChipsButton;

        private void OnEnable()
        {
            if (BlackJackGameManager.Instance)
                Pluse_Minus_ButtonClick(0);

            TimerButtonClick();
            PlayButton.interactable = false;
        }

        public void Start()
        {
            Pluse_Minus_ButtonClick(0);
        }

        public void CloseButtonClick()
        {
            BlackJack_NetworkManager.isconnected = false;
            Constants.GotoScene("Home");
        }

        public void Pluse_Minus_ButtonClick(int n)
        {
            AmountSlider.value += n;
            ChangeMinMaxAmountText();
        }

        public void ChangeMinMaxAmountText()
        {
            int value = (int)AmountSlider.value;
            MinMaxText.text = $"${ Constance.AmountShow(BlackJackGameManager.Instance.MinMaxesBetAmounts[value].Min) } - ${Constance.AmountShow(BlackJackGameManager.Instance.MinMaxesBetAmounts[value].Max)}";

            PlayBuyButtonSwitch(value);
        }


        private void PlayBuyButtonSwitch(int value)
        {
            if (BlackJackGameManager.Instance.MinMaxesBetAmounts[value].Max <= Constants.CHIPS)
            {
                playGameButton.SetActive(true);
                buyChipsButton.SetActive(false);
            }
            else if (BlackJackGameManager.Instance.MinMaxesBetAmounts[value].Max > Constants.CHIPS)
            {
                playGameButton.SetActive(false);
                buyChipsButton.SetActive(true);
            }
        }

        public void PlayButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            Constance.Min_BetAmoutForBlackJack = BlackJackGameManager.Instance.MinMaxesBetAmounts[(int)AmountSlider.value].Min;
            Constance.Max_BetAmoutForBlackJack = BlackJackGameManager.Instance.MinMaxesBetAmounts[(int)AmountSlider.value].Max;

            string timer;

            if (Constance.TimerForBlackJack == 0)
                timer = "normal";
            else
                timer = "fast";

            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["minAmount"] = Constance.Min_BetAmoutForBlackJack,
                ["maxAmount"] = Constance.Max_BetAmoutForBlackJack,
                ["timer"] = timer,
            };
            Debug.LogWarning("BlackJackPlayData " + jsonnode.ToString());
            Constants.blackJackMinMaxIndex = (int)AmountSlider.value;

            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());

            BlackJackGameManager.Instance.TableSetUi.SetActive(false);
            BlackJackGameManager.Instance.GamePlayUI.SetActive(true);
        }

        public void BuyChipsButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            Debug.Log("Buy Chips Button Pressed - BlackJack");
            Constants.buyChipButtonClicked = true;
            Constants.GotoScene("Home");
        }

        public void TimerButtonClick()
        {
            if (Constance.TimerForBlackJack == 0)
            {
                TimerText.text = "Normal";
            }
            else
            {
                TimerText.text = "Fast";
            }
        }
        public void SetTimer()
        {
            Constance.TimerForBlackJack = Constance.TimerForBlackJack == 0 ? 1 : 0;
            TimerButtonClick();
        }
    }
}