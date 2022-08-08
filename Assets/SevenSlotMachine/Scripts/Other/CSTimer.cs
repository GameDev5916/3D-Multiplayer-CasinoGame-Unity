using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void TimerDelegate(CSTimer timer, double seconds);

public interface ICSTimer {
	void TimerTick (CSTimer timer, double seconds);
	void TimerStop (CSTimer timer, double seconds);
}

[Serializable]
public class CSTimer : ICSUpdateable
{
    public event TimerDelegate timerTickEvent = null;
    public event TimerDelegate timerStopEvent = null;
	public DateTime startDate;
	public DateTime endDate;
	public TimeSpan time;
	public string key;
    public float percent;
    private double _totalSec;


	private bool _running = false;
	public bool running {
		get { return _running; }
		set {
			if (_running == value)
				return;
			_running = value;
		}
	}

	public CSTimer(DateTime startDate, TimeSpan ts, string key)
	{
		this.startDate = startDate;
		this.time = ts;
		this.endDate = startDate.Add (ts);
		this.key = key;
        _totalSec = TotalSeconds();
	}

    public CSTimer(CSTimerData data) : this (data.startDate, data.time, data.key)
    {
    }

	public CSTimer (DateTime startDate, int hours, string k) : this (startDate, TimeSpan.FromHours (hours), k)
	{
	}

	void OnDestroy()
	{
		UnsubscribeAll ();
	}

	public void Tick()
	{
		if (!_running)
			return;

		double seconds = DeltaNow ().TotalSeconds;
        percent = Mathf.Clamp(1f - (float)(seconds / _totalSec), 0f, 1f);
		if (seconds > 0)
		{
			timerTickEvent (this, seconds);
		}
		else
		{
			timerStopEvent (this, 0);
			UnsubscribeAll ();
		}
	}

	public void Add(TimeSpan ts)
	{
		time.Add (ts);
		endDate = endDate.Add (ts);
	}

	public void AddMinutes(int minute)
	{
		Add (TimeSpan.FromMinutes (minute));
	}

	public void AddHours(int hours)
	{
		Add (TimeSpan.FromHours (hours));
	}

	public void AddDays(int days)
	{
		Add (TimeSpan.FromDays (days));
	}

	public void Subtract(TimeSpan ts)
	{
		time.Subtract (ts);
		endDate = endDate.Subtract (ts);
	}

	public void SubtractMinutes(int minute)
	{
		Subtract (TimeSpan.FromMinutes (minute));
	}

	public void SubtractHours(int hours)
	{
		Subtract (TimeSpan.FromHours (hours));
	}

	public void SubtractDays(int days)
	{
		Subtract (TimeSpan.FromDays (days));
	}

	public TimeSpan DeltaNow()
	{
		return endDate - CSTime.Now ();
	}

	public TimeSpan Delta(DateTime date)
	{
		return endDate - date;
	}

	public TimeSpan Delta()
	{
		return Delta (startDate);
	}

	public int DeltaDays()
	{
		return Delta ().Days;
	}

	public int DeltaHours()
	{
		return Delta ().Hours;
	}

	public int DeltaMinutes()
	{
		return Delta ().Minutes;
	}

	public int DeltaSeconds()
	{
        return Delta ().Seconds;
	}

	public double TotalSeconds()
	{
		return Delta ().TotalSeconds;
	}

	public double UnsignedTotalSeconds()
	{
		double seconds = TotalSeconds ();
		return seconds < 0 ? 0 : seconds;
	}

	public double TotalMinutes()
	{
		return Delta ().TotalMinutes;
	}

	public bool isTimeUp()
	{
		return TotalSeconds () < 0;
	}

	public void Subscribe(ICSTimer sender)
	{
		if (sender == null)
			return;
		timerTickEvent += sender.TimerTick;
		timerStopEvent += sender.TimerStop;
		running = true;
	}

	public void Unsubscribe(ICSTimer sender)
	{
		if (timerTickEvent == null)
			return;

        timerTickEvent -= sender.TimerTick;
		timerStopEvent -= sender.TimerStop;

        if (timerTickEvent == null || timerTickEvent.GetInvocationList ().Length == 0)
			running = false;
	}

	public void UnsubscribeAll()
	{
		if (timerTickEvent == null)
			return;

		Delegate[] list = timerTickEvent.GetInvocationList ();
		foreach (var item in list)
		{
			timerTickEvent -= (item as TimerDelegate);
		}
		running = false;
	}
}

[Serializable]
public class CSTime
{
	public static DateTime Now()
	{
		return DateTime.Now;
	}
}

[Serializable]
public class CSTimerData
{
    public DateTime startDate;
    public TimeSpan time;
    public string key;

    public CSTimerData(CSTimer timer)
    {
        startDate = timer.startDate;
        time = timer.time;
        key = timer.key;
    }
}