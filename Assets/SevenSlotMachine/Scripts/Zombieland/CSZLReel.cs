using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSZLReel : CSReels {
    public CSZLBGStartAlert alert;

    public override void BonusGame(int scatterCount)
    {
        base.BonusGame(scatterCount);
        alert.graveCount = SettingsForScatter(scatterCount);
        alert.Appear();
    }

    protected override void UpdateFreeGamePanel(bool value)
    {
        base.UpdateFreeGamePanel(value);
        CSZLFreeGamePanel freePanel = (freeGamePanel as CSZLFreeGamePanel);

        if (value)
        {
            freePanel.multiplier = alert.bonusGame.multiplier + 1;
            freePanel.freeSpins = alert.bonusGame.freeSpins;
        }
        else
        {
            //freePanel.muliplierEnable = false;
        }
    }

    protected int SettingsForScatter(int count)
    {
        Debug.Assert(count >= 3, "Table Scatter could not be less the 3: " + count);
        count = Mathf.Min(5, count);

        int graveCount = 3;
        switch (count)
        {
            case 3: graveCount = 3; break;
            case 4: graveCount = 4; break;
            case 5: graveCount = 5; break;
            default: break;
        }
        return graveCount;
    }
}
