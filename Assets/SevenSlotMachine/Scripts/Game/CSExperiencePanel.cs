using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSExperiencePanel : MonoBehaviour
{
    public RectTransform progress;
    public RectTransform bar;
    public GameObject partilcePrefab;
    public Transform icon;
    public float power;
    public float increasePercent;
    public Text levelText;
    public Text percentText;
    public CSLevelUpAlert alert;

    private Vector2 _size;
    private float _max;
    private float _xp = 0;
    public float xp {
        get { return _xp; }
        set {
            _xp = value;
            CSGameSettings.instance.xp = value;
        }
    }
    private int _level;
    public int level
    {
        get { return _level; }
        set {
            if (_level == value)
                return;
            _level = value;
            CSGameSettings.instance.level = value;
        }
    }

    void Awake()
	{
        CSGameSettings settings = CSGameSettings.instance;
        _level = settings.level;
        _xp = settings.xp;
		_size = progress.sizeDelta;
		_max = MaxForLevel (_level);
        levelText.text = _level.ToString();
        UpdateBar(_xp / _max);
	}

    public void AddValue(float value, RectTransform t = null)
    {
        if (value <= 0)
            return;

        AnimateProgressWith(value, _level, _max, _xp);
        TrueValues(value);
        AddParticle(t);
    }

    private void AnimateProgressWith(float value, int l, float m, float v)
    {
        int curLevel = l;
        float prev = v;
        LeanTween.cancel(gameObject);
        LeanTween.value(gameObject, v, (v + value), 1f).setOnUpdate((float f) =>
        {
            v += f - prev;
            if (v >= m)
            {
                l++;
                levelText.text = l.ToString();
                v -= m;
                m = MaxForLevel(l);
            }
            UpdateBar(Mathf.Clamp(v / m, 0f, 1f));
            prev = f;
        }).setOnComplete(() => {
            if (_level > curLevel)
                Alert();
        });
    }

    private void TrueValues(float value)
    {
        xp += value;
        while (_xp >= _max)
        {
            level++;
            xp -= _max;
            _max = MaxForLevel(_level);
        }
    }
    private void UpdateBar(float p)
    {
        bar.offsetMax = new Vector2(-_size.x - (-_size.x * p), bar.offsetMax.y);
        percentText.text = (p * 100).ToString("F1") + "%";
    }

    private float MaxForLevel(int l)
	{
        return Mathf.Pow(power + power * increasePercent * (l - 1), 2);
	}

    private float AddParticle(RectTransform t)
    {
        if (t == null || !gameObject.activeSelf)
            return 0;

        ParticleSystem p = Instantiate(partilcePrefab, t.position, Quaternion.identity).GetComponent<ParticleSystem>();
        LeanTween.move(p.gameObject, icon.position, p.main.duration * 0.9f);
        return p.main.duration;
    }

    private void Alert()
    {
        //alert.Appear(_level);
    }
}
