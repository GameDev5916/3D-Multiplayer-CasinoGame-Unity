using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PokerGiftScript : MonoBehaviour
{
    public string GiftItemName;
    public long GiftItemPrice;
    public GameObject selectImage;
    public GameObject unselectImage;
    public Image GiftItemSprite;
    public TextMeshProUGUI PriceBox;

    public void SelectGiftButtonClick()
    {
        Debug.Log("SelectedGift " + GiftItemName);
        PokerGiftPanel.SelectGift?.Invoke(gameObject.GetComponent<PokerGiftScript>());
    }
}
