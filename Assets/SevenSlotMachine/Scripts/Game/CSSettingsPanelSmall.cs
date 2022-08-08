using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSSettingsPanelSmall : MonoBehaviour {
	public CSSettingsPanel settings;
	public float duration;
	private CanvasGroup _canvasGroup;
	private bool _show;
	public bool show {
		get { return _show; }
		set {
			if (_show == value)
				return;
			_show = value;

			if (_show)
				Appear ();
			else
				Disappear ();

			_canvasGroup.interactable = _show;
			_canvasGroup.blocksRaycasts = _show;
		}
	}

	void Start ()
	{
		_canvasGroup = GetComponent <CanvasGroup> ();
        transform.Find("Sound").GetComponent<Toggle>().isOn = CSGameSettings.instance.sound;
		_show = false;
	}

	public void Appear()
	{
		Scale (1f).setEaseOutBack ();
	}

	public void Disappear()
	{
		Scale (0f).setEaseInBack ().setIgnoreTimeScale (true);
	}

	private LTDescr Scale(float scale)
	{
		LeanTween.cancel (gameObject);
		return LeanTween.scale (gameObject, Vector3.one * scale, duration);
	}

	public void OnSound(Toggle toggle)
	{
        CSGameSettings.instance.sound = toggle.isOn;
	}

	public void OnSettings()
	{
		settings.Appear ();
	}

	public void OnContactUs()
	{
        //SendEmail(
        //    CSGameManager.instance.supportEmail, "Hi"
        //);
	}

	void SendEmail (string email, string subject = "", string body = "")
	{
		Application.OpenURL("mailto:" + email + "?subject=" + MyEscapeURL(subject) + "&body=" + MyEscapeURL(body));
	}

	string MyEscapeURL (string url)
	{
		return WWW.EscapeURL(url).Replace("+","%20");
	}
}
