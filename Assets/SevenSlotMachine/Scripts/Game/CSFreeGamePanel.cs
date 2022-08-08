using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CSFreeGamePanel : MonoBehaviour {
    public GameObject freeSpinGameObject;
    private bool _freeSpinEnable;
    public bool freeSpinEnable
    {
        get { return _freeSpinEnable; }
        set
        {
            if (_freeSpinEnable == value)
                return;
            _freeSpinEnable = value;
            if (freeSpinGameObject != null)
                freeSpinGameObject.SetActive(value);
        }
    }

    public TextMeshProUGUI freeSpinsLabel;
    private int _freeSpins;
    public int freeSpins
    {
        get { return _freeSpins; }
        set
        {
            if (_freeSpins == value)
                return;
            _freeSpins = value;
            freeSpinEnable = true;
            if (freeSpinsLabel != null)
                freeSpinsLabel.text = value.ToString();
        }
    }

    public Text winLabel;
    private float _win;
    public float win
    {
        get { return _win; }
        set
        {
            _win = value;
            if (winLabel != null)
                winLabel.text = value.ToString("");
        }
    }

    public virtual void SetWin(float value)
    {
        win += value;
    }
}
