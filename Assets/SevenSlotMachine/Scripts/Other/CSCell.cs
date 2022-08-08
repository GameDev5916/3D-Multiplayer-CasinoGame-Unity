using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CSCell
{
	public int column /*x*/, row /*y*/;

	public CSCell(int column, int row)
	{
		this.row = row;
		this.column = column;
	}

	public static CSCell Zero()
	{
		return new CSCell (0, 0);
	}

	public static CSCell One()
	{
		return new CSCell (1, 1);
	}

	public static CSCell Up()
	{
		return new CSCell (0, 1);
	}

	public static CSCell Down()
	{
		return new CSCell (0, -1);
	}

	public static CSCell Right()
	{
		return new CSCell (1, 0);
	}

	public static CSCell Left()
	{
		return new CSCell (-1, 0);
	}

	public CSCell NextColumn()
	{
		return new CSCell (column + 1, row);
	}

	public CSCell NextRow()
	{
		return new CSCell (column, row + 1);
	}

	public CSCell PreviousColumn()
	{
		return new CSCell (column - 1, row);
	}

	public CSCell PreviousRow()
	{
		return new CSCell (column, row - 1);
	}

	public static CSCell operator + (CSCell c1, CSCell c2)
	{
		return new CSCell (c1.column + c2.column, c1.row + c2.row);
	}

	public static CSCell operator - (CSCell c1, CSCell c2)
	{
		return new CSCell (c1.column - c2.column, c1.row - c2.row);
	}

	public static Vector3 operator * (Vector3 v, CSCell c)
	{
		return new Vector3 (v.x * (float)c.column, v.y * (float)c.row);
	}

	public static bool operator == (CSCell a, CSCell b)
	{
		return a.column == b.column && a.row == b.row;
	}
	public static bool operator != (CSCell a, CSCell b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		return column == ((CSCell)obj).column && row == ((CSCell)obj).row;
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public override string ToString()
	{
		return column + ", " + row;
	}
}

[System.Serializable]
public class CS2DBoolArray
{
    [System.Serializable]
    public struct L2D
    {
        public bool[] l1;

        public bool this[int i]
        {
            get { return l1[i]; }
            set { l1[i] = value; }
        }
    }
    public L2D[] l0;

    public bool this[int c, int r]
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
