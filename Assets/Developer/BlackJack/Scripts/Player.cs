using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using LitJson;
using System;
using DG.Tweening;
using TMPro;
using Facebook.Unity;

namespace BalckJack
{
    public class Player : MonoBehaviour
    {
        public int Position;
        public int turn;
        public string playerId;

        public bool isPlayerBet;

        public Text PlayerName;
        public RawImage PlayerImage;

        public Text TotalCardNumber;
        public Text PP_TotalCardNumber;
        public Text BetAmount;
        public Text PP_BetAmount;
        public Text PlayerOptionText;

        [Space]
        public GameObject PerfectPairGameObject;
        public GameObject ChipsObject;
        public GameObject PP_ChipsObject;
        public GameObject CardDataObject;
        public GameObject ActivePlayer;
        public GameObject InvitePlayer;
        public GameObject StandPlayer;

        [Space]
        [Header("Result")]
        public GameObject WinImage;
        public GameObject LoseImage;
        public GameObject PushImage;

        [Space]
        [Header("PP_Result")]
        public GameObject PP_WinImage;
        public GameObject PP_LoseImage;
        public GameObject PP_PushImage;

        [Space]
        public GameObject CardParent;
        public GameObject PP_CardParent;

        [Header("ObjectsForChipsAnimation")]
        public GameObject ResultObject;
        public GameObject PP_ResultObject;
        public GameObject ProfilePicObject;
        [Space]
        public Image TimerImage;

        public long BetAmounts;
        public Transform GiftButton;
        public GameObject giftIconImage, giftItemImage;

        private void OnEnable()
        {
            BlackJack_NetworkManager.BetTimerAction += OnTimerChange;
            BlackJack_NetworkManager.PlayerTimerStartAction += OnPlayerTimerChange;
            BlackJack_NetworkManager.WinLoseAction += DisplayResultImage;
            BlackJack_NetworkManager.OnGameRestart += ResetPlayerOnGameRestart;
            BlackJack_NetworkManager.RoomPlayerStandUp += PlayerStandUpReset;
            GamePlayManager.BetAmount += SetYourBet;
            GamePlayManager.PP_BetAmount += SetYourPP_Bet;
            GamePlayManager.DisplayPlayerCard += CardDistributionStart;
        }

        private void OnDisable()
        {
            BlackJack_NetworkManager.BetTimerAction -= OnTimerChange;
            BlackJack_NetworkManager.PlayerTimerStartAction -= OnPlayerTimerChange;
            BlackJack_NetworkManager.WinLoseAction -= DisplayResultImage;
            BlackJack_NetworkManager.OnGameRestart -= ResetPlayerOnGameRestart;
            BlackJack_NetworkManager.RoomPlayerStandUp -= PlayerStandUpReset;
            GamePlayManager.BetAmount -= SetYourBet;
            GamePlayManager.PP_BetAmount -= SetYourPP_Bet;
            GamePlayManager.DisplayPlayerCard -= CardDistributionStart;
        }

        private void Start()
        {
            ActivePlayer.SetActive(false);
            InvitePlayer.SetActive(false);
            StandPlayer.SetActive(true);
            ResetPlayerData();
        }

        //public void SetPlayerData(JSONNode jsonNode)
        //{
        //    for (int i = 0; i < jsonNode.Count; i++)
        //    {
        //        if(jsonNode[i]["position"].AsInt == Position)
        //        {
        //            PlayerName.text = jsonNode[i]["name"].ToString();
        //            playerId = jsonNode[i]["playerId"].ToString();
        //            Debug.LogError(jsonNode[i]["name"]);
        //        }
        //    }

        //    //if (TablePostion != 3)
        //    //    return;

        //    //CardList[0].GetComponent<Card>().SetDataOnCard("hearts", 4);
        //    //CardList[1].GetComponent<Card>().SetDataOnCard("spades", 12);
        //    //SetCardData();
        //}


        //public void SetCardData()
        //{
        //    TotalCardNumber.text = "21";

        //    GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
        //    Card.GetComponent<Card>().SetDataOnCard("hearts", 4);
        //    Card.GetComponent<Card>().ShowCard();

        //    Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
        //    Card.GetComponent<Card>().SetDataOnCard("spades", 10);
        //    Card.GetComponent<Card>().ShowCard();

        //    CardDataObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
        //}



        public void DisplayPlayerCards(JSONNode jsonNode, int cardIndexPlayerHand)      // Display card at Distribution Start First Time (2 Rounds)
        {
            JSONNode data = jsonNode["playerHand"];

            for (int i = 0; i < data.Count; i++)
            {
                if (playerId == data[i]["playerId"])
                {
                    JSONNode playerHand = data[i]["playerHand"];
                    Debug.LogWarning("playerHand " + playerHand.ToString());

                    if (cardIndexPlayerHand == 1)
                    {
                        TotalCardNumber.transform.parent.gameObject.SetActive(true);
                        TotalCardNumber.text = data[i]["score"];
                    }

                    string CardSuit = playerHand[cardIndexPlayerHand]["suit"];
                    int CardID = Constance.GetCardIndex(playerHand[cardIndexPlayerHand]["value"]);
                    GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
                    Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);
                }
            }
        }


        //private void DisplayPlayerCards(JSONNode jsonNode, int cardIndexPlayerHand)      // Display card at Distribution Start First Time
        //{
        //    JSONNode data = jsonNode["playerHand"];

        //    for (int i = 0; i < data.Count; i++)
        //    {
        //        if (playerId == data[i]["playerId"])
        //        {
        //            JSONNode playerHand = data[i]["playerHand"];
        //            Debug.LogWarning("playerHand " + playerHand.ToString());

        //            TotalCardNumber.transform.parent.gameObject.SetActive(true);
        //            TotalCardNumber.text = data[i]["score"];

        //            for (int j = 0; j < playerHand.Count; j++)
        //            {
        //                string CardSuit = playerHand[j]["suit"];
        //                int CardID = Constance.GetCardIndex(playerHand[j]["value"]);
        //                GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
        //                Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);
        //            }
        //        }
        //    }
        //}

        public void SetYourBet(long Amount)
        {
            if (playerId == Constants.PLAYER_ID)
            {
                BetAmounts = Amount;

                JSONNode data = new JSONObject
                {
                    ["playerId"] = playerId,
                    ["bet"] = BetAmounts,
                };

                //Debug.Log("PlayerBet: " + data.ToString());

                BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("playerBet", data.ToString());
            }
        }

        public void SetYourPP_Bet(long Amount)
        {
            PP_ChipsObject.SetActive(true);
            PP_BetAmount.text = $"${ Constance.AmountShow(Amount)}";
        }

        private void CardDistributionStart(JSONNode jsonNode)
        {
            //Debug.LogWarning("~~~~~~1" + jsonNode.ToString());
            StartCoroutine(PlayerCardDistribution(jsonNode));
        }

        IEnumerator PlayerCardDistribution(JSONNode jsonNode)
        {
            Transform p = BlackJackGameManager.Instance.PlayersParent.transform;

            for (int i = 0; i < p.childCount; i++)
            {
                Player player = p.GetChild(i).GetComponent<Player>();

                if (player.playerId == jsonNode["playerId"])
                {
                    PlayerOptionText.text = jsonNode["selectOption"];
                }

                if (player.playerId == jsonNode["playerId"] && jsonNode["selectOption"].Value == "HIT")
                {
                    PlayerCardAnimation(player, jsonNode);
                    //Debug.LogWarning("CardDistributionHIT " + jsonNode.ToString());
                    yield return new WaitForSeconds(0.4f);
                }

                if (player.playerId == jsonNode["playerId"] && jsonNode["selectOption"].Value == "SPLIT")
                {
                    PlayerCardAnimation(player, jsonNode);
                    player.PP_ChipsObject.SetActive(true);
                    player.PP_BetAmount.text = "$" + Constance.AmountShow(player.BetAmounts);
                    Debug.LogWarning("PPAmount " + player.BetAmounts + " playerID " + p.transform.GetChild(i).gameObject.name + "Child " + p.childCount);
                    Debug.LogWarning("CardDistributionSPLIT " + jsonNode.ToString());
                    yield return new WaitForSeconds(0.4f);
                }

                if (player.playerId == jsonNode["playerId"] && jsonNode["selectOption"].Value == "DOUBLE")
                {
                    player.BetAmount.text = "$" + Constance.AmountShow(player.BetAmounts * 2);
                }
            }
        }

        public void PlayerCardAnimation(Player blackjackPlayer, JSONNode jsonNode)
        {
            //Debug.LogWarning("~~~~~~2= " + jsonNode.ToString());
            if (jsonNode["isSplit"] == 0)
            {
                if (blackjackPlayer.turn == 1)
                {
                    RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
                    r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
                    r.DOMove(blackjackPlayer.CardParent.transform.position, 0.5f).OnComplete(() =>
                    {
                        PlayerCardDisplay(jsonNode);
                        Destroy(r.gameObject);
                    });
                }
                else if (blackjackPlayer.turn == 2)
                {
                    RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
                    r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
                    r.DOMove(blackjackPlayer.PP_CardParent.transform.position, 0.5f).OnComplete(() =>
                    {
                        PlayerCardDisplay(jsonNode);
                        Destroy(r.gameObject);
                    });
                }
            }
            if (jsonNode["isSplit"] == 1)
            {
                RectTransform r1 = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
                r1.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
                r1.DOMove(blackjackPlayer.CardParent.transform.position, 0.4f).OnComplete(() =>
                {
                    Destroy(r1.gameObject);
                    RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
                    r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
                    r.DOMove(blackjackPlayer.CardParent.transform.position, 0.4f).OnComplete(() =>
                    {
                        PlayerCardDisplay(jsonNode);
                        Destroy(r.gameObject);
                    });
                });
            }
        }

        public void PlayerCardDisplay(JSONNode jsonNode)    // Use for Displaying card After Player Option Selected
        {
            if (playerId == jsonNode["playerId"])
            {
                if (jsonNode["message"])
                {
                    Constants.ShowWarning(jsonNode["message"].Value);
                    Debug.LogWarning("Balance Low " + jsonNode["message"].Value);
                    return;
                }
            }

            if (jsonNode["isSplit"] == 0)   // Display Cards without Split ---> playerSelectAction 1
            {
                if (playerId == jsonNode["playerId"])
                {
                    PlayerOptionText.text = jsonNode["selectOption"];

                    if (jsonNode["card"] != null)
                    {
                        if (turn == 1)  // Card added in FirstDeck after Hit
                        {
                            string CardSuit = jsonNode["card"]["suit"].Value;
                            int CardID = Constance.GetCardIndex(jsonNode["card"]["value"]);

                            GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
                            Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);

                            TotalCardNumber.transform.parent.gameObject.SetActive(true);
                            TotalCardNumber.text = jsonNode["score"];
                        }
                        else if (turn == 2) // Card added in SecondDeck after Hit
                        {
                            string CardSuit = jsonNode["card"]["suit"].Value;
                            int CardID = Constance.GetCardIndex(jsonNode["card"]["value"]);

                            GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(1).GetChild(0).transform);
                            Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);

                            PP_TotalCardNumber.transform.parent.gameObject.SetActive(true);
                            PP_TotalCardNumber.text = jsonNode["score"];
                        }
                    }

                    if (jsonNode["isBurst"] == true)    // when cardScore exceeds 21
                    {
                        BlackJackGameManager.Instance.TurnOffOptionButtons();
                        TimerImage.enabled = false;
                    }

                    if (jsonNode["isStand"] == true)
                    {
                        TimerImage.enabled = false;
                    }
                }
            }
            else if (jsonNode["isSplit"] == 1)  // Display Cards with Split ---> playerSelectAction 2
            {
                if (jsonNode["playerId"].Value == Constants.PLAYER_ID)   // Update the Balance after Bet
                {
                    GamePlayManager.instance.BalanceText.text = "$" + jsonNode["playerAmount"].Value;
                    GamePlayManager.instance.balanceAmount = jsonNode["playerAmount"];
                }

                if (playerId == jsonNode["playerId"])
                {
                    Debug.LogWarning("CardSplit");
                    GamePlayManager.instance.PP_BetButton.SetActive(false);

                    PlayerOptionText.text = jsonNode["selectOption"];

                    for (int i = 0; i < CardParent.transform.childCount; i++)
                    {
                        Destroy(CardParent.transform.GetChild(i).gameObject);
                    }

                    // FirstDeck ========>
                    JSONNode firstDeck = jsonNode["fisrtDeck"];
                    for (int i = 0; i < firstDeck["playerHand"].Count; i++)
                    {
                        Debug.LogWarning("FirstDeck " + i + " Card " + firstDeck["playerHand"][i].ToString());

                        string CardSuit = firstDeck["playerHand"][i]["suit"].Value;
                        int CardID = Constance.GetCardIndex(firstDeck["playerHand"][i]["value"]);

                        GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(0).GetChild(0).transform);
                        Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);
                    }
                    TotalCardNumber.transform.parent.gameObject.SetActive(true);
                    TotalCardNumber.text = firstDeck["score"];

                    // SecondDeck =======>
                    JSONNode secondDeck = jsonNode["secondDeck"];
                    for (int i = 0; i < secondDeck["playerHand"].Count; i++)
                    {
                        Debug.LogWarning("SecondDeck " + i + " Card " + secondDeck["playerHand"][i].ToString());

                        string CardSuit = secondDeck["playerHand"][i]["suit"].Value;
                        int CardID = Constance.GetCardIndex(secondDeck["playerHand"][i]["value"]);

                        GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardDataObject.transform.GetChild(1).GetChild(0).transform);
                        Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);
                    }
                    PP_TotalCardNumber.transform.parent.gameObject.SetActive(true);
                    PP_TotalCardNumber.text = secondDeck["score"];

                    CardDataObject.transform.GetChild(1).gameObject.SetActive(true);    // PP_CardParent Activate
                    CardDataObject.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
                }
            }
        }



        public void DoubleButtonClick()
        {
            //if (TablePostion != 3)
            //    return;

            //PlayerCardDisplay(0);
            BetAmounts *= 2;
            SetYourBet(BetAmounts);
        }

        public void PlayerLeftBlackJack()
        {
            //Debug.Log("PlayerLeftBlackJack");
            if (Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                InvitePlayer.SetActive(false);
                StandPlayer.SetActive(true);
            }
            else if (!Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                InvitePlayer.SetActive(true);
                StandPlayer.SetActive(false);
            }
            ResetPlayerData();
            if (playerId == Constants.PLAYER_ID)
            {
                UserInfoUpdate();
            }
        }

        public void ResetPlayerData()  // Reset Player Data at First Time Joining
        {
            //Debug.Log("ResetBlackJackPlayerData");
            isPlayerBet = false;
            playerId = "";
            PlayerName.text = "";
            PlayerOptionText.text = "";
            BetAmount.text = "";
            ChipsObject.SetActive(false);
            PP_BetAmount.text = "";
            PP_ChipsObject.SetActive(false);
        }

        private void ResetPlayerOnGameRestart(JSONNode jsonNode)
        {
            if (jsonNode["status"] == true)
            {
                //Debug.LogWarning("GameRestarted " + jsonNode.ToString());
                isPlayerBet = false;
                TotalCardNumber.transform.parent.gameObject.SetActive(false);
                BetAmount.text = "";
                ChipsObject.SetActive(false);
                PP_BetAmount.text = "";
                PP_ChipsObject.SetActive(false);
                PlayerOptionText.text = "";

                WinImage.SetActive(false);
                LoseImage.SetActive(false);
                PushImage.SetActive(false);

                //for (int i = 0; i < CardParent.transform.childCount; i++)
                //{
                //    Player player = CardParent.transform.GetChild(i).GetComponent<Player>();
                //    //player.CardParent
                //}

                //for (int i = 0; i < CardParent.transform.childCount; i++)
                //{
                //    Destroy(CardParent.transform.GetChild(i).gameObject);
                //}

                if (playerId == Constants.PLAYER_ID)
                {
                    UserInfoUpdate();
                    //GamePlayManager.instance.BetButton.interactable = true;
                }

                if (PP_CardParent.activeInHierarchy)
                {
                    Debug.LogWarning("PPReset");
                    //for (int i = 0; i < PP_CardParent.transform.childCount; i++)
                    //{
                    //    Destroy(PP_CardParent.transform.GetChild(i).gameObject);
                    //}

                    PP_TotalCardNumber.transform.parent.gameObject.SetActive(false);

                    PP_WinImage.SetActive(false);
                    PP_LoseImage.SetActive(false);
                    PP_PushImage.SetActive(false);
                }
            }
        }

        private void PlayerStandUpReset(JSONNode jsonNode)
        {
            if (playerId == jsonNode["playerId"] && playerId == Constants.PLAYER_ID)
            {
                Constants.isJoinByStandUp = true;
                Debug.Log("ConstatPlayerID " + Constants.PLAYER_ID);

                for (int i = 0; i < BlackJackGameManager.Instance.PlayersParent.transform.childCount; i++)
                {
                    Player _bjPlayer = BlackJackGameManager.Instance.PlayersParent.transform.GetChild(i).GetComponent<Player>();

                    if (_bjPlayer.playerId == "")
                    {
                        _bjPlayer.ActivePlayer.SetActive(false);
                        _bjPlayer.StandPlayer.SetActive(true);
                        _bjPlayer.InvitePlayer.SetActive(false);
                        Debug.Log("PokerPlayerID " + _bjPlayer.playerId);
                    }
                }

                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
                playerId = "";
                PlayerName.text = "";
                //IsPlayerInTable = false;
                ResetPlayerOnGameRestart(jsonNode);
                //ResetPlayerData();
            }
            else if (playerId == jsonNode["playerId"] && playerId != Constants.PLAYER_ID && !Constants.isJoinByStandUp) // Todo Set Bool to By Pass this when Own Player Stand Up
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(false);
                InvitePlayer.SetActive(true);
                playerId = "";
                PlayerName.text = "";
                //IsPlayerInTable = false;
                ResetPlayerOnGameRestart(jsonNode);
                //ResetPlayerData();
                Debug.Log("playerID==playerID");
            }
            else if (playerId == jsonNode["playerId"] && playerId != Constants.PLAYER_ID && Constants.isJoinByStandUp)
            {
                ActivePlayer.SetActive(false);
                StandPlayer.SetActive(true);
                InvitePlayer.SetActive(false);
                playerId = "";
                PlayerName.text = "";
                //IsPlayerInTable = false;
                ResetPlayerOnGameRestart(jsonNode);
                //ResetPlayerData();
                Debug.Log("playerID==playerID");
            }

            Constants.blackJackTimer = jsonNode["timer"];
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
                    //Debug.LogError("UserInfoJSON: " + jsonNode.ToString());
                }
                else
                    Constants.Logout();
            }));
        }

        public void FriendInvitePanelButton()
        {
            //BlackJackGameManager.Instance.FriendsPanel.SetActive(true);
            if (FacebookManager.Instance != null)
                FacebookManager.Instance.InitCallback();
            MainNetworkManager.Instance.OnSendingFriendList();
        }

        private void OnTimerChange(JSONNode jsonNode)
        {
            if (!isPlayerBet)
            {
                GamePlayManager.instance.BetButton.interactable = true;
                TimerImage.enabled = true;
                TimerImage.fillAmount = jsonNode["timer"].AsFloat / jsonNode["totalTime"].AsFloat;

                if (jsonNode["status"] == false)
                {
                    GamePlayManager.instance.BetButton.interactable = false;
                    GamePlayManager.instance.BetButtonOnSlider.interactable = false;
                }
            }
        }

        private void OnPlayerTimerChange(JSONNode jsonNode)
        {
            if (playerId == jsonNode["playerId"])
            {
                TimerImage.enabled = true;
                TimerImage.fillAmount = jsonNode["timer"].AsFloat / jsonNode["totalTime"].AsFloat;
            }
        }

        private void DisplayResultImage(JSONNode jsonNode)
        {
            StartCoroutine(ResultDisplayAnimation(jsonNode));
        }

        IEnumerator ResultDisplayAnimation(JSONNode jsonNode)       //Todo Restart Event 4 Sec Dealay from Backend , PP_ChipObject Display when Split
        {
            if (playerId == jsonNode["playerId"].Value)
            {
                string result = jsonNode["message"].Value;
                Debug.Log("DisplayResult " + jsonNode["message"].Value);
                if (jsonNode["turn"] == 1)
                {
                    if (result == "Win")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < CardParent.transform.childCount; i++)
                        {
                            Destroy(CardParent.transform.GetChild(i).gameObject);
                        }
                        TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        WinImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        ChipsObject.SetActive(false);
                        BetAmount.text = "";
                        ChipsAnimation(GamePlayManager.instance.DealerChips.transform, ResultObject.transform, jsonNode["winamount"].Value);
                    }
                    else if (result == "Lost")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < CardParent.transform.childCount; i++)
                        {
                            Destroy(CardParent.transform.GetChild(i).gameObject);
                        }
                        TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        LoseImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        ChipsObject.SetActive(false);
                        BetAmount.text = "";
                        ChipsAnimation(ResultObject.transform, GamePlayManager.instance.DealerChips.transform, jsonNode["winamount"].Value);
                    }
                    else if (result == "Push")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < CardParent.transform.childCount; i++)
                        {
                            Destroy(CardParent.transform.GetChild(i).gameObject);
                        }
                        TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        PushImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        ChipsObject.SetActive(false);
                        BetAmount.text = "";
                        ChipsAnimation(ResultObject.transform, ProfilePicObject.transform, jsonNode["winamount"].Value);
                    }
                }
                else if (jsonNode["turn"] == 2)
                {
                    if (result == "Win")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < PP_CardParent.transform.childCount; i++)
                        {
                            Destroy(PP_CardParent.transform.GetChild(i).gameObject);
                        }
                        PP_TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, PP_CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(PP_CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        PP_WinImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        PP_ChipsObject.SetActive(false);
                        PP_BetAmount.text = "";
                        ChipsAnimation(GamePlayManager.instance.DealerChips.transform, PP_ResultObject.transform, jsonNode["winamount"].Value);
                    }
                    else if (result == "Lost")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < PP_CardParent.transform.childCount; i++)
                        {
                            Destroy(PP_CardParent.transform.GetChild(i).gameObject);
                        }
                        PP_TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, PP_CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(PP_CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        PP_LoseImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        PP_ChipsObject.SetActive(false);
                        PP_BetAmount.text = "";
                        ChipsAnimation(PP_ResultObject.transform, GamePlayManager.instance.DealerChips.transform, jsonNode["winamount"].Value);
                    }
                    else if (result == "Push")
                    {
                        yield return new WaitForSeconds(2f);
                        for (int i = 0; i < PP_CardParent.transform.childCount; i++)
                        {
                            Destroy(PP_CardParent.transform.GetChild(i).gameObject);
                        }
                        PP_TotalCardNumber.transform.parent.gameObject.SetActive(false);
                        RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, PP_CardParent.transform).GetComponent<RectTransform>();
                        yield return new WaitForSeconds(1f);
                        Destroy(r.gameObject);
                        CardDiscardAnimation(PP_CardParent.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
                        yield return new WaitForSeconds(1f);
                        PP_PushImage.SetActive(true);
                        yield return new WaitForSeconds(1f);
                        PP_ChipsObject.SetActive(false);
                        PP_BetAmount.text = "";
                        ChipsAnimation(PP_ResultObject.transform, ProfilePicObject.transform, jsonNode["winamount"].Value);
                    }
                }
            }

            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)   // Update the Balance after Bet
            {
                GamePlayManager.instance.WinText.text = "$" + jsonNode["winamount"].Value;
                GamePlayManager.instance.BalanceText.text = "$" + jsonNode["playerAmount"].Value;
                GamePlayManager.instance.balanceAmount = jsonNode["playerAmount"];
            }
        }

        private void ChipsAnimation(Transform startPoint, Transform endPoint, string Amount)
        {
            Debug.Log("ChipsAnimation");
            RectTransform r = Instantiate(GamePlayManager.instance.WinLoseChipsPrefab, startPoint).GetComponent<RectTransform>();
            r.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = Amount;
            r.DOMove(endPoint.position, 2f).OnComplete(() =>
            {
                Destroy(r.gameObject);
                ResetPlayer();
            }
            );
        }

        private void ResetPlayer()
        {
            isPlayerBet = false;
            TotalCardNumber.transform.parent.gameObject.SetActive(false);
            BetAmount.text = "";
            ChipsObject.SetActive(false);
            PP_BetAmount.text = "";
            PP_ChipsObject.SetActive(false);
            PlayerOptionText.text = "";

            WinImage.SetActive(false);
            LoseImage.SetActive(false);
            PushImage.SetActive(false);

            //for (int i = 0; i < CardParent.transform.childCount; i++)
            //{
            //    Player player = CardParent.transform.GetChild(i).GetComponent<Player>();
            //    //player.CardParent
            //}

            //for (int i = 0; i < CardParent.transform.childCount; i++)
            //{
            //    Destroy(CardParent.transform.GetChild(i).gameObject);
            //}

            if (playerId == Constants.PLAYER_ID)
            {
                UserInfoUpdate();
                //GamePlayManager.instance.BetButton.interactable = true;
            }

            if (PP_CardParent.activeInHierarchy)
            {
                Debug.LogWarning("PPReset");
                for (int i = 0; i < PP_CardParent.transform.childCount; i++)
                {
                    Destroy(PP_CardParent.transform.GetChild(i).gameObject);
                }

                PP_TotalCardNumber.transform.parent.gameObject.SetActive(false);

                PP_WinImage.SetActive(false);
                PP_LoseImage.SetActive(false);
                PP_PushImage.SetActive(false);
            }
        }

        private void CardDiscardAnimation(Transform startPoint, Transform endPoint)
        {
            Debug.Log("ChipsAnimation");
            RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, startPoint).GetComponent<RectTransform>();
            r.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            r.DOMove(endPoint.position, 1f).OnComplete(() =>
            {
                Destroy(r.gameObject);
            }
            );
        }

        public void StandPlayerButtonClick()
        {
            Debug.Log("Player Stand");
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            JSONNode jsonnode = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["maxAmount"] = GamePlayManager.instance.balanceAmount,
                ["roomName"] = Constants.RoomName,
                ["timer"] = Constants.blackJackTimer
            };

            Debug.LogWarning("StandPlayer======= " + jsonnode.ToString());
            Constants.isJoinByStandUp = false;
            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("createJoinPublicRoom", jsonnode.ToString());
        }

        public void GiftButtonClick()
        {
            Debug.Log("GiftButtonClickBJ");
            MainNetworkManager.Instance.MainSocket?.Emit("tableItems");
            Constants.BlackJackGiftReceiverID = playerId;
            Constants.BlackJackGiftSenderID = Constants.PLAYER_ID;
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
    }
}