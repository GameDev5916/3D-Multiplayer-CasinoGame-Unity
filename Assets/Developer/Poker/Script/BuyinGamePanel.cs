using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
namespace Casino_Poker
{
    public class BuyinGamePanel : MonoBehaviour
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
            Min = GameManager_Poker.Instance.MinMaxBuyinAmounts[Constants.pokerMinMaxIndex].Min;
            Max = GameManager_Poker.Instance.MinMaxBuyinAmounts[Constants.pokerMinMaxIndex].Max;
            PluseAmount = (Max - Min) / 40;
            slider.maxValue = 40;
            slider.value = Constants.AutoBuyAmount == 0 ? 40 : ((Constants.AutoBuyAmount - Min) / PluseAmount);

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
            GameManager_Poker.Instance.BuyInPanel.SetActive(false);
            //Constants.GotoScene("Home");
        }

        public void PlayButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            current = Min + ((long)slider.value * PluseAmount);
            Constants.AutoBuyAmount = current;
            string timer;
            if (Constants.TIMER_POKER == 0)
                timer = "normal";
            else
                timer = "fast";

            Constants.AutoBuyAmount = Constants.PokerMaxAmount;

            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["position"] = Constants.SelectedSeat,
                ["minAmount"] = Min,
                ["buyAmount"] = Constants.isJoinByStandUp ? 0 : current,
                ["maxAmount"] = Max,
                ["timer"] = timer,
                ["stake"] = GameManager_Poker.Instance.MinMaxStakesAmounts[Constants.pokerMinMaxIndex].Min,
                ["autobuy"] = Constants.AutoReBuy,
                ["buyatmax"] = Constants.BuyMaxOn,
                ["tablelimit"] = 5,
                ["isStandUp"] = Constants.isJoinByStandUp,
            };
            string newBackstageItemID = System.Guid.NewGuid().ToString();
            Constants.RoomName = newBackstageItemID;

            Debug.Log("RoomName::::" + newBackstageItemID);
            //if (Constants.RoomName != "")
            //    jsonnode["roomName"] = Constants.RoomName;
            jsonnode["roomName"] = newBackstageItemID;

            Debug.LogError("PlayButtonAtBuyInPanel======= " + jsonnode.ToString());

            NetworkManager_Poker.Instance.PokerSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
            //JSONNode jsonnode = new JSONObject
            //{
            //    ["playerId"] = Constants.PLAYER_ID,
            //    ["position"] = Constants.SelectedSeat,
            //    ["minAmount"] = Min,
            //    ["buyAmount"] = Constants.isJoinByStandUp ? 0 : current,
            //    ["maxAmount"] = Max,
            //    ["timer"] = timer,
            //    ["stake"] = GameManager_Poker.Instance.MinMaxStakesAmounts[Constants.pokerMinMaxIndex].Min,
            //    //["maxstackAmount"] = GameManager_Poker.Instance.MinMaxStakesAmounts[(int)AmountSlider.value].Max,
            //    ["autobuy"] = Constants.AutoReBuy,
            //    ["buyatmax"] = false,
            //    ["tablelimit"] = 5,
            //    ["isStandUp"] = Constants.isJoinByStandUp,
            //};
            //if (Constants.RoomName != "")
            //    jsonnode["roomName"] = Constants.RoomName;

            //Debug.LogError("PlayButtonAtBuyInPanel======= " + jsonnode.ToString());
            Constants.PokerMaxAmount = current;

            //NetworkManager_Poker.Instance.PokerSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
            GameManager_Poker.Instance.BuyInPanel.SetActive(false);
            GameManager_Poker.Instance.GamePlayPanel.SetActive(true);
        }
    }
}