using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField]
    private Text loadingTxt;
    [SerializeField]
    private Image progressBar;

    float time;

    // Start is called before the first frame update
    void Start()
    {
        if (Constants.ISLOGIN)
        {
            JSONNode data = new JSONObject
            {
                ["unique_id"] = Constants.PLAYER_ID,
            };

            StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
            {
                if (IsSuccess)
                {
                    JSONNode jsonNode = JSON.Parse(result)["data"];
                    Constants.SetPlayerData(jsonNode);
                    //Debug.LogError(jsonNode.ToString());
                    StartCoroutine(LoadHomeScene());
                }
                else
                    Constants.Logout();
            }));
        }
        else
        {
            StartCoroutine(LoadHomeScene());
        }

        //StartCoroutine(LoadHomeScene());
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time < 0.3f) loadingTxt.text = "Loading";
        else if (time < 0.6f) loadingTxt.text = "Loading.";
        else if (time < 0.9f) loadingTxt.text = "Loading..";
        else if (time < 1.2f) loadingTxt.text = "Loading...";
        else time = 0;
    }

    IEnumerator LoadHomeScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(Constants.ISLOGIN ? "Home" : "Login");
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            yield return null;
            progressBar.fillAmount = asyncOperation.progress;

            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }
        }
    }
}
