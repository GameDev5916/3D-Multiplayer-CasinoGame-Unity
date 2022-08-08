using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSReel : MonoBehaviour {
	public GameObject symbol;
	private Vector2 _tileSize;
	private int _count = 4;
	private CSSymbol[] _symbols;
	private Vector2 _size;
    private int _column;
    public CS2DBoolArray symbolEnabled;

    private CSReelRandom _reelRandom;

    public CSSymbol this[int index]
    {
        get {
            return _symbols[index];
        }
        set {
            _symbols[index] = value;
        }
    }

	void Awake ()
	{
		_symbols = new CSSymbol[_count];
	}

	public void StartWithSize(Vector2 tile, int idx)
	{
		_tileSize = tile;
        _column = idx;

        _reelRandom = new CSReelRandom(symbol.GetComponent<CSSymbol>().percents, symbolEnabled, _column);
		_size = new Vector2 (tile.x, tile.y * _count);
		(transform as RectTransform).sizeDelta = _size;
		CreateSymbols (idx);
    }

	private void CreateSymbols(int column)
	{
		for (int i = 0; i < _count; i++)
		{
			_symbols[i] = CreateSymbol (new CSCell(column, i));
		}
	}

	private CSSymbol CreateSymbol(CSCell cell)
	{
		CSSymbol script = Instantiate (symbol, transform).GetComponent <CSSymbol>();
        script.StartWith (_reelRandom.SmartRandomSymbol(), cell, PositionForRow (cell.row));
		return script;
	}

	private Vector3 PositionForRow(int row)
	{
		return new Vector3 (0f, _tileSize.y * 0.5f + _tileSize.y * row);
	}

    public LTDescr Animate(System.Action callback)
	{
        return Parallax(6, PositionForRow(-1), PositionForRow(3), 2.5f, 0.1f).setOnComplete(() => {
            CSSoundManager.instance.Play("reel_stop");
            if (callback != null)
                callback();
        });
	}

    private LTDescr Parallax(int roll, Vector3 min, Vector3 max, float duration, float delay)
    {
        int count = _symbols.Length;
        roll *= count;
        float h = _tileSize.y * roll;
        float prev = 0f;

        int lastRoll = roll - count;

        LTDescr action = LeanTween.value(gameObject, 0f, 1f, duration).setOnUpdate((float dt) =>
        {
            float curr = h * dt;
            int currTile = (int)(curr / _tileSize.y);

            float delta = curr - prev;
            for (int i = 0; i < count; i++)
            {
                CSSymbol s = _symbols[i];
                Vector3 v = s.transform.localPosition;
                v.y -= delta;

                if (v.y - min.y <= 0.01f)
                {
                    v.y = max.y + (v.y - min.y);
                    CSSymbolType type = CSSymbolType.SymbolNone;
                    if (currTile > lastRoll && i < count - 1)
                    {
                        type = _reelRandom.SmartRandomSymbol();
                    }
                    else
                    {
                        type = _reelRandom.RandomSymbol();
                    }
                    s.SetType(type);
                }

                s.transform.localPosition = v;
            }
            prev = curr;
        }).setEaseInOutSine().setDelay((float)_column * delay);

        return action;
    }
}