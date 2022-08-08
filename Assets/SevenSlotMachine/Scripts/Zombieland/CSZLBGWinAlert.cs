using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CSZLBGWinAlert : CSAlertRewardAnim {
    public CSZLBonusGame bonusGame;
    public CSLFBGAlertBoard boardScript;
    public CSReels reels;

    public override void Appear(Action callback = null)
    {
        _reward = bonusGame.coins;
        boardScript.multiplier = bonusGame.multiplier;
        boardScript.freeSpins = bonusGame.freeSpins;

        base.Appear(callback);
    }

    public override void OnCollect()
    {
        //(reels.freeGamePanel as CSZLFreeGamePanel).multiplier = boardScript.multiplier + 1;
        reels.basePanel.coins.Add(reels.basePanel.win, false);
        reels.freeGame = true;
        AddCoins();
        Disappear(Reset);
    }

    private void Reset()
    {
        boardScript.multiplier = 0;
        boardScript.freeSpins = 0;
    }
}
