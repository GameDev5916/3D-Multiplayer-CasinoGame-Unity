using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;


namespace BalckJack
{
    public class DelarCards : MonoBehaviour
    {
        public Text TotalCardNumber;
        public GameObject CardObject;

        public static Action<JSONNode> ShowDealerCards, OnDealerHandDisplay;

        private void OnEnable()
        {
            ShowDealerCards += CardAnimationOnStart;
            OnDealerHandDisplay += CardAnimationOnDealerHand;
            BlackJack_NetworkManager.OnGameRestart += ResetDealerCardsOnRestart;
        }

        private void OnDisable()
        {
            ShowDealerCards -= CardAnimationOnStart;
            OnDealerHandDisplay -= CardAnimationOnDealerHand;
            BlackJack_NetworkManager.OnGameRestart -= ResetDealerCardsOnRestart;
        }

        private void CardAnimationOnStart(JSONNode jsonNode)
        {
            RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
            r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
            r.DOScale(new Vector3(1.5f,1.5f,1.5f), 0.3f);
            r.DOMove(CardObject.transform.position, 0.5f).OnComplete(() =>
            {
                DisplayDealerCards(jsonNode);
                Destroy(r.gameObject);
            });
        }

        private void DisplayDealerCards(JSONNode jsonNode)
        {
            string CardSuit = jsonNode["dealerHand"][0]["suit"].Value;
            int CardID = Constance.GetCardIndex(jsonNode["dealerHand"][0]["value"]);

            GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardObject.transform);
            Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);

            TotalCardNumber.transform.parent.gameObject.SetActive(true);
            TotalCardNumber.text = jsonNode["dealerScore"];
        }


        private void CardAnimationOnDealerHand(JSONNode jsonNode)
        {
            RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, BlackJackGameManager.Instance.DealerCardBox.transform).GetComponent<RectTransform>();
            r.DOLocalRotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.Linear).SetLoops(-1);
            r.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.3f);
            r.DOMove(CardObject.transform.position, 0.5f).OnComplete(() =>
            {
                DisplayDealerHand(jsonNode);
                Destroy(r.gameObject);
            });
        }

        private void DisplayDealerHand(JSONNode jsonNode)
        {
            string CardSuit = jsonNode["card"]["suit"].Value;
            int CardID = Constance.GetCardIndex(jsonNode["card"]["value"]);

            GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardObject.transform);
            Card.GetComponent<Card>().SetDataOnCard(CardSuit, CardID);

            TotalCardNumber.transform.parent.gameObject.SetActive(true);
            TotalCardNumber.text = jsonNode["score"];
        }

        private void ResetDealerCardsOnRestart(JSONNode jsonNode)
        {
            StartCoroutine(ResetDealerCards(jsonNode));
        }

        IEnumerator ResetDealerCards(JSONNode jsonNode)
        {
            yield return new WaitForSeconds(1f);

            if (jsonNode["status"] == true && CardObject.transform.childCount > 0)
            {
                for (int i = 0; i < CardObject.transform.childCount; i++)
                {
                    Destroy(CardObject.transform.GetChild(i).gameObject);
                }

                Constants.ShowWarning("Round Restarted");
                TotalCardNumber.transform.parent.gameObject.SetActive(false);
                RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, CardObject.transform).GetComponent<RectTransform>();
                yield return new WaitForSeconds(1f);
                Destroy(r.gameObject);
                CardDiscardAnimation(CardObject.transform, BlackJackGameManager.Instance.DiscardedCardBox.transform);
            }
        }

        private void CardDiscardAnimation(Transform startPoint, Transform endPoint)
        {
            Debug.Log("ChipsAnimation");
            RectTransform r = Instantiate(BlackJackGameManager.Instance.DummyCard, startPoint).GetComponent<RectTransform>();
            r.DOScale(new Vector3(0.4f, 0.4f, 0.4f), 1f);
            r.DOMove(endPoint.position, 1f).OnComplete(() =>
            {
                Destroy(r.gameObject);
            }
            );
        }

        //public void SetDelarCardData()
        //{
        //    GameObject Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardObject.transform);
        //    Card.GetComponent<Card>().SetDataOnCard("diamonds", 2);
        //    Card.GetComponent<Card>().ShowCard();

        //    Card = Instantiate(BlackJackGameManager.Instance.CardPrefab, CardObject.transform);
        //    Card.GetComponent<Card>().SetDataOnCard("diamonds", 2);
        //}
    }
}