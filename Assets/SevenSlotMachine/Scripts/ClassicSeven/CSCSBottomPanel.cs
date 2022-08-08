using UnityEngine;
using UnityEngine.UI;

public class CSCSBottomPanel : CSBottomPanel {
    public Button gamble;
    public CSCSGamble gambleGame;
    public CSReels reels;

    private Vector3 _startPosition;
    private bool _showGamble = true;
    public bool showGamble {
        get { return _showGamble; }
        set {
            SetShowGamble(value);
        }
    }

    public override void Loaded()
    {
        _startPosition = gamble.transform.localPosition;
        SetShowGamble(false, false);

        _step = Mathf.Min(maxStep, CSGameSettings.instance.classicSevenBetStep);
        base.Loaded();
    }

    public override void OnChangeMultipiler(int val)
    {
        base.OnChangeMultipiler(val);
        CSGameSettings.instance.classicSevenBetStep = _step;
    }

    public override void OnBetMax()
    {
        base.OnBetMax();
        CSGameSettings.instance.classicSevenBetStep = _step;
    }

    private void SetShowGamble(bool value, bool animate = true)
    {
        if (_showGamble == value)
            return;
        _showGamble = value;
        gamble.interactable = value;
        Vector3 vector = value ? _startPosition : _startPosition -
            new Vector3(0f, (gamble.transform as RectTransform).sizeDelta.y);

        if (animate)
        {
            Move(vector.y);
        }
        else
        {
            gamble.transform.localPosition = vector;
        }
    }

    private LTDescr Move(float to)
    {
        LeanTween.cancel(gamble.gameObject);
        return LeanTween.moveLocalY(gamble.gameObject, to, 0.3f).setEaseInOutSine();
    }

    public void OnGamble()
    {
        reels.SetAutoSpin(false);
        gambleGame.Appear(win, () =>
        {
            showGamble = false;
        });
    }

    protected override void WinAmount(float value)
    {
        //Debug.LogError(value);
        //NewSloatManager.Instance.UpdateWinAmount((long)value);
        showGamble = value > 0;
    }
}
