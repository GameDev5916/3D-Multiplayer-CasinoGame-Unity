using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
using Casino_Poker;
using BalckJack;

public class FriendScript : MonoBehaviour
{
    public RawImage profilePic;
    public TextMeshProUGUI friendName, srNumber;
    public GameObject onlineIndicator, offlineIndicator;
    public Button inviteButton;
    public string friendId;
    long roomStack;

    public void InviteButton()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (GameManager_Poker.Instance) roomStack = GameManager_Poker.Instance.MinMaxStakesAmounts[Constants.pokerMinMaxIndex].Min;
        else if (BlackJackGameManager.Instance) roomStack = BlackJackGameManager.Instance.MinMaxesBetAmounts[Constants.blackJackMinMaxIndex].Max;

        JSONNode jsonnode = new JSONObject
        {
            ["friendPlayerId"] = friendId,
            ["senderName"] = Constants.NAME,
            ["roomStake"] = roomStack,
            ["roomType"] = Constants.RoomType,
            ["senderPlayerId"] = Constants.PLAYER_ID,
            ["roomName"] = Constants.RoomName,
            ["maxAmount"] = Constants.PokerMaxAmount,
            ["position"] = Constants.SelectedInvite,
        };

        Debug.Log("InviteSendMainSocket " + jsonnode.ToString());
        Debug.Log("InviteSendMainSocket " + Constants.PokerMaxAmount);
        MainNetworkManager.Instance.MainSocket?.Emit("inviteFriendRequest", jsonnode.ToString());
        //NetworkManager_Poker.Instance.PokerSocket?.Emit("inviteFriendRequest", jsonnode.ToString());


        Constants.ShowWarning("Invitation Request Send.");
        inviteButton.interactable = false;

        FriendsPanel.CloseFriendsPanel?.Invoke();
        //StartCoroutine(EnableInviteButton());
    }
}
