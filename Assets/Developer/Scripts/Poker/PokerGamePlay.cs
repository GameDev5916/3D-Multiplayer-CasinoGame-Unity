using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using LitJson;

namespace Casino_Poker
{
    public class PokerGamePlay : MonoBehaviour
    {
        public static PokerGamePlay Instance;

        [SerializeField] public GameObject MiddelCards;
        [SerializeField] private GameObject Slider;

        [SerializeField] private Slider Slider_Amount;

        [SerializeField] private TextMeshProUGUI SlideAmountText;
        [SerializeField] private TextMeshProUGUI MinBetAmount;
        [SerializeField] private TextMeshProUGUI TotalBetInEndOfRound;
        public Text TotalChips;

        public TextMeshProUGUI Message;
        public GameObject TipPrefab;
        public Transform DealerSit;
        public GameObject GiftObjectPrefab;

        [Header("Buttons")]
        [SerializeField] private Button CheckButton;
        [SerializeField] private Button FoldButton;
        [SerializeField] private Button CallButton;
        [SerializeField] private Button RaiseButton;
        [SerializeField] private Button ConfirmButton;
        [SerializeField] private Button AllinButton;


        [Header("PreSelectionButtons")]
        [SerializeField] private Button CheckSelectButton;
        [SerializeField] private Button CallSelectButton;
        [SerializeField] private Button FoldSelectButton;
        [SerializeField] private Button AllinSelectButton;


        [Header("Color ForSelectOptions")]
        public Color Call;
        public Color Fold;
        public Color Check;
        public Color Raise;
        public Color Allin;
        public Color Default;

        [Header("Game Win")]
        [SerializeField] private GameObject WinerPanel;
        [SerializeField] private TextMeshProUGUI WinnerName;
        [SerializeField] private TextMeshProUGUI HandName;
        [SerializeField] private GameObject HandNameBg;

        public static Action ONRoundComplete;

        public static long SmallBlindAmount;

        public List<long> RaiseAmountOfSider;

        private long MaxBetAmount;

        public bool isCheckSelected = false, isCallSelected = false, isAllInSelected = false, isFoldSelected = false;
        public Sprite selectButtonSprite, deselectButtonSprite;
        public Transform selectionButtonParent;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);
        }


        private void OnEnable()
        {
            DisableAllButton();

            TotalChips.text = Constants.CHIPS.ToString();

            NetworkManager_Poker.PlayerOption += GetPlayerOption;
            NetworkManager_Poker.GameCardDistriBution += AddMiddelCards;
            NetworkManager_Poker.PlayerTurn += PlayerTurnChange;
            NetworkManager_Poker.TotalBetAmount += SetTotalBetAmount;
            NetworkManager_Poker.Gamewin += ShowWiningCards;
            NetworkManager_Poker.GameRestart += GameRestart;
            NetworkManager_Poker.TipDealer += DealerTipAnimation;
            NetworkManager_Poker.PokerSendGiftAction += GiftSendingAnimation;

            Debug.Log("PlayersParent");
            for (int i = 0; i < 5; i++)
            {
                PokerPlayer player = GameManager_Poker.Instance.PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                player.ResetPlayer();
                player.ActivePlayer.SetActive(false);
                player.InvitePlayer.SetActive(false);
                player.StandPlayer.SetActive(true);
            }
        }

        private void OnDisable()
        {
            NetworkManager_Poker.PlayerOption -= GetPlayerOption;
            NetworkManager_Poker.GameCardDistriBution -= AddMiddelCards;
            NetworkManager_Poker.PlayerTurn -= PlayerTurnChange;
            NetworkManager_Poker.TotalBetAmount -= SetTotalBetAmount;
            NetworkManager_Poker.Gamewin -= ShowWiningCards;
            NetworkManager_Poker.GameRestart -= GameRestart;
            NetworkManager_Poker.TipDealer -= DealerTipAnimation;
            NetworkManager_Poker.PokerSendGiftAction -= GiftSendingAnimation;
        }

        private void GetPlayerOption(JSONNode jsonNode)
        {
            //FoldButton.enabled = jsonNode["fold"].AsBool;
            //RaiseButton.enabled = jsonNode["raise"].AsBool;
            //AllinButton.enabled = jsonNode["allin"].AsBool;
            Message.gameObject.SetActive(false);
            ResetSelectionButtons();
            FoldButton.gameObject.SetActive(jsonNode["fold"].AsBool);
            RaiseButton.gameObject.SetActive(jsonNode["raise"].AsBool);
            AllinButton.gameObject.SetActive(jsonNode["allin"].AsBool);

            Debug.LogError($"minRaiseBet: {jsonNode["minRaiseBet"].AsLong}  maxRaiseBet: {jsonNode["maxRaiseBet"].AsLong}");

            if (jsonNode["raise"].AsBool)
            {
                SetRaiseAmountSlider(jsonNode["minRaiseBet"].AsLong, jsonNode["maxRaiseBet"].AsLong);
            }

            MaxBetAmount = jsonNode["maxRaiseBet"].AsLong;

            Debug.LogError(Slider_Amount.minValue);
            Debug.LogError(Slider_Amount.maxValue);

            if (jsonNode["call"].AsBool)
            {
                CallButton.enabled = true;
                CallButton.gameObject.SetActive(true);
                CheckButton.gameObject.SetActive(false);
            }
            else if (jsonNode["check"].AsBool)
            {
                CheckButton.enabled = true;
                CheckButton.gameObject.SetActive(true);
                CallButton.gameObject.SetActive(false);
            }
        }

        public void ResetSelectionButtons()
        {
            for (int i = 0; i < selectionButtonParent.childCount; i++)
            {
                selectionButtonParent.GetChild(i).GetComponent<Image>().sprite = deselectButtonSprite;
            }
            AllBoolFalse();
            selectionButtonParent.gameObject.SetActive(false);
        }

        public void CheckButtonClick()
        {
            SendPlayerDecision(0, "check");
        }

        public void CallButtonClick()
        {
            SendPlayerDecision(0, "call");
        }

        public void RaiseButtonClick()
        {
            RaiseButton.gameObject.SetActive(false);
            Slider_Amount.value = 0;
            SliderValueChange();
            Slider.SetActive(true);
            ConfirmButton.gameObject.SetActive(true);
        }

        public void FoldButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            SendPlayerDecision(0, "fold");
        }

        public void ConfirmBetButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            RaiseButton.gameObject.SetActive(true);
            Slider.SetActive(false);
            ConfirmButton.gameObject.SetActive(false);
            SendPlayerDecision(RaiseAmountOfSider[(int)Slider_Amount.value], "raise");
        }

        public void AllinButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            SendPlayerDecision(0, "allin");
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.J))
        //    {
        //        SmallBlindAmount = 500;
        //        SetRaiseAmountSlider(2000,4000);
        //    }
        //}


        public void CheckSelectButtonClick()
        {
            if (isCheckSelected)
            {
                AllBoolFalse();
                isCheckSelected = false;
            }
            else if (!isCheckSelected)
            {
                AllBoolFalse();
                isCheckSelected = true;
            }

            ChangeSelectionSprite(CheckSelectButton, isCheckSelected);
            SendPlayerOptionSelection("check", isCheckSelected);
        }

        public void CallSelectButtonClick()
        {
            if (isCallSelected)
            {
                AllBoolFalse();
                isCallSelected = false;
            }
            else if (!isCallSelected)
            {
                AllBoolFalse();
                isCallSelected = true;
            }

            ChangeSelectionSprite(CallSelectButton, isCallSelected);
            SendPlayerOptionSelection("call", isCallSelected);
        }

        public void FoldSelectButtonClick()
        {
            if (isFoldSelected)
            {
                AllBoolFalse();
                isFoldSelected = false;
            }
            else if (!isFoldSelected)
            {
                AllBoolFalse();
                isFoldSelected = true;
            }

            ChangeSelectionSprite(FoldSelectButton, isFoldSelected);
            SendPlayerOptionSelection("fold", isFoldSelected);
        }

        public void AllInSelectButtonClick()
        {
            if (isAllInSelected)
            {
                AllBoolFalse();
                isAllInSelected = false;
            }
            else if (!isAllInSelected)
            {
                AllBoolFalse();
                isAllInSelected = true;
            }

            ChangeSelectionSprite(AllinSelectButton, isAllInSelected);
            SendPlayerOptionSelection("allin", isAllInSelected);
        }



        private void ChangeSelectionSprite(Button button, bool isSelected)
        {
            for (int i = 0; i < selectionButtonParent.childCount; i++)
            {
                selectionButtonParent.GetChild(i).GetComponent<Image>().sprite = deselectButtonSprite;
            }

            if (isSelected)
            {
                button.GetComponent<Image>().sprite = selectButtonSprite;
            }
            //else
            //{
            //    button.GetComponent<Image>().sprite = deselectButtonSprite;

            //}
        }

        private void AllBoolFalse()
        {
            isCallSelected = false;
            isCheckSelected = false;
            isFoldSelected = false;
            isAllInSelected = false;
        }

        private void SetRaiseAmountSlider(long min, long max)
        {
            RaiseAmountOfSider.Clear();

            Slider_Amount.maxValue = ((max - min) / SmallBlindAmount);

            for (int i = 0; i < Slider_Amount.maxValue; i++)
            {
                RaiseAmountOfSider.Add(min + (SmallBlindAmount * i));
            }

            Slider_Amount.value = 0;
            SliderValueChange();
        }

        public void SliderValueChange()
        {
            //SlideAmountText.text = Constants.NumberShow((long)Slider_Amount.value);

            if (Slider_Amount.value == Slider_Amount.maxValue)
            {
                SlideAmountText.text = "All In";
                AllinButton.gameObject.SetActive(true);
                ConfirmButton.gameObject.SetActive(false);
                RaiseButton.gameObject.SetActive(false);
            }
            else
            {
                SlideAmountText.text = Constants.NumberShow(RaiseAmountOfSider[(int)Slider_Amount.value]);

                if (RaiseButton.gameObject.activeInHierarchy)
                    return;

                AllinButton.gameObject.SetActive(false);
                ConfirmButton.gameObject.SetActive(true);
            }
        }

        public void DisableAllButton()
        {
            //CallButton.interactable = CheckButton.interactable = RaiseButton.interactable = FoldButton.interactable = ConfirmButton.interactable = AllinButton.interactable = false;
            CallButton.gameObject.SetActive(false);
            CheckButton.gameObject.SetActive(false);
            RaiseButton.gameObject.SetActive(false);
            FoldButton.gameObject.SetActive(false);
            ConfirmButton.gameObject.SetActive(false);
            AllinButton.gameObject.SetActive(false);
            Slider.SetActive(false);
            //Debug.LogError("Disable Buttons");
        }

        public void EnableAllButton()
        {
            //CallButton.interactable = CheckButton.interactable = RaiseButton.interactable = FoldButton.interactable = ConfirmButton.interactable = AllinButton.interactable = true;
            CallButton.gameObject.SetActive(true);
            CheckButton.gameObject.SetActive(true);
            RaiseButton.gameObject.SetActive(true);
            FoldButton.gameObject.SetActive(true);
            ConfirmButton.gameObject.SetActive(true);
            AllinButton.gameObject.SetActive(true);
            //Debug.LogError("Enable Buttons");
        }

        private void SendPlayerDecision(long raiseberAmount, string option)
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["raiseBetAmount"] = raiseberAmount,
                ["playerOption"] = option,
            };
            Debug.LogError(data.ToString());
            NetworkManager_Poker.Instance.PokerSocket?.Emit("playRound", data.ToString());
            Debug.Log("playRound::::::::::::");
            Debug.Log(Constants.RoomName);
            DisableAllButton();
            Message.gameObject.SetActive(false);
            selectionButtonParent.gameObject.SetActive(true);
        }

        private void SendPlayerOptionSelection(string optionSelected, bool isSelected)
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["selectOption"] = optionSelected,
                ["selectAction"] = isSelected,
            };

            Debug.LogWarning(data.ToString());
            NetworkManager_Poker.Instance.PokerSocket?.Emit("selectOption", data.ToString());
        }

        public void AddMiddelCards(JSONNode jsonNode)
        {
            //for (int i = 0; i < jsonNode.Count; i++)
            //{
            //    //JSONNode cardData = jsonNode[i]["cards"];
            //    CardScript card = Instantiate(GameManager_Poker.Instance.Card, MiddelCards.transform).GetComponent<CardScript>();
            //    card.SetDataOnCard(jsonNode[i]["suits"].Value, jsonNode[i]["value"].AsInt - 1);
            //}
        }

        public void ResetAllData()
        {
            for (int i = 0; i < GameManager_Poker.Instance.PlayersParent.transform.childCount; i++)
            {
                PokerPlayer _pokerPlayer = GameManager_Poker.Instance.PlayersParent.transform.GetChild(i).GetComponent<PokerPlayer>();
                _pokerPlayer.ResetPlayer();
            }

            for (int i = 0; i < MiddelCards.transform.childCount; i++)
            {
                Destroy(MiddelCards.transform.GetChild(i).gameObject);
            }

            TotalBetInEndOfRound.text = "0";
            HandNameBg.SetActive(false);
        }

        private void PlayerTurnChange(JSONNode jsonNode)
        {
            Debug.LogError("Hello::::::::: " + jsonNode["playerId"].Value + " : " + Constants.PLAYER_ID + (jsonNode["playerId"].Value == Constants.PLAYER_ID));
            if (jsonNode["playerId"].Value == Constants.PLAYER_ID)
            {
                //EnableAllButton();
            }
            else
            {
                DisableAllButton();
                Debug.LogWarning("TurnChange");
                ResetSelectionButtons();
                selectionButtonParent.gameObject.SetActive(true);
            }
        }

        public void ShowWinner(string name)
        {
            WinerPanel.SetActive(true);
            WinnerName.text = name;
            Invoke(nameof(DisableWinnerPanle), 2);
        }

        public void DisableWinnerPanle()
        {
            WinerPanel.SetActive(false);
        }

        private void SetTotalBetAmount(JSONNode jsonNode)
        {
            TotalBetInEndOfRound.text = Constants.NumberShow(jsonNode["tablePortAmount"].AsLong);
            ONRoundComplete?.Invoke();
        }

        public void ShowWiningCards(JSONNode jsonNode)
        {
            JSONNode json = jsonNode["dealerCard"];
            Debug.LogError(json.ToString());
            for (int i = 0; i < MiddelCards.transform.childCount; i++)
            {
                CardScript cs = MiddelCards.transform.GetChild(i).GetComponent<CardScript>();
                for (int j = 0; j < json.Count; j++)
                {
                    int cardId = json[j]["value"].AsInt;
                    string cardSuits = json[j]["suits"].Value;
                    if (cs.CardId == cardId && cs.CardSuits == cardSuits)
                    {
                        //MiddelCards.transform.GetChild(i).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                        cs.DarkImage.enabled = false;
                        break;
                    }
                    else
                    {
                        cs.EnableDarkImage();
                    }
                }
            }

            ShowWinHandName(jsonNode["handInfo"]["type"].ToString());
        }

        public void ShowWinHandName(string name)
        {
            HandName.text = name;
            HandNameBg.SetActive(true);
        }

        private void GameRestart(JSONNode jsonNode)
        {
            ResetAllData();
            Debug.Log("GameRestart");
            ResetSelectionButtons();
            if (jsonNode["status"].Value != "True")
            {
                Message.gameObject.SetActive(true);
                Message.text = jsonNode["message"].Value;
            }
            else
                Message.gameObject.SetActive(false);
        }

        public void ShopButtonClick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            HomeScreenUIManager.Instance.ShopPanel.SetActive(true);
        }

        public void TipToDealerButton()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
            };

            NetworkManager_Poker.Instance.PokerSocket?.Emit("tipToDealer", data.ToString());
            //UpdatePokerChips();
            //DealerTipAnimation(data);
        }

        public void UpdatePokerChips()
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
                    TotalChips.text = Constants.NumberShow(Constants.CHIPS);
                    Debug.LogError("PokerChipsUpdatedFromGameplay " + jsonNode.ToString());
                }
                else
                    Constants.Logout();
            }));
        }

        private void DealerTipAnimation(JSONNode jsonNode)
        {
            Transform p = GameManager_Poker.Instance.PlayersParent.transform;
            Debug.Log("AfterDealerTip");
            for (int i = 0; i < p.childCount; i++)
            {
                //Debug.Log("TipPlayerID: " + jsonNode["playerId"].Value);
                if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == "" ||
                p.GetChild(i).GetComponent<PokerPlayer>().playerId != jsonNode["playerId"].Value)
                    continue;

                p.GetChild(i).GetComponent<PokerPlayer>().ShowAndSetTotalAmount(jsonNode["playerAmount"]);
                TipAnimation(p.GetChild(i).GetComponent<PokerPlayer>(), jsonNode["tip"].Value);
            }
        }

        private void TipAnimation(PokerPlayer pokerPlayer, string tipAmount)
        {
            Debug.Log("TipAnimation");
            RectTransform r = Instantiate(TipPrefab, pokerPlayer.transform).GetComponent<RectTransform>();
            r.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tipAmount;
            r.DOMove(DealerSit.position, 1.5f).OnComplete(() =>
            {
                Destroy(r.gameObject);
            }
            );
        }

        Transform giftSpawnPosition;
        Transform giftEndPosition;
        private void GiftSendingAnimation(JSONNode jsonNode)
        {
            if (jsonNode["status"] == false)
            {
                Constants.ShowWarning(jsonNode["message"].Value);
            }
            else if (jsonNode["gitAction"] == "all")
            {
                Debug.Log("GiftSendToAll");
                Transform p = GameManager_Poker.Instance.PlayersParent.transform;

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == "")
                        continue;

                    for (int j = 0; j < p.childCount; j++)
                    {
                        if (p.GetChild(j).GetComponent<PokerPlayer>().playerId == jsonNode["senderId"])
                        {
                            giftSpawnPosition = p.GetChild(j).GetComponent<PokerPlayer>().PlayerImage.transform;
                            p.GetChild(j).GetComponent<PokerPlayer>().ShowAndSetTotalAmount(jsonNode["playerAmount"]);
                        }
                    }


                    PokerPlayer pokerPlayer = p.GetChild(i).GetComponent<PokerPlayer>();
                    Sprite giftSprite = GameManager_Poker.Instance.GetSprite(jsonNode["data"]["itemname"]);

                    RectTransform r = Instantiate(GiftObjectPrefab, giftSpawnPosition).GetComponent<RectTransform>();
                    r.transform.SetAsFirstSibling();
                    pokerPlayer.giftIconImage.SetActive(false);
                    r.transform.GetComponent<Image>().sprite = giftSprite;
                    r.DOMove(pokerPlayer.GiftButton.position, 1.8f).OnComplete(() =>
                    {
                        Destroy(r.gameObject);
                        pokerPlayer.giftItemImage.SetActive(true);
                        pokerPlayer.giftItemImage.GetComponent<Image>().sprite = giftSprite;
                    });
                }
            }
            else if (jsonNode["gitAction"] == "own")
            {
                Debug.Log("GiftSendToPlayer");
                Transform p = GameManager_Poker.Instance.PlayersParent.transform;

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == "")
                        continue;

                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == jsonNode["senderId"])
                    {
                        giftSpawnPosition = p.GetChild(i).GetComponent<PokerPlayer>().PlayerImage.transform;
                        p.GetChild(i).GetComponent<PokerPlayer>().ShowAndSetTotalAmount(jsonNode["playerAmount"]);
                    }
                }

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == "")
                        continue;

                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == jsonNode["receiverId"])
                    {
                        giftEndPosition = p.GetChild(i).GetComponent<PokerPlayer>().gameObject.transform;
                    }
                }

                Sprite giftSprite = GameManager_Poker.Instance.GetSprite(jsonNode["data"]["itemname"]);
                GiftSendAnimation(giftSpawnPosition, giftEndPosition, giftSprite);
            }
        }

        private void GiftSendAnimation(Transform spawnPosition, Transform endPosition, Sprite giftSprite)
        {
            //Debug.Log("GiftAnimation");

            PokerPlayer pokerPlayer = endPosition.GetComponent<PokerPlayer>();

            RectTransform r = Instantiate(GiftObjectPrefab, spawnPosition).GetComponent<RectTransform>();
            pokerPlayer.giftIconImage.SetActive(false);
            r.transform.GetComponent<Image>().sprite = giftSprite;
            r.DOMove(pokerPlayer.GiftButton.position, 1.8f).OnComplete(() =>
                {
                    Destroy(r.gameObject);
                    pokerPlayer.giftItemImage.SetActive(true);
                    pokerPlayer.giftItemImage.GetComponent<Image>().sprite = giftSprite;
                });
        }
    }
}