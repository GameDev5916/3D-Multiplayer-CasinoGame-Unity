using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSZLTopPanel : CSLFTopPanel
{
    public CSSettingsPanel settings;

    public void OnSettings()
    {
        settings.Appear();
    }
}
