using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using System.Collections;
using Newtonsoft.Json;

namespace Casino_Poker
{
    public class PokerPlayer : MonoBehaviour
    {
        public int Position;
        public int SeatIndex;
        public string playerId;
        private long TotalAmountInGame;
        public bool IsPlayerInTable;

        [Space]
        public GameObject ActivePlayer;
        public GameObject InvitePlayer;
        public GameObject NotActiveBG;
        public GameObject StandPlayer;
        public GameObject WinParticals;
        public RawImage PlayerImage;
        public Transform GiftButton;
        public GameObject giftIconImage, giftItemImage;

        [Space]
        public TextMeshProUGUI PlayerName;
        public TextMeshProUGUI TotalChips;
        public TextMeshProUGUI BetAmount;
        public TextMeshProUGUI SelectOptionText;
        [Space]
        public Image Blind;
        public Image Timer;
        public Image BetOptionImage;

        [Header("Cards")]
        [Space]
        public GameObject CardParent;
        public CardScript Card1;
        public CardScript Card2;

        private void OnEnable()
        {
            NetworkManager_Poker.GameStart += GameStart;
            NetworkManager_Poker.PlayerCardDistribution += CardDistribution;
            NetworkManager_Poker.PlayerTurn += PlayerTurn;
            NetworkManager_Poker.TimerChange += OnTimerChange;
            NetworkManager_Poker.PlayerOptionSelect += SelectedOption;
            NetworkManager_Poker.ShowAllPlayerCard += ShowOtherPlayerCard;
            NetworkManager_Poker.Gamewin += GameWin;
            NetworkManager_Poker.GameWinSinglePlayer += GamewinWithSinglePlayer;
            NetworkManager_Poker.RoomPlayerStandUp += PlayerStandUpReset;

            PokerGamePlay.ONRoundComplete += OnOneRoundCompleted;
        }

        private void OnDisable()
        {
            NetworkManager_Poker.GameStart -= GameStart;
            NetworkManager_Poker.PlayerCardDistribution -= CardDistribution;
            NetworkManager_Poker.PlayerTurn -= PlayerTurn;
            NetworkManager_Poker.TimerChange -= OnTimerChange;
            NetworkManager_Poker.PlayerOptionSelect -= SelectedOption;
            NetworkManager_Poker.ShowAllPlayerCard -= ShowOtherPlayerCard;
            NetworkManager_Poker.Gamewin -= GameWin;
            NetworkManager_Poker.GameWinSinglePlayer -= GamewinWithSinglePlayer;
            NetworkManager_Poker.RoomPlayerStandUp -= PlayerStandUpReset;

            PokerGamePlay.ONRoundComplete -= OnOneRoundCompleted;
        }

        private void Start()
        {
            SetInFirtTimeOpen();
        }

        public void SetInFirtTimeOpen()
        {
            if (Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
            }
            else if (!Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
            }
            playerId = "";
            PlayerName.text = "";
            IsPlayerInTable = false;
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            GiftButton.transform.GetComponent<Button>().interactable = true;
            NotActiveBG.SetActive(false);
            Timer.enabled = false;
            Blind.enabled = false;
            BetOptionImage.enabled = false;
            BetAmount.text = "";
            SelectOptionText.text = "";
            TotalChips.text = Constants.NumberShow(TotalAmountInGame);
            Card1.ResetCards();
            Card2.ResetCards();
            WinParticals.SetActive(false);

            if (playerId == Constants.PLAYER_ID)
            {
                UserInfoUpdate();
            }
        }

        void UserInfoUpdate()
        {
            JSONNode data = new JSONObject
            {
                ["unique_id"] = Constants.PLAYER_ID,
            };

            StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
            {
                if (IsSuccess)
                {
                    JSONNode jsonNode = JSON.Parse(result)["data"];
                    Constants.SetPlayerData(jsonNode);
                    Debug.LogError("USER_INFO POKERPLAYER" + jsonNode.ToString());
                }
                else
                    Constants.Logout();
            }));
        }

        public void PlayerLeft()
        {
            SetInFirtTimeOpen();
        }

        public void OnOneRoundCompleted()
        {
            BetAmount.text = "";
            ShowBetOption(false, "", PokerGamePlay.Instance.Default);
        }

        private void GameStart(JSONNode jsonNode)
        {
            PokerGamePlay.Instance.Message.gameObject.SetActive(false);
            //PokerGamePlay.Instance.selectionButtonParent.gameObject.SetActive(true);
            Blind.enabled = false;
            JSONNode json = jsonNode["gamedata"];
            for (int i = 0; i < json.Count; i++)
            {
                if (json[i]["playerId"].Value == Constants.PLAYER_ID)
                {
                    PokerGamePlay.Instance.UpdatePokerChips();
                }

                if (json[i]["playerId"].Value == playerId)
                {
                    ShowAndSetTotalAmount(json[i]["playerAmount"].AsLong);

                    //Debug.LogError(i + " : "+Constants.NumberShow(json[i]["totalBetAmount"].AsLong));
                    //if (json[i]["totalBetAmount"].AsLong != 0)
                    //    BetAmount.text = Constants.NumberShow(json[i]["totalBetAmount"].AsLong);
                    //else
                    //    BetAmount.text = "";

                    if (json[i]["inGamePlayerName"].Value == "smallBlind")
                    {
                        Blind.enabled = true;
                        Blind.sprite = GameManager_Poker.Instance.SmallBlind;
                        ShowBetOption(true, "SB", PokerGamePlay.Instance.Default);
                        BetAmount.text = Constants.NumberShow(json[i]["totalBetAmount"].AsLong);
                        PokerGamePlay.SmallBlindAmount = json[i]["totalBetAmount"].AsLong;
                    }
                    else if (json[i]["inGamePlayerName"].Value == "bigBlind")
                    {
                        Blind.enabled = true;
                        Blind.sprite = GameManager_Poker.Instance.BigBlind;
                        BetAmount.text = Constants.NumberShow(json[i]["totalBetAmount"].AsLong);
                        ShowBetOption(true, "BB", PokerGamePlay.Instance.Default);
                    }
                    else
                    {
                        BetAmount.text = "";
                    }
                }
            }
        }

        private void CardDistribution(JSONNode jsonNode)
        {
            //if (playerId != Constants.PLAYER_ID)
            //{
            //    Card1.gameObject.SetActive(true);
            //    Card2.gameObject.SetActive(true);
            //    return;
            //}

            //JSONNode card = jsonNode["cards"];
            //ShowCards(card);
        }

        public void ShowCards(JSONNode card)
        {
            //Debug.LogError(card.ToString());
            //Debug.LogError($" {PlayerName.text} {card[0]["suits"].Value}  {card[0]["value"].AsInt - 1} ");
            //Debug.LogError($"{PlayerName.text} {card[1]["suits"].Value}  {card[1]["value"].AsInt - 1} ");
            Card1.SetDataOnCard(card[0]["suits"].Value, card[0]["value"].AsInt - 1);
            Card2.SetDataOnCard(card[1]["suits"].Value, card[1]["value"].AsInt - 1);
        }

        private void PlayerTurn(JSONNode jsonNode)
        {
            if (jsonNode["playerId"].Value == playerId)
            {
                Timer.enabled = true;
                Timer.fillAmount = 1;
            }
            else
            {
                Timer.enabled = false;
            }
        }

        private void OnTimerChange(JSONNode jsonNode)
        {
            if (jsonNode["playerId"].Value == playerId)
            {
                Timer.fillAmount = jsonNode["time"].AsFloat / jsonNode["turnChangeTime"].AsFloat;
                Timer.enabled = true;
            }
            else
                Timer.enabled = false;

            //if (jsonNode["playerId"].Value != playerId) return;
            //Timer.fillAmount = jsonNode["time"].AsFloat / jsonNode["turnChangeTime"].AsFloat;
        }

        private void GameWin(JSONNode jsonNode)
        {
            PokerGamePlay.Instance.ResetSelectionButtons();

            if (jsonNode["winnerId"].Value == playerId)
            {
                //Debug.LogError(PlayerName.text);
                //PokerGamePlay.Instance.ShowWinner(PlayerName.text);

                JSONNode json = jsonNode["winerCard"];
                CardScript cs = null;

                for (int i = 0; i < CardParent.transform.childCount; i++)
                {
                    if (i == 0) cs = Card1;
                    else if (i == 1) cs = Card2;

                    Debug.LogError(json.ToString());
                    for (int j = 0; j < json.Count; j++)
                    {
                        int cardId = json[j]["value"].AsInt;
                        string cardSuits = json[j]["suits"].Value;

                        if (cs.CardId == cardId && cs.CardSuits == cardSuits)
                        {
                            //CardParent.transform.GetChild(i).localScale = new Vector3(1.1f, 1.1f, 1.1f);
                            cs.DarkImage.enabled = false;
                            break;
                        }
                        else
                        {
                            cs.EnableDarkImage();
                        }
                    }
                }
                WinParticals.SetActive(true);
                Debug.LogError("Winning :" + jsonNode["winamount"].AsLong);
                //TotalChips.text = Constants.NumberShow(jsonNode["winamount"].AsLong);
                ShowAndSetTotalAmount(jsonNode["winamount"].AsLong);
                PokerGamePlay.Instance.DisableAllButton();
            }
            else
            {
                CardParent.transform.GetChild(0).GetComponent<CardScript>().EnableDarkImage();
                CardParent.transform.GetChild(1).GetComponent<CardScript>().EnableDarkImage();
                NotActiveBG.SetActive(true);
            }
            OnOneRoundCompleted();
            PokerGamePlay.Instance.ResetSelectionButtons();
        }

        private void GamewinWithSinglePlayer(JSONNode jsonNode)
        {
            if (jsonNode["winnerId"].Value != playerId)
                return;
            Debug.LogError("dsfjasdjfkladjsfkladjs klfjskl :" + jsonNode["winamount"].AsLong);
            //TotalChips.text = Constants.NumberShow(jsonNode["winamount"].AsLong);
            ShowAndSetTotalAmount(jsonNode["winamount"].AsLong);
            PokerGamePlay.Instance.DisableAllButton();
            Timer.enabled = false;
            OnOneRoundCompleted();
            WinParticals.SetActive(true);
            PokerGamePlay.Instance.ShowWinHandName("No Player Left");
        }

        private void SelectedOption(JSONNode jsonNode)
        {
            if (jsonNode["playerId"].Value == playerId)
            {
                //if (jsonNode["alreadyOptionSelect"] == true)
                //{
                //    PokerGamePlay.Instance.ResetSelectionButtons();
                //}
                //else
                //{
                if (jsonNode["playerBetAmount"].AsLong != 0)
                {
                    BetAmount.text = Constants.NumberShow(jsonNode["playerBetAmount"].AsLong);
                }

                ShowAndSetTotalAmount(jsonNode["playerAmount"].AsLong);

                string call = jsonNode["playerOption"].Value;

                switch (call)
                {
                    case "call":
                        ShowBetOption(true, "C", PokerGamePlay.Instance.Call);
                        BetAmount.text = Constants.NumberShow(jsonNode["playerBetAmount"].AsLong);
                        break;
                    case "fold":
                        PlayerFold();
                        ShowBetOption(true, "F", PokerGamePlay.Instance.Fold);
                        BetAmount.text = "";
                        break;
                    case "raise":
                        ShowBetOption(true, "R", PokerGamePlay.Instance.Raise);
                        BetAmount.text = Constants.NumberShow(jsonNode["playerBetAmount"].AsLong);
                        break;
                    case "check":
                        ShowBetOption(true, "C", PokerGamePlay.Instance.Check);
                        BetAmount.text = "";
                        break;
                    case "allin":
                        ShowBetOption(true, "A", PokerGamePlay.Instance.Allin);
                        ShowAndSetTotalAmount(jsonNode["playerBetAmount"].AsLong);
                        PlayerAllIn();
                        break;
                    default:
                        break;
                        //}
                }
            }
        }

        private void ShowOtherPlayerCard(JSONNode jsonNode)
        {
            Timer.enabled = false;
            if (playerId == Constants.PLAYER_ID)
                return;

            for (int i = 0; i < jsonNode.Count; i++)
            {
                if (jsonNode[i]["playerId"].Value == playerId)
                {
                    JSONNode card = jsonNode[i]["cards"];
                    ShowCards(card);
                }
            }
        }

        public void PlayerFold()
        {
            NotActiveBG.SetActive(true);
            TotalChips.text = "<color=red>Fold</color>";
            Timer.enabled = false;
            Card1.EnableDarkImage();
            Card2.EnableDarkImage();
            Debug.LogWarning("~~~PlayerFold~~~~");
        }

        public void PlayerAllIn()
        {
            //NotActiveBG.SetActive(true);
            TotalChips.text = "<color=orange>ALL IN</color>";
            Timer.enabled = false;
            //Card1.EnableDarkImage();
            //Card2.EnableDarkImage();
        }

        public void ShowBetOption(bool Imageenable, string OptionName, Color color)
        {
            BetOptionImage.enabled = Imageenable;
            SelectOptionText.text = OptionName;
            BetOptionImage.color = color;
        }

        public void ShowAndSetTotalAmount(long amount)
        {
            TotalAmountInGame = amount;
            TotalChips.text = Constants.NumberShow(TotalAmountInGame);
        }

        public void StandPlayerButtonClick()
        {
            Debug.Log("Player Stand");
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            Constants.isJoinByStandUp = false;
            Constants.SelectedSeat = SeatIndex;
#if true
            GameManager_Poker.Instance.TableSelectPanel.SetActive(false);
            if(Constants.BuyMaxOn) {
                string timer;
                if (Constants.TIMER_POKER == 0)
                    timer = "normal";
                else
                    timer = "fast";

                Constants.AutoBuyAmount =  Constants.PokerMaxAmount;

                JSONNode jsonnode = new JSONObject
                {
                    ["playerId"] = Constants.PLAYER_ID,
                    ["position"] = Constants.SelectedSeat,
                    ["minAmount"] =  Constants.PokerMaxAmount / 5,
                    ["buyAmount"] =  Constants.PokerMaxAmount,
                    ["maxAmount"] =  Constants.PokerMaxAmount,
                    ["timer"] = timer,
                    ["stake"] = GameManager_Poker.Instance.MinMaxStakesAmounts[Constants.pokerMinMaxIndex].Min,
                    ["autobuy"] = Constants.AutoReBuy,
                    ["buyatmax"] = true,
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
                GameManager_Poker.Instance.GamePlayPanel.SetActive(true);
            } else {
                GameManager_Poker.Instance.BuyInPanel.SetActive(true);
            }
            
#else
            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["maxAmount"] = Constants.PokerMaxAmount,
                ["roomName"] = Constants.RoomName,
                ["isStandUp"] = false,
            };

            Debug.LogError("StandPlayer======= " + jsonnode.ToString());
            NetworkManager_Poker.Instance.PokerSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
#endif
        }

        public void FriendInviteButton()
        {
            if (FacebookManager.Instance != null)
                FacebookManager.Instance.InitCallback();
            Constants.SelectedInvite = SeatIndex;
            MainNetworkManager.Instance.OnSendingFriendList();
        }

        public void GiftButtonClick()
        {
            Debug.Log("GiftButtonClick");
            MainNetworkManager.Instance.MainSocket?.Emit("tableItems");
            Constants.PokerGiftReceiverID = playerId;
            Constants.PokerGiftSenderID = Constants.PLAYER_ID;
        }

        public void ProfilePanelButtonClick()
        {
            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = playerId,
            };

            Debug.LogWarning("PlayerProfileButton " + jsonnode.ToString());
            MainNetworkManager.Instance.MainSocket?.Emit("playerInfo", jsonnode.ToString());
            //NetworkManager_Poker.Instance.PokerSocket?.Emit("playerInfo", jsonnode.ToString());
        }

        private void PlayerStandUpReset(JSONNode jsonNode)
        {
            if (playerId == jsonNode["playerId"] && playerId == Constants.PLAYER_ID)
            {
                Constants.isJoinByStandUp = true;
                Debug.Log("ConstatPlayerID " + Constants.PLAYER_ID);
                PokerGamePlay.Instance.Message.gameObject.SetActive(false);

                for (int i = 0; i < GameManager_Poker.Instance.PlayersParent.transform.childCount; i++)
                {
                    PokerPlayer _pokerPlayer = GameManager_Poker.Instance.PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();

                    //if (_pokerPlayer.playerId == "")
                    {
                        _pokerPlayer.ActivePlayer.SetActive(false);
                        _pokerPlayer.StandPlayer.SetActive(true);
                        _pokerPlayer.InvitePlayer.SetActive(false);
                        Debug.Log("PokerPlayerID " + _pokerPlayer.playerId);
                    }
                }

                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
                playerId = "";
                PlayerName.text = "";
                IsPlayerInTable = false;
                ResetPlayer();
            }
            else if (playerId == jsonNode["playerId"] && playerId != Constants.PLAYER_ID && !Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(false);
                InvitePlayer.SetActive(true);
                playerId = "";
                PlayerName.text = "";
                IsPlayerInTable = false;
                ResetPlayer();
                Debug.Log("playerID==playerID");
            }
            else if (playerId == jsonNode["playerId"] && playerId != Constants.PLAYER_ID && Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
                playerId = "";
                PlayerName.text = "";
                IsPlayerInTable = false;
                ResetPlayer();
                Debug.Log("playerID==playerID");
            }
        }
    }
}