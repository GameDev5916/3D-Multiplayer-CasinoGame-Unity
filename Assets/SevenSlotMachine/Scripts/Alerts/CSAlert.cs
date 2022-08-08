using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CSAlert : MonoBehaviour {
    public ParticleSystem fireworks;
    public RectTransform ribbon;
    public Transform title;
    public Transform board;
    public CanvasGroup canvas;
    public CanvasGroup innerCanvas;

    public GameObject _title;
    protected GameObject _board;
    private float _ribbonWidth;
    private Image _image;
    private GameObject _particle;
    private CSGlowAnimation _glowScript;

    private Vector3 _titleShowPosition;
    private Vector3 _boardShowPosition;

    private bool _active;
    public bool active
    {
        get { return _active; }
        set
        {
            if (value == _active)
                return;
            _active = value;
            CanvasStatus(_active);
        }
    }

    public virtual void Awake()
    {
        _image = GetComponent<Image>();
        _ribbonWidth = ribbon.rect.width;
        _title = title.transform.Find("Title").gameObject;
        _glowScript = _title.GetComponent<CSGlowAnimation>();
        _board = board.GetChild(0).gameObject;

        _titleShowPosition = _title.transform.localPosition;
        _boardShowPosition = _board.transform.localPosition;

        Init();
    }

    virtual public void Appear(System.Action callback = null)
    {
        CSSoundManager.instance.Play("Win_0");
        HideBoards();
        active = true;
        AddPaticle();
        //_image.color = Color.clear;
        LeanTween.alpha(_image.rectTransform, 0.8f, 0.4f);
        ScaleAction(() => {
            ScaleActionCompleted();
            if (callback != null)
                callback();
        });
    }

    protected virtual void ScaleActionCompleted()
    {
        MoveAction(_title, _titleShowPosition);
        MoveAction(_board, _boardShowPosition);
    }

    virtual public void Disappear(System.Action callback = null)
    {
        DestroyPaticle();
        LeanTween.alphaCanvas(canvas, 0f, 0.4f).setOnComplete(() => {
            active = false;
            if (callback != null)
                callback();
        });
    }

    virtual public void Init()
    {
        ribbon.sizeDelta = new Vector2(0f, ribbon.rect.height);
        HideBoards();
    }

    private void HideBoards()
    {
        //Vector3 t = new Vector3(_titleShowPosition.x, _titleShowPosition.y - (title as RectTransform).sizeDelta.y);
        //(_title.transform as RectTransform).localPosition = t;
        //Vector3 b = new Vector3(_boardShowPosition.x, _boardShowPosition.y + (board as RectTransform).sizeDelta.y);
        //(_board.transform as RectTransform).localPosition = b;
    }

    LTDescr ScaleAction(System.Action callback)
    {
        ribbon.sizeDelta = new Vector2(0f, ribbon.rect.height);
        Vector2 to = new Vector2(_ribbonWidth, ribbon.sizeDelta.y);
        return LeanTween.value(ribbon.gameObject, ribbon.sizeDelta, to, 0.5f).setOnUpdate(delegate (Vector2 obj) {
            ribbon.sizeDelta = obj;
        }).setOnComplete(callback);
    }

    LTDescr MoveAction(GameObject obj, Vector3 position, System.Action callback = null)
    {
        //LTDescr action = LeanTween.move(obj, position, 1f).setEaseOutCubic();
        LTDescr action = LeanTween.moveLocal(obj, position, 1f).setEaseOutCubic();
        if (callback != null) action.setOnComplete(callback);
        return action;
    }

    protected void AddPaticle()
    {
        if (fireworks != null)
        {
            _particle = Instantiate(fireworks).gameObject;
        }
    }

    protected void DestroyPaticle()
    {
        if (_particle != null)
        {
            Destroy(_particle);
        }
    }

    private void CanvasStatus(bool value)
    {
        canvas.blocksRaycasts = value;
        canvas.interactable = value;
        canvas.alpha = value ? 1f : 0f;

        innerCanvas.blocksRaycasts = value;
        innerCanvas.interactable = value;

        if (_glowScript != null)
        {
            _glowScript.enable = value;
        }
    }

    virtual public void OnCollect()
    {
        Disappear();
    }

    virtual public void OnShare()
    {
        //SimpleShare.instance.ShareScreenshot("Share", "I've just win HUGE amount of coins in this game, Come play with me", 0f, 0f);
    }
}
