using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using SimpleJSON;
using UnityEngine.UI;

public class EnterCodePanel : MonoBehaviour
{
    [SerializeField] private GameObject BG;
    private TMP_InputField CurrentInputFiled;
    public TMP_InputField CodeText;


    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
        CodeText.text = "";
        MainNetworkManager.OnEnterCode += CodeEnter;
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() => HomeScreenUIManager.Instance.EnterCodePanel.SetActive(false));
    }

    public void SelectInputField(TMP_InputField inputField)
    {
        CurrentInputFiled = inputField;
        WebglInput.instance.SelectInputField(inputField);
    }

    public void DoneButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        JSONNode data = new JSONObject
        {
            ["playerId"] = Constants.PLAYER_ID,
            ["inviterefercode"] = CodeText.text,
        };

        Debug.LogError(data.ToString());

        MainNetworkManager.Instance.MainSocket?.Emit("enterinvitecode", data.ToString());
        //Constants.ApiCall(Constants.API_Enter_Refer_Code);
    }

    private void CodeEnter(JSONNode jsonNode)
    {
        if(jsonNode["status"].AsInt == 0)
        {
            Constants.ShowWarning(jsonNode["message"].Value);
        }
        else
        {
            Constants.ShowWarning(jsonNode["messssage"].Value);
            Constants.SetPlayerData(jsonNode["data"]);
            CloseButtonClick();
        }
    }
}
