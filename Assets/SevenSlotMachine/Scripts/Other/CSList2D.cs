using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CSCardList2D
{
    [Serializable]
    public struct List2D
    {
        public CSCardData[] l1;

        public CSCardData this[int i]
        {
            get { return l1[i]; }
            set { l1[i] = value; }
        }
    }
    public List2D[] l0;

    public CSCardData this[int c, int r]
    {
        get
        {
            return l0[c][r];
        }
        set
        {
            l0[c][r] = value;
        }
    }
}

