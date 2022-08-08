using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using Casino_Poker;
using System;
using DG.Tweening;

public class GameStatDoubleScript : MonoBehaviour
{
    public GameObject BG;

    [Header("Name-ProfilePic-Tier")]
    public RawImage ownProfilePic;
    public TextMeshProUGUI ownNameBox;
    public Image ownTier;
    public RawImage oppProfilePic;
    public TextMeshProUGUI oppNameBox;
    public Image oppTier;

    [Header("Wallet")]
    public TextMeshProUGUI ownWalletValue;
    public TextMeshProUGUI oppWalletValue;

    [Header("HandsPlayed")]
    public TextMeshProUGUI ownHandValue;
    public TextMeshProUGUI oppHandValue;

    [Header("BiggestPot")]
    public TextMeshProUGUI ownBiggestPotValue;
    public TextMeshProUGUI oppBiggestPotValue;

    [Header("BestHand")]
    public GameObject ownCardsParent;
    public TextMeshProUGUI ownHandType;
    public GameObject ownHandAvailable;
    public GameObject ownNA;
    public GameObject oppCardsParent;
    public TextMeshProUGUI oppHandType;
    public GameObject oppHandAvailable;
    public GameObject oppNA;

    [Header("HoldemPrecentBar")]
    public Image ownHoldemFillImage;
    public Text ownHoldemPerValue;
    public Image oppHoldemFillImage;
    public Text oppHoldemPerValue;

    [Header("SitNGoPrecentBar")]
    public Image ownSitNGoFillImage;
    public Text ownSitNGoPerValue;
    public Image oppSitNGoFillImage;
    public Text oppSitNGoPerValue;

    [Header("SpinWinPrecentBar")]
    public Image ownSpinWinFillImage;
    public Text ownSpinWinPerValue;
    public Image oppSpinWinFillImage;
    public Text oppSpinWinPerValue;

    [Header("PokerWon")]
    public TextMeshProUGUI ownPokerWonValue;
    public TextMeshProUGUI oppPokerWonValue;

    [Header("SpinWinWon")]
    public TextMeshProUGUI ownSpinWinWonValue;
    public TextMeshProUGUI oppSpinWinWonValue;

    [Header("SitNGoWon")]
    public TextMeshProUGUI ownSitNGoWonValue;
    public TextMeshProUGUI oppSitNGoWonValue;

    [Header("Buddies")]
    public TextMeshProUGUI ownBuddiesValue;
    public TextMeshProUGUI oppBuddiesValue;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).From(new Vector2(-1300, 0)).SetEase(Ease.InSine);
        PlayerProfilePanel.SetGameStatPanel += SetPanelData;
    }

    private void OnDisable()
    {
        PlayerProfilePanel.SetGameStatPanel -= SetPanelData;
    }

    public void BackButtonGameStatDouble()
    {
        Debug.Log("BackButtonClick-DoubleProfile");
        BG.GetComponent<RectTransform>().DOAnchorPosX(-1300, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine).OnComplete(() => gameObject.SetActive(false));
    }

    private void SetPanelData(JSONNode jsonNode)
    {
        JSONNode ownPlayer = jsonNode["ownerPlayerData"];
        JSONNode oppPlayer = jsonNode["oppositePlayerData"];

        // Tier ===>
        ownTier.sprite = GameManager_Poker.Instance.TierSprites[ownPlayer["viptierlevel"] - 1];
        oppTier.sprite = GameManager_Poker.Instance.TierSprites[oppPlayer["viptierlevel"] - 1];

        // Name ===>
        ownNameBox.text = ownPlayer["name"];
        oppNameBox.text = oppPlayer["name"];

        // ProfilePic ===>
        if (ownPlayer["profilePic"].Value != "" && ownPlayer["profilePic"].Value != "null")
        {
            Constants.GetImageFrom64String(ownPlayer["profilePic"].Value, (Texture image) =>
            {
                ownProfilePic.texture = image;
                //Debug.Log("ProfilePanelPic" + ownPlayer["profilePic"].Value);
            });
        }
        else
        {
            Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
            {
                ownProfilePic.texture = image;
                //Debug.Log("DefaultImageSet");
            });
        }

        if (oppPlayer["profilePic"].Value != "" && oppPlayer["profilePic"].Value != "null")
        {
            Constants.GetImageFrom64String(oppPlayer["profilePic"].Value, (Texture image) =>
            {
                oppProfilePic.texture = image;
                //Debug.Log("ProfilePanelPic" + oppPlayer["profilePic"].Value);
            });
        }
        else
        {
            Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
            {
                oppProfilePic.texture = image;
                //Debug.Log("DefaultImageSet");
            });
        }

        // Wallet ===>
        ownWalletValue.text = "$ " + Constants.NumberShow(ownPlayer["wallet"].AsLong);
        oppWalletValue.text = "$ " + Constants.NumberShow(oppPlayer["wallet"].AsLong);

        // HandsPlayed ===>
        ownHandValue.text = ownPlayer["handPlayed"].Value;
        oppHandValue.text = oppPlayer["handPlayed"].Value;

        // BiggestPot ===>
        ownBiggestPotValue.text = "$ " + Constants.NumberShow(ownPlayer["biggestpotwon"].AsLong);
        oppBiggestPotValue.text = "$ " + Constants.NumberShow(oppPlayer["biggestpotwon"].AsLong);

        // BestHandCards&Type ===>
        if (ownPlayer["bestwininghand"].Count == 0 || ownPlayer["bestwininghand"] == "")
        {
            ownNA.SetActive(true);
            ownHandAvailable.SetActive(false);

            Debug.Log("N/A Own");
        }
        else if (ownPlayer["bestwininghand"].Count != 0)
        {
            ownNA.SetActive(false);
            ownHandAvailable.SetActive(true);

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

        if (oppPlayer["bestwininghand"].Count == 0 || oppPlayer["bestwininghand"] == "")
        {
            oppNA.SetActive(true);
            oppHandAvailable.SetActive(false);

            Debug.Log("N/A Opp");
        }
        else if (oppPlayer["bestwininghand"].Count != 0)
        {
            oppNA.SetActive(false);
            oppHandAvailable.SetActive(true);

            Debug.Log("OppCardAvailable");
            for (int i = 0; i < oppCardsParent.transform.childCount; i++)
            {
                String cardSuit = oppPlayer["bestwininghand"][i]["suits"];
                int cardId = oppPlayer["bestwininghand"][i]["value"].AsInt - 1;
                Image card = oppCardsParent.transform.GetChild(i).GetComponent<Image>();
                SetSpriteOnCard(cardSuit, cardId, card);
            }
            oppHandType.text = oppPlayer["playerHandInfo"].Value;
        }

        // Holdem FillBar + Precent ===>
        ownHoldemFillImage.fillAmount = (float)ownPlayer["winholdem"] / 100;
        ownHoldemPerValue.text = ownPlayer["winholdem"].AsInt + "%";
        oppHoldemFillImage.fillAmount = (float)oppPlayer["winholdem"] / 100;
        oppHoldemPerValue.text = oppPlayer["winholdem"].AsInt + "%";

        // SitNGo FillBar + Precent ===>
        ownSitNGoFillImage.fillAmount = (float)ownPlayer["winsitgo"] / 100;
        ownSitNGoPerValue.text = ownPlayer["winsitgo"].AsInt + "%";
        oppSitNGoFillImage.fillAmount = (float)oppPlayer["winsitgo"] / 100;
        oppSitNGoPerValue.text = oppPlayer["winsitgo"].AsInt + "%";

        // SpinWin FillBar + Precent ===>
        ownSpinWinFillImage.fillAmount = (float)ownPlayer["winspin"] / 100;
        ownSpinWinPerValue.text = ownPlayer["winspin"].AsInt + "%";
        oppSpinWinFillImage.fillAmount = (float)oppPlayer["winspin"] / 100;
        oppSpinWinPerValue.text = oppPlayer["winspin"].AsInt + "%";

        // Poker Win Count ===>
        ownPokerWonValue.text = ownPlayer["pokerwincount"].Value;
        oppPokerWonValue.text = oppPlayer["pokerwincount"].Value;

        // SitNGo Count ===>
        ownSitNGoWonValue.text = ownPlayer["sitngo"].Value;
        oppSitNGoWonValue.text = oppPlayer["sitngo"].Value;

        // SpinWinWon Count ===>
        ownSpinWinWonValue.text = ownPlayer["spinandwin"].Value;
        oppSpinWinWonValue.text = oppPlayer["spinandwin"].Value;

        // Buddies Count ===>
        ownBuddiesValue.text = ownPlayer["buddies"].Value;
        oppBuddiesValue.text = oppPlayer["buddies"].Value;
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
