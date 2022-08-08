using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Line", menuName = "Game/Game/Line")]
public class CSLine : ScriptableObject
{
    public int number;
    public Sprite sprite;
    public List<CSCell> line;
    public int count
    {
        get
        {
            if (line == null)
                return 0;
            return line.Count;
        }
    }

    public CSCell this[int index]
    {
        get
        {
            return line[index];
        }
        set
        {
            line[index] = value;
        }
    }
}