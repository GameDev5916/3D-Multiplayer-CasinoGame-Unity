using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class CSZLBonusGame : MonoBehaviour {
    public CSZLBGGrave[] graves;
    public TextMeshProUGUI selectCount;
    private CanvasGroup _canvas;
    public RectTransform hud;
    public RectTransform game;
    public CSZLBGWinAlert alert;
    [HideInInspector] public int multiplier;
    [HideInInspector] public int freeSpins;
    [HideInInspector] public int coins;

    private int _select = 0;
    public int select {
        get { return _select; }
        set {
            if (_select == value)
                return;
            _select = value;
            selectCount.text = "Graves left: " + value;
            if (_select == 0)
            {
                GameOver();
            }
        }
    }

    private bool _enable;
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
        _canvas = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        for (int i = 0; i < graves.Length; i++)
        {
            graves[i].ValueChangedEvent += ValueChanged;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < graves.Length; i++)
        {
            graves[i].ValueChangedEvent -= ValueChanged;
        }
    }

    public void Appear(int count)
    {
        coins = 0;
        multiplier = 0;
        freeSpins = 5;
        select = count;

        Move(true).setOnComplete(() => { enable = true; });
    }

    private void GameOver()
    {
        enable = false;
        LeanTween.delayedCall(1f, ShowAlarm);
    }

    private void ShowAlarm()
    {
        alert.Appear();
        Disappear();
    }


    private void ValueChanged(CSZLBGRewardTypes rewardType)
    {
        if (select <= 0)
            return;

        switch (rewardType)
        {
            case CSZLBGRewardTypes.Coins_500: coins += 500; break;
            case CSZLBGRewardTypes.Coins_1000: coins += 1000; break;
            case CSZLBGRewardTypes.Coins_2000: coins += 2000; break;
            case CSZLBGRewardTypes.FreeSpins_3: freeSpins += 3; break;
            case CSZLBGRewardTypes.FreeSpins_5: freeSpins += 5; break;
            case CSZLBGRewardTypes.FreeSpins_8: freeSpins += 8; break;
            case CSZLBGRewardTypes.Multiplier_1: multiplier += 1; break;
            case CSZLBGRewardTypes.Multiplier_2: multiplier += 2; break;
            default: break;
        }
        select -= 1;
    }

    public void Disappear()
    {
        foreach (var item in graves)
            item.Disappear();
        Move(false);
    }

    private LTDescr Move(bool value)
    {
        LeanTween.moveY(game, value ? -game.rect.size.y : 0f, 1f).setEaseInOutSine();
        return LeanTween.moveY(hud, hud.position.y + (value ? -1f : 1f) * hud.rect.size.y * 0.5f, 1f).setEaseInOutSine();
    }
}
