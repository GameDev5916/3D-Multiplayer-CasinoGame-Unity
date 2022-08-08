using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using DG.Tweening;

public class LoginAndRegistrationPanel : MonoBehaviour
{
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject RegistrationPanel;
    [SerializeField] public GameObject warningMsg;
    [SerializeField] private GameObject ForgotPasswordPanel;
    [SerializeField] private GameObject ResetPasswordPanel;

    [Header("For Login Panel")]
    [Space]
    [SerializeField] private TMP_InputField Email_ID;
    [SerializeField] private TMP_InputField Password;

    [Header("For Registation Panel")]
    [Space]
    [SerializeField] private TMP_InputField Name_Registation;
    [SerializeField] private TMP_InputField Email_ID_Registation;
    [SerializeField] private TMP_InputField Password_Registation;
    private TMP_InputField currentSelectedField;

    [Header("For ForgotPass Panel")]
    [Space]
    [SerializeField] private TMP_InputField Email_ID_ForgotPass;

    [Header("For ResetPassword Panel")]
    [Space]
    [SerializeField] private TMP_InputField Code;
    [SerializeField] private TMP_InputField New_Password;
    [SerializeField] private TMP_InputField Confirm_Password;
    private string email_For_resetPassword;

    [SerializeField] private TMP_Dropdown SelectCountry;
    public Sprite a;


    public const string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

    public static bool IsEmail(string email)
    {
        if (email != null && Regex.IsMatch(email, MatchEmailPattern))
        {
            return true;
        }
        else
        {
            Debug.LogError("Email Id Is Not Valid");
            return false;
        }
    }

    public bool CheckPassword(string password)
    {
        if (password != "")
        {
            if (password.Length >= 6)
            {
                return true;
            }
            else
            {
                Debug.LogError("Password have minimum 6 charactor");
                return false;
            }
        }
        else
        {
            Debug.LogError("Password not be empty");
            return false;
        }
    }

    private void OnEnable()
    {
        WebglInput.WebglInputText += SetText;

        SelectCountry.options.Clear();

        if (Constants.instance != null)
        {
            for (int i = 0; i < Constants.instance.Flags.Count; i++)
            {
                //SelectCountry.options.Add(new TMP_Dropdown.OptionData() { text = jsonNode[i]["name"].Value, image = a });
                string a = Constants.instance.Flags[i].name;
                var aa = a.Split('_');
                SelectCountry.options.Add(new TMP_Dropdown.OptionData() { text = aa[1], image = Constants.instance.Flags[i] });
            }
        }
    }

    IEnumerator GetTexture(string url,Action<Texture> action)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        Texture myTexture = DownloadHandlerTexture.GetContent(www);
        action?.Invoke(myTexture);
    }

    private void OnDisable()
    {
        WebglInput.WebglInputText -= SetText;
    }

    public void SelectInputField(TMP_InputField inputField)
    {
        //        currentSelectedField = inputField;

        //#if PLATFORM_WEBGL && !UNITY_EDITOR
        //                    Application.ExternalCall("Getinput", "Enter Field", inputField.text);
        //#else
        //        Debug.LogError("This PlatFrom Not Supported");
        //#endif
        currentSelectedField = inputField;
        if (WebglInput.instance != null)
            WebglInput.instance.SelectInputField(inputField);
    }

    public void SetText(string text)
    {
        currentSelectedField.text = text;
        currentSelectedField.DeactivateInputField();
    }

    public void LoginButtonClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Debug.Log("Login Button Free Spin: " + Constants.TimerCompletedForBonus);
        if (!IsEmail(Email_ID.text))
        {
            StartCoroutine(ShowWarningMsg("Email id is not valid"));
        }
        else if (!CheckPassword(Password.text))
        {
            StartCoroutine(ShowWarningMsg("Password must be have minimum 6 charactor long"));
        }
        else if (IsEmail(Email_ID.text) && CheckPassword(Password.text))
        {
            Login.instance.GustLoginButtonClick(Email_ID.text, Password.text);
        }
    }

    public void CreateNewUser()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        RegistrationPanel.SetActive(true);
        RegistrationPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);

        LoginPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                LoginPanel.SetActive(false);
            });

        //LoginPanel.SetActive(false);
        //RegistrationPanel.SetActive(true);

        Name_Registation.text = "";
        Email_ID_Registation.text = "";
        Password_Registation.text = "";
    }

    public void RegistationButtonCLick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (Name_Registation.text == "")
        {
            StartCoroutine( ShowWarningMsg("Name id is not valid"));
        }
        else if (!IsEmail(Email_ID_Registation.text))
        {
            StartCoroutine(ShowWarningMsg("Email id is not valid"));
        }
        else if (!CheckPassword(Password_Registation.text))
        {
            StartCoroutine(ShowWarningMsg("Password must be have minimum 6 charactor long"));
        }
        else if (IsEmail(Email_ID_Registation.text) && CheckPassword(Password_Registation.text))
        {
            Login.instance.GustResitration(Name_Registation.text, Email_ID_Registation.text, Password_Registation.text, SelectCountry.captionText.text);
        }
    }

    public void CloseButtonClickOnRegistation()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        RegistrationPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                ForgotPasswordPanel.SetActive(false);
            });
        LoginPanel.SetActive(true);
        LoginPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);

        //RegistrationPanel.SetActive(false);
        //LoginPanel.SetActive(true);
        Email_ID.text = "";
        Password.text = "";
    }

    public void FbLoginButtonCLick()
    {
        Login.instance.FB_LoginButtonClick();
    }

    public void GoogleLoginButtonClick()
    {
        Login.instance.GoogleLoginButtonClick();
    }

   public IEnumerator ShowWarningMsg(string msg)
    {
        warningMsg.SetActive(false);
        warningMsg.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = msg;
        warningMsg.SetActive(true);

        yield return new WaitForSeconds(2f);

        warningMsg.SetActive(false);
    }

    #region Forgot Password

    public void ForgotPassword()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Email_ID_ForgotPass.text = "";
        ForgotPasswordPanel.SetActive(true);
        ForgotPasswordPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);

        LoginPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                LoginPanel.SetActive(false);
            });
    }

    public void BackButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        ForgotPasswordPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                ForgotPasswordPanel.SetActive(false);
            });
        LoginPanel.SetActive(true);
        LoginPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }

    public void SendButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        if (!IsEmail(Email_ID_ForgotPass.text))
        {
            StartCoroutine(ShowWarningMsg("Email id is not valid"));
            return;
        }

        GameObject Loading = Instantiate(Constants.instance.LoadingScreen);

        JSONNode data = new JSONObject
        {
            ["email"] = Email_ID_ForgotPass.text,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_Forgot_Password, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                Constants.ShowWarning("Check your Email");
                email_For_resetPassword = Email_ID_ForgotPass.text;
                ForgotPasswordPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
                    .OnComplete(() =>
                    {
                        Destroy(Loading);
                        ForgotPasswordPanel.SetActive(false);
                    });

                Code.text = "";
                New_Password.text = "";
                Confirm_Password.text = "";

                ResetPasswordPanel.SetActive(true);
                ResetPasswordPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
            }
            else
            {
                Destroy(Loading);
                Constants.ShowWarning("Invalid Email Id");
            }
        }));
    }

    #endregion Forgot Password

    #region ResetPassword

    public void ResetPasswordButtonClick()
    {
        if (Code.text == "")
        {
            StartCoroutine(ShowWarningMsg("Code can not be null"));
            return;
        }
        else if(!CheckPassword(New_Password.text))
        {
            StartCoroutine(ShowWarningMsg("Password must be have minimum 6 charactor long"));
            return;
        }
        else if (New_Password.text != Confirm_Password.text)
        {
            StartCoroutine(ShowWarningMsg("New Password and Confrim Password Are not Same"));
            return;
        }

        JSONNode data = new JSONObject
        {
            ["email"] = email_For_resetPassword,
            ["token"] = Code.text,
            ["password"] = New_Password.text,
            ["confirm_password"] = Confirm_Password.text,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_Reset_Password, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                Constants.ShowWarning("Password Reset Successfully");
                ResetPassBackButtonClick();
            }
            else
            {
                Constants.ShowWarning("Code Is Invalid");
                ResetPassBackButtonClick();
            }
        }));
    }

    public void ResetPassBackButtonClick()
    {
        ResetPasswordPanel.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
                   .OnComplete(() =>
                   {
                       ResetPasswordPanel.SetActive(false);
                   });
        LoginPanel.SetActive(true);
        LoginPanel.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }
    #endregion ResetPassword
}
