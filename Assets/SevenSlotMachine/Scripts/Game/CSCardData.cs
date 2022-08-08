using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Game/Game/CardData")]
[System.Serializable]
public class CSCardData : ScriptableObject
{
    public Sprite sprite;
    public CSCardValue value;
}
