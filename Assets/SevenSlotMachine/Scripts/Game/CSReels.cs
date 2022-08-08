using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public interface IFreeGame
{
    void FreeGameValueChanged(bool value);
}

public abstract class CSReels : MonoBehaviour {
    public event System.Action<bool> FreeGameValueChangedEvent;
    public GameObject reel;
    public CSLine[] lines;
    public CSBottomPanel basePanel;
    public CSFreeGamePanel freeGamePanel;
    public HorizontalScrollSnap scrollView;
    public CSTopPanel topPanel;
    public CSAlertRewardAnim rewardAlert;
     public CSReel[] reels;
    private Vector2 _tileSize;
    protected bool _autoSpin;
    private CSReelAutoSpin _stateAutoSpin;

    public bool autoSpin
    {
        get { return _autoSpin; }
    }


    private bool _enable;
    public bool enable
    {
        get { return _enable; }
        set
        {
            if (_enable == value)
                return;
            _enable = value;
            if (!_freeGame && !_autoSpin)
            {
                basePanel.SetEnable(value);
            }
        }
    }

    protected CSReelsAnimation _reelAnimation;
    protected bool _freeGame;
    public bool freeGame {
        get { return _freeGame; }
        set {
            if (_freeGame == value)
                return;
            _freeGame = value;
            basePanel.enableSpin = !value;

            if (FreeGameValueChangedEvent != null)
            {
                FreeGameValueChangedEvent(value);
            }

            if (value)
                StartFreeGame();
            else
                EndFreeGame();
        }
    }

    private CSExpandWild _expandWild;

    void Start ()
    {
        _stateAutoSpin = GetComponent<CSReelAutoSpin>();
        _expandWild = GetComponent<CSExpandWild>();
        _reelAnimation = GetComponent<CSReelsAnimation>();
        reels = new CSReel[5];
        _tileSize = GetTileSize ();
        CreateReels ();
        enable = true;
    }

    private Vector2 GetTileSize()
    {
        RectTransform t = transform as RectTransform;
        return new Vector2 (t.rect.size.x / 5f, t.rect.size.y / 3f);
    }

    private void CreateReels()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            reels [i] = CreateReel (i);
        }
    }

    private CSReel CreateReel(int idx)
    {
        CSReel r = Instantiate (reel, transform).GetComponent <CSReel>();
        r.StartWithSize (_tileSize, idx);
        r.transform.localPosition = PositionForColumn (idx);
        r.transform.SetAsFirstSibling();
        return r;
    }

    private Vector3 PositionForColumn(int column)
    {
        return new Vector3 (_tileSize.x * 0.5f + _tileSize.x * column, 0f);
    }

    private void AddXPParticle()
    {
        //coins
    }

    public void Spin()
    {
        if (!_enable)
            return;

        if(NewSloatManager.Instance.TotalBetAMount < 10000)
        {
            Debug.LogError("Please Bet First");
            Constants.ShowWarning("Minimum Bet Amount is 10,000");
            return;
        }

        if(NewSloatManager.Instance.TotalBetAMount > Constants.CHIPS)
        {
            Debug.LogError("Not Have Enough Money:");
            Constants.ShowWarning("Not Have Enough Chips For Bet");
            return;
        }

        Constants.CHIPS -= NewSloatManager.Instance.TotalBetAMount;

        //Debug.LogError("Bet Amount : " + q );

        //if (!_freeGame && basePanel.win > 0f)
        //{
        //    basePanel.coins.Add(basePanel.win, false);
        //}

        //bool canSpin = basePanel.CanSpin;

        //if (canSpin && !_freeGame)
        //{
        //    basePanel.coins.Add(-basePanel.totalBet, false);
        //    basePanel.AddXP();
        //}
        //if (!canSpin && !_freeGame)
        //{
        //    SetAutoSpin(false);
        //    //topPanel.OnBuyCoins();
        //    return;
        //}

        enable = false;
        _reelAnimation.StopAnimatePlayLines();
        basePanel.win = 0;

        PlayerPrefs.SetInt("Total_Spin", PlayerPrefs.GetInt("Total_Spin", 0) + 1);
        print("Spin================>>> " + PlayerPrefs.GetInt("Total_Spin", 0));

        CSSoundManager.instance.Play("spin_start");
        CSSoundManager.instance.Play("reel_spin");

        Animate(() => {
            CSSoundManager.instance.Stop("reel_spin");
            CSSoundManager.instance.Play("spin_stop");
            _expandWild.CheckExpandWild(FindPayLines);
        });
    }

    protected void FreeGameAutoSpin()
    {
        if (freeGamePanel.freeSpins <= 0)
        {
            freeGame = false;
        }
        else
        {
            freeGamePanel.freeSpins -= 1;
            Spin();
        }
    }

    protected void AutoSpin()
    {
        if (freeGame)
            return;
        if (!_autoSpin)
            return;
        Spin();
        print("Free Auto Spin =================================");
    }

    public void SetAutoSpin(bool value)
    {
        if (freeGame)
            return;

        if (_autoSpin == value)
            return;
        _autoSpin = value;

        if (_stateAutoSpin != null)
        {
            _stateAutoSpin.SwapSpinSprite(basePanel);
        }

        if (value)
            AutoSpin();
    }

    private void Animate(System.Action c)
    {
        int count = reels.Length;
        for (int i = 0; i < count; i++)
        {
            reels[i].Animate(i == count - 1 ? c : null);
        }
    }

    public CSSymbol CellToSymbol(CSCell cell)
    {
        CSSymbol symbol = reels[cell.column][cell.row];
        Debug.Assert(symbol != null, "Coundn't find symbol at cell: " + cell);
        return symbol;
    }


    void FindPayLines()
    {
        enable = true;
        var paylines = CSFindMatchManager.instance.GetPayLines(lines, CellToSymbol, _freeGame);
        if (paylines == null || paylines.Count == 0)
        {
            if (_freeGame)
                FreeGameAutoSpin();
            if (_autoSpin)
                AutoSpin();
            return;
        }
        CalculateResult(paylines);
    }

    protected virtual void CalculateResult(List<CSPayline> paylines)
    {
        float lineBet = basePanel.totalBet / lines.Length;
        float win = CSFindMatchManager.instance.WinAmountForPaylines(paylines, lineBet);

        if (win > 0)
        {
            CSSoundManager.instance.Play("Win_1");
        }

        if (_freeGame)
        {
            freeGamePanel.SetWin(win);
            _reelAnimation.StartAnimatePlayLines(paylines, FreeGameAutoSpin);
        }
        else
        {
            basePanel.win = win;
            if (_autoSpin)
            {
                _reelAnimation.StartAnimatePlayLines(paylines, AutoSpin);
            }
            else
            {
                _reelAnimation.StartAnimatePlayLines(paylines);
            }
        }
    }

    public virtual void BonusGame(int scatterCount)
    {
        SetAutoSpin(false);
    }

    public void StartFreeGame()
    {
        _autoSpin = false;
        SetPage(1);
        UpdateFreeGamePanel(true);
        LeanTween.delayedCall(1f, FreeGameAutoSpin);
    }

    public void EndFreeGame()
    {
        SetPage(0);
        UpdateFreeGamePanel(false);
        CSGameManager.instance.expandWild = false;
    }

    private bool SetPage(int page)
    {
        bool changed = page != scrollView.CurrentPage;
        if (changed) scrollView.GoToScreen(page);
        return changed;
    }

    protected virtual void UpdateFreeGamePanel(bool value)
    {
        if (freeGamePanel == null)
            return;

        if (value)
        {
            freeGamePanel.win = 0;
        }
        else
        {
            if (freeGamePanel.win > 0f)
            {
                ShowAlert(freeGamePanel.win);
            }
            freeGamePanel.freeSpins = 0;
        }
    }

    private void ShowAlert(float reward)
    {
        if (rewardAlert == null)
            return;
        rewardAlert.AppearWithReward(reward);
    }
}
