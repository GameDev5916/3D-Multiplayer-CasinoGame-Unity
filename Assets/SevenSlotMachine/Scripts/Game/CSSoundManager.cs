using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Linq;

public class CSSoundManager : MonoBehaviour {
    public static CSSoundManager instance = null;
    public Sound[] sounds;
    private string _playingMusic = String.Empty;

    public bool sound {
        get { return CSGameSettings.instance.sound; }
        set { CSGameSettings.instance.sound = value;}
    }

    public bool music
    {
        get { return CSGameSettings.instance.music; }
        set { CSGameSettings.instance.music = value;}
    }

    public float volume
    {
        get { return CSGameSettings.instance.volume; }
        set { CSGameSettings.instance.volume = value;}
    }

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            Loaded();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Loaded()
    {
        foreach (Sound item in sounds)
        {
            item.AddSource(gameObject.AddComponent<AudioSource>());
        }
    }

    public void PlayMusic(string name)
    {
        //if (!music)
        //    return;
        if (Constants.MUSIC == 1)
            return;
        if (_playingMusic == name)
            return;
        _playingMusic = name;
        Play(name);
    }

    public void StopMusic()
    {
        if (_playingMusic == string.Empty)
            return;

        Stop(_playingMusic);
        _playingMusic = string.Empty;
    }

    public void Tap()
    {
        Play("move");
    }

    public void Stop(string effect)
    {
        ClipForName(effect, (clip) =>
        {
            clip.Stop();
        });
    }

    public void Pause(string effect)
    {
        ClipForName(effect, (clip) =>
        {
            clip.Pause();
        });
    }

    public void Play(string effect)
    {
        //if (!sound) return;
        if (Constants.SOUND != 0) return;
        ClipForName(effect, (clip) =>
        {
            clip.Play();
        });
    }

    private void ClipForName(string clipName, Action<Sound> callback)
    {
        Sound clip = Array.Find(sounds, s => s.name == clipName);
        if (clip == null) return;
        callback(clip);
    }

    public void PauseAll(bool value)
    {
        //AudioListener.pause = value;
        ClipForName("reel_spin", (clip) =>
        {
            if (value)
                clip.Pause();
            else
                clip.UnPause();
        });
    }
}



[Serializable]
public class Sound
{
    public AudioClip clip;
    public bool loop;
    [HideInInspector]public AudioSource source;
    [Range(0f, 1f)]
    public float volume = 1f;
    public string name { get { return clip.name; } }

    public bool isPlaying
    {
        get {
            if (source == null)
                return false;
            return source.isPlaying;
        }
    }

    public void AddSource(AudioSource s)
    {
        s.playOnAwake = false;
        source = s;
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
    }

    public void Play()
    {
        if (source == null)
            return;
        source.Play();
    }

    public void Stop()
    {
        if (source == null)
            return;
        source.Stop();
    }

    public void Pause()
    {
        if (source == null)
            return;
        source.Pause();
    }

    public void UnPause()
    {
        if (source == null)
            return;
        source.UnPause();
    }
}

