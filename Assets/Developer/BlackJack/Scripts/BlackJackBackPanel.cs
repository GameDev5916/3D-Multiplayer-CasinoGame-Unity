using UnityEngine;
using Casino_Poker;
using DG.Tweening;
using SimpleJSON;
using BalckJack;

public class BlackJackBackPanel : MonoBehaviour
{
    public GameObject BG;

    private void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(450, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine);
    }

    public void ExitToLobbyButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        BlackJack_NetworkManager.Instance.Disconnection();
        BlackJack_NetworkManager.isconnected = false;
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
        BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("standUp", jsonnode.ToString());
        CloseButtonClick();
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.3f).From(new Vector2(450, 0)).SetEase(Ease.Linear)
             .OnComplete(() => BlackJackGameManager.Instance.BackOptionPanel.SetActive(false));
    }
}
