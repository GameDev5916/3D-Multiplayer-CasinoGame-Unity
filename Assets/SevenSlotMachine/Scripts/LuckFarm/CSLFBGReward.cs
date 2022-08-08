using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CSLFBGRewardTypes
{
    Coins_500,
    Coins_1000,
    Coins_2000,
    Coins_3000,
    FreeSpins_2,
    FreeSpins_5,
    FreeSpins_10,
    Multiplier_1,
    Multiplier_2,
    MillSpins_1,
    MillArrow_1,
    ExpandWild,
}

public class CSLFBGReward : MonoBehaviour {
    public CSLFBGRewardTypes type;
}
