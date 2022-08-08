using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CSLobbyPresent : MonoBehaviour, ICSTimer {
    public TextMeshProUGUI timerLabel;
    public GameObject present;
    public Image bar;
    public CSBankCoinPanel coinPanel;
    public float reward;

    [CSEnumFlag]
    public CSTimeProperty timeProperty;

	private bool _active = false;
	public bool active {
		get{return _active;}
		set{
			if (_active == value) {
				return;
			}
			_active = value;
            if (value)
            {
                CreatePresent();
            }
            else
            {
                coinPanel.Add(reward, transform as RectTransform);
            }
        }
	}

	void Start()
	{
		active = !CSTimerManager.instance.SubsricbeToPresentTimer (this);
	}

	void OnEnable()
	{
        CSTimerManager.instance.TimerCreatedEvent += CSTimerManager_instance_timerCreatedEvent;
	}

    void OnDisable()
	{
		CSTimerManager.instance.UnsubsricbeFromPresentTimer (this);
        CSTimerManager.instance.TimerCreatedEvent -= CSTimerManager_instance_timerCreatedEvent;
	}

	void CSTimerManager_instance_timerCreatedEvent (CSTimer arg1, string arg2)
	{
		if (arg2 == "present")
		{
			active = !CSTimerManager.instance.SubsricbeToPresentTimer (this);
		}
	}

	public void TimerTick (CSTimer timer, double seconds)
	{
        timerLabel.text = CSUtilities.TimeFormat((float)seconds, timeProperty);
        bar.fillAmount = timer.percent;
	}

	public void TimerStop (CSTimer timer, double seconds)
	{
		CSTimerManager.instance.DestroyTimer (timer);
        bar.fillAmount = 0f;
        timerLabel.text = string.Empty;
		active = true;
	}

    private void CreatePresent()
    {
        Instantiate(present, transform.position, Quaternion.identity, transform);
    }
}