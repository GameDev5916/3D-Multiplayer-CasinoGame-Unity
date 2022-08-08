using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class CSSceneManager : MonoBehaviour {
    public static CSSceneManager instance = null;

    private Action<float, bool> _callback = null;
    private AsyncOperation _sceneAsync = null;
    private AsyncOperation _assetAsync = null;
    private float _minsec = 0f;
    private float _elapsed = 0f;
    private string _sceneName = null;
    private int _sceneIdx = 0;
    private float _sceneProgress = 0f;
    private float _assetProgress = 0f;

    private bool _sceneComplete = false;
    private bool _assetComplete = false;
    private bool _enable = false;

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
    }

    private void Update()
    {
        if (!_enable)
            return;

        Tick();
    }

    public void LoadScene(int idx, Action<float, bool> callback)
    {
        if (_callback != null)
            return;

        _sceneIdx = idx;
        _assetAsync = Resources.UnloadUnusedAssets();
        //_sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        _callback = callback;

        StartLoad();
    }

    public void LoadScene(string sceneName, Action<float, bool> callback)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Assert(false, "Scene couldn't be loaded: " + sceneName);
            return;
        }

        if (_callback != null)
            return;

        _sceneName = sceneName;
        _assetAsync = Resources.UnloadUnusedAssets();
        //_sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        _callback = callback;

        StartLoad();
    }

    private void StartLoad()
    {
        _elapsed = 0f;
        _sceneProgress = 0f;
        _assetProgress = 0f;
        _enable = true;
    }

    private void EndLoad()
    {
        _enable = false;
        _callback = null;
        _sceneAsync = null;
        _assetAsync = null;
        _sceneName = null;

        _sceneComplete = false;
        _assetComplete = false;
    }

    private void Tick()
    {
        if (_callback == null)
            return;

        PurgeAssets();
        Next();
        LoadScene();

        _elapsed += Time.deltaTime;
        bool complete = IsComplete();

        if (complete)
        {
            _enable = false;
            EndLoad();
        }
        else
        {
            float prog = _sceneProgress + _assetProgress;
            _callback(Mathf.Clamp01(prog), complete);
        }
    }

    private void Next()
    {
        if (_assetAsync != null && _assetAsync.isDone && _sceneAsync == null)
        {
            _assetAsync = null;
            if (_sceneName != null)
            {
                _sceneAsync = SceneManager.LoadSceneAsync(_sceneName);
            }
            else
            {
                _sceneAsync = SceneManager.LoadSceneAsync(_sceneIdx);
            }
            _sceneAsync.allowSceneActivation = false;
        }
    }

    private bool IsComplete()
    {
        bool s =_elapsed >= _minsec && _assetComplete && _sceneComplete;
        if (s)
        {
            Debug.Log("completed");
        }
        return s;
        //return _elapsed >= _minsec && _assetComplete && _sceneComplete;
    }

    private void PurgeAssets()
    {
        if (_assetAsync == null)
            return;
        _assetProgress = Mathf.Clamp01(_assetAsync.progress / 0.9f) / 2f;
        Debug.Log("_assetAsync " + _assetAsync.progress);
        if (_assetAsync.isDone)
        {
            _assetComplete = true;
        }
    }

    private void LoadScene()
    {
        if (_sceneAsync == null)
            return;

        _sceneProgress = Mathf.Clamp01(_sceneAsync.progress / 0.9f) / 2f;

        if (_sceneAsync.progress >= 0.9f)
        {
            _sceneComplete = true;
            _sceneAsync.allowSceneActivation = true;
        }

        //if (_sceneAsync.isDone)
        //{
        //    _sceneComplete = true;
        //}
    }
}
