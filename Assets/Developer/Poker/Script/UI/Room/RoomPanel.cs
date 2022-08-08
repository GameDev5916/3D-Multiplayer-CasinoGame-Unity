using System.Collections;
using System.Collections.Generic;
using LitJson;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace Casino_Poker
{
    public class RoomPanel : MonoBehaviour
    {
        [SerializeField] private GameObject PlayerDataPrefab;
        [SerializeField] private GameObject Content;
        [SerializeField] private Button StartButton;

        private void OnEnable()
        {
            NetworkManager_Poker.Instance.PokerSocket.Emit("CREATE_ROOM");
            StartButton.gameObject.SetActive(false);
            NetworkManager_Poker.RefletPlayerList += ShowAllPlayerInRoom;
        }

        private void OnDisable()
        {
            NetworkManager_Poker.RefletPlayerList -= ShowAllPlayerInRoom;
        }

        public void ShowAllPlayerInRoom(JSONNode jsonNode)
        {
            DestroyAllObjectINContent();
            //EnableStartIfMoreThanOnePlayer(jsonNode);
            for (int i = 0; i < jsonNode.Count; i++)
            {
                PlayerINRoom playerdata = Instantiate(PlayerDataPrefab, Content.transform).GetComponent<PlayerINRoom>();

                playerdata.PlayerID.text = jsonNode[i]["playerId"].ToString();
                playerdata.PlayerName.text = jsonNode[i]["roomName"].ToString();
                //playerdata.SetImage(jsonvale["profile_picture"].ToString());

                //if (i == 0)
                //    playerdata.StartImage.SetActive(true);
            }
        }

        private void EnableStartIfMoreThanOnePlayer(JSONNode jsonNode)
        {
            if (jsonNode.Count > 1)
            {
                JsonData jsonvale = JsonMapper.ToObject(jsonNode[0]);

                if (Constants.PLAYER_ID == jsonvale["playerId"].ToString())
                    StartButton.gameObject.SetActive(true);
            }
            else
            {
                StartButton.gameObject.SetActive(false);
            }
        }

        public void DestroyAllObjectINContent()
        {
            for (int i = 0; i < Content.transform.childCount; i++)
            {
                Destroy(Content.transform.GetChild(i).gameObject);
            }
        }

        public void StartGamePlayButtonClick()
        {
            //Start Game
            NetworkManager_Poker.Instance.PokerSocket.Emit(Constants.STARTGAME);
        }

        public void BackButtonClickFormOwner()
        {
            UIManager_Poker.Instance.HomePanel.SetActive(true);
            UIManager_Poker.Instance.RoomPanel.SetActive(false);

            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
            };
            Debug.LogError("Desconnect Emit : " + jsonnode.ToString());
            NetworkManager_Poker.Instance.PokerSocket.Emit(Constants.DISCONNECTEDMANULLY, jsonnode.ToString());

            //Desconnected From Room ,Delete Room
            //NetworkManager.Instance.PokerSocket.Emit(Constants.LEAVE_ROOM, PlayerPrefs.GetString(Constants.PLAYERID));
            //NetworkManager.Instance.PokerSocket.Emit(Constants.BACK_FROM_ROOM, PlayerPrefs.GetString(Constants.PLAYERID));
        }
    }
}