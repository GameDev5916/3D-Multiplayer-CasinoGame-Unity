using UnityEngine;
using UnityEngine.UI;

public class CSLFBottomPanel : CSBottomPanel {
    public override void Loaded()
    {
        _step = Mathf.Min(maxStep, CSGameSettings.instance.luckyFarmBetStep);
        base.Loaded();
    }

    public override void OnChangeMultipiler(int val)
    {
        base.OnChangeMultipiler(val);
        CSGameSettings.instance.luckyFarmBetStep = _step;
    }

    public override void OnBetMax()
    {
        base.OnBetMax();
        CSGameSettings.instance.luckyFarmBetStep = _step;
    }
}
