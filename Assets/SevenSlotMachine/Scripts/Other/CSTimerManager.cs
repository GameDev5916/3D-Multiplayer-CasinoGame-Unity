using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public interface ICSUpdateable {
	void Tick ();
}

public class CSTimerManager : MonoBehaviour {
	public event Action<CSTimer, string> TimerCreatedEvent;
	public static CSTimerManager instance = null;
	public Dictionary<string, CSTimer> _timers;
    private List<CSTimer> _add;
    private List<CSTimer> _remove;

	void Awake ()
	{
		if (instance == null)
		{
			DontDestroyOnLoad (gameObject);
			instance = this;
            Loaded();
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}
	}

    private void Loaded()
    {
        _timers = LoadTimers();
        _add = new List<CSTimer>();
        _remove = new List<CSTimer>();
    }

    void Update()
	{
        AddTimers();
        DestroyTimers();
        if (_timers == null || _timers.Count == 0)
			return;
		foreach (var item in _timers) {
			item.Value.Tick ();
		}
	}

    private Dictionary<string, CSTimer> LoadTimers()
    {
        var dict = new Dictionary<string, CSTimer>();
        var timers = CSGameSettings.instance.data.timers;

        foreach (KeyValuePair<string, CSTimerData> item in timers)
        {
            dict.Add(item.Key, new CSTimer(item.Value));
        }
        return dict;
    }

    private void AddTimers()
    {
        if (_add.Count == 0)
            return;

        for (int i = 0; i < _add.Count; i++)
        {
            CSTimer timer = _add[i];
            if (!_timers.ContainsKey(timer.key))
            {
                CSGameSettings.instance.AddTimer(timer);
                _timers.Add(timer.key, timer);
                if (TimerCreatedEvent.GetInvocationList().Length > 0)
                {
                    TimerCreatedEvent(timer, timer.key);
                }
            }
        }
        _add.Clear();
    }

    private void DestroyTimers()
    {
        if (_remove.Count == 0)
            return;

        for (int i = 0; i < _remove.Count; i++)
        {
            CSTimer timer = _remove[i];
            if (_timers.ContainsKey(timer.key))
            {
                _timers.Remove(timer.key);
                CSGameSettings.instance.RemoveTimer(timer);
            }
        }
        _remove.Clear();
    }

	public CSTimer CreateTimerHour(int hours, string key)
	{
		if (_timers.ContainsKey (key))
			return null;

		CSTimer timer = new CSTimer (CSTime.Now (), hours, key);

        AddTimer(timer);

		return timer;
	}

    public CSTimer CreateTimerMinutes(double minutes, string key)
    {
        if (_timers.ContainsKey(key))
        {
            return null;
        }
        var timer = new CSTimer(CSTime.Now(), TimeSpan.FromMinutes(minutes), key);

        AddTimer(timer);

        return timer;
    }

    private CSTimer TimerForKey(string key)
	{
		if (!_timers.ContainsKey (key))
			return null;
		return _timers[key];
	}

	public void DestroyTimerForKey(string key)
	{
        if (!_timers.ContainsKey(key))
            return;
        _remove.Add(_timers[key]);
	}

	public void DestroyTimer(CSTimer timer)
	{
        if (!_timers.ContainsValue(timer))
            return;
        _remove.Add(timer);
	}

    public void AddTimer(CSTimer timer)
    {
        if (timer == null)
            return;
        _add.Add(timer);
    }

	public bool SubsricbeToTimer(ICSTimer sender, string key)
	{
		CSTimer timer = TimerForKey (key);
		if (timer == null)
        {
            return false;
        }

		timer.Subscribe (sender);
		return true;
	}

	public void UnsubsricbeFromTimer(ICSTimer sender, string key)
	{
		CSTimer timer = TimerForKey (key);
		if (timer == null)
			return;
        timer.Unsubscribe (sender);
	}

	// Helper

    public CSTimer CreatePresentTimer(double minutes)
	{
        return CreateTimerMinutes (minutes, "present");
	}

	public void DestroyPresentTimer()
	{
		DestroyTimerForKey ("present");
	}

	public CSTimer CreateWheelTimer(int hours)
	{
        return CreateTimerHour (hours, "wheel");
	}

	public void DestroyWheelTimer()
	{
		DestroyTimerForKey ("wheel");
	}

	public CSTimer GetPresentTimer()
	{
		return TimerForKey ("present");
	}

	public CSTimer GetWheelTimer()
	{
		return TimerForKey ("wheel");
	}

	public bool SubsricbeToPresentTimer(ICSTimer sender)
	{
		return SubsricbeToTimer (sender, "present");
	}

	public bool SubsricbeToWheelTimer(ICSTimer sender)
	{
		return SubsricbeToTimer (sender, "wheel");
	}

	public void UnsubsricbeFromPresentTimer(ICSTimer sender)
	{
		UnsubsricbeFromTimer (sender, "present");
	}

	public void UnsubsricbeFromWheelTimer(ICSTimer sender)
	{
		UnsubsricbeFromTimer (sender, "wheel");
	}
}
