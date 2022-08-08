using System;
using UnityEngine;
using DG.Tweening;
using SimpleJSON;
using BalckJack;

public class BlackJackGiftPanel : MonoBehaviour
{
    public GameObject BG;
    public GameObject giftItemsContent;
    public GameObject GiftButtonPrefab;
    BlackJackGiftScript blackjackGift;

    public static Action<BlackJackGiftScript> SelectGift;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(710, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine);
        SelectGift += OnGiftSelected;
        MainNetworkManager.SetGiftPanel += SetBlackJackGifts;
        CheckCurrentGame();
    }

    private void OnDisable()
    {
        SelectGift -= OnGiftSelected;
        MainNetworkManager.SetGiftPanel -= SetBlackJackGifts;
    }


    public void BuyGiftButtonClick()
    {
        JSONNode jsonnode = new JSONObject
        {
            ["itemName"] = blackjackGift.GiftItemName,
            ["buyAction"] = "buy",
            ["senderId"] = Constants.PLAYER_ID,
            ["receiverId"] = Constants.BlackJackGiftReceiverID,
            ["roomName"] = Constants.RoomName
        };

        BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("buyGift", jsonnode.ToString());
        //MainNetworkManager.Instance.MainSocket?.Emit("buyGift", jsonnode.ToString());
        Debug.LogWarning("BuyGiftButtonBJ " + jsonnode.ToString());
        //PokerGamePlay.Instance.UpdatePokerChips();
        CloseButtonClick();
    }

    public void SendToAllButtonClick()
    {
        JSONNode jsonnode = new JSONObject
        {
            ["itemName"] = blackjackGift.GiftItemName,
            ["buyAction"] = "all",
            ["senderId"] = Constants.BlackJackGiftSenderID,
            ["receiverId"] = Constants.BlackJackGiftReceiverID,
            ["roomName"] = Constants.RoomName
        };

        BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("buyGift", jsonnode.ToString());
        //MainNetworkManager.Instance.MainSocket?.Emit("buyGift", jsonnode.ToString());
        Debug.LogWarning("SendToAllButtonBJ " + jsonnode.ToString());
        //PokerGamePlay.Instance.UpdatePokerChips();
        CloseButtonClick();
    }

    private void SetBlackJackGifts(JSONNode jsonNode)
    {
        if (jsonNode["staus"] == true)
        {
            for (int i = 0; i < jsonNode["data"].Count; i++)
            {
                GameObject _giftItem = Instantiate(GiftButtonPrefab, giftItemsContent.transform);
                _giftItem.transform.SetParent(giftItemsContent.transform);
                BlackJackGiftScript blackjackGiftScript = _giftItem.GetComponent<BlackJackGiftScript>();

                blackjackGiftScript.GiftItemName = jsonNode["data"][i]["itemname"];
                blackjackGiftScript.GiftItemPrice = jsonNode["data"][i]["itemprice"];
                blackjackGiftScript.PriceBox.text = Constants.NumberShow(blackjackGiftScript.GiftItemPrice);
                blackjackGiftScript.GiftItemSprite.sprite = BlackJackGameManager.Instance.GetSprite(blackjackGiftScript.GiftItemName);
            }

            OnGiftSelected(giftItemsContent.transform.GetChild(0).GetComponent<BlackJackGiftScript>());
        }
        else
        {
            Constants.ShowWarning("Something went wrong!");
        }
    }

    private void OnGiftSelected(BlackJackGiftScript blackjackGiftScript)
    {
        blackjackGift = blackjackGiftScript;

        for (int i = 0; i < giftItemsContent.transform.childCount; i++)
        {
            giftItemsContent.transform.GetChild(i).GetComponent<BlackJackGiftScript>().unselectImage.SetActive(true);
            giftItemsContent.transform.GetChild(i).GetComponent<BlackJackGiftScript>().selectImage.SetActive(true);
        }

        blackjackGift.selectImage.SetActive(true);
        blackjackGift.unselectImage.SetActive(false);
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).From(new Vector2(710, 0)).SetEase(Ease.Linear)
             .OnComplete(() =>
             {
                 for (int i = 0; i < giftItemsContent.transform.childCount; i++)
                 {
                     Destroy(giftItemsContent.transform.GetChild(i).gameObject);
                     //Debug.Log("allGiftsRemoveOnClose");
                 }

                 BlackJackGameManager.Instance.BlackJackGiftPanel.SetActive(false);
             });
    }

    private void CheckCurrentGame()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == "Poker")
            Constants.RoomType = "poker";
        else if (currentScene == "BlackJack")
            Constants.RoomType = "blackjack";
        else
            Debug.Log("No Any Game Running");

        //Debug.Log("Room Type: " + Constants.RoomType);
    }
}
