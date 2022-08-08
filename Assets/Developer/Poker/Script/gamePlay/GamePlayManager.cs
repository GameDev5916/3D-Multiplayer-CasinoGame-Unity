using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Casino_Poker
{
    public class GamePlayManager : MonoBehaviour
    {
        public static GamePlayManager Instance;

        public GameObject MiddelCardsObject;
        public List<CardScript> MiddelCards;

        public static Action Flod;
        public int CurrentPlayerTurn;

        public List<GameObject> PlayerPositionsList;
        public List<GameObject> PlayerList;
        public GameObject Player;
        public int TotalNumberOfPlayerINGame;

        public GameObject Buttons;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);

            //Show 5 Middel card in list
            for (int i = 0; i < MiddelCardsObject.transform.childCount; i++)
            {
                MiddelCards.Add(MiddelCardsObject.transform.GetChild(i).GetChild(0).GetComponent<CardScript>());
                MiddelCardsObject.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

            //For Test
            TotalNumberOfPlayerINGame = 0;
            SetAllPlayerPosition();
        }

        void SetAllPlayerPosition()
        {
            int currentPlayerIndexInJsonArray = 0;
            for (int i = 0; i < 6; i++)
            {
                Player player = Instantiate(Player, PlayerPositionsList[0].transform.GetChild(i).transform).GetComponent<Player>();
                player.Playerid = currentPlayerIndexInJsonArray;
                player.TotalCoin = currentPlayerIndexInJsonArray;
                PlayerList.Add(player.gameObject);

                currentPlayerIndexInJsonArray++;
                if (currentPlayerIndexInJsonArray >= 6) currentPlayerIndexInJsonArray = 0;
            }
        }

        public void ShowMiddelCard(int index)
        {
            MiddelCards[index].transform.parent.gameObject.SetActive(true);
            MiddelCards[index].gameObject.SetActive(true);
            MiddelCards[index].ShowCardInMiddle();
        }

        IEnumerator ShowCardThreeMiddelCards()
        {
            int n = 0;
            while (n < 3)
            {
                yield return new WaitForSeconds(.5f);
                ShowMiddelCard(n);
                n++;
            }
        }

        public void FlockButtonCLick()
        {
            Flod?.Invoke();
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        StartCoroutine(ShowCardThreeMiddelCards());
        //    }
        //    if (Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        ShowMiddelCard(3);
        //    }
        //    if (Input.GetKeyDown(KeyCode.Alpha2))
        //    {
        //        ShowMiddelCard(4);
        //    }
        //}

        //public void ShowPlayOption()
        //{
        //}
    }
}