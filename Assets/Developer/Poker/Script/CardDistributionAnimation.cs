using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using SimpleJSON;
namespace Casino_Poker
{
    public class CardDistributionAnimation : MonoBehaviour
    {
        public GameObject CardTogo;
        public RectTransform a;
        private JSONNode PlayerCardData;

        private void OnEnable()
        {
            NetworkManager_Poker.PlayerCardDistribution += DisctributionAnimation;
            NetworkManager_Poker.GameCardDistriBution += AddMiddelCards;
        }

        private void OnDisable()
        {
            NetworkManager_Poker.PlayerCardDistribution -= DisctributionAnimation;
            NetworkManager_Poker.GameCardDistriBution -= AddMiddelCards;
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.Q))
        //    {
        //        //StartCoroutine(Disctribution());
        //        GO(a);
        //    }
        //}

        public void DisctributionAnimation(SimpleJSON.JSONNode jsonNode)
        {
            PlayerCardData = jsonNode["cards"];
            StartCoroutine(PlayerDisctribution());
        }

        public void AddMiddelCards(JSONNode jsonNode)
        {
            StartCoroutine(MidelCardDisctribution(jsonNode));
        }

        IEnumerator PlayerDisctribution()
        {
            Transform p = GameManager_Poker.Instance.PlayersParent.transform;
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < p.childCount; i++)
                {
                    if (p.GetChild(i).GetComponent<PokerPlayer>().playerId == "")
                        continue;

                    PlayerCardAnimation(p.GetChild(i).GetComponent<PokerPlayer>(),j);
                    yield return new WaitForSeconds(.3f);
                }
            }
        }

        IEnumerator MidelCardDisctribution(JSONNode jsonNode)
        {
            RectTransform p = PokerGamePlay.Instance.MiddelCards.GetComponent<RectTransform>();
            for (int i = 0; i < jsonNode.Count; i++)
            {
                MiddelCardAnimation(p, jsonNode[i]["suits"].Value, jsonNode[i]["value"].AsInt);
                yield return new WaitForSeconds(.5f);
            }
        }

        public void PlayerCardAnimation(PokerPlayer pokerPlayer,int j)
        {
            RectTransform r = Instantiate(CardTogo, transform).GetComponent<RectTransform>();
            r.DOLocalRotate(new Vector3(0, 0, 180), .15f).SetEase(Ease.Linear).SetLoops(-1);
            r.DOMove(pokerPlayer.CardParent.transform.position, .2f).OnComplete(() =>
            {
                Destroy(r.gameObject);

                // SHOW CARD DATA AS FACE UP TO OUR PLAYER AND SHOW CARD AS FACE DOWN TO OTHER PLAYER
                if(j == 0)
                {
                    if(pokerPlayer.playerId == Constants.PLAYER_ID)
                    {
                        pokerPlayer.Card1.SetDataOnCard(PlayerCardData[0]["suits"].Value, PlayerCardData[0]["value"].AsInt - 1);
                    }
                    else
                    {
                        pokerPlayer.Card1.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (pokerPlayer.playerId == Constants.PLAYER_ID)
                    {
                        pokerPlayer.Card2.SetDataOnCard(PlayerCardData[1]["suits"].Value, PlayerCardData[1]["value"].AsInt - 1);
                    }
                    else
                    {
                        pokerPlayer.Card2.gameObject.SetActive(true);
                    }
                }
            });
        }

        public void MiddelCardAnimation(RectTransform middel, string suits, int cardid)
        {
            RectTransform r = Instantiate(CardTogo, transform).GetComponent<RectTransform>();
            r.DOLocalRotate(new Vector3(0, 0, 180), .15f).SetEase(Ease.Linear).SetLoops(-1);
            r.DOMove(middel.position, .2f).OnComplete(() =>
            {
                Destroy(r.gameObject);
                CardScript card = Instantiate(GameManager_Poker.Instance.Card, middel).GetComponent<CardScript>();
                card.SetDataOnCard(suits, cardid - 1);
            });
        }

        //public void GO(RectTransform rr)
        //{
        //    RectTransform r = Instantiate(CardTogo, transform).GetComponent<RectTransform>();
        //    r.DOLocalRotate(new Vector3(0, 0, 180), .1f).SetEase(Ease.Linear).SetLoops(-1);
        //    r.DOMove(rr.position, .2f).OnComplete(() =>
        //    {
        //        //Destroy(r.gameObject);
        //    });
        //}
    }
}