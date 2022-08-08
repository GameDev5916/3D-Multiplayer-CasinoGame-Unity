using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackJackGiftScript : MonoBehaviour
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
        BlackJackGiftPanel.SelectGift?.Invoke(gameObject.GetComponent<BlackJackGiftScript>());
    }
}
