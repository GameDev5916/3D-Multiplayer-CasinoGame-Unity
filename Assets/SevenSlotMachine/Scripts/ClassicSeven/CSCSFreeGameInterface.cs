using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSCSFreeGameInterface : MonoBehaviour, IFreeGame {
    public Sprite freeGameSprite;
    public CSReels reels;
    private Sprite _gamePlaySprite;
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _gamePlaySprite = _image.sprite;
    }

    private void OnEnable()
    {
        reels.FreeGameValueChangedEvent += FreeGameValueChanged;
    }

    private void OnDisable()
    {
        reels.FreeGameValueChangedEvent -= FreeGameValueChanged;
    }

    public void FreeGameValueChanged(bool value)
    {
        _image.sprite = value ? freeGameSprite : _gamePlaySprite;
    }
}
