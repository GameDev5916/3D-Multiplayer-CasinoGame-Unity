using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Casino_Poker
{
    public class RoomData : MonoBehaviour
    {
        public string RoomId;
        public TextMeshProUGUI RoomName;
        //public TextMeshProUGUI TotalINRoom;
        public TextMeshProUGUI TotalJoinInRoom;

        [SerializeField] private RawImage Player_Profile_Pic;

        public void JoinButtonClick()
        {
            if (RoomId == "")
                return;

            JSONNode jsonnode = new JSONObject
            {
                ["roomName"] = RoomId,
                ["playerId"] = Constants.PLAYER_ID,
                ["playerAmount"] = "1000"
            };

            Debug.LogError(jsonnode.ToString());

            UIManager_Poker.Instance.RoomListPanel.SetActive(false);
            UIManager_Poker.Instance.RoomPanel.SetActive(true);
            NetworkManager_Poker.Instance.PokerSocket.Emit(Constants.JOINROOM, jsonnode.ToString());
        }
    }
}