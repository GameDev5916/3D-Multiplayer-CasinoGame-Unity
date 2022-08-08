using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
namespace BalckJack
{
    public class BuyInBlackJack : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI MinAmount;
        [SerializeField] private TextMeshProUGUI MaxAmount;
        [SerializeField] private TextMeshProUGUI CurrentSelectedAmount;
        [SerializeField] private TextMeshProUGUI TotalAmount;
        [SerializeField] private Slider slider;

        public long Min;
        public long Max;
        public long current;
        public long PluseAmount;

        public Button PlayButton;

        private void OnEnable()
        {
            //Min = GameManager_Poker.Instance.MinMaxBuyinAmounts[9].Min;
            //Max = GameManager_Poker.Instance.MinMaxBuyinAmounts[9].Max;
            Min = BlackJackGameManager.Instance.MinMaxesBetAmounts[Constants.blackJackMinMaxIndex].Min;
            Max = BlackJackGameManager.Instance.MinMaxesBetAmounts[Constants.blackJackMinMaxIndex].Max;
            slider.value = 0;
            slider.maxValue = 20;
            PluseAmount = (Max - Min) / 20;

            OnSliderValueChange();

            MinAmount.text = Constants.NumberShow(Min);
            MaxAmount.text = Constants.NumberShow(Max);
            TotalAmount.text = $"Your Account Balance : {Constants.NumberShow(Constants.CHIPS)}";
        }

        public void OnSliderValueChange()
        {
            current = Min + ((long)slider.value * PluseAmount);
            CurrentSelectedAmount.text = Constants.NumberShow(current);

            if (current > Constants.CHIPS)
                PlayButton.interactable = false;
            else
                PlayButton.interactable = true;
        }

        public void Pluse_Minus_ButtonClick(int n)
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            slider.value += n;
            OnSliderValueChange();
        }

        public void CloseButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            Constants.GotoScene("Home");
        }

        public void PlayButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["maxAmount"] = current,
                ["roomName"] = Constants.RoomName,
                ["position"] = Constants.blackJackPosition
            };

            Debug.LogError("PlayButtonAtBuyInPanel======= " + jsonnode.ToString());

            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
            BlackJackGameManager.Instance.BuyInPanel.SetActive(false);
        }
    }
}
