using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSZLGamble : MonoBehaviour {
    public Transform cardParent;
    public GameObject contentPrefab;
    private CSZLGambleContent _content = null;

    public Image background;
    public CanvasGroup canvas;
    public Text bankText;
    public Text betText;
    public Button collectButton;
    public CSAlertRewardAnim alert;
    public CSBottomPanel bottomPanel;

    private float _bet;
    public float bet
    {
        get { return _bet; }
        set
        {
            _bet = value;
            betText.text = _bet.ToString("");
        }
    }

    private float _bank;
    public float bank {
        get { return _bank; }
        set {
            _bank = value;
            bankText.text = _bank.ToString("");
            interactable = _bank > 0;
        }
    }

    private bool _interactable = false;
    public bool interactable {
        get { return _interactable; }
        set {
            _interactable = value;
            collectButton.interactable = value;
        }
    }

    private bool _enable = false;
    public bool enable {
        get { return _enable; }
        set {
            if (_enable == value)
                return;
            _enable = value;
            canvas.interactable = value;
            canvas.blocksRaycasts = value;
        }
    }

    public void Appear(float bet, System.Action callback = null)
    {
        interactable = false;
        this.bet = bet;
        BackgroundAlpha(0.8f);
        LeanTween.scale(gameObject, Vector3.one * 0.9f, 0.6f).setOnComplete(() =>
        {
            if (callback != null)
                callback();
            enable = true;
            CreateContent();
        }).setEaseOutBack();
    }

    public void Disappear(System.Action callback = null)
    {
        bottomPanel.win = 0;

        enable = false;
        BackgroundAlpha(0.0f);
        LeanTween.scale(gameObject, Vector3.one * 0.0f, 0.6f).setOnComplete(() =>
        {
            if (callback != null)
                callback();
        }).setEaseInBack();
    }

    private void CreateContent()
    {
        _content = Instantiate(contentPrefab, cardParent).GetComponent<CSZLGambleContent>();
        _content.ResultEvent += ContentResult;
        _content.CardSelectedEvent += CardSelected;
    }

    private void DestroyContent()
    {
        if (_content == null)
            return;
        _content.ResultEvent -= ContentResult;
        _content.CardSelectedEvent -= CardSelected;
        _content.Clean();
        _content = null;
    }

    private void BackgroundAlpha(float alpha)
    {
        LeanTween.cancel(background.rectTransform);
        LeanTween.alpha(background.rectTransform, alpha, 0.3f);
    }

    private void CardSelected(bool isWin, CSCard c1, CSCard c2)
    {
        interactable = isWin;
    }

    private void ContentResult(bool isWin, CSZLGambleContent sender)
    {
        DestroyContent();

        interactable = isWin;

        if (isWin)
        {
            bet *= 2;
            CreateContent();
        }
        else
        {
            bet = 0;
            Disappear();
        }
    }

    public void OnCollect()
    {
        Disappear();
        DestroyContent();
        alert.AppearWithReward(bet);
    }
}
