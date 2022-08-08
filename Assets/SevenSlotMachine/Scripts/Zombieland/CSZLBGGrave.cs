using UnityEngine;
using UnityEngine.UI;

public class CSZLBGGrave : MonoBehaviour {
    public GameObject[] animations;
    public event System.Action<CSZLBGRewardTypes> ValueChangedEvent;

    private CSZLBGZombieHand _curr;

    public bool interactable {
        get { return _toggle.interactable; }
        set {
            if (_toggle.interactable == value)
                return;
            _toggle.interactable = value;
        }
    }

    public bool enable {
        get { return _toggle.isOn; }
        set {
            if (_toggle.isOn == value)
                return;
            _toggle.isOn = value;
        }
    }

    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnValueChanged(bool value)
    {
        interactable = false;
        if (value) Appear();

        if (ValueChangedEvent != null)
            ValueChangedEvent(_curr.RewardType());
    }

    private CSZLBGZombieHand CreateZombieHand()
    {
        var obj = Instantiate(animations[Random.Range(0, animations.Length)], transform);
        return obj.GetComponent<CSZLBGZombieHand>().Instantiate();
    }

    public void Appear()
    {
        _curr = CreateZombieHand();
    }

    public void Disappear()
    {
        enable = false;
        interactable = true;
        if (_curr != null)
        {
            _curr.appear = false;
        }
    }
}
