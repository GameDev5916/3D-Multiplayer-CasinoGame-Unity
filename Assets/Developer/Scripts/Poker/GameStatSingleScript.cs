using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using Casino_Poker;
using System;
using DG.Tweening;

public class GameStatSingleScript : MonoBehaviour
{
    public GameObject BG;

    [Header("Tier")]
    public Image ownTier;

    [Header("Wallet")]
    public TextMeshProUGUI ownWalletValue;

    [Header("HandsPlayed")]
    public TextMeshProUGUI ownHandValue;

    [Header("BiggestPot")]
    public TextMeshProUGUI ownBiggestPotValue;

    [Header("BestHand")]
    public GameObject ownCardsParent;
    public TextMeshProUGUI ownHandType;
    public GameObject ownHandAvailable;
    public GameObject ownNA;

    [Header("HoldemPrecentBar")]
    public Image ownHoldemFillImage;
    public Text ownHoldemPerValue;

    [Header("SitNGoPrecentBar")]
    public Image ownSitNGoFillImage;
    public Text ownSitNGoPerValue;

    [Header("SpinWinPrecentBar")]
    public Image ownSpinWinFillImage;
    public Text ownSpinWinPerValue;

    [Header("PokerWon")]
    public TextMeshProUGUI ownPokerWonValue;

    [Header("SpinWinWon")]
    public TextMeshProUGUI ownSpinWinWonValue;

    [Header("SitNGo")]
    public TextMeshProUGUI ownSitNGoWonValue;

    [Header("Buddies")]
    public TextMeshProUGUI ownBuddiesValue;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).From(new Vector2(-1300, 0)).SetEase(Ease.InSine);
        PlayerProfilePanel.SetGameStatPanel += SetPanelData;
    }

    private void OnDisable()
    {
        PlayerProfilePanel.SetGameStatPanel -= SetPanelData;
    }

    public void BackButtonGameStatSigle()
    {
        Debug.Log("BackButtonClick-SingleProfile");
        BG.GetComponent<RectTransform>().DOAnchorPosX(-1300, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine).OnComplete(() => gameObject.SetActive(false));
    }

    private void SetPanelData(JSONNode jsonNode)
    {
        JSONNode ownPlayer = jsonNode["ownerPlayerData"];

        ownTier.sprite = GameManager_Poker.Instance.TierSprites[ownPlayer["viptierlevel"] - 1];

        // Wallet ===>
        ownWalletValue.text = "$ " + Constants.NumberShow(ownPlayer["wallet"].AsLong) ;

        // HandsPlayed ===>
        ownHandValue.text = ownPlayer["handPlayed"].Value;

        // BiggestPot ===>
        ownBiggestPotValue.text = "$ " + Constants.NumberShow(ownPlayer["biggestpotwon"].AsLong) ;

        //BestHand ===>
        if (ownPlayer["bestwininghand"].Count == 0 || ownPlayer["bestwininghand"] == "")
        {
            ownNA.SetActive(true);
            ownHandAvailable.SetActive(false);
            ownHandType.gameObject.SetActive(false);

            Debug.Log("N/A Own");
        }
        else if (ownPlayer["bestwininghand"].Count != 0)
        {
            ownNA.SetActive(false);
            ownHandAvailable.SetActive(true);
            ownHandType.gameObject.SetActive(true);

            Debug.Log("OwnCardAvailable");
            for (int i = 0; i < ownCardsParent.transform.childCount; i++)
            {
                String cardSuit = ownPlayer["bestwininghand"][i]["suits"];
                int cardId = ownPlayer["bestwininghand"][i]["value"].AsInt - 1;
                Image card = ownCardsParent.transform.GetChild(i).GetComponent<Image>();
                SetSpriteOnCard(cardSuit, cardId, card);
            }
            ownHandType.text = ownPlayer["playerHandInfo"].Value;
        }


        // Holdem FillBar + Precent ===>
        ownHoldemFillImage.fillAmount = (float)ownPlayer["winholdem"] / 100;
        ownHoldemPerValue.text = ownPlayer["winholdem"].AsInt + "%";

        // SitNGo FillBar + Precent ===>
        ownSitNGoFillImage.fillAmount = (float)ownPlayer["winsitgo"] / 100;
        ownSitNGoPerValue.text = ownPlayer["winsitgo"].AsInt + "%";

        // SpinWin FillBar + Precent ===>
        ownSpinWinFillImage.fillAmount = (float)ownPlayer["winspin"] / 100;
        ownSpinWinPerValue.text = ownPlayer["winspin"].AsInt + "%";

        // Poker Win Count ===>
        ownPokerWonValue.text = ownPlayer["pokerwincount"].Value;

        // SitNGo Count ===>
        ownSitNGoWonValue.text = ownPlayer["sitngo"].Value;

        // SpinWinWon Count ===>
        ownSpinWinWonValue.text = ownPlayer["spinandwin"].Value;

        // Buddies Count ===>
        ownBuddiesValue.text = ownPlayer["buddies"].Value;
    }

    public string CardSuits;
    public int CardId;
    public void SetSpriteOnCard(string cardSuits, int cardId, Image card)
    {
        CardSuits = cardSuits;
        CardId = cardId + 1;

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

        card.sprite = GameManager_Poker.Instance.AllCardsSprites[Constants.THEAM].Cards[CardSuitsId].CardsSprites[cardId];
    }
}
