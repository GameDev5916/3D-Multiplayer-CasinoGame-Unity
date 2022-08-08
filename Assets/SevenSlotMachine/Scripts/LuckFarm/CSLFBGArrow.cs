using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSLFBGArrow : MonoBehaviour {
    public GameObject glow;
    private Image _glow;
    private Color clean = new Color(1, 0.9504018f, 0f, 0f);
    private bool _animate;
    public bool animate {
        get { return _animate; }
        set {
            if (_animate == value)
                return;
            _animate = value;
            Animate(value);
        }
    }

    private void Animate(bool value)
    {
        LeanTween.cancel(gameObject);
        LeanTween.cancel(glow);

        if (value)
        {
            Debug.Log("animate");
            transform.localScale = Vector3.one;
            LeanTween.scale(gameObject, new Vector3(0.9f, 1.1f, 1f), 1f).setLoopPingPong(-1);

            LeanTween.value(glow, 0f, 0.5f, 1f).setOnUpdate((v) =>
            {
                clean.a = v;
                _glow.color = clean;
            }).setLoopPingPong(-1);
        }
        else
        {
            transform.localScale = Vector3.one;
            clean.a = 0f;
            _glow.color = clean;
        }
    }

    private void OnDestroy()
    {
        animate = false;
    }

    private void OnDisable()
    {
        animate = false;
    }

    private void Start()
    {
        _glow = glow.GetComponent<Image>();
        //animate = true;
    }
}
