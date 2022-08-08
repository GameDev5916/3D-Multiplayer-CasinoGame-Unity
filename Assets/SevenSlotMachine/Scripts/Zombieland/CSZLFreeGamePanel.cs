using UnityEngine;
using TMPro;

public class CSZLFreeGamePanel : CSFreeGamePanel {
    public GameObject multiplierGameObject;
    private bool _muliplierEnable;
    public bool muliplierEnable
    {
        get { return _muliplierEnable; }
        set
        {
            _muliplierEnable = value;
            if (multiplierGameObject != null)
                multiplierGameObject.SetActive(value);
        }
    }

    public TextMeshProUGUI multiplierLabel;
    private int _multiplier;
    public int multiplier
    {
        get { return _multiplier; }
        set
        {
            _multiplier = value;
            //muliplierEnable = !(value <= 1);
            if (multiplierLabel != null)
                multiplierLabel.text = "X" + value.ToString();
        }
    }

    public override void SetWin(float value)
    {
        if (multiplier <= 1)
        {
            base.SetWin(value);
        }
        else
        {
            win += multiplier * value;
        }
    }
}
