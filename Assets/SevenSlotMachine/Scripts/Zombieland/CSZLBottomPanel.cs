using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSZLBottomPanel : CSBottomPanel {
    public CSZLGamble gamble;
    public Button gambleButton;

    private bool _gamebleInteractable = false;
    public bool gamebleInteractable {
        get { return _gamebleInteractable; }
        set
        {
            if (_gamebleInteractable == value)
                return;

            _gamebleInteractable = value;
            gambleButton.interactable = value;
        }
    }

    public override void Loaded()
    {
        _step = Mathf.Min(maxStep, CSGameSettings.instance.zombielandBetStep);
        base.Loaded();
    }

    protected override void WinAmount(float value)
    {
        gamebleInteractable = value > 0;
    }

    public void OnGameble()
    {
        gamble.Appear(win, ()=> {
            gamebleInteractable = false;
        });
    }

    //public override void OnSpin()
    //{
    //    base.OnSpin();
    //    base.WinAmount(win);
    //}

    public override void OnChangeMultipiler(int val)
    {
        base.OnChangeMultipiler(val);
        CSGameSettings.instance.zombielandBetStep = _step;
    }

    public override void OnBetMax()
    {
        base.OnBetMax();
        CSGameSettings.instance.zombielandBetStep = _step;
    }
}
