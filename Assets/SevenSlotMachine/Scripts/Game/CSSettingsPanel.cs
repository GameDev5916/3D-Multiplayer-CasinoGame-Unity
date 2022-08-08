using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSSettingsPanel : MonoBehaviour {
	public float duration;
	public GameObject board;
    public CanvasGroup fbInvite;
    public CanvasGroup fbLogin;
    public CSBankCoinPanel coinPanel;

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

	void Awake()
	{
		_canvas = GetComponent <CanvasGroup>();
		_background = GetComponent <Image> ();

        UpdateValues();
	}

	void Start ()
	{
        //UpdateFacebookStatus();
	}

    private void UpdateValues()
    {
        CSGameSettings settings = CSGameSettings.instance;
        board.transform.Find("Content/Effects/Sound/Toggle").GetComponent<Toggle>().isOn = settings.sound;
        board.transform.Find("Content/Effects/Music/Toggle").GetComponent<Toggle>().isOn = settings.music;
        board.transform.Find("Content/Volume/Slider").GetComponent<Slider>().value = settings.volume;
    }

	public void Appear()
	{
        UpdateValues();
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
		Time.timeScale = _active ? 0f : 1f;
        CSSoundManager.instance.PauseAll(value);
	}

    private void UpdateFacebookStatus()
    {
        //if (CSFacebookManager.instance.isLoggedIn)
        //{
        //    CanvasStatus(fbInvite, true);
        //    CanvasStatus(fbLogin, false);
        //}
        //else
        //{
        //    CanvasStatus(fbInvite, false);
        //    CanvasStatus(fbLogin, true);
        //}
    }

    private void CanvasStatus(CanvasGroup canvas, bool status)
    {
        canvas.alpha = status ? 1f : 0f;
        canvas.interactable = status;
        canvas.blocksRaycasts = status;
    }

	public void OnClose()
	{
		Disappear (delegate() {
			active = false;
		});
	}

	public void OnSound(bool value)
	{
        CSGameSettings.instance.sound = value;
	}

    public void OnMusic(bool value)
	{
        CSGameSettings.instance.music = value;
	}

    public void OnVolume(float value)
	{
        CSGameSettings.instance.volume = value;
        AudioListener.volume = value;
	}

	public void OnFacebookLogin()
	{
        //CSFacebookManager.instance.Login((login) =>
        //{
        //    UpdateFacebookStatus();
        //});
	}

    public void OnFacebookLogout()
    {
        //CSFacebookManager.instance.Logout(() =>
        //{
        //    UpdateFacebookStatus();
        //});
    }

	public void OnFacebookInvite()
	{
        //CSFacebookManager facebook = CSFacebookManager.instance;
        //if (!facebook.isLoggedIn)
        //{
        //    return;
        //}
        //facebook.AppRequest("Invite Friend", "Hi, Try this Game", (count) =>
        //{
        //    coinPanel.Add(count * 1000f);
        //});
    }

	public void OnRestore()
	{
        //CSIAPManager.instance.RestorePurchases();
	}
}
