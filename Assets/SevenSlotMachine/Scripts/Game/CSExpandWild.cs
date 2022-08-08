using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSExpandWild : MonoBehaviour {
    private CSReels _reels;

    private void Awake()
    {
        _reels = GetComponent<CSReels>();
    }

    public void CheckExpandWild(System.Action callback)
    {
        if (!CSGameManager.instance.expandWild)
        {
            callback();
            return;
        }

        List<CSSymbol> wilds = GetWilds();
        if (wilds == null || wilds.Count == 0)
        {
            callback();
            return;
        }

        float delay = 0f;

        for (int i = 0; i < wilds.Count; i++)
        {
            CSSymbol symbol = wilds[i];

            float time = SwapMove(symbol);
            LeanTween.delayedCall(time, () => {
                ExpandWild(symbol);
            });
            time += 1f;
            if (delay < time)
                delay = time;
        }

        LeanTween.delayedCall(delay, callback);
    }

    private void ExpandWild(CSSymbol symbol)
    {
        Vector2 pivot = new Vector2(0.5f, 1f - (1f / 3f * 0.5f));
        RectTransform rect = symbol.transform as RectTransform;

        float duration = symbol.SetExpand();
        rect.pivot = pivot;
        LeanTween.delayedCall(duration * 0.7f, () => { SetReelsWild(symbol); });
    }

    private float SwapMove(CSSymbol symbol)
    {
        if (symbol.cell.row >= 2)
            return 0f;
        CSSymbol top = _reels.CellToSymbol(new CSCell(symbol.cell.column, 2));
        symbol.transform.SetSiblingIndex(_reels.reels.Length);

        return SwapSymbols(symbol, top, 10);
    }

    private float SwapSymbols(CSSymbol s1, CSSymbol s2, float speed)
    {
        _reels.reels[s1.cell.column][s1.cell.row] = s2;
        _reels.reels[s2.cell.column][s2.cell.row] = s1;

        CSCell t = s1.cell;
        s1.cell = s2.cell;
        s2.cell = t;

        Vector3 p1 = s1.transform.position;
        Vector3 p2 = s2.transform.position;

        float duration = CSUtilities.CalculateDuration(p1, p2, speed);

        LeanTween.move(s1.gameObject, p2, duration);
        LeanTween.move(s2.gameObject, p1, duration);
        return duration;
    }

    private List<CSSymbol> GetWilds()
    {
        var wilds = new List<CSSymbol>();
        for (int c = 0; c < _reels.reels.Length; c++)
        {
            for (int r = 2; r >= 0; r--)
            {
                if (_reels.reels[c][r].type == CSSymbolType.SymbolWild)
                {
                    wilds.Add(_reels.reels[c][r]);
                    break;
                }
            }
        }
        return wilds;
    }

    private void SetReelsWild(CSSymbol symbol)
    {
        for (int i = 0; i < 3; i++)
        {
            CSSymbol s = _reels.reels[symbol.cell.column][i];
            //if (symbol == s) continue;
            //if (s.type == CSSymbolType.SymbolWild) continue;
            //s.SetType(CSSymbolType.SymbolWild);
            s.type = CSSymbolType.SymbolWild;
            s.replacement = symbol;
        }
    }
}
