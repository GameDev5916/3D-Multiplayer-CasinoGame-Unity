using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CSLevelUpAlert : CSAlertRewardAnim {
    private int _level;
    public TextMeshProUGUI level;

	private RectTransform _levelUpText;
	private RectTransform _stars;

	private Dictionary<int, StarData> _starPos;

    public override void Awake()
    {
        _stars = title.Find("Stars") as RectTransform;
        _levelUpText = title.Find("LevelUp Text") as RectTransform;

        base.Awake();
    }
    protected override void ScaleActionCompleted()
    {
        base.ScaleActionCompleted();
        StarAction(_stars);
    }

    private float Reward()
    {
        return Mathf.Pow((float)_level, 2) * 0.44f * 1000f;
    }

    public void Appear(int level, Action callback = null)
    {
        _level = level;
        this.level.text = _level.ToString();
        _starPos = DefineStarInit(_stars);
        _reward = Reward();
        Appear(callback);
        CSSoundManager.instance.Play("levelup");
    }

    LTDescr MoveAction(RectTransform obj, Vector3 position, System.Action callback = null)
	{
		LTDescr action = LeanTween.move (obj, position, 1f).setEaseOutCubic ();
		if (callback != null) action.setOnComplete (callback);
		return action;
	}

	void StarAction(RectTransform stars)
	{
		for (int i = 0; i < stars.childCount; i++)
		{
            SetStar(stars.GetChild(i) as RectTransform, _starPos[0], false);// reset pos
			SetStar(stars.GetChild (i) as RectTransform, _starPos[i]);
		}
	}

	private Dictionary<int, StarData> DefineStarInit(RectTransform stars)
	{
		Dictionary<int, StarData> dic = new Dictionary<int, StarData> ();
		for (int i = 0; i < stars.childCount; i++)
		{
			dic.Add (i, new StarData(stars.GetChild (i) as RectTransform));
			SetStar (stars.GetChild (i) as RectTransform, dic[0], false);
		}
		return dic;
	}

	private void SetStar(RectTransform r, StarData data, bool animate = true)
	{
		if (animate)
		{
			LeanTween.move (r, data.position, 1f).setEaseOutBack ();
			LeanTween.rotateZ (r.gameObject, data.rotation.z, 1f).setEaseOutBack ();
			LeanTween.color (r, data.color, 0.2f);
		}
		else
		{
			r.anchoredPosition = data.position;
			r.eulerAngles = data.rotation;
			r.GetComponent <Image> ().color = Color.clear;
		}
	}
}

public class StarData
{
	public Vector3 position;
	public Vector3 rotation;
	public Color color;

	public StarData(RectTransform t)
	{
		position = t.anchoredPosition;
		rotation = t.eulerAngles;
		color = t.GetComponent <Image> ().color;
	}
}