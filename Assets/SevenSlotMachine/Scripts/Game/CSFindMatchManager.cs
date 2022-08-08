using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSFindMatchManager : MonoBehaviour {
    public static CSFindMatchManager instance = null;

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public List<CSPayline> GetPayLines(CSLine[] lines, System.Func<CSCell, CSSymbol> symbolToCell, bool freeGame = false)
    {
        List<CSPayline> paylines = new List<CSPayline>();

        for (int r = 0; r < lines.Length; r++)
        {
            CSLine line = lines[r];
            CSPayline payline = new CSPayline(line);

            for (int i = 0; i < line.count; i++)
            {
                CSSymbol symbol = symbolToCell(line[i]);
                if (!payline.AddSymbol(symbol))
                {
                    break;
                }
            }

            if (payline.isWin())
            {
                paylines.Add(payline);
            }
        }

        if (!freeGame)
        {
            CSScatterPlayLine scatterPlayLine = CheckScatter(symbolToCell);
            if (scatterPlayLine != null)
                paylines.Insert(0, scatterPlayLine);
        }

        return paylines;
    }

    public CSScatterPlayLine CheckScatter(System.Func<CSCell, CSSymbol> symbolToCell)
    {
        CSScatterPlayLine scatter = new CSScatterPlayLine();

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                CSSymbol symbol = symbolToCell(new CSCell(c, r));
                if (symbol.type == CSSymbolType.SymbolScatter)
                {
                    scatter.AddSymbol(symbol);
                }
            }
        }
        return scatter.symbols.Count >= 3 ? scatter : null;
    }

    public float WinAmountForPaylines(List<CSPayline> paylines, float lineBet)
    {
        float win = 0;
        for (int i = 0; i < paylines.Count; i++)
        {
            win += lineBet * paylines[i].win;
        }
        return win;
    }
}

public class CSListNavigation<T> : List<T>
{
    private int _idx = 0;
    public int idx
    {
        get { return _idx; }
        set { _idx = CSInfinScrollValue(value, 0, Count - 1); }
    }

    public CSListNavigation(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Add(list[i]);
        }
    }

    private static int CSInfinScrollValue(int cur, int min, int max)
    {
        int v = cur;
        if (cur < 0) v = max;
        if (cur > max) v = min;
        return v;
    }

    public T Next
    {
        get { idx++; return this[idx]; }
    }

    public T Previous
    {
        get { idx--; return this[idx]; }
    }

    public T Current
    {
        get { return this[idx]; }
    }
}

public class CSPayline
{
    public CSLine line;
    public CSSymbolType type;
    public List<CSSymbol> symbols;
    public float win;

    public int count {
        get {
            if (symbols == null)
                return 0;
            return symbols.Count;
        }
    }

    public CSPayline(CSLine line = null)
    {
        this.line = line;
        this.type = CSSymbolType.SymbolNone;
        this.symbols = new List<CSSymbol>();

        win = 0f;
    }

    public virtual bool AddSymbol(CSSymbol symbol)
    {
        Debug.Assert(symbol != null, "Symbol could not be null");

        if (symbol.type == CSSymbolType.SymbolScatter)
        {
            return false;
        }

        if (symbols.Contains(symbol))
        {
            return true;
        }

        if (symbols.Count == 0)
        {
            type = symbol.type;
        }
        else
        {
            if (type == CSSymbolType.SymbolWild)
                type = symbol.type;
        }

        if (type != symbol.type && symbol.type != CSSymbolType.SymbolWild)
            return false;

        symbols.Add(symbol);

        return true;
    }

    public bool isWin()     {         if (type == CSSymbolType.SymbolNone)             return false;
         win = GetRule().Win(count);         return win > 0f;     }

    private CSRule GetRule()
    {
        CSRule rule = null;
        for (int i = 0; i < symbols.Count && rule == null; i++)
        {
            CSSymbol symbol = symbols[i];
            if (symbol.type == type)
                rule = symbol.Rule();
        }

        Debug.Assert(rule != null, "Could not find rule for type: " + type);
        return rule;
    }
}

public class CSScatterPlayLine : CSPayline
{
    public bool runned = false;

    public CSScatterPlayLine() : base() {
        this.type = CSSymbolType.SymbolScatter;
    }

	public override bool AddSymbol(CSSymbol symbol)
	{
        if (symbol.type != CSSymbolType.SymbolScatter || symbols.Contains(symbol))
            return false;

        symbols.Add(symbol);
        return true;
    }
}