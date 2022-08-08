using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CSAlertRewardAnim : CSAlert {
    public CSBankCoinPanel coinPanel;
    public TextMeshProUGUI rewardLabel;
    protected float _reward;

    protected override void ScaleActionCompleted()
    {
        base.ScaleActionCompleted();
        AnimateRewardCoins(0, _reward);
    }

    public void AppearWithReward(float reward, Action callback = null)
    {
        _reward = reward;
        Appear(callback);
    }

    public override void Appear(Action callback = null)
    {
        EnableShareButton(false);
        if (rewardLabel != null)
            rewardLabel.text = "";
        base.Appear(callback);
    }

    public void AddCoins()
    {
        if (coinPanel == null)
            return;
        if (_reward <= 0)
            return;
        coinPanel.Add(_reward);
    }

    public void AnimateRewardCoins(float from, float to)
    {
        if (rewardLabel == null)
            return;
        CSUtilities.LabelAction(rewardLabel, from, to, 1f, true, false);
    }

    public override void OnCollect()
    {
        AddCoins();
        base.OnCollect();
    }

    virtual public void OnDouble()
    {
        //CSAdMobManager.instance.ShowAds(() =>
        //{
        //    float newReward = _reward * 2f;
        //    AnimateRewardCoins(_reward, newReward);
        //    _reward = newReward;
        //    EnableShareButton(true);
        //});
    }

    private void EnableShareButton(bool value)
    {
        //Transform share = _board.transform.Find("Layout/Share");
        //Transform doubl = _board.transform.Find("Layout/Double");
        //if (share == null || doubl == null)
        //    return;

        //share.gameObject.SetActive(value);
        //doubl.gameObject.SetActive(!value);
    }
}
