using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CSGameScaler : MonoBehaviour {
    private void Scale()
    {
        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();

        if (SystemInfo.deviceModel.Contains("iPad"))
        {
            scaler.matchWidthOrHeight = 0f;
        }

        if (SystemInfo.deviceModel.Contains("iPhone"))
        {
            scaler.matchWidthOrHeight = 1f;
        }
    }
}
