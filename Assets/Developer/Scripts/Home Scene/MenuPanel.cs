using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Facebook.Unity;
using SimpleJSON;

public class MenuPanel : MonoBehaviour
{
    public GameObject BG;

    public Image SoundImage, MusicImage;

    public Sprite SoundON, SoundOFF;
    public Sprite MusicON, MusicOFF;

    public GameObject ReferCode;

    private void OnEnable()
    {
        //BG.transform.DOLocalMove(new Vector3(-530, 0,0), .5f).SetRelative(true);
        BG.GetComponent<RectTransform>().DOAnchorPosX(0, .5f).From(new Vector2(530, 0)).SetEase(Ease.InSine);

        SoundImage.sprite = Constants.SOUND == 0 ? SoundON : SoundOFF;
        MusicImage.sprite = Constants.MUSIC == 0 ? MusicON : MusicOFF;

        //Debug.LogError(Constants.IS_REFER_CODE_USED);

        ReferCode.SetActive(Constants.IS_REFER_CODE_USED != "True");
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosX(530, .5f).From(new Vector2(0, 0)).SetEase(Ease.Linear)
             .OnComplete(() => HomeScreenUIManager.Instance.ManuPanel.SetActive(false));

        //BG.transform.DOLocalMove(new Vector3(530, 0, 0), .5f).SetRelative(true)
        //    .OnComplete(() => HomeScreenUIManager.Instance.ManuPanel.SetActive(false));
        //HomeScreenUIManager.Instance.ManuPanel.SetActive(false);
    }

    public void SoundButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        Constants.SOUND = Constants.SOUND == 0 ? 1 : 0;
        SoundImage.sprite = Constants.SOUND == 0 ? SoundON : SoundOFF;
    }

    public void MusicButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Constants.MUSIC = Constants.MUSIC == 0 ? 1 : 0;
        MusicImage.sprite = Constants.MUSIC == 0 ? MusicON : MusicOFF;
        SoundManager.Instance.BgSound();
    }

    public void ContactUsButtonClick()
    {
    }

    public void SuportButtonClick()
    {
    }

    public void ReferCodeButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        HomeScreenUIManager.Instance.EnterCodePanel.SetActive(true);
        HomeScreenUIManager.Instance.ManuPanel.SetActive(false);
    }

    public void ShareButtobnClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

#if PLATFORM_WEBGL && !UNITY_EDITOR
                    Application.ExternalCall("TriggerShareDialogue");
#else
        StartCoroutine(TakeScreenshotAndShare());
#endif
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        yield return new WaitForEndOfFrame();

        //Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        //ss.Apply();

        //string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        //File.WriteAllBytes(filePath, ss.EncodeToPNG());

        //// To avoid memory leaks
        //Destroy(ss);

        new NativeShare()
            .SetSubject("Casino").SetText("Try Out This Game, It's Amazing").SetUrl("https://play.google.com/store/apps/details?id=com.vasu.casino.test")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();

        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();
    }

    public void LogOutButtonClick()
    {
        //Debug.Log("Logout After Clear " + Constants.TimerCompletedForBonus);
        Constants.Logout();
    }

    public void UserInfoUpdate()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PlayerId,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.SetPlayerData(jsonNode);
                Debug.LogError("USER_INFO " + jsonNode.ToString());
            }
            else
                Constants.Logout();
        }));
    }
}
