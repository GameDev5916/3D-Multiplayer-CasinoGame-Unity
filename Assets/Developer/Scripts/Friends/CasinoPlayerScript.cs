using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;

public class CasinoPlayerScript : MonoBehaviour
{
    public RawImage profilePic;
    public TextMeshProUGUI playerName, srNumber;
    public GameObject onlineIndicator, offlineIndicator;
    public Button addfriendButton;
    public string friendId;

    public void AddFriendButton()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        JSONNode jsonnode = new JSONObject
        {
            ["ownerPlayerId"] = Constants.PLAYER_ID,
            ["friendPlayerId"] = friendId,
        };

        MainNetworkManager.Instance.MainSocket?.Emit("addToFriend", jsonnode.ToString());
        Debug.Log("AddFriendButtonClick " + jsonnode.ToString());

        Constants.ShowWarning("Friend Request Sent.");
        addfriendButton.interactable = false;

        //FriendsPanel.CloseFriendsPanel?.Invoke();

        //StartCoroutine(EnableInviteButton());
    }
}
