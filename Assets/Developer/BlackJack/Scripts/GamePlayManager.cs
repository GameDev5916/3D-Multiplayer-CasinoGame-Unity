using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using Casino_Poker;
using DG.Tweening;

namespace BalckJack
{
    public class GamePlayManager : MonoBehaviour
    {
        #region Singleton
        public static GamePlayManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        [Header("Amounts Text")]
        public Text BalanceText;
        public Text WinText;
        public Text BetText;
        public Text PP_BetText;       // PP - Perfect Pair

        [Header("Min/Max Amounts Text")]
        public Text MinBetAmount;
        public Text MinBetAmoutFor_PP;
        public Text MaxBetAmount, MaxBetAmoutFor_PP;

        [Header("Sliders")]
        public Slider BetAmountslider;
        public Slider PP_BetAmountslider;

        [Header("GameObject")]
        public GameObject BetAmountsliderGameObject;
        public GameObject PP_BetAmountsliderGameObject;
        public GameObject PP_BetButton;

        [Header("Buttons")]
        public Button HitButton;
        public Button StandButton;
        public Button DoubleButton;
        public Button SplitButton;
        public Button BetButton;
        public Button BetButtonOnSlider;

        [Space]
        public GameObject DealerChips;
        public GameObject WinLoseChipsPrefab;
        public GameObject GiftObjectPrefab;

        public bool onetime;
        int stepSize = 100;
        public long balanceAmount;

        public static Action<long> BetAmount;
        public static Action<long> PP_BetAmount;
        public static Action<JSONNode> DisplayPlayerCard;

        private void OnEnable()
        {
            SetAmoutDataInUI();
            BlackJack_NetworkManager.BlackJackSendGiftAction += GiftSendingAnimation;
        }

        private void OnDisable()
        {
            BlackJack_NetworkManager.BlackJackSendGiftAction -= GiftSendingAnimation;
            onetime = false;
            BetAmountslider.value = 0;
            PP_BetAmountslider.value = 0;
            BetAmountsliderGameObject.SetActive(false);
            PP_BetAmountsliderGameObject.SetActive(false);
        }

        public void SetAmoutDataInUI()
        {
            BetText.text = $"Min { Constance.AmountShow(Constance.Min_BetAmoutForBlackJack)}";

            MinBetAmount.text = $"Min { Constance.AmountShow(Constance.Min_BetAmoutForBlackJack)}";
            MinBetAmoutFor_PP.text = $"Min { Constance.AmountShow(Constance.Min_BetAmoutForBlackJack)}";
            MaxBetAmount.text = $"Max { Constance.AmountShow(Constance.Max_BetAmoutForBlackJack)}";
            MaxBetAmoutFor_PP.text = $"Max { Constance.AmountShow(Constance.Max_BetAmoutForBlackJack)}";

            BetAmountslider.minValue = Constance.Min_BetAmoutForBlackJack;
            PP_BetAmountslider.minValue = Constance.Min_BetAmoutForBlackJack;

            BetAmountslider.maxValue = Constance.Max_BetAmoutForBlackJack;
            PP_BetAmountslider.maxValue = Constance.Max_BetAmoutForBlackJack;

            if (Constance.Min_BetAmoutForBlackJack > 100)
                stepSize = 1000;
            else
                stepSize = 100;
        }

        public void BetSliderOpen()
        {
            BetAmountsliderGameObject.SetActive(true);
            onetime = true;
            BetButtonSwitch((int)BetAmountslider.value);
        }

        public void PP_BetSliderOpen()
        {
            PP_BetAmountsliderGameObject.SetActive(true);
            onetime = true;
        }

        public void BetButtonCLick()
        {
            BetAmountsliderGameObject.SetActive(false);
            onetime = false;
            BetAmount?.Invoke((int)BetAmountslider.value);
            BetButton.interactable = false;
        }

        public void PP_BetButtonCLick()
        {
            PP_BetAmountsliderGameObject.SetActive(false);
            onetime = false;
            PP_BetAmount?.Invoke((int)PP_BetAmountslider.value);
        }

        public void BetAmountSliderValueChange()
        {
            if (onetime)
            {
                int value = (int)BetAmountslider.value;
                value -= value % stepSize;
                BetAmountslider.value = value;
            }
            BetButtonSwitch((int)BetAmountslider.value);
            BetText.text = $"${ Constance.AmountShow((int)BetAmountslider.value)}";
        }

        private void BetButtonSwitch(int value)
        {
            if (value <= balanceAmount)
            {
                BetButtonOnSlider.interactable = true;
            }
            else if (value > balanceAmount)
            {
                BetButtonOnSlider.interactable = false;
            }
        }

        public void PP_BetAmountSliderValueChange()
        {
            if (onetime)
            {
                int value = (int)PP_BetAmountslider.value;
                value -= value % stepSize;
                PP_BetAmountslider.value = value;
            }
            PP_BetText.text = $"${ Constance.AmountShow((int)PP_BetAmountslider.value)}";
        }

        public void BackButtonCLick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            BlackJackGameManager.Instance.BackOptionPanel.SetActive(true);
        }


        public void HitButtonClick()
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["playerOption"] = "HIT"
            };

            Debug.Log("HitOption " + data.ToString());
            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("playAction", data.ToString());
        }

        public void StandButtonClick()
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["playerOption"] = "STAND"
            };

            Debug.Log("StandOption " + data.ToString());
            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("playAction", data.ToString());
            BlackJackGameManager.Instance.TurnOffOptionButtons();
        }

        public void DoubleButtonCLick()
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["playerOption"] = "DOUBLE"
            };

            Debug.Log("DoubleOption " + data.ToString());
            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("playAction", data.ToString());

            BlackJackGameManager.Instance.TurnOffOptionButtons();
        }

        public void SplitButtonLClick()
        {
            JSONNode data = new JSONObject
            {
                ["playerId"] = Constants.PLAYER_ID,
                ["playerOption"] = "SPLIT"
            };

            Debug.Log("SplitOption " + data.ToString());
            BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("playAction", data.ToString());
        }

        Transform giftSpawnPosition;
        Transform giftEndPosition;

        /// <summary>
        /// BlackJack Gift Sending Animation Part
        /// </summary>
        private void GiftSendingAnimation(JSONNode jsonNode)
        {
            if (jsonNode["status"] == false)
            {
                Constants.ShowWarning(jsonNode["message"].Value);
            }
            else if (jsonNode["gitAction"] == "all")
            {
                Debug.Log("GiftSendToAll");
                Transform p = BlackJackGameManager.Instance.PlayersParent.transform;

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<Player>().playerId == "")
                        continue;

                    for (int j = 0; j < p.childCount; j++)
                    {
                        if (p.GetChild(j).GetComponent<Player>().playerId == jsonNode["senderId"])
                        {
                            giftSpawnPosition = p.GetChild(j).GetComponent<Player>().gameObject.transform;
                        }
                    }

                    if(jsonNode["senderId"] == Constants.PLAYER_ID)
                    {
                        balanceAmount = jsonNode["playerAmount"];
                        BalanceText.text = Constance.AmountShow(jsonNode["playerAmount"]);
                    }

                    Player blackjackPlayer = p.GetChild(i).GetComponent<Player>();
                    Sprite giftSprite = BlackJackGameManager.Instance.GetSprite(jsonNode["data"]["itemname"]);

                    RectTransform r = Instantiate(GiftObjectPrefab, giftSpawnPosition).GetComponent<RectTransform>();
                    r.transform.SetAsFirstSibling();
                    r.transform.GetComponent<Image>().sprite = giftSprite;
                    r.DOMove(blackjackPlayer.GiftButton.position, 1.5f).OnComplete(() =>
                    {
                        Destroy(r.gameObject);
                        blackjackPlayer.giftIconImage.SetActive(false);
                        blackjackPlayer.giftItemImage.SetActive(true);
                        blackjackPlayer.giftItemImage.GetComponent<Image>().sprite = giftSprite;
                    });
                }
            }
            else if (jsonNode["gitAction"] == "own")
            {
                Debug.Log("GiftSendToPlayer");
                Transform p = BlackJackGameManager.Instance.PlayersParent.transform;

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<Player>().playerId == "")
                        continue;

                    if (p.GetChild(i).GetComponent<Player>().playerId == jsonNode["senderId"])
                    {
                        giftSpawnPosition = p.GetChild(i).GetComponent<Player>().gameObject.transform;
                    }

                    if (jsonNode["senderId"] == Constants.PLAYER_ID)
                    {
                        balanceAmount = jsonNode["playerAmount"];
                        BalanceText.text = Constance.AmountShow(jsonNode["playerAmount"]);
                    }
                }

                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<Player>().playerId == "")
                        continue;

                    if (p.GetChild(i).GetComponent<Player>().playerId == jsonNode["receiverId"])
                    {
                        giftEndPosition = p.GetChild(i).GetComponent<Player>().gameObject.transform;
                    }
                }

                Sprite giftSprite = BlackJackGameManager.Instance.GetSprite(jsonNode["data"]["itemname"]);
                GiftSendAnimation(giftSpawnPosition, giftEndPosition, giftSprite);
            }
        }

        private void GiftSendAnimation(Transform spawnPosition, Transform endPosition, Sprite giftSprite)
        {
            //Debug.Log("GiftAnimation");

            Player blackjackPlayer = endPosition.GetComponent<Player>();

            RectTransform r = Instantiate(GiftObjectPrefab, spawnPosition).GetComponent<RectTransform>();
            r.transform.GetComponent<Image>().sprite = giftSprite;
            r.DOMove(blackjackPlayer.GiftButton.position, 1.5f).OnComplete(() =>
            {
                Destroy(r.gameObject);
                blackjackPlayer.giftIconImage.SetActive(false);
                blackjackPlayer.giftItemImage.SetActive(true);
                blackjackPlayer.giftItemImage.GetComponent<Image>().sprite = giftSprite;
            });
        }
    }
}