using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class CSGameManager : MonoBehaviour {
	public static CSGameManager instance = null;
    [HideInInspector] public bool expandWild;

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
        Application.targetFrameRate = 60;
    }
}
