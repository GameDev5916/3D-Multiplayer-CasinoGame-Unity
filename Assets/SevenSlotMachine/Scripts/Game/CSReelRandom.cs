using System.Collections.Generic;
using UnityEngine;

public class CSReelRandom
{
    public List<CSSymbolPercent> symbols;
    private float _totalChance = 0;

    public CSReelRandom(CSSymbolPercent[] percents, CS2DBoolArray enabled, int column)
    {
        this.symbols = new List<CSSymbolPercent>();
        for (int i = 0; i < percents.Length; i++)
        {
            if (enabled[i, column])
            {
                CSSymbolPercent p = percents[i];
                this.symbols.Add(p);
                _totalChance += p.percent;
            }
        }
    }

    public CSSymbolType SmartRandomSymbol()
    {
        float random = Random.Range(0f, _totalChance);
        CSSymbolType symbol = CSSymbolType.SymbolNone;

        for (int i = 0; i < symbols.Count && symbol == CSSymbolType.SymbolNone; i++)
        {
            CSSymbolPercent symbolPercent = symbols[i];
            random -= symbolPercent.percent;

            if (random <= 0)
                symbol = symbolPercent.type;
        }

        Debug.Assert(symbol != CSSymbolType.SymbolNone, "Symbol could not be none, random value: " + random);
        return symbol;
    }

    public CSSymbolType RandomSymbol()
    {
        return symbols[Random.Range(0, symbols.Count)].type;
    }

    public CSSymbolType TrueRandom()
    {
        return CSUtilities.RandomSymbolValue<CSSymbolType>();
    }

    public void Test(int count)
    {
        Dictionary<CSSymbolType, int> dic = new Dictionary<CSSymbolType, int>();
        for (int i = 0; i < count; i++)
        {
            CSSymbolType type = SmartRandomSymbol();
            if (dic.ContainsKey(type))
            {
                dic[type]++;
            }
            else
            {
                dic.Add(type, 1);
            }
        }
        foreach (var item in dic)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }
}