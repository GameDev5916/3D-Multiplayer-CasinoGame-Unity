using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Casino_Poker
{
    public class CardScript : MonoBehaviour
    {
        public int CardId;
        public Image FaceDown, FaceUp;

        public bool IsCardFaceUP;
        public float ZRotation;
        public Image DarkImage;

        public string CardSuits;

        private Quaternion OrinalRotation;

        private void Awake()
        {
            OrinalRotation = transform.rotation;
        }

        public void SetDataOnCard(string cardSuits, int cardId)
        {
            CardSuits = cardSuits;
            CardId = cardId+1;

            gameObject.SetActive(true);
            // Get Card Suit Id From CardSuits String
            int CardSuitsId = 0;
            if (Constants.SuitEnum.hearts.ToString() == CardSuits)
                CardSuitsId = 0;
            else if (Constants.SuitEnum.clubs.ToString() == CardSuits)
                CardSuitsId = 1;
            else if (Constants.SuitEnum.diamonds.ToString() == CardSuits)
                CardSuitsId = 2;
            else if (Constants.SuitEnum.spades.ToString() == CardSuits)
                CardSuitsId = 3;

            FaceDown.sprite = GameManager_Poker.Instance.AllCardsSprites[Constants.THEAM].CardBackSprite;
            FaceUp.sprite = GameManager_Poker.Instance.AllCardsSprites[Constants.THEAM].Cards[CardSuitsId].CardsSprites[cardId];
            ShowCard();
        }

        public void ShowCard()
        {
            Vector3 rotation;
            if (IsCardFaceUP) rotation = new Vector3(0, 180, ZRotation);
            else rotation = new Vector3(0, 0, ZRotation);

            transform.DORotate(new Vector3(0, 90, ZRotation), .25f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                //if (IsCardFaceUP) FaceDown.transform.SetAsLastSibling();
                //else FaceUp.transform.SetAsLastSibling();

                if (IsCardFaceUP) FaceUp.transform.SetAsFirstSibling();
                else FaceDown.transform.SetAsFirstSibling();

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
                        if (IsCardFaceUP) FaceDown.transform.SetAsLastSibling();
                        else FaceUp.transform.SetAsLastSibling();

                        transform.DORotate(new Vector3(0, 0, 0), .25f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            IsCardFaceUP = !IsCardFaceUP;
                        });
                    });
        }

        public void EnableDarkImage()
        {
            DarkImage.enabled = true;
            DarkImage.transform.SetAsLastSibling();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                ResetCards();
        }

        public void ResetCards()
        {
            IsCardFaceUP = false;
            transform.rotation = OrinalRotation;
            DarkImage.enabled = false;
            DarkImage.transform.SetAsLastSibling();
            FaceUp.transform.SetAsFirstSibling();
            gameObject.SetActive(false);
        }
    }
}