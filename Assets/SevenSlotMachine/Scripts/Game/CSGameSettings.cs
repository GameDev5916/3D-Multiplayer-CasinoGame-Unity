using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CSGameSettings : MonoBehaviour {
	public static CSGameSettings instance = null;
	public uint lastWin = 0;

	public CSGameData data;

	public bool music {
		get{ return data.music; }
		set{
			if (data.music == value)
				return;
			data.music = value;
			Save ();
		}
	}

	public bool sound {
		get{ return data.sound; }
		set{
			if (data.sound == value)
				return;
			data.sound = value;
			Save ();
		}
	}

    public float highscore {
		get{ return data.highscore; }
		set{
			playcount += 1;

			if (data.highscore >= value)
				return;
			data.highscore = value;
			Save ();
		}
	}

	public uint playcount {
		get{ return data.playcount; }
		set{
			if (data.playcount == value)
				return;
			data.playcount = value;
			Save ();
		}
	}

	public bool ads {
		get{ return data.ads; }
		set{
			if (data.ads == value)
				return;
			data.ads = value;
			Save ();
		}
	}

    public float coins {
		get{ return data.coins; }
		set{
			data.coins = value;
			Save ();
		}
	}

    public float xp
    {
        get { return data.xp; }
        set
        {
            data.xp = value;
            Save();
        }
    }

    public int level
    {
        get { return data.level; }
        set
        {
            data.level = value;
            Save();
        }
    }

    public string selectedTheme
    {
        get { return data.selectedTheme; }
        set
        {
            if (data.selectedTheme == value)
                return;
            data.selectedTheme = value;
            Save();
        }
    }


    public int zombielandBetStep
    {
        get { return data.zombielandBetStep; }
        set
        {
            data.zombielandBetStep = value;
            Save();
        }
    }

    public int luckyFarmBetStep
    {
        get { return data.luckyFarmBetStep; }
        set
        {
            data.luckyFarmBetStep = value;
            Save();
        }
    }

    public int classicSevenBetStep
    {
        get { return data.classicSevenBetStep; }
        set
        {
            data.classicSevenBetStep = value;
            Save();
        }
    }

    public float volume
    {
        get { return data.volume; }
        set
        {
            data.volume = value;
            Save();
        }
    }

	void Awake ()
	{
		if (instance == null)
		{
			DontDestroyOnLoad (gameObject);
			instance = this;
		    Load ();
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}
	}

	string FilePath()
	{
		return Path.Combine (Application.persistentDataPath, "settings.dat");
	}

	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = GetFileStream ();

		bf.Serialize (file, data);

		file.Close ();
	}

	public void Load()
	{
		if (!File.Exists (FilePath ()))
		{
			Reset ();
		}
		else
		{
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = GetFileStream ();

			data = bf.Deserialize (file) as CSGameData;

			file.Close ();

            AudioListener.volume = data.volume;
		}
	}

	public void Reset()
	{
		Delete ();
		data = new CSGameData ();
		Save ();
	}

	public void Delete()
	{
		string path = FilePath ();
		if (File.Exists (path))
			File.Delete (path);
	}

	FileStream GetFileStream()
	{
		return File.Open(FilePath (), FileMode.OpenOrCreate);
	}

	public void AddTimer(CSTimer timer)
	{
		if (timer == null)
			return;

        data.timers.Add (timer.key, new CSTimerData(timer));
		Save ();
	}

	public void RemoveTimer(CSTimer timer)
	{
        if (timer == null)
            return;
        RemoveTimer (timer.key);
	}

	public void RemoveTimer(string key)
	{
		if (!data.timers.ContainsKey (key))
			return;
		data.timers.Remove (key);
		Save ();
	}

    public bool AvalibleDailyReward()
    {
        return data.TotalInstallDays > data.installDays;
    }

    public void DailyRewardCollected()
    {
        data.installDays = data.TotalInstallDays;
        Save();
    }

    public int RewardDay(int rewardDayCount)
    {
        return (int)(data.TotalInstallDays % rewardDayCount);
    }
}

[Serializable]
public class CSGameData
{
	public bool sound;
	public bool music;
    public float highscore;
	public uint playcount;
	public bool ads;
    public float coins;
	public DateTime installDate;
    public Dictionary<string, CSTimerData> timers;
    public float xp;
    public int level;
    public int installDays;
    public string selectedTheme;
    public int zombielandBetStep;
    public int luckyFarmBetStep;
    public int classicSevenBetStep;
    public float volume;


    public int TotalInstallDays
    {
        get { return (CSTime.Now() - installDate).Days; }
    }

	public CSGameData()
	{
		installDate = CSTime.Now ();
        installDays = -1;
        coins = 2500f;
        timers = new Dictionary<string, CSTimerData> ();
		sound = true;
		music = true;
        volume = 1f;
		ads = true;
		highscore = 0f;
		playcount = 0;
        level = 1;
        xp = 0f;
        selectedTheme = string.Empty;
        zombielandBetStep = 1;
        luckyFarmBetStep = 1;
        classicSevenBetStep = 1;
	}
}