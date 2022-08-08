using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSZLBGStartAlert : CSAlert {
    public CSZLBonusGame bonusGame;
    public CSReels reels;
    [HideInInspector] public int graveCount;

    public override void OnCollect()
    {
        base.OnCollect();
        bonusGame.Appear(graveCount);
        reels.GetComponent<CSReelsAnimation>().StopAnimatePlayLines();
    }
}
