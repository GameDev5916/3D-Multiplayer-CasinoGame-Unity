using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using TMPro;

namespace Casino_Poker
{
    public class HomePanel : MonoBehaviour
    {
        public static HomePanel Instance;
        public TextMeshProUGUI PlayerID;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            PlayerID.text = Constants.PLAYER_ID;
        }

        public void StartButtonCLick(string PlayerId)
        {
            Constants.PLAYER_ID = PlayerId;
            PlayerID.text = PlayerId;
        }

        public void RoomListButtonClick()
        {
            UIManager_Poker.Instance.RoomListPanel.SetActive(true);
            UIManager_Poker.Instance.HomePanel.SetActive(false);
        }

        public void CreateRoomButtonCLick()
        {
            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["playerAmount"] = "1000"
            };

            NetworkManager_Poker.Instance.PokerSocket.Emit(Constants.CREATE_ROOM, jsonnode.ToString());

            UIManager_Poker.Instance.RoomPanel.SetActive(true);
            UIManager_Poker.Instance.HomePanel.SetActive(false);
        }
    }
}