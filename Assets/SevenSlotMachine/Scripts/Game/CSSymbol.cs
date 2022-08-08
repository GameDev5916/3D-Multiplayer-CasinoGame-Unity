using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public enum CSSymbolType
{
    SymbolNone,
    SymbolWild,
    SymbolScatter,
    Symbol_3,
    Symbol_4,
    Symbol_5,
    Symbol_6,
    Symbol_7,
    Symbol_8,
    Symbol_9,
    Symbol_10,
    Symbol_11,
    Symbol_12,
    Symbol_13,
    //SymbolXP,
}

[RequireComponent(typeof(Image))]
public abstract class CSSymbol : MonoBehaviour {
    public CSSymbolData[] data;
    protected CSSymbolData curr = null;
    public Dictionary<CSSymbolType, CSSymbolData> dictionaryData;
    public CSSymbolType type;
    public CSCell cell;
    public CSSymbolData wildExpand;
    public CSSymbolData wildExpandStay;
    [HideInInspector] public CSSymbol replacement = null;
    [HideInInspector] public CSSymbolPercent[] percents;

	private Image _image;
    private int _animationId;
    protected ParticleSystem _particle;
    private RectTransform _rect;
    private float _totalChance;

	void Awake()
	{
        _rect = transform as RectTransform;
        _image = GetComponent <Image> ();
        dictionaryData = CreateDictionaryData(data);
    }

	public void StartWith(CSSymbolType t, CSCell cell, Vector3 position)
	{
		this.cell = cell;
        _rect.localPosition = position;
		SetType (t);
	}

    public void SetType(CSSymbolType t)
	{
        /*
            Disabled because we should reset pivot even when types are matching
            Enable because on expand wild we set size to SetNativeSize
            Disabled once again we are checking on CSExpandWild method SetReelsWild if type == wild continue
        */
        //if (type == t) return;

		if (t == CSSymbolType.SymbolNone)
        {
			_image.sprite = null;
			_image.rectTransform.sizeDelta = Vector3.zero;
			return;
		}

        type = t;
        _rect.pivot = new Vector2(0.5f, 0.5f);
        replacement = null;

        curr = dictionaryData[t];
        Debug.Assert(curr != null, "Coundn't find data for type " + t);

        _image.sprite = curr.sprite;
		_image.SetNativeSize ();
        //_rect.sizeDelta = new Vector2(118, 120);
        _rect.sizeDelta = new Vector2(100, 102);
    }


    public virtual void AddParticle(CSPayline payline = null, float mult = 1f)
    {
        Debug.Assert(curr != null, "Current Data cound not be null for type:" + type);
        if (curr.particle == null)
            return;

        GameObject obj = Instantiate(curr.particle);
        obj.transform.position = transform.position;
        _particle = obj.GetComponent<ParticleSystem>();

        LifeTimeParticles(_particle.GetComponentsInChildren<ParticleSystem>(), mult);

        _particle.Play();
    }

    private void LifeTimeParticles(ParticleSystem[] components, float mult)
    {
        List<ParticleSystem> particles = new List<ParticleSystem>(components);

        for (int i = 0; i < particles.Count; i++)
        {
            ParticleSystem.MainModule main = particles[i].main;
            main.duration = main.duration * mult;
            ParticleSystem.MinMaxCurve curve = main.startLifetime;
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant: curve.constant = curve.constant * mult;  break;
                case ParticleSystemCurveMode.TwoConstants:
                    curve = new ParticleSystem.MinMaxCurve(curve.constantMin * mult,
                                                           curve.constantMax * mult); break;
                default:
                    break;
            }
            main.startLifetime = curve;
        }
    }

    public virtual void DestroyParticle()
    {
        if (_particle == null)
            return;
        if (_particle.isPlaying)
        {
            Destroy(_particle.gameObject);
        }
    }

    public virtual void StartAnimation(CSPayline payline, float multiplier = 1f)
    {
        if (replacement != null)
        {
            _animationId = replacement.ExpandWildStayAnimation(multiplier);
        }
        else
        {
            _animationId = CSUtilities.AnimateWithFrames(_image, curr.animationData, multiplier).id;
            AddParticle(payline, multiplier);
        }
    }

    private int ExpandWildStayAnimation(float multiplier)
    {
        return CSUtilities.AnimateWithFrames(_image, wildExpandStay.animationData, multiplier).id;
    }

    public virtual void StopAnimation()
    {
        LeanTween.cancel(_animationId);
    }

    private Dictionary<CSSymbolType, CSSymbolData> CreateDictionaryData(CSSymbolData[] array)
    {
        Dictionary<CSSymbolType, CSSymbolData> dictionary = new Dictionary<CSSymbolType, CSSymbolData>();
        for (int i = 0; i < array.Length; i++)
        {
            dictionary.Add(array[i].type, array[i]);
        }


        return dictionary;
    }

    public CSRule Rule()
    {
        return curr.rule;
    }

    public float SetExpand()
    {
        _image.sprite = wildExpand.animationData.frames[0];
        _image.SetNativeSize();
        CSUtilities.AnimateWithFrames(_image, wildExpand.animationData);
        return wildExpand.animationData.duration;
    }
}

[System.Serializable]
public struct CSSymbolPercent {
    public float percent;
    public CSSymbolType type;

    public override string ToString()
    {
        return percent + ", " + type;
    }
}