using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CSCSBonusGameSettings
{
    public int freeSpins;
    public float coins;
    public int multiplier;

    public CSCSBonusGameSettings(int freeSpins, float coins, int multiplier)
    {
        this.freeSpins = freeSpins;
        this.coins = coins;
        this.multiplier = multiplier;
    }

    public static CSCSBonusGameSettings Zero
    {
        get { return new CSCSBonusGameSettings(0, 0f, 0); }
    }
}


public class CSCSReels : CSReels {
    public CSCSWinAlert alert;
    private CSCSBonusGameSettings _settings;

    protected override void UpdateFreeGamePanel(bool value)
    {
        base.UpdateFreeGamePanel(value);
        CSCSFreeGamePanel freePanel = (freeGamePanel as CSCSFreeGamePanel);

        if (value)
        {
            freePanel.totalBet = (int)basePanel.totalBet;
            freePanel.multiplier = _settings.multiplier;
            freePanel.freeSpins = _settings.freeSpins;
        }
    }

    public override void BonusGame(int scatterCount)
    {
        base.BonusGame(scatterCount);
        _settings = SettingsForScatter(scatterCount);
        alert.Appear(_settings);
    }

    protected CSCSBonusGameSettings SettingsForScatter(int count)
    {
        Debug.Assert(count >= 3, "Table Scatter could not be less the 3: " + count);
        count = Mathf.Min(5, count);
        CSCSBonusGameSettings settings = CSCSBonusGameSettings.Zero;

        switch (count)
        {
            case 3: settings = new CSCSBonusGameSettings(8, 1000, 2); break;
            case 4: settings = new CSCSBonusGameSettings(12, 2500, 3); break;
            case 5: settings = new CSCSBonusGameSettings(15, 5000, 3); break;
            default: break;
        }
        return settings;
    }
}
