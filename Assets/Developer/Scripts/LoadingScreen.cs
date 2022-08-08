using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private Text loadingTxt;
    [SerializeField]
    private Image progressBar;

    [HideInInspector] public string loadSceneName;
    float time;

    private void OnEnable()
    {
        //StartCoroutine(LoadScene());
    }

    private void Start()
    {
        StartCoroutine(LoadScene());
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

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(loadSceneName);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            yield return null;
            progressBar.fillAmount = asyncOperation.progress;

            if (asyncOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(.5f);
                asyncOperation.allowSceneActivation = true;
            }
        }
    }
}
