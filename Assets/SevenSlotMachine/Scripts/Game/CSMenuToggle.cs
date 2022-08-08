using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CSMenuToggle : MonoBehaviour {
    public CSSettingsPanelSmall miniSettings;
    private Toggle _toggle;
    private LayerMask _toggleMask;

	void Awake ()
    {
        _toggle = GetComponent<Toggle>();
        _toggleMask = LayerMask.NameToLayer("MenuPanel");
	}

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || PointerOverGameObject())
            return;

        _toggle.isOn = false;
    }

    public void OnMiniSettings(bool value)
    {
        miniSettings.show = value;
    }

    private bool PointerOverGameObject()
    {
        return Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, _toggleMask).collider != null;
    }
}
