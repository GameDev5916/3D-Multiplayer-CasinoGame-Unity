using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CSLFBGAlertBoard : MonoBehaviour {
    public GameObject multiplierGameObject;
    public TextMeshProUGUI multiplierLabel;
    private int _multiplier;
    public int multiplier
    {
        get { return _multiplier; }
        set
        {
            _multiplier = value;
            multiplierGameObject.SetActive(value > 0);
            multiplierLabel.text = "X" + value.ToString();
        }
    }

    public GameObject freeSpinsGameObject;
    public TextMeshProUGUI freeSpinsLabel;
    private int _freeSpins;
    public int freeSpins
    {
        get { return _freeSpins; }
        set
        {
            _freeSpins = value;
            freeSpinsGameObject.SetActive(value > 0);
            freeSpinsLabel.text = value.ToString();
        }
    }

    public GameObject expandWildGameObject;
    private bool _expandWild;
    public bool expandWild {
        get { return _expandWild; }
        set {
            _expandWild = value;
            expandWildGameObject.SetActive(value);
        }
    }
}
