// <copyright file="SigninSampleScript.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations

namespace SignInSample
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Google;
    using SimpleJSON;
    using UnityEngine;
    using UnityEngine.UI;

    public class GoogleSignInScript : MonoBehaviour
    {
        public static GoogleSignInScript instance;
        public Text statusText;

        public string webClientId = "<your client id here>";

        private GoogleSignInConfiguration configuration;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Awake()
        {
            instance = this;
            configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestIdToken = true
            };
        }

        public void OnSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        public void OnSignOut()
        {
            AddStatusText("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.LogError("SignOut");
        }

        public void OnDisconnect()
        {
            AddStatusText("Calling Disconnect");
            Debug.LogError("annncva");
            GoogleSignIn.DefaultInstance.Disconnect();
            Login.instance.GoogleLoginCancal();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        GoogleSignIn.SignInException error =
                                (GoogleSignIn.SignInException)enumerator.Current;
                        AddStatusText("Got Error: " + error.Status + " " + error.Message);
                        Debug.LogError("Got Error:");
                        Login.instance.GoogleLoginCancal();
                    }
                    else
                    {
                        AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                        Debug.LogError("Got Unexpected Exception?!?");
                        Login.instance.GoogleLoginCancal();
                    }
                }
            }
            else if (task.IsCanceled)
            {
                AddStatusText("Canceled");
                Debug.LogError("Canceled");

                Login.instance.GoogleLoginCancal();
            }
            else
            {
                AddStatusText("Welcome: " + task.Result.AuthCode + "!");
                AddStatusText("Welcome: " + task.Result.DisplayName + "!");
                AddStatusText("Welcome: " + task.Result.Email + "!");
                AddStatusText("Welcome: " + task.Result.ImageUrl + "!");
                AddStatusText("Welcome: " + task.Result.UserId + "!");

                JSONNode data = new JSONObject
                {
                    ["player_Id"] = task.Result.UserId,
                    ["Full_Name"] = task.Result.DisplayName,
                    ["Email"] = string.IsNullOrEmpty(task.Result.Email) ? "testuser@gmail.com" : task.Result.Email,
                    ["profile_Url"] = task.Result.ImageUrl.ToString(),
                };

                print("Google Sign in Data = " + data.ToString());
                Login.instance.OnLoginCompleted(data.ToString());
            }
        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently()
                  .ContinueWith(OnAuthenticationFinished);
        }


        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddStatusText("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        private List<string> messages = new List<string>();
        void AddStatusText(string text)
        {
            if (messages.Count == 5)
            {
                messages.RemoveAt(0);
            }
            messages.Add(text);
            string txt = "";
            foreach (string s in messages)
            {
                txt += "\n" + s;
            }
            if (statusText != null)
                statusText.text = txt;
        }
    }
}
