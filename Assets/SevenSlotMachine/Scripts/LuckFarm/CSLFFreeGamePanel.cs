using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CSLFFreeGamePanel : CSZLFreeGamePanel {
    public Text totalBetLabel;
    private int _totalBet;
    public int totalBet
    {
        get { return _totalBet; }
        set
        {
            if (_totalBet == value)
                return;
            _totalBet = value;
            if (totalBetLabel)
                totalBetLabel.text = value.ToString();
        }
    }
}
