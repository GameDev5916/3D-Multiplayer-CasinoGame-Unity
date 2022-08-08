using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CSZLBGRewardTypes
{
    Coins_500,
    Coins_1000,
    Coins_2000,
    FreeSpins_3,
    FreeSpins_5,
    FreeSpins_8,
    Multiplier_1,
    Multiplier_2,
}

public class CSZLBGReward : MonoBehaviour {
    public CSZLBGRewardTypes type;

    private bool _animate;
    public bool animate {
        get { return _animate; }
        set {
            if (_animate == value)
                return;
            _animate = value;
            LeanTween.cancel(gameObject);
            LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), value ? 1f : 0f, 1f);
        }
    }
}
