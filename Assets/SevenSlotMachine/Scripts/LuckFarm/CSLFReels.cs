using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSLFReels : CSReels {
    public CSLFBGStartAlert alert;

    protected override void UpdateFreeGamePanel(bool value)
    {
        base.UpdateFreeGamePanel(value);
        CSLFFreeGamePanel freePanel = (freeGamePanel as CSLFFreeGamePanel);

        if (value)         {             freePanel.totalBet =(int) basePanel.totalBet;             freePanel.multiplier = alert.bonusGame.mulitplier;
            freePanel.freeSpins = alert.bonusGame.freeSpins;         }
    }

    public override void BonusGame(int scatterCount)
    {
        base.BonusGame(scatterCount);
        alert.settings = SettingsForScatter(scatterCount);
        alert.Appear();
    }

    protected CSLFBonusGameSettings SettingsForScatter(int count)
    {
        Debug.Assert(count >= 3, "Table Scatter could not be less the 3: " + count);
        count = Mathf.Min(5, count);
        CSLFBonusGameSettings settings = CSLFBonusGameSettings.Zero;

        switch (count)
        {
            case 3: settings = new CSLFBonusGameSettings(3, 0, 5); break;
            case 4: settings = new CSLFBonusGameSettings(4, 0, 7); break;
            case 5: settings = new CSLFBonusGameSettings(5, 0, 10); break;
            default: break;
        }
        return settings;
    }
}
