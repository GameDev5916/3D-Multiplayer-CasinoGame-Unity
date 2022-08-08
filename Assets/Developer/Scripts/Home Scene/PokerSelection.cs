using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PokerSelection : MonoBehaviour
{
    [SerializeField] private RectTransform Joinroom;
    [SerializeField] private RectTransform SelectMode;
    [SerializeField] private RectTransform Event;

    private void OnEnable()
    {
        Joinroom.DOAnchorPosY(-50, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
        SelectMode.DOAnchorPosY(-50, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack).SetDelay(.2f);
        Event.DOAnchorPosY(-50, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack).SetDelay(.4f);
    }

    public void BackButtonClick()
    {
        HomePanel.Instance.ScrollView.SetActive(true);
        HomeScreenUIManager.Instance.PokerSelection.SetActive(false);
        //TopPanel.Instance.BG.enabled = false;
    }

    public void JoinTableButtonClick()
    {
        Constants.PokerJoinRoom = true;
        HomeScreenUIManager.Instance.HomePanel.SetActive(false);
        HomeScreenUIManager.Instance.PokerSelection.SetActive(false);
        HomeScreenUIManager.Instance.TopPanel.SetActive(false);
        Constants.GotoScene("Poker");
    }
}
