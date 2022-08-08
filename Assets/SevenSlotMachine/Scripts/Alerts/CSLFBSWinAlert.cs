using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSLFBSWinAlert : CSAlertRewardAnim {
    public CSLFBonusGame bonusGame;
    public CSLFBGAlertBoard boardScript;
    public CSReels reels;

    public override void Appear(Action callback = null)
    {
        _reward = bonusGame.coins;
        boardScript.multiplier = bonusGame.mulitplier;
        boardScript.freeSpins = bonusGame.freeSpins;
        boardScript.expandWild = CSGameManager.instance.expandWild;

        base.Appear(callback);
    }

    public override void OnCollect()
    {
        reels.basePanel.coins.Add(reels.basePanel.win, false);
        AddCoins();
        reels.freeGame = true;
        Disappear(Reset);
    }

    private void Reset()
    {
        boardScript.multiplier = 0;
        boardScript.freeSpins = 0;
        boardScript.expandWild = false;

        bonusGame.coins = 0;
        bonusGame.mulitplier = 0;
        bonusGame.freeSpins = 0;
        bonusGame.expandWild = false;
        bonusGame.millArrows = 0;
    }
}
