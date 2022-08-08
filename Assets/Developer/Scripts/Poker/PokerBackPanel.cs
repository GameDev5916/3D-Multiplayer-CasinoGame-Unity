using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Casino_Poker;
using SimpleJSON;
using DG.Tweening;

public class PokerBackPanel : MonoBehaviour
{
    public GameObject BG;
    public Button StandUpButton;
    public Text StandUpText;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(450, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine);
        if(Constants.isJoinByStandUp) {
            StandUpButton.gameObject.SetActive(false);
        } else {
            StandUpButton.gameObject.SetActive(true);
        }
    }

    public void ExitToLobbyButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        NetworkManager_Poker.Instance.Disconnection();
        Constants.isJoinByStandUp = false;
        Constants.GotoScene("Home");
    }

    public void StandUpButtonClick()
    {
        JSONNode jsonnode = new JSONObject
        {
            ["playerId"] = Constants.PLAYER_ID,
        };

        Debug.Log("StandUPButtonClicked " + jsonnode.ToString());
        NetworkManager_Poker.Instance.PokerSocket?.Emit("standUp", jsonnode.ToString());
        CloseButtonClick();
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).From(new Vector2(450, 0)).SetEase(Ease.Linear)
             .OnComplete(() => GameManager_Poker.Instance.BackOptionPanel.SetActive(false));
    }
}
