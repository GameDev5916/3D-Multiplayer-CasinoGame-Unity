using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using System;
using SimpleJSON;

public class FacebookManager : MonoBehaviour
{
    public static FacebookManager Instance;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }



    public void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
            //GetFriends();
            //Debug.Log("IntiCallBack");

            if (FB.IsLoggedIn) GetFriends();
            else FB.LogOut();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    void GetFriends()
    {
        string query = "/me/friends";

        FB.API(query, HttpMethod.GET, result =>
        {
            Debug.Log("the raw fbmanager" + result.RawResult);
            var dictionary = (Dictionary<string, object>)Facebook.MiniJSON.Json.Deserialize(result.RawResult);
            var friendsList = (List<object>)dictionary["data"];
            //friendsText.text = string.Empty;

            foreach (var dict in friendsList)
            {
                Dictionary<string, object> friend = (Dictionary<string, object>)dict;
                if (friend.ContainsKey("id"))
                {
                    Debug.LogError("Friend Added");
                    string frndID = friend["id"].ToString();
                    if (!Constants.instance.fbFriendList.Contains(frndID))
                    {
                        Constants.instance.fbFriendList.Add(frndID);
                    }
                }
            }
        });
    }
}