using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CSZLGambleAlert : CSAlertRewardAnim {
    public CSBottomPanel hudPanel;

    public  void Appear(float coins)
    {
        _reward = coins;
        Appear(null);
    }

    public override void OnCollect()
    {
        hudPanel.win = 0;
        base.OnCollect();
    }
}
