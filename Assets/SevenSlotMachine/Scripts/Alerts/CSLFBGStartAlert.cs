using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSLFBGStartAlert : CSAlert {
    public CSLFBonusGame bonusGame;
    public CSLFBonusGameSettings settings;

    public void OnStart()
    {
        Disappear();
        BonusGame();
    }

    public void BonusGame()
    {
        bonusGame.Appear(settings);
    }
}

