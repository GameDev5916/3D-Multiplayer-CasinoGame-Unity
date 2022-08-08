using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class CSButtonTap : MonoBehaviour {
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            CSSoundManager.instance.Tap();
        });
    }
}
