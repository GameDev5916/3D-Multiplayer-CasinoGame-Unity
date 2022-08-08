using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum CSSuit
{
    Club,
    Diamond,
    Heart,
    Spade,
}

public enum CSRank
{
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace,
}

[Serializable]
public struct CSCardValue {
    public CSSuit suit;
    public CSRank rank;

    public int rankIndex {
        get { return (int)rank; }
    }

    public int suitIndex
    {
        get { return (int)suit; }
    }

    public static CSCardValue random
    {
        get {
            CSSuit suit = (CSSuit)Random.Range(0, Enum.GetValues(typeof(CSSuit)).Length);
            CSRank rank = (CSRank)Random.Range(0, Enum.GetValues(typeof(CSRank)).Length);
            return new CSCardValue(suit, rank);
        }
    }

    public CSCardValue (CSSuit suit, CSRank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    public static bool operator ==(CSCardValue a, CSCardValue b)
    {
        return a.suit == b.suit && a.rank == b.rank;
    }
    public static bool operator !=(CSCardValue a, CSCardValue b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return suit == ((CSCardValue)obj).suit && rank == ((CSCardValue)obj).rank;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString()
    {
        return suit + ", " + rank;
    }
}

public class CSCard : MonoBehaviour {
    public CSCardList2D cardDatas;
    public event Action<CSCard> selectedEvent;
    private Sprite _backSprite;
    private Image _image;
    private Button _botton;

    private CSCardData _data;
    public CSCardData data {
        get { return _data; }
    }

    private bool _flip = false;
    public bool flip {
        get { return _flip; }
        set { SetFlip(value); }
    }

    public bool interactible {
        get { return _botton.interactable; }
        set { _botton.interactable = value; }
    }

    private CSCardValue _cardValue = new CSCardValue();
    public CSCardValue cardValue {
        get { return _cardValue; }
        set {
            _cardValue = value;
            _data = DataForValue(value);
        }
    }

    private void Awake()
    {
        _botton = GetComponent<Button>();
        _image = GetComponent<Image>();
        _backSprite = _image.sprite;
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
    }

    public void LoadWitValue(CSCardValue value, bool flipped, Action<CSCard> selectCallback)
    {
        cardValue = value;
        SetFlip(flipped, false);
        selectedEvent += selectCallback;
    }

    private void UnsubscribeEvent()
    {
        if (selectedEvent == null)
            return;

        foreach (var d in selectedEvent.GetInvocationList())
        {
            selectedEvent -= (Action<CSCard>)d;
        }
    }

    public void SetFlip(bool value, bool animate = true)
    {
        if (value == _flip)
            return;
        _flip = value;

        LeanTween.cancel(gameObject);

        if (animate)
        {
            LTSeq seq = LeanTween.sequence();
            seq.append(LeanTween.scaleX(gameObject, 0f, 0.2f));
            seq.append(Flip);
            seq.append(LeanTween.scaleX(gameObject, 1f, 0.2f));
        }
        else
        {
            Flip();
        }
    }

    private void Flip()
    {
        _image.sprite = _flip ? _data.sprite : _backSprite;
    }

    private CSCardData DataForValue(CSCardValue value)
    {
        return cardDatas[value.suitIndex, value.rankIndex];
    }


    public void OnClick()
    {
        Debug.Log("adsf");
        if (selectedEvent != null)
            selectedEvent(this);
    }
}

