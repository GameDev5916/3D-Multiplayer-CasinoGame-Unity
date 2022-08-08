using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSReelsAnimation : MonoBehaviour {
    public List<GameObject> All_Lines;
    private CSReels _reels;
    private Image _line;

    private IEnumerator _paylineCoroutine = null;
    private CSListNavigation<CSPayline> _paylines = null;

	private void Awake()
	{
        _line = transform.Find("Line").GetComponent<Image>();
        _reels = GetComponent<CSReels>();
	}

    public void StartAnimatePlayLines(List<CSPayline> paylines, System.Action callback = null)
    {
        if (paylines.Count == 0 || _paylines != null)
            return;

        _paylines = new CSListNavigation<CSPayline>(paylines);
        _paylineCoroutine = callback == null ? AnimatePaylines(_paylines) : AnimatePaylinesFreeGame(_paylines, callback);
        StartCoroutine(_paylineCoroutine);
    }

    public void StopAnimatePlayLines()
    {
        if (_paylines == null)
            return;

        StopCoroutine(_paylineCoroutine);
        StopPayline(_paylines.Current);
        DestroyParticlePayline(_paylines.Current);

        foreach (GameObject obj in All_Lines)
        {
            obj.SetActive(false);
            obj.transform.SetSiblingIndex(6);
        }
        _line.color = new Color(1f, 1f, 1f, 0f);
        _paylineCoroutine = null;
        _paylines = null;
    }

    private IEnumerator AnimatePaylines(CSListNavigation<CSPayline> paylines)
    {
        StartPayline(paylines.Current);

        while (true)
        {
            yield return new WaitForSeconds(1.5f);
            StopPayline(paylines.Current);
            yield return new WaitForSeconds(0.5f);
            StartPayline(paylines.Next);
        }
    }

    private IEnumerator AnimatePaylinesFreeGame(CSListNavigation<CSPayline> paylines, System.Action callback)
    {
        float multiplier = 0.5f;
        for (int i = 0; i < paylines.Count; i++)
        {
            StartPayline(paylines[i], multiplier);
            yield return new WaitForSeconds(1.5f * multiplier);
            StopPayline(paylines[i]);
            yield return new WaitForSeconds(0.5f * multiplier);
        }

        if (callback != null)
            callback();
    }

    private void StartPayline(CSPayline payline, float multiplier = 1f)
    {
        if (payline.type != CSSymbolType.SymbolScatter)
            SetLineForPayline(payline);
        else
            ScatterPayline(payline as CSScatterPlayLine);

        foreach (var item in payline.symbols)
        {
            item.StartAnimation(payline, multiplier);
        }
    }

    private void ScatterPayline(CSScatterPlayLine payline)
    {
        if (payline.runned)
            return;
        payline.runned = true;
        _reels.BonusGame(payline.symbols.Count);
    }

    private void StopPayline(CSPayline payline)
    {
        _line.color = new Color(1f, 1f, 1f, 0f);
        foreach (var item in payline.symbols)
        {
            item.StopAnimation();
        }
    }

    public void DestroyParticlePayline(CSPayline payline)
    {
        foreach (var item in payline.symbols)
        {
            item.DestroyParticle();
        }
    }

    private void SetLineForPayline(CSPayline payline)
    {
        _line.color = new Color(1f, 1f, 1f, 1f);

        int index = Mathf.Clamp(payline.count, 0, _reels.reels.Length);
        //_line.transform.SetSiblingIndex(_reels.reels.Length - index);
        //_line.sprite = payline.line.sprite;

        foreach (GameObject obj in All_Lines)
        {
            obj.SetActive(false);
            obj.transform.SetSiblingIndex(6);
        }
        All_Lines[payline.line.number - 1].transform.SetSiblingIndex(_reels.reels.Length - index);
        All_Lines[payline.line.number - 1].SetActive(true);
    }
}
