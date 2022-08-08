using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Casino_Poker
{
    public class Player : MonoBehaviour
    {
        public int Playerid;
        public int TotalCoin;

        public TextMeshProUGUI TotalCoinText;
        public TextMeshProUGUI BetAmountText;

        public GameObject ShowStats;

        public GameObject PlayerCards;

        public List<CardScript> CardScript;

        private void OnEnable()
        {
            GamePlayManager.Flod += FlodButtonCLick;
        }

        private void OnDisable()
        {
            GamePlayManager.Flod -= FlodButtonCLick;
        }

        private void Awake()
        {
            Playerid = transform.GetSiblingIndex();

            for (int i = 0; i < PlayerCards.transform.childCount; i++)
            {
                CardScript.Add(PlayerCards.transform.GetChild(i).GetComponent<CardScript>());
            }
        }

        private void Start()
        {
            TotalCoinText.text = TotalCoin.ToString();
            //if (Constants.PLAYERID == Playerid)
            //    Invoke(nameof(ShowPlayerCard), 1);
        }

        public void SetCardData(SimpleJSON.JSONNode jsonNode)
        {
            if (Playerid == int.Parse(jsonNode["playerId"].Value))
            {
                for (int i = 0; i < jsonNode["playerCard"].Count; i++)
                {
                    int CardId = int.Parse(jsonNode["playerCard"][i]["card"]["val"].Value) - 1;
                    string CardSuits = jsonNode["playerCard"][i]["cardType"].Value;
                    CardScript[i].SetDataOnCard(CardSuits, CardId);
                }
            }
        }

        public void ShowPlayerCard()
        {
            for (int i = 0; i < CardScript.Count; i++)
            {
                CardScript[i].ShowCard();
            }
        }

        public void FlodButtonCLick()
        {
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
            //if (Playerid == GamePlayManager.Instance.CurrentPlayerTurn)
            {
                for (int i = 0; i < CardScript.Count; i++)
                {
                    CardScript[i].ShowCard();
                }
            }
        }
    }
}