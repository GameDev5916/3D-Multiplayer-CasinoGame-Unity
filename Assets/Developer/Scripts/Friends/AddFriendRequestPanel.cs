using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SimpleJSON;
using DG.Tweening;
using BalckJack;
using Casino_Poker;

public class AddFriendRequestPanel : MonoBehaviour
{
    public TextMeshProUGUI senderName;
    public TextMeshProUGUI messageText;
    public RawImage profilePic;
    public GameObject BG;

    private void OnEnable()
    {
        MainNetworkManager.SetFriendRequestPanel += SetPanelData;
        BG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }

    private void OnDisable()
    {
        MainNetworkManager.SetFriendRequestPanel += SetPanelData;
    }

    public void AcceptRequestButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        JSONNode jsonnode = new JSONObject
        {
            ["ownerPlayerId"] = Constants.instance.AddFriendRequestJsonData["ownerPlayerId"],
            ["friendPlayerId"] = Constants.instance.AddFriendRequestJsonData["friendPlayerId"],
            ["acceptDecline"] = true,
        };

        MainNetworkManager.Instance.MainSocket?.Emit("acceptaddToFriendRequest", jsonnode.ToString());
        Debug.Log("Accept friend request " + jsonnode.ToString());

        CloseAddFriendRequestPanel();
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        JSONNode jsonnode = new JSONObject
        {
            ["ownerPlayerId"] = Constants.instance.AddFriendRequestJsonData["ownerPlayerId"],
            ["friendPlayerId"] = Constants.instance.AddFriendRequestJsonData["friendPlayerId"],
            ["acceptDecline"] = false,
        };
        MainNetworkManager.Instance.MainSocket?.Emit("acceptaddToFriendRequest", jsonnode.ToString());
        Debug.Log("Reject friend request " + jsonnode.ToString());

        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                HomeScreenUIManager.Instance?.AddFriendRequestPanel.SetActive(false);
                GameManager_Poker.Instance?.InvitationRequestPanel.SetActive(false);
                BlackJackGameManager.Instance?.InviteRequestPanel.SetActive(false);

                //if (HomeScreenUIManager.Instance)
                //{
                //    Debug.Log("InvitationClose-Home");
                //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                //}
                //else if (GameManager_Poker.Instance)
                //{
                //    Debug.Log("InvitationClose-Poker");
                //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                //}
                //else if (BlackJackGameManager.Instance)
                //{
                //    Debug.Log("InvitationClose-BlackJack");
                //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                //}
            });
    }

    private void CloseAddFriendRequestPanel()
    {
        Debug.Log("CloseOnAccept");
        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
     .OnComplete(() =>
     {
         HomeScreenUIManager.Instance?.AddFriendRequestPanel.SetActive(false);
         GameManager_Poker.Instance?.InvitationRequestPanel.SetActive(false);
         BlackJackGameManager.Instance?.InviteRequestPanel.SetActive(false);

         //if (HomeScreenUIManager.Instance)
         //{
         //    Debug.Log("InvitationClose-Home");
         //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
         //}
         //else if (GameManager_Poker.Instance)
         //{
         //    Debug.Log("InvitationClose-Poker");
         //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
         //}
         //else if (BlackJackGameManager.Instance)
         //{
         //    Debug.Log("InvitationClose-BlackJack");
         //    //BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
         //}
     });
    }

    private void SetPanelData(JSONNode jsonNode)
    {
        //if (jsonNode["status"] == true)
        //{
        Debug.Log("Set Friend Request Panel Data " + jsonNode.ToString());
        Constants.instance.AddFriendRequestJsonData = jsonNode;

        if (jsonNode["ownerProfilePic"].Value != "" & jsonNode["ownerProfilePic"].Value != "null")
        {
            Constants.GetImageFrom64String(jsonNode["ownerProfilePic"].Value, (Texture image) =>
            {
                profilePic.texture = image;
            });
        }
        else
        {
            Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
            {
                profilePic.texture = image;
            });
        }
        senderName.text = jsonNode["ownerName"].Value;
        //messageText.text = "Invited you to play " + jsonNode["roomType"];
        //tableAmount.text = "Join their " + jsonNode["roomStake"].Value + " / " + (jsonNode["roomStake"] * 2).ToString() + " table.";
        //}
        //else if (jsonNode["status"] == false)
        //{
        //    Constants.ShowWarning(jsonNode["message"]);
        //}
    }
}
