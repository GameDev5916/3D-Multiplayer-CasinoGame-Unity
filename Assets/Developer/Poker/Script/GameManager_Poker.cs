using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace Casino_Poker
{
    public class GameManager_Poker : MonoBehaviour
    {
        public static GameManager_Poker Instance;

        [Header("Panels")]
        public GameObject TableSelectPanel;
        public GameObject GamePlayPanel;
        public GameObject WinPanel;
        public GameObject FriendsPanel;
        public GameObject BuyInPanel;
        public GameObject InvitationRequestPanel;
        public GameObject BackOptionPanel;
        public GameObject PokerGiftPanel;
        public GameObject PlayerProfilePanel;
        public GameObject AddFriendRequestPanel;

        public List<Constants.CardThems> AllCardsSprites;
        public List<MiniMax> MinMaxStakesAmounts;
        public List<MiniMax> MinMaxBuyinAmounts;
        public List<GiftItems> GiftItemsSprites;
        public List<Sprite> TierSprites;

        public Sprite SmallBlind, BigBlind;

        public GameObject PlayersParent;
        public GameObject Card;
        public GameObject NetworkManagerPoker;

        public int FriendsSelectedFunction;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void OnEnable()
        {
            NetworkManager_Poker.RoomJoin += OnJoinRoom;
            //NetworkManager_Poker.NewPlayerJoin += NewPlayerJoinRoom;
            NetworkManager_Poker.PlayerLeft += OnPlayerleft;
            NetworkManager_Poker.PlayerAmount += OpenBuyInPanel;
            NetworkManager_Poker.GameStatus += OnGameStatus;
            MainNetworkManager.JoinInvitation += OnPokerInvitation;
            MainNetworkManager.PlayerFriendListAction += OpenFriendsPanel;
            MainNetworkManager.TableItemAction += OpenGiftsPanel;
            MainNetworkManager.OnPlayerProfileOpen += OpenProfilePanel;
            MainNetworkManager.AcceptFriendRequest += OpenFriendRequestPanel;

            if (Constants.isJoinByInvitation) TableSelectPanel.SetActive(false);
            else if (!Constants.isJoinByInvitation) TableSelectPanel.SetActive(true);
           
        }

        private void OnDisable()
        {
            NetworkManager_Poker.RoomJoin -= OnJoinRoom;
            //NetworkManager_Poker.NewPlayerJoin -= NewPlayerJoinRoom;
            NetworkManager_Poker.PlayerLeft -= OnPlayerleft;
            NetworkManager_Poker.PlayerAmount -= OpenBuyInPanel;
            NetworkManager_Poker.GameStatus -= OnGameStatus;
            MainNetworkManager.JoinInvitation -= OnPokerInvitation;
            MainNetworkManager.PlayerFriendListAction -= OpenFriendsPanel;
            MainNetworkManager.TableItemAction -= OpenGiftsPanel;
            MainNetworkManager.OnPlayerProfileOpen -= OpenProfilePanel;
            MainNetworkManager.AcceptFriendRequest -= OpenFriendRequestPanel;
        }

        public void JoinByInvitation()
        {
            if (Constants.isJoinByInvitation == true)
            {
                Constants.isJoinByInvitation = false;
                TableSelectPanel.SetActive(false);

                JSONNode jsonnode = new JSONObject
                {
                    ["friendPlayerId"] = Constants.PLAYER_ID,
                    ["requestaction"] = true,
                    ["senderPlayerId"] = Constants.instance.FrinedInvitationJsonData["senderPlayerId"],
                    ["roomName"] = Constants.instance.FrinedInvitationJsonData["roomName"],
                    ["roomStake"] = Constants.instance.FrinedInvitationJsonData["roomStake"],
                    ["maxAmounts"] = Constants.instance.FrinedInvitationJsonData["maxAmount"],
                    ["minAmounts"] = Constants.instance.FrinedInvitationJsonData["minAmount"],
                    ["roomType"] = Constants.instance.FrinedInvitationJsonData["roomType"],
                    ["position"] = Constants.instance.FrinedInvitationJsonData["position"]
                };
                NetworkManager_Poker.Instance.PokerSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
                //MainNetworkManager.Instance.MainSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
                Debug.Log("isJoinByInvitation (GameManagerPoker) " + jsonnode.ToString());
            }
        }

        private void OnGameStatus(JSONNode jsonNode)
        {
            Debug.Log("GameStatus");
            Debug.Log(jsonNode);
            Debug.Log(jsonNode["playerId"].Value == Constants.PLAYER_ID);
            Debug.Log(jsonNode["playerId"].Value);
            Debug.Log(Constants.PLAYER_ID);
            Debug.Log(jsonNode["message"].Value);
            string msg = jsonNode["message"].Value;
            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)
            {
                Debug.Log(Constants.PLAYER_ID);
                Constants.ShowWarning(msg);
            }
        }

        private void OnJoinRoom(JSONNode jsonNode)
        {
            //UserInfoUpdate();
            if(!jsonNode["isGameStarted"].AsBool) {
                PokerGamePlay.Instance.ResetAllData();
            }
            int CurrentPlayerIndexinJson = 0;
            JSONNode json = jsonNode["roomData"];
            JSONNode activePlayers = jsonNode["activePlayers"];
            Debug.Log("roomData;;;;;;;");
            Debug.Log(jsonNode["isGameStarted"]);
            Debug.Log(activePlayers[0].Value);
            bool isJoinByStandUp = false;

            // Find Index of Player in JsonNode
            for (int i = 0; i < jsonNode["roomData"].Count; i++)
            {
                if (json[i]["playerId"].Value == Constants.PLAYER_ID)
                {
                    Constants.RoomName = jsonNode["roomName"];
                    CurrentPlayerIndexinJson = i;
                    isJoinByStandUp = !json[i]["playerIsSit"].AsBool;
                    Constants.AutoReBuy = json[i]["autobuy"].AsBool;
                }
            }

            int total = 5;  // For 9 Player Change total = 9

            for (int i = 0; i < total; i++)
            {
                PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                bool checkFlag = false;
                for(int j = 0; j < activePlayers.Count; j++) {
                    if(activePlayers[j].Value == player.playerId) {
                        checkFlag = true;
                    }
                }
                if(checkFlag && jsonNode["isGameStarted"].AsBool){

                } else {
                player.playerId = "";
                player.ResetPlayer();
                }               
            }

            for (int i = 0; i < total; i++)
            {
                int selectedIndex = -1;
                for (int k = 0; k < jsonNode["roomData"].Count; k++) {
                    if(json[k]["position"].AsInt == (i + 1)){
                        selectedIndex = k;
                    }
                }
                Debug.Log("selectedIndex;;;;" + selectedIndex);
                JSONNode jsonPlayer = json[selectedIndex];
                //Debug.LogError($"I : {i} J : {j} Total : {jsonNode["roomData"].Count}");

                int selectedSeat = i;

                PokerPlayer player = PlayersParent.transform.GetChild(selectedSeat).GetComponent<PokerPlayer>();

                player.playerId = jsonPlayer["playerId"].Value;
                player.PlayerName.text = jsonPlayer["name"].Value;
                player.Position = jsonPlayer["position"];
                player.ShowAndSetTotalAmount(jsonPlayer["playerAmount"].AsLong);


                if (isJoinByStandUp)
                {
                    player.ActivePlayer.SetActive(false);
                    player.InvitePlayer.SetActive(false);
                    player.StandPlayer.SetActive(true);
                }
                else if (!string.IsNullOrWhiteSpace(player.playerId) && jsonPlayer["playerIsSit"].AsBool)
                {
                    player.ActivePlayer.SetActive(true);
                    player.InvitePlayer.SetActive(false);
                    player.StandPlayer.SetActive(false);
                }
                else
                {
                    player.ActivePlayer.SetActive(false);
                    player.InvitePlayer.SetActive(true);
                    player.StandPlayer.SetActive(false);
                }

                if (player.playerId == Constants.PLAYER_ID)
                {
                    Debug.LogWarning("OwnPlayerID match");
                    player.giftIconImage.SetActive(true);
                    player.giftItemImage.SetActive(false);
                    player.GiftButton.GetComponent<Button>().interactable = true;
                }

                if (jsonPlayer["profilePic"].Value != "" && jsonPlayer["profilePic"].Value != "null")
                {
                    Constants.GetImageFrom64String(jsonPlayer["profilePic"].Value, (Texture image) =>
                    {
                        player.PlayerImage.texture = image;
                    });
                }
                else
                {
                    Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
                    {
                        player.PlayerImage.texture = image;
                    });
                }

            }

            // if(!jsonNode["isGameStarted"].AsBool) {
                PokerGamePlay.Instance.UpdatePokerChips();
            // }
            

            if (isJoinByStandUp)
            {
                PokerGamePlay.Instance.Message.gameObject.SetActive(false);
                Debug.LogWarning("StandupJoin");
                for (int i = 0; i < PlayersParent.transform.childCount; i++)
                {
                    PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                    if (player.playerId == "")
                    {
                        player.ActivePlayer.SetActive(false);
                        player.InvitePlayer.SetActive(false);
                        player.StandPlayer.SetActive(true);
                    }
                }
            }
            else
            {
                PokerGamePlay.Instance.Message.gameObject.SetActive(true);
                for (int i = 0; i < PlayersParent.transform.childCount; i++)
                {
                    PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                    if (player.playerId == "")
                    {
                        player.ResetPlayer();
                        player.ActivePlayer.SetActive(false);
                        player.InvitePlayer.SetActive(true);
                        player.StandPlayer.SetActive(false);
                    }
                }
            }
        }

        public bool IsPlayerInRoom(string playerId)
        {
            for (int i = 0; i < PlayersParent.transform.childCount; i++)
            {
                PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                if (player.playerId == playerId)
                    return true;
            }
            return false;
        }

        //void UserInfoUpdate()
        //{
        //    JSONNode data = new JSONObject
        //    {
        //        ["unique_id"] = Constants.PLAYER_ID,
        //    };

        //    StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
        //    {
        //        if (IsSuccess)
        //        {
        //            JSONNode jsonNode = JSON.Parse(result)["data"];
        //            Constants.SetPlayerData(jsonNode);
        //            Debug.LogError("USER_INFO " + jsonNode.ToString());
        //            PokerGamePlay.Instance.TotalChips.text = Constants.NumberShow(Constants.CHIPS);
        //        }
        //        else
        //        {
        //            Debug.LogError("Faild");
        //        }
        //    }));
        //}

        //public void NewPlayerJoinRoom(JSONNode jsonNode)
        //{
        //    for (int i = 0; i < jsonNode.Count; i++)
        //    {
        //        JSONNode json = jsonNode;
        //        //if (json[i]["playerId"].Value == Constants.PLAYER_ID)
        //        //    Constants.RoomName = jsonNode["roomName"];

        //        PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
        //        Debug.LogError($"{i} : {player.playerId}  : {json[i]["playerId"].Value}");
        //        if (player.playerId == json[i]["playerId"].Value)
        //            continue;

        //        player.ActivePlayer.SetActive(true);
        //        player.InvitePlayer.SetActive(false);
        //        player.StandPlayer.SetActive(false);
        //        player.playerId = json[i]["playerId"].Value;
        //        player.PlayerName.text = json[i]["name"].Value;
        //        player.TotalChips.text = Constants.NumberShow(json[i]["playerAmount"].AsLong);

        //        if (json[i]["profilePic"].Value != "" && json[i]["profilePic"].Value != "null")
        //        {
        //            Constants.GetImageFrom64String(json[i]["profilePic"].Value, (Texture image) =>
        //            {
        //                player.PlayerImage.texture = image;
        //            });
        //        }
        //        else
        //        {
        //            Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
        //            {
        //                player.PlayerImage.texture = image;
        //            });
        //        }
        //    }
        //}

        public void OnPlayerleft(JSONNode jsonNode)
        {
            for (int i = 0; i < PlayersParent.transform.childCount; i++)
            {
                PokerPlayer player = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                if (player.playerId == jsonNode["playerId"].Value)
                {
                    player.PlayerLeft();
                }
            }

            //UserInfoUpdate();
        }

        public void OpenBuyInPanel(JSONNode jsonNode)
        {
            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)
            {
                Constants.AutoReBuy = false;
                BuyInPanel.SetActive(true);
            }
        }

        public void BackButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            BackOptionPanel.SetActive(true);
        }

        public void FriendInviteButton()
        {
            if (FacebookManager.Instance != null)
                FacebookManager.Instance.InitCallback();
            switch (GameManager_Poker.Instance.FriendsSelectedFunction)
            {
                case 0:
                case 1:
                case 2:
                default:
                    MainNetworkManager.Instance.OnSendingFriendList();
                    break;
            }
        }

        private void OnPokerInvitation(JSONNode jsonNode)
        {
            if (jsonNode["friendPlayerId"] == Constants.PLAYER_ID)
            {
                Debug.Log("Invitation-Poker");
                InvitationRequestPanel.SetActive(true);
                MainNetworkManager.SetInvitePanel?.Invoke(jsonNode);
            }
        }

        private void OpenFriendsPanel(JSONNode jsonNode)
        {
            FriendsPanel.SetActive(true);
            MainNetworkManager.SetFriendsPanel?.Invoke(jsonNode);
        }

        private void OpenGiftsPanel(JSONNode jsonNode)
        {
            PokerGiftPanel.SetActive(true);
            MainNetworkManager.SetGiftPanel?.Invoke(jsonNode);
        }

        private void OpenProfilePanel(JSONNode jsonNode)
        {
            PlayerProfilePanel.SetActive(true);
            MainNetworkManager.SetPlayerProfilePanel?.Invoke(jsonNode);
        }

        public Sprite GetSprite(string ID)
        {
            return GiftItemsSprites.Find(x => x.identifier == ID).giftSprite;
        }

        private void OpenFriendRequestPanel(JSONNode jsonNode)
        {
            if (jsonNode["friendPlayerId"].Value == Constants.PLAYER_ID)
            {
                AddFriendRequestPanel.SetActive(true);
                MainNetworkManager.SetFriendRequestPanel?.Invoke(jsonNode);
            }
            //Debug.Log("OpenFriendRequestPanel~~~Action");
        }

        public void DisableGiftButton()
        {
            for (int i = 0; i < PlayersParent.transform.childCount; i++)
            {
                Button GiftButtonPoker = PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>().GiftButton.GetComponent<Button>();
                GiftButtonPoker.interactable = false;
            }
        }
    }


    [System.Serializable]
    public class MiniMax
    {
        public long Min;
        public long Max;
    }

    [System.Serializable]
    public class GiftItems
    {
        public string identifier;
        public Sprite giftSprite;
    }
}

