using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using UnityEngine.Networking;
using DG.Tweening;
using System;
using UnityEditor;

public class ProfilePanel : MonoBehaviour
{
    public GameObject BG1, BG2;

    public RawImage Profile_Pic;
    public Text PlayerName;
    public Text PlayerName2;
    public Text ChipsText;
    public Text GoldText;
    public Text VipTireText;
    public Text LevelText;
    public Image LevelSlider;
    public GameObject EditIcon;

    public TMP_InputField CountryName;
    private TMP_InputField CurrentInputFiled;

    public TMP_Dropdown SelectCountry;

    //public Texture2D tex;
    //public string a;

    private void Start()
    {
        SelectCountry.options.Clear();

        for (int i = 0; i < Constants.instance.Flags.Count; i++)
        {
            string a = Constants.instance.Flags[i].name;
            var aa = a.Split('_');
            SelectCountry.options.Add(new TMP_Dropdown.OptionData() { text = aa[1], image = Constants.instance.Flags[i] });
        }

        if (Constants.COUNTRY != "")
        {
            if (SelectCountry.options.FindIndex(option => option.text == Constants.COUNTRY) != 0)
                SelectCountry.value = SelectCountry.options.FindIndex(option => option.text == Constants.COUNTRY);
        }
    }

    private void OnEnable()
    {
        //a = Convert.ToBase64String(tex.EncodeToPNG());
        BG1.transform.DOLocalMove(new Vector3(-710, -85, 0), .5f).From(new Vector3(-1500, -85, 0)).SetRelative(true).SetEase(Ease.InOutFlash);
        BG2.transform.DOLocalMove(new Vector3(260, -85, 0), .5f).From(new Vector3(2188, -85, 0)).SetRelative(true).SetEase(Ease.InOutFlash);

        SetPlayerData();
        Constants.ProfilePicUpadate += SetProfileImage;
        WebglInput.WebglInputText += InputFildTextFromWebGl;
        Constants.On_Level_Or_Percentage_Update += LevelUpdate;
    }

    private void OnDisable()
    {
        Constants.ProfilePicUpadate -= SetProfileImage;
        WebglInput.WebglInputText -= InputFildTextFromWebGl;
        Constants.On_Level_Or_Percentage_Update -= LevelUpdate;
    }

    public void SetProfileImage()
    {
        Profile_Pic.texture = Constants.PROFILE_PIC_TEXTURE;
    }

    public void SetPlayerData()
    {
        PlayerName.text = Constants.NAME;
        PlayerName2.text = Constants.NAME;
        ChipsText.text = Constants.NumberShow(Constants.CHIPS);
        GoldText.text = Constants.NumberShow(Constants.GOLDS);

        VipTireText.text = Constants.VIP_TIER_LEVEL switch
        {
            1 => "Bronze",
            2 => "Gold",
            3 => "Platinum",
            4 => "Emerald",
            5 => "Diamond",
            6 => "Diamond Plus",
            _ => "Null"
        };

        if (Constants.COUNTRY != "null")
        CountryName.text = Constants.COUNTRY;

        if (Constants.PROFILE_PIC_TEXTURE)
            SetProfileImage();

        if (Constants.LOGIN_TYPE != "guest")
            EditIcon.SetActive(false);
        else
            EditIcon.SetActive(true);

        LevelUpdate();
    }

    public void LevelUpdate()
    {
        LevelText.text = $"LV {Constants.LEVEL}";
        LevelSlider.fillAmount = Constants.LEVEL_PERCENTAGE / 100f;
    }

    public void OnCountryValueChange()
    {
        StartCoroutine(UpdateProfile());
    }

    public IEnumerator UpdateProfile()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
            ["profile_pic"] = Constants.PLAYER_PHOTO_URL,
            ["country"] = SelectCountry.captionText.text,
        };

        Debug.LogError(data.ToString());
        UnityWebRequest result = UnityWebRequest.Put(Constants.API_PROFILE_UPDATE, data.ToString());
        result.method = UnityWebRequest.kHttpVerbPOST;
        result.SetRequestHeader("Content-Type", "application/json");
        result.SetRequestHeader("Accept", "application/json");

        yield return result.SendWebRequest();

        if (result.result != UnityWebRequest.Result.Success)
        {
            OnCountryValueChange();
            Debug.LogError("faild");
        }
        else
        {
            Debug.Log("Update Success");
            Constants.COUNTRY = CountryName.text;
        }
    }

    public void SelectInputField(TMP_InputField inputField)
    {
        CurrentInputFiled = inputField;
        WebglInput.instance.SelectInputField(inputField);
    }

    public void InputFildTextFromWebGl(string Text)
    {
        CurrentInputFiled.text = Text;
        OnCountryValueChange();
    }

    public void LevelRewardButtonCLick()
    {
    }

    public void GiftButtonClick()
    {
    }

    public void CloseButtonCLick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        BG2.transform.DOLocalMove(new Vector3(2188, 0, 0), .5f).SetRelative(true).SetEase(Ease.InOutFlash);
        BG1.transform.DOLocalMove(new Vector3(-710, 0, 0), .5f).SetRelative(true).SetEase(Ease.InOutFlash).OnComplete(() =>
        {
            HomeScreenUIManager.Instance.ProfilePanel.SetActive(false);
            HomeScreenUIManager.Instance.HomePanel.SetActive(true);
            HomeScreenUIManager.Instance.TopPanel.SetActive(true);
            HomeScreenUIManager.Instance.SlotSelectionPanel.SetActive(false);
            HomePanel.Instance.ScrollView.SetActive(true);
            //TopPanel.Instance.BG.enabled = false;
        });
    }

    public void PickImageFromGalary()
    {
        PickImage(512);
    }

    private void PickImage(int maxSize)
    {
        if (Constants.LOGIN_TYPE != "guest")
            return;

        if (!Constants.CheckImageGallaryPermission())
            return;

        Texture2D texture = null;
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                texture = NativeGallery.LoadImageAtPath(path, maxSize,false);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }
                Constants.PROFILE_PIC_TEXTURE = texture;
                Profile_Pic.texture = texture;

                Debug.LogError(Convert.ToBase64String(texture.EncodeToPNG()));

                JSONNode data = new JSONObject
                {
                    ["unique_id"] = Constants.PLAYER_ID,
                    ["profile_pic"] = Convert.ToBase64String(texture.EncodeToPNG()),
                    ["country"] = SelectCountry.captionText.text,
                };

                StartCoroutine(Constants.ApiCall(Constants.API_PROFILE_UPDATE, data.ToString(), (bool IsSuccess, string result) =>
                {
                    if (IsSuccess)
                    {
                        JSONNode jsonNode = JSON.Parse(result)["data"];
                        Debug.LogError(jsonNode.ToString());

                        Constants.PLAYER_PHOTO_URL = jsonNode["profile_pic"].Value;
                        Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_URL, (Texture tex) =>
                        {
                            Constants.PROFILE_PIC_TEXTURE = tex;
                        });
                    }
                    else
                    {
                        Debug.LogError("Faild");
                    }
                }));
            }
        });
    }
}
