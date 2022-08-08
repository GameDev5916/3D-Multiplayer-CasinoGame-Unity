using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GiftPanel : MonoBehaviour
{
    [SerializeField] private GameObject YouHaveGift;
    [SerializeField] private GameObject YouDontHaveGift;

    public TextMeshProUGUI FreeSpin;
    public Transform BG;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);

        YouHaveGift.SetActive(Constants.FREE_BONUS_SPIN_IN_GIFT > 0);
        YouDontHaveGift.SetActive(Constants.FREE_BONUS_SPIN_IN_GIFT <= 0);

        FreeSpin.text = $"<color=#FDEF3C>{Constants.FREE_BONUS_SPIN_IN_GIFT} Free</color> Spin";
    }

    public void FreeSpinButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        FreeBonusTimerAndShop.Instance.IsFromGift = true;
        Constants.GotoScene("Bonus Spins");
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                HomeScreenUIManager.Instance.GiftPanel.SetActive(false);
            });
    }
}
