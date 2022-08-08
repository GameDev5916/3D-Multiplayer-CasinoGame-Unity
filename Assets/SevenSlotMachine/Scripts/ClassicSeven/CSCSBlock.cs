using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSCSBlock : MonoBehaviour {
    public GameObject blick;
    private RectTransform _rectTransform;

    private bool _animate = false;
    public bool animate {
        get { return _animate; }
        set {
            if (_animate == value)
                return;
            _animate = value;
            float y = _rectTransform.rect.height * 0.5f + 100f;
            LeanTween.moveLocalY(blick, -y, 4f).setEaseInOutSine().setLoopPingPong(-1);
        }
    }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start ()
    {
        animate = true;
	}

    private void OnDestroy()
    {
        animate = false;
    }
}
