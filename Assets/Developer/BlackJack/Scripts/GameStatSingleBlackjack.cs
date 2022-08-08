using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using Casino_Poker;
using System;
using DG.Tweening;
using BalckJack;

public class GameStatSingleBlackjack : MonoBehaviour
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

    [Header("BlackJackPrecentBar")]
    public Image ownBlackJackFillImage;
    public Text ownBlackJackPerValue;

    [Header("GameStartDate")]
    public TextMeshProUGUI ownGameStartDateValue;

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

        ownTier.sprite = BlackJackGameManager.Instance.TierSprites[ownPlayer["viptierlevel"] - 1];

        // Wallet ===>
        ownWalletValue.text = "$ " + Constants.NumberShow(ownPlayer["wallet"].AsLong);

        // HandsPlayed ===>
        ownHandValue.text = ownPlayer["handPlayed"].Value;

        // BiggestPot ===>
        ownBiggestPotValue.text = "$ " + Constants.NumberShow(ownPlayer["biggestpotwon"].AsLong);

        // SitNGo FillBar + Precent ===>
        ownBlackJackFillImage.fillAmount = (float)ownPlayer["winblackjack"] / 100;
        ownBlackJackPerValue.text = ownPlayer["winblackjack"].AsInt + "%";

        // GameStartDate ===>
        ownGameStartDateValue.text = ownPlayer["gamestartingdate"].Value;

        // Buddies Count ===>
        ownBuddiesValue.text = ownPlayer["buddies"].Value;
    }
}
