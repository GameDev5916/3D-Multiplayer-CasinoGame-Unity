using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BalckJack
{
    public class Card : MonoBehaviour
    {
        public int CardNumber;
        public Image BackSide, FontSide, DarkImage;

        public bool IsCardFaceUP;

        private void Start()
        {
            CardNumber = Random.Range(0, 13);
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Space))
        //        SetDataOnCard("hearts",10);
        //}

        public void SetDataOnCard(string CardSuits, int CardID)
        {
            // Get Card Suit Id From CardSuits String
            int CardSuitsId = 0;

            if (Constance.SuitEnum.Hearts.ToString() == CardSuits)
                CardSuitsId = 0;
            else if (Constance.SuitEnum.Clubs.ToString() == CardSuits)
                CardSuitsId = 1;
            else if (Constance.SuitEnum.Diamonds.ToString() == CardSuits)
                CardSuitsId = 2;
            else if (Constance.SuitEnum.Spades.ToString() == CardSuits)
                CardSuitsId = 3;

            //CardSuitsId = Random.Range(0, 3);
            BackSide.sprite = BlackJackGameManager.Instance.AllCardsSprites[0].CardBackSprite;
            FontSide.sprite = BlackJackGameManager.Instance.AllCardsSprites[0].Cards[CardSuitsId].CardsSprites[CardID];
            ShowCard();
        }

        public void ShowCard()
        {
            Vector3 rotation;
            if (IsCardFaceUP) rotation = new Vector3(0, 180, 0);
            else rotation = new Vector3(0, 0, 0);

            transform.DORotate(new Vector3(0, 90, 0), .25f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (IsCardFaceUP) BackSide.transform.SetAsLastSibling();
                else FontSide.transform.SetAsLastSibling();

                transform.DORotate(rotation, .25f).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    IsCardFaceUP = !IsCardFaceUP;
                });
            });
        }

        public void ShowCardInMiddle()
        {
            transform.DOScale(new Vector3(1f, 1f, 1f), .3f)
                .SetEase(Ease.OutBack)
                .From(new Vector3(0f, 0f, 0f))
                .OnComplete(() =>
                {
                });

            transform.DORotate(new Vector3(0, 90, 0), .25f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        if (IsCardFaceUP) BackSide.transform.SetAsLastSibling();
                        else FontSide.transform.SetAsLastSibling();

                        transform.DORotate(new Vector3(0, 0, 0), .25f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            IsCardFaceUP = !IsCardFaceUP;
                        });
                    });
        }
    }
}

