using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class WebglInput : MonoBehaviour
{
    public static WebglInput instance;

    private TMP_InputField currentSelectedField;

    public static Action<string> WebglInputText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SelectInputField(TMP_InputField inputField)
    {
        currentSelectedField = inputField;

#if PLATFORM_WEBGL && !UNITY_EDITOR
                    Application.ExternalCall("GetinputForWebGl", "Enter Field", inputField.text);
#else
        Debug.Log("This PlatFrom Not Supported");
#endif
    }

    public void SetText(string text)
    {
        WebglInputText?.Invoke(text);
    }
}
