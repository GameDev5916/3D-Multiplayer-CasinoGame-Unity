using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CSCSTopPanel : CSTopPanel {
    public CSInfo info;
    public TextMeshProUGUI Chips;

    public void OnInfo()
    {
        info.Appear();
    }
}
