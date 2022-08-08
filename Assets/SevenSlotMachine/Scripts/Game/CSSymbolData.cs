using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SymbolData", menuName = "Game/Game/SymbolData")]
public class CSSymbolData : ScriptableObject {
    public CSSymbolType type;
    public GameObject particle;
    public CSAnimationData animationData;
    public Sprite sprite;
    public CSRule rule;
}

[System.Serializable]
public class CSAnimationData
{
    public Sprite[] frames;
    public float duration;
    public float delay;
}

[System.Serializable]
public class CSRule
{
    public List<float> reward;
    public bool substitutesForWild = true;


    public float Win(int matchCount)
    {
        if (reward.Count == 0 || matchCount <= 0) return 0f;
        return reward[Mathf.Min(matchCount, reward.Count) - 1];
    }
}

