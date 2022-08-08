using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CSGameStore : MonoBehaviour {
    public TextMeshProUGUI coinsLabel;
    public CSBankCoinPanel coinPanel;
	public float duration;
	public GameObject board;
	private Image _background;
	private CanvasGroup _canvas;

	private int _scaleId = 0;
	private int _alphaId = 0;
	private int _alphaBoardId = 0;

	private bool _active;
	public bool active {
		get {return _active; }
		set {
			if (value == _active)
				return;
			_active = value;
			CanvasStatus (_active);
		}
	}

    private float _coins;
    public float coins {
        get { return _coins; }
        set {
            _coins = value;
            coinsLabel.text = string.Format("You already have <sprite=0> {0} coins", CSUtilities.FormatNumber(value));
        }
    }

	void Awake()
	{
		_canvas = GetComponent <CanvasGroup>();
		_background = GetComponent <Image> ();
	}

    private void OnEnable()
    {
        coinPanel.bankValueChanged += CoinPanelValueChanged;
    }

    private void OnDisable()
    {
        coinPanel.bankValueChanged -= CoinPanelValueChanged;
    }

    public void Appear()
	{
        coins = coinPanel.bank;
		active = true;
		AlphaBackground (0.75f);
		AlphaBoard (1f);
		Scale (LeanTweenType.easeOutBack);
	}

	public void Disappear(System.Action callback)
	{
		Scale (LeanTweenType.easeInBack);
		AlphaBoard (0f);
		AlphaBackground (0f).setOnComplete (callback);
	}

	private LTDescr Scale(LeanTweenType type)
	{
		LeanTween.cancel (_scaleId);

		float scale = 0f;
		switch (type) {
		case LeanTweenType.easeInBack:  scale = 0f; break;
		case LeanTweenType.easeOutBack: scale = 1f; break;
		default:break;
		}

		board.transform.localScale = Vector3.one * (scale > 0.5f ? 0f : 1f);
		LTDescr action = LeanTween.scale (board, Vector3.one * scale, duration).setEase (type).setIgnoreTimeScale (true);
		_scaleId = action.id;
		return action;
	}

	private LTDescr AlphaBackground(float value)
	{
		LeanTween.cancel (_alphaId);

		LTDescr action = Alpha (_background, value, 0.4f);
		_alphaId = action.id;
		return action;
	}

	private LTDescr AlphaBoard(float value)
	{
		LeanTween.cancel (_alphaBoardId);

		CanvasGroup bcanvas = board.GetComponent <CanvasGroup> ();
		bcanvas.alpha = (value > 0.5f ? 0f : 1f);

		LTDescr action = LeanTween.alphaCanvas (board.GetComponent <CanvasGroup> (), value, 0.3f).setIgnoreTimeScale (true);
		_alphaBoardId = action.id;
		return action;
	}

	private LTDescr Alpha(Image image, float value, float dur)
	{
		Color color = image.color;
		color.a = (value > 0.5f ? 0f : color.a);
		image.color = color;

		return LeanTween.value (image.gameObject, color.a, value, dur).setOnUpdate (delegate(float obj) {
			color.a = obj;
			image.color = color;
		}).setIgnoreTimeScale (true);
	}

	private void CanvasStatus(bool value)
	{
		_canvas.blocksRaycasts = value;
		_canvas.interactable = value;
		//Time.timeScale = _active ? 0f : 1f;
        //CSSoundManager.instance.PauseAll(value);
	}

	public void OnClose()
	{
		Disappear (delegate() {
			active = false;
		});
	}

	//public void OnPack(CSStoreCoinPack pack)
	//{
 //       if (!pack.GetComponent<Toggle>().isOn)
 //           return;
 //       // create iap id
 //       //CSIAPManager.instance.Purchase(pack.productId);
 //   }

    private void CoinPanelValueChanged(CSBankCoinPanel sender)
    {
        if (!_active)
            return;

        coins = sender.bank;
    }
}
