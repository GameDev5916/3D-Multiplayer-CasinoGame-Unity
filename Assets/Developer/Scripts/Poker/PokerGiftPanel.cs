using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Casino_Poker;
using SimpleJSON;
using System;

public class PokerGiftPanel : MonoBehaviour
{
    public GameObject BG;
    public GameObject giftItemsContent;
    public GameObject GiftButtonPrefab;
    PokerGiftScript pokerGift;

    public static Action<PokerGiftScript> SelectGift;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(710, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine);
        SelectGift += OnGiftSelected;
        MainNetworkManager.SetGiftPanel += SetPokerGifts;
        CheckCurrentGame();
    }

    private void OnDisable()
    {
        SelectGift -= OnGiftSelected;
        MainNetworkManager.SetGiftPanel -= SetPokerGifts;
    }


    public void BuyGiftButtonClick()
    {
        JSONNode jsonnode = new JSONObject
        {
            ["itemName"] = pokerGift.GiftItemName,
            ["buyAction"] = "buy",
            ["senderId"] = Constants.PLAYER_ID,
            ["receiverId"]= Constants.PokerGiftReceiverID,
            ["roomName"] = Constants.RoomName
        };

        NetworkManager_Poker.Instance.PokerSocket?.Emit("buyGift", jsonnode.ToString());
        //MainNetworkManager.Instance.MainSocket?.Emit("buyGift", jsonnode.ToString());
        Debug.LogWarning("BuyGiftButton " + jsonnode.ToString());
        PokerGamePlay.Instance.UpdatePokerChips();
        CloseButtonClick();

        GameManager_Poker.Instance.DisableGiftButton();
    }

    public void SendToAllButtonClick()
    {
        JSONNode jsonnode = new JSONObject
        {
            ["itemName"] = pokerGift.GiftItemName,
            ["buyAction"] = "all",
            ["senderId"] = Constants.PokerGiftSenderID,
            ["receiverId"] = Constants.PokerGiftReceiverID,
            ["roomName"] = Constants.RoomName
        };

        NetworkManager_Poker.Instance.PokerSocket?.Emit("buyGift", jsonnode.ToString());
        //MainNetworkManager.Instance.MainSocket?.Emit("buyGift", jsonnode.ToString());
        Debug.LogWarning("SendToAllButton " + jsonnode.ToString());
        PokerGamePlay.Instance.UpdatePokerChips();
        CloseButtonClick();

        GameManager_Poker.Instance.DisableGiftButton();
    }

    private void SetPokerGifts(JSONNode jsonNode)
    {
        if (jsonNode["staus"] == true)
        {
            for (int i = 0; i < jsonNode["data"].Count; i++)
            {
                GameObject _giftItem = Instantiate(GiftButtonPrefab, giftItemsContent.transform);
                _giftItem.transform.SetParent(giftItemsContent.transform);
                PokerGiftScript pokerGiftScript = _giftItem.GetComponent<PokerGiftScript>();

                pokerGiftScript.GiftItemName = jsonNode["data"][i]["itemname"];
                pokerGiftScript.GiftItemPrice = jsonNode["data"][i]["itemprice"];
                pokerGiftScript.PriceBox.text = Constants.NumberShow(pokerGiftScript.GiftItemPrice);
                pokerGiftScript.GiftItemSprite.sprite = GameManager_Poker.Instance.GetSprite(pokerGiftScript.GiftItemName);
                //Debug.LogWarning("GIFT " + jsonNode["data"][i]["itemname"].Value);
            }

            OnGiftSelected(giftItemsContent.transform.GetChild(0).GetComponent<PokerGiftScript>());
        }
        else
        {
            Constants.ShowWarning("Something went wrong!");
        }
    }

    private void OnGiftSelected(PokerGiftScript pokerGiftScript)
    {
        pokerGift = pokerGiftScript;

        for (int i = 0; i < giftItemsContent.transform.childCount; i++)
        {
            giftItemsContent.transform.GetChild(i).GetComponent<PokerGiftScript>().unselectImage.SetActive(true);
            giftItemsContent.transform.GetChild(i).GetComponent<PokerGiftScript>().selectImage.SetActive(true);
        }

        pokerGift.selectImage.SetActive(true);
        pokerGift.unselectImage.SetActive(false);
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

                 GameManager_Poker.Instance.PokerGiftPanel.SetActive(false);
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
