using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CSCSWinAlert : CSAlertRewardAnim {
    public CSLFBGAlertBoard boardScript;
    public CSReels reels;
    private CSCSBonusGameSettings _settings;

    public void Appear(CSCSBonusGameSettings settings, Action callback = null)
    {
        _settings = settings;
        _reward = settings.coins;
        boardScript.multiplier = settings.multiplier;
        boardScript.freeSpins = settings.freeSpins;

        Appear(callback);
    }

    public override void OnCollect()
    {
        reels.basePanel.coins.Add(reels.basePanel.win, false);
        reels.freeGame = true;
        AddCoins();
        Disappear(Reset);
    }

    private void Reset()
    {
        boardScript.multiplier = 0;
        boardScript.freeSpins = 0;
        boardScript.expandWild = false;
    }
}
