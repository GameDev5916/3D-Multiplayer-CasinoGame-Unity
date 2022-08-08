using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public struct CSLFBonusGameSettings
{
    public int spins;
    public int arrows;
    public int freeSpins;

    public CSLFBonusGameSettings (int spins, int arrows, int freeSpins)
    {
        this.spins = spins;
        this.arrows = arrows;
        this.freeSpins = freeSpins;
    }

    public static CSLFBonusGameSettings Zero {
        get { return new CSLFBonusGameSettings(0, 0, 0); }
    }
}

public class CSLFBonusGame : MonoBehaviour {
    public CSLFBGMill mill;
    public GameObject tutorial;
    public TextMeshProUGUI coinLabel;
    public GameObject coinEnable;
    public CSLFBSWinAlert alert;
    private bool _running;
    private CanvasGroup _canvas;

    private int _coins;
    public int coins {
        get { return _coins; }
        set {
            _coins = value;
            coinLabel.text = value.ToString();
            coinEnable.SetActive(value > 0);
        }
    }

    public TextMeshProUGUI multiplierLabel;
    public GameObject multilierEnable;
    private int _mulitplier;
    public int mulitplier
    {
        get { return _mulitplier; }
        set
        {
            _mulitplier = value;
            multiplierLabel.text = "X" + value.ToString();
            multilierEnable.SetActive(value > 0);
        }
    }

    public TextMeshProUGUI freeSpinsLabel;
    public GameObject freeSpinsEnable;
    private int _freeSpins;
    public int freeSpins
    {
        get { return _freeSpins; }
        set
        {
            _freeSpins = value;
            freeSpinsLabel.text = value.ToString();
            freeSpinsEnable.SetActive(value > 0);
        }
    }

    public TextMeshProUGUI millSpinLabel;
    private int _millSpins;
    public int millSpins
    {
        get { return _millSpins; }
        set
        {
            if (_millSpins == value)
                return;
            _millSpins = value;
            millSpinLabel.text = "Mill \nSpins: " + value.ToString();
        }
    }

    public TextMeshProUGUI millArrowsLabel;
    private int _millArrows;
    public int millArrows
    {
        get { return _millArrows; }
        set
        {
            value = Mathf.Min(value, 3);
            _millArrows = value;
            millArrowsLabel.text = "Mill \nArrows: " + (value + 1).ToString();
            mill.millCount = Mathf.Max(value, 0);
        }
    }

    public GameObject expandWildGameObject;
    private bool _expandWild;
    public bool expandWild
    {
        get { return _expandWild; }
        set
        {
            _expandWild = value;
            expandWildGameObject.SetActive(value);
        }
    }

    private bool _active;
    public bool active
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;
            _active = value;
            CanvasStatus(_active);
        }
    }

    private void Awake()
    {
        _canvas = GetComponent<CanvasGroup>();
    }

    public void Appear(CSLFBonusGameSettings settings)
    {
        _mulitplier = 1;
        millSpins = settings.spins;
        millArrows = settings.arrows;
        freeSpins = settings.freeSpins;

        active = true;
        LeanTween.alphaCanvas(_canvas, 1f, 0.4f);

        //AnimateTutorial(true);
    }

    public void Disappear(System.Action callback = null)
    {
        LeanTween.alphaCanvas(_canvas, 0f, 0.4f).setOnComplete(() => {
            active = false;
            if (callback != null)
                callback();
        });
    }

    private void OnEnable()
    {
        mill.RotatetionEnded += RotationEnded;
    }

    private void OnDisable()
    {
        mill.RotatetionEnded -= RotationEnded;
        //AnimateTutorial(false);
    }

    private void RotationStart()
    {
        if (_running || _millSpins <= 0)
            return;
        _running = true;

        millSpins -= 1;
        mill.Roate();
    }

    private void RotationEnded(List<CSLFBGReward> list)
    {
        foreach (var item in list)
        {
            RewardForType(item.type);
        }
        _running = false;
        IsGameOver();
    }

    private void RewardForType(CSLFBGRewardTypes reward)
    {
        switch (reward)
        {
            case CSLFBGRewardTypes.Coins_500: coins += 500; break;
            case CSLFBGRewardTypes.Coins_1000: coins += 1000; break;
            case CSLFBGRewardTypes.Coins_2000: coins += 2000; break;
            case CSLFBGRewardTypes.Coins_3000: coins += 3000; break;
            case CSLFBGRewardTypes.FreeSpins_2: freeSpins += 2; break;
            case CSLFBGRewardTypes.FreeSpins_5: freeSpins += 5; break;
            case CSLFBGRewardTypes.FreeSpins_10: freeSpins += 10; break;
            case CSLFBGRewardTypes.MillArrow_1: millArrows += 1; break;
            case CSLFBGRewardTypes.MillSpins_1: millSpins += 1; break;
            case CSLFBGRewardTypes.Multiplier_1: mulitplier += 1; break;
            case CSLFBGRewardTypes.Multiplier_2: mulitplier += 2; break;
            case CSLFBGRewardTypes.ExpandWild: {
                    expandWild = true;
                    CSGameManager.instance.expandWild = true;
                    break;
                }
            default: break;
        }
    }

    private void AnimateTutorial(bool value)
    {
        if (value)
        {
            LeanTween.cancel(tutorial);
            tutorial.transform.localScale = Vector3.one;
            LeanTween.scale(tutorial, Vector3.one * 1.1f, 1f).setLoopPingPong(-1);
        }
        else
        {
            LeanTween.cancel(tutorial);
        }
    }

    private void IsGameOver()
    {
        if (_millSpins > 0)
            return;
        LeanTween.delayedCall(1f, () =>
        {
            Disappear();
            alert.Appear();
        });
    }

    private void CanvasStatus(bool value)
    {
        _canvas.blocksRaycasts = value;
        _canvas.interactable = value;
        //_canvas.alpha = value ? 1f : 0f;
    }

    public void OnSpin()
    {
        RotationStart();
    }
}
