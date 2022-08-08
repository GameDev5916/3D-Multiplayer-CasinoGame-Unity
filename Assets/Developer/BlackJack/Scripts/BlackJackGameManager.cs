using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using Casino_Poker;
using DG.Tweening;

namespace BalckJack
{
    public class BlackJackGameManager : MonoBehaviour
    {
        public static BlackJackGameManager Instance;

        public List<Constance.CardThems> AllCardsSprites;
        public List<Constance.MiniMax> MinMaxesBetAmounts;
        public List<GiftItems> GiftItemsSprites;
        public List<Sprite> TierSprites;

        public GameObject TableSetUi, GamePlayUI;
        public GameObject CardPrefab;
        public GameObject DummyCard;
        public GameObject DealerCardBox, DiscardedCardBox;
        public GameObject PlayersParent, FriendsPanel, BuyInPanel, InviteRequestPanel, AddFriendRequestPanel, BackOptionPanel, BlackJackGiftPanel, PlayerProfilePanel;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void OnEnable()
        {
            BlackJack_NetworkManager.PlayerJoinRoom += OnJoinRoom;
            BlackJack_NetworkManager.PlayerLeftRoomAction += OnPlayerLeftRoom;
            BlackJack_NetworkManager.BetActionShowChips += ShowBetChips;
            BlackJack_NetworkManager.PlayerOption += OnGettingPlayerOption;
            BlackJack_NetworkManager.PlayerAmount += OpenBuyInPanel;
            BlackJack_NetworkManager.StartPlayerCardDistribution += CardDistributionStart;

            MainNetworkManager.PlayerFriendListAction += OpenFriendsPanel;
            MainNetworkManager.JoinInvitation += OnBlackJackInvitation;
            MainNetworkManager.AcceptFriendRequest += OpenFriendRequestPanel;
            MainNetworkManager.TableItemAction += OpenGiftsPanel;
            MainNetworkManager.OnPlayerProfileOpen += OpenProfilePanel;
        }

        private void OnDisable()
        {
            BlackJack_NetworkManager.PlayerJoinRoom -= OnJoinRoom;
            BlackJack_NetworkManager.PlayerLeftRoomAction -= OnPlayerLeftRoom;
            BlackJack_NetworkManager.BetActionShowChips -= ShowBetChips;
            BlackJack_NetworkManager.PlayerOption -= OnGettingPlayerOption;
            BlackJack_NetworkManager.PlayerAmount -= OpenBuyInPanel;
            BlackJack_NetworkManager.StartPlayerCardDistribution -= CardDistributionStart;

            MainNetworkManager.PlayerFriendListAction -= OpenFriendsPanel;
            MainNetworkManager.JoinInvitation -= OnBlackJackInvitation;
            MainNetworkManager.AcceptFriendRequest -= OpenFriendRequestPanel;
            MainNetworkManager.TableItemAction -= OpenGiftsPanel;
            MainNetworkManager.OnPlayerProfileOpen -= OpenProfilePanel;
        }

        public void JoinByInvitation()
        {
            if (Constants.isJoinByInvitation == true)
            {
                GamePlayUI.SetActive(true);
                TableSetUi.SetActive(false);
                Constants.isJoinByInvitation = false;

                JSONNode jsonnode = new JSONObject
                {
                    ["friendPlayerId"] = Constants.PLAYER_ID,
                    ["requestaction"] = true,
                    ["senderPlayerId"] = Constants.instance.FrinedInvitationJsonData["senderPlayerId"],
                    ["roomName"] = Constants.instance.FrinedInvitationJsonData["roomName"],
                    ["roomStake"] = Constants.instance.FrinedInvitationJsonData["roomStake"],
                    ["roomType"] = Constants.instance.FrinedInvitationJsonData["roomType"]
                };

                BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
                //MainNetworkManager.Instance.MainSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
                Debug.Log("isJoinByInvitation (GameManagerBlackJack) " + jsonnode.ToString());
            }
        }

        public void OpenBuyInPanel(JSONNode jsonNode)
        {
            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)
            {
                BuyInPanel.SetActive(true);
            }
        }

        private void OnJoinRoom(JSONNode jsonNode)
        {
            int CurrentPlayerIndexinJson = 0;
            JSONNode json = jsonNode["roomData"];

            // Find Index of Player in JsonNode
            for (int i = 0; i < jsonNode["roomData"].Count; i++)
            {
                if (json[i]["playerId"].Value == Constants.PLAYER_ID)
                {
                    Constants.RoomName = jsonNode["roomName"];
                    CurrentPlayerIndexinJson = i;
                }
            }
            int total = 5;  // For 9 Player Change total = 9

            for (int i = 0, j = CurrentPlayerIndexinJson; i < total; i++, j++)
            {
                //Debug.LogError($"I : {i} J : {j} Total : {jsonNode["roomData"].Count}");
                Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();
                // if not data then set player position acouding to j
                //if(json[j].Count == 0)
                if (json[j]["playerId"].Value == "null")
                {
                    player.Position = j + 1;
                    if (j == (total - 1)) j = -1;
                    continue;
                }
                // if reach end of the json than start from zero(0)
                if (player.playerId == json[j]["playerId"].Value)
                {
                    if (j == (total - 1)) j = -1;
                    continue;
                }

                player.ActivePlayer.SetActive(true);
                player.InvitePlayer.SetActive(false);
                player.StandPlayer.SetActive(false);
                player.TimerImage.enabled = false;
                player.playerId = json[j]["playerId"].Value;
                player.PlayerName.text = json[j]["name"].Value;
                player.Position = json[j]["position"].AsInt;

                if (Constants.PLAYER_ID == json[j]["playerId"].Value)
                {
                    GamePlayManager.instance.BalanceText.text = "$" + json[j]["playerAmount"].Value;
                    GamePlayManager.instance.balanceAmount = json[j]["playerAmount"];

                    GamePlayManager.instance.WinText.text = "$" + json[j]["lastWinAmount"].Value;
                    GamePlayManager.instance.PP_BetButton.SetActive(false);
                    GamePlayManager.instance.BetButton.interactable = false;
                    Constants.blackJackPosition = json[j]["position"].AsInt;

                    TurnOffOptionButtons();
                }

                if (json[j]["profilePic"].Value != "" && json[j]["profilePic"].Value != "null")
                {
                    Constants.GetImageFrom64String(json[j]["profilePic"].Value, (Texture image) =>
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

                if (j == (total - 1)) j = -1;
            }

            if (Constants.isJoinByStandUp)
            {
                Debug.LogWarning("StandupJoin");
                for (int i = 0; i < PlayersParent.transform.childCount; i++)
                {
                    Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();
                    if (player.playerId == "")
                    {
                        player.ActivePlayer.SetActive(false);
                        player.InvitePlayer.SetActive(false);
                        player.StandPlayer.SetActive(true);
                        player.ResetPlayerData();
                    }
                }
                Constants.isJoinByStandUp = false;
            }
            else if(!Constants.isJoinByStandUp)
            {
                for (int i = 0; i < PlayersParent.transform.childCount; i++)
                {
                    Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();
                    if (player.playerId == "")
                    {
                        player.ActivePlayer.SetActive(false);
                        player.InvitePlayer.SetActive(true);
                        player.StandPlayer.SetActive(false);
                        player.ResetPlayerData();
                    }
                }
                Constants.isJoinByStandUp = false;
            }
        }

        private void OnPlayerLeftRoom(JSONNode jsonNode)
        {
            for (int i = 0; i < PlayersParent.transform.childCount; i++)
            {
                Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();
                if (player.playerId == jsonNode["playerId"].Value)
                {
                    player.PlayerLeftBlackJack();
                }
            }
        }

        private void ShowBetChips(JSONNode jsonNode)
        {
            for (int i = 0; i < PlayersParent.transform.childCount; i++)    // Display Bet Chips with Amount , Disable Timer Circle
            {
                Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();
                if (player.playerId == jsonNode["playerId"].Value)
                {
                    player.isPlayerBet = jsonNode["playerIsBet"];
                    player.TimerImage.enabled = false;
                    player.ChipsObject.SetActive(true);
                    player.BetAmounts = jsonNode["betAmount"];
                    player.BetAmount.text = $"${ Constance.AmountShow(player.BetAmounts)}";
                }
            }

            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)   // Update the Balance after Bet
            {
                GamePlayManager.instance.BalanceText.text = "$" + jsonNode["playerAmount"].Value;
                GamePlayManager.instance.balanceAmount = jsonNode["playerAmount"];
            }
        }

        public void TurnOffOptionButtons()
        {
            GamePlayManager.instance.StandButton.interactable = false;
            GamePlayManager.instance.HitButton.interactable = false;
            GamePlayManager.instance.SplitButton.interactable = false;
            GamePlayManager.instance.DoubleButton.interactable = false;
        }

        private void OnGettingPlayerOption(JSONNode jsonNode)
        {
            StartCoroutine(PlayerOptionSet(jsonNode));
        }

        IEnumerator PlayerOptionSet(JSONNode jsonNode)
        {
            yield return new WaitForSeconds(1.5f);

            for (int i = 0; i < PlayersParent.transform.childCount; i++)
            {
                Player player = PlayersParent.transform.GetChild(i).GetComponent<Player>();

                if (jsonNode["playerId"] == player.playerId)
                {
                    if (jsonNode["turn"] == 1)
                    {
                        //Debug.LogWarning("FirstDeckActive");
                        player.turn = 1;
                        for (int j = 0; j < player.CardParent.transform.childCount; j++)
                        {
                            //Debug.Log("FirstDeck Card Highlight");
                            player.CardParent.transform.GetChild(j).GetComponent<Card>().DarkImage.transform.SetAsFirstSibling();
                        }
                        for (int j = 0; j < player.PP_CardParent.transform.childCount; j++)
                        {
                            //Debug.Log("SecondDeck Card Dull");
                            player.PP_CardParent.transform.GetChild(j).GetComponent<Card>().DarkImage.transform.SetAsLastSibling();
                        }
                    }
                    else if (jsonNode["turn"] == 2)
                    {
                        //Debug.LogWarning("SecondDeckActive");
                        player.turn = 2;
                        for (int j = 0; j < player.CardParent.transform.childCount; j++)
                        {
                            //Debug.Log("FirstDeck Card Dull");
                            player.CardParent.transform.GetChild(j).GetComponent<Card>().DarkImage.transform.SetAsLastSibling();
                        }
                        for (int j = 0; j < player.PP_CardParent.transform.childCount; j++)
                        {
                            //Debug.Log("SecondDeck Card Highlight");
                            player.PP_CardParent.transform.GetChild(j).GetComponent<Card>().DarkImage.transform.SetAsFirstSibling();
                        }
                    }
                }
            }

            if (jsonNode["playerId"] == Constants.PLAYER_ID)
            {
                GamePlayManager.instance.StandButton.interactable = jsonNode["stand"];
                GamePlayManager.instance.HitButton.interactable = jsonNode["hit"];
                GamePlayManager.instance.SplitButton.interactable = jsonNode["split"];
                GamePlayManager.instance.DoubleButton.interactable = jsonNode["double"];
            }
        }

        private void OpenFriendsPanel(JSONNode jsonNode)
        {
            FriendsPanel.SetActive(true);
            MainNetworkManager.SetFriendsPanel?.Invoke(jsonNode);
        }

        private void OnBlackJackInvitation(JSONNode jsonNode)
        {
            if (jsonNode["friendPlayerId"] == Constants.PLAYER_ID)
            {
                Debug.Log("Invitation-BlackJack");
                InviteRequestPanel.SetActive(true);
                MainNetworkManager.SetInvitePanel?.Invoke(jsonNode);
            }
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

        public Sprite GetSprite(string ID)
        {
            return GiftItemsSprites.Find(x => x.identifier == ID).giftSprite;
        }

        private void OpenGiftsPanel(JSONNode jsonNode)
        {
            BlackJackGiftPanel.SetActive(true);
            MainNetworkManager.SetGiftPanel?.Invoke(jsonNode);
        }

        private void OpenProfilePanel(JSONNode jsonNode)
        {
            PlayerProfilePanel.SetActive(true);
            MainNetworkManager.SetPlayerProfilePanel?.Invoke(jsonNode);
        }

        private void CardDistributionStart(JSONNode jsonNode)
        {
            StartCoroutine(PlayerCardDistribution(jsonNode));
        }

        IEnumerator PlayerCardDistribution(JSONNode jsonNode)
        {
            Transform p = PlayersParent.transform;
            for (int j = 0; j < 2; j++) // used for 2 Rounds Distribution
            {
                for (int i = 0; i < p.childCount; i++)  // used for check each 5 player sit
                {
                    if (p.GetChild(i).GetComponent<Player>().playerId == "")
                        continue;

                    for (int k = 0; k < jsonNode["playerHand"].Count; k++)  // used to check each playerHand data
                    {
                        if (p.GetChild(i).GetComponent<Player>().playerId == jsonNode["playerHand"][k]["playerId"])
                        {
                            PlayerCardAnimation(p.GetChild(i).GetComponent<Player>(), jsonNode, j);
                            Debug.LogWarning("CardDistribution " + jsonNode.ToString());
                            yield return new WaitForSeconds(0.4f);
                        }
                    }
                }
            }
        }

        public void PlayerCardAnimation(Player blackjackPlayer, JSONNode jsonNode, int cardIndexInPlayerHand)
        {
            RectTransform r = Instantiate(DummyCard, DealerCardBox.transform).GetComponent<RectTransform>();
            r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
            r.DOMove(blackjackPlayer.CardParent.transform.position, 0.4f).OnComplete(() =>
            {
                Destroy(r.gameObject);
                blackjackPlayer.DisplayPlayerCards(jsonNode, cardIndexInPlayerHand);
            });
        }
    }

    [System.Serializable]
    public class GiftItems
    {
        public string identifier;
        public Sprite giftSprite;
    }
}