using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CSZLGambleContent : MonoBehaviour {
    public event Action<bool, CSZLGambleContent> ResultEvent;
    public event Action<bool, CSCard, CSCard> CardSelectedEvent;
    [HideInInspector] public List<CSCardValue> cardDeck;
    public GameObject cardPrefab;
    private Transform[] _cards;
    private CanvasGroup _canvas;
    public float space;
    private bool _enable = false;
    public bool enable {
        get { return _enable; }
        set {
            if (_enable == value)
                return;
            _enable = value;
            _canvas.interactable = value;
            _canvas.blocksRaycasts = value;
        }
    }

    private void Awake()
    {
        cardDeck = CreateCardDeck();
        _canvas = GetComponent<CanvasGroup>();
        _cards = CreateCards(5);
        MoveCards(_cards, 0.14f, () => {
            enable = true;
        });
    }

    public Transform[] CreateCards(int count)
    {
        var array = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = CardAtIdx(i, count);
        }
        return array;
    }

    private void MoveCards(Transform[] cards, float delay, System.Action callback)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            LTDescr action = LeanTween.moveLocal(cards[i].gameObject, PositionForIdx(i, cards.Length), 1f)
                                      .setDelay(delay * (float)i)
                                      .setEaseInOutCubic();

            action.setOnStart(() =>
            {
                CSSoundManager.instance.Play("card_place");
            });

            if (i == cards.Length - 1)
            {
                action.setOnComplete(callback);
            }
        }
    }

    public void Clean()
    {
        enable = false;
        LeanTween.alphaCanvas(_canvas, 0f, 0.2f).setDestroyOnComplete(true);
    }

    private Transform CardAtIdx(int idx, int count)
    {
        Transform t = Instantiate(cardPrefab, transform).transform;
        t.localPosition = PositionForIdx(idx, count, (transform as RectTransform).rect.width);
        CSCardValue value = idx == 0 ? FirstCardValue(CSRank.Three, CSRank.King) : RandomCardValue();
        t.GetComponent<CSCard>().LoadWitValue(value, idx == 0, CardSelected);

        return t;
    }

    private Vector2 PositionForIdx(int idx, int count, float offset = 0f)
    {
        float m = (float)(-count / 2 + idx) + ((count % 2 == 0) ? + 0.5f : 0);
        Vector2 size = (cardPrefab.transform as RectTransform).sizeDelta;
        return new Vector2((size.x + space) * m + offset, 0f);
    }

    private List<CSCardValue> CreateCardDeck()
    {
        var deck = new List<CSCardValue>();
        foreach (CSSuit suit in Enum.GetValues(typeof(CSSuit)))
        {
            foreach (CSRank rank in Enum.GetValues(typeof(CSRank)))
            {
                deck.Add(new CSCardValue(suit, rank));
            }
        }
        return deck;
    }

    private CSCardValue RandomCardValue()
    {
        CSCardValue value = cardDeck[Random.Range(0, cardDeck.Count)];
        cardDeck.Remove(value);
        return value;
    }

    private CSCardValue FirstCardValue(CSRank f, CSRank t)
    {
        CSSuit suit = (CSSuit)Random.Range(0, Enum.GetValues(typeof(CSSuit)).Length);
        CSRank rank = (CSRank)Random.Range((int)f, (int)t);
        CSCardValue value = new CSCardValue(suit, rank);
        return value;
    }

    private void CardSelected(CSCard sender)
    {
        if (sender.flip)
            return;
        sender.flip = true;

        enable = false;
        CSCard first = _cards[0].GetComponent<CSCard>();

        bool win = IsWin(first, sender);
        CardSelectedEvent(win, first, sender);

        LeanTween.delayedCall(1f, () =>
        {
            ResultAnimation(first, sender);
        });
    }

    private void ResultAnimation(CSCard c1, CSCard c2)
    {
        bool win = IsWin(c1, c2);
        GameObject obj = win ? c2.gameObject : c1.gameObject;

        LeanTween.moveLocalY(obj, 25f, 0.3f).setEaseInOutSine();
        LeanTween.delayedCall(1.2f, () =>
        {
            if (ResultEvent != null)
                ResultEvent(win, this);
        });
    }

    private bool IsWin(CSCard c1, CSCard c2)
    {
        return c1.cardValue.rankIndex < c2.cardValue.rankIndex;
    }
}
