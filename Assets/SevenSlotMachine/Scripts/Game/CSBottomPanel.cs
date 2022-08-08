using UnityEngine;
using UnityEngine.UI;

public class CSBottomPanel : MonoBehaviour {
    public CSBankCoinPanel coins;
    public Button spinButton;

    public Text betLabel;
    private long _totalBet = 0;
    public long totalBet
    {
        get { return _totalBet; }
        set
        {
            if (_totalBet == value)
                return;

            if (betLabel != null)
            {
                BetLabelAnimate(_totalBet, value);
            }

            _totalBet = value;
        }
    }

    public Text winLabel;
    private float _win;
    public float win
    {
        get { return _win; }
        set
        {
            _win = value;
            WinAmount(value);
            if (winLabel != null)
            {
                winLabel.text = value.ToString("");
                Debug.LogError("aaa");
                NewSloatManager.Instance.UpdateWinAmount((long)_win);
            }
        }
    }

    public int minBet;
    public int maxStep;
    public CSExperiencePanel xp;
    protected int _step = 1;

    private bool _enableSpin = true;
    public bool enableSpin {
        get { return _enableSpin; }
        set {
            if (_enableSpin == value)
                return;
            _enableSpin = value;
            spinButton.interactable = value;
        }
    }

    public bool CanSpin
    {
        get {
            return coins.bank >= _totalBet;
        }
    }

    private Button[] _buttons;
    private bool _enable;

    void Awake()
    {
        _buttons = transform.GetComponentsInChildren<Button>();
    }

    void Start()
    {
        Loaded();
    }

    public virtual void Loaded()
    {
        UpdateBet(_step);
    }

    public virtual void OnChangeMultipiler(int val)
    {
        _step = Mathf.Clamp(_step + val, 1, maxStep);
        UpdateBet(_step);
    }

    public virtual void OnBetMax()
    {
        _step = maxStep;
        UpdateBet(_step);
    }

    private void UpdateBet(int val)
    {
        totalBet = minBet * val;
    }

    protected virtual void WinAmount(float value)
    {
        if (value <= 0)
            return;
        coins.bank += value;
    }

    public void AddXP()
    {
        xp.AddValue(_totalBet, betLabel.rectTransform);
    }
    public void SetEnable(bool value)
    {
        if (_enable == value)
            return;
        _enable = value;

        for (int i = 0; i < _buttons.Length; i++)
        {
            if (_buttons[i].tag == "GambleButton")
            {
                continue;
            }
            _buttons[i].interactable = value;
        }
    }

    private void BetLabelAnimate(float from, float to)
    {
        LeanTween.cancel(betLabel.gameObject);
        LeanTween.value(betLabel.gameObject, from, to, 0.2f).setOnUpdate((float v) =>
        {
            betLabel.text = v.ToString("0");
        });
    }
}
