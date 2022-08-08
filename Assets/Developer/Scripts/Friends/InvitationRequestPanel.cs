using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using SimpleJSON;
using Casino_Poker;
using Facebook.Unity;
using Facebook.MiniJSON;
using BalckJack;

public class InvitationRequestPanel : MonoBehaviour
{
    public TextMeshProUGUI senderName;
    public TextMeshProUGUI tableAmount;
    public TextMeshProUGUI messageText;
    public RawImage profilePic;
    public GameObject BG;

    private void OnEnable()
    {
        MainNetworkManager.SetInvitePanel += SetPanelData;
        BG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
        //SetPanelData();
    }

    private void OnDisable()
    {
        MainNetworkManager.SetInvitePanel -= SetPanelData;
    }

    public void JoinTableButton()
    {
        //if (NetworkManager_Poker.Instance || BlackJackGameManager.Instance)
        //{
        //    JSONNode jsonnode = new JSONObject
        //    {
        //        ["friendPlayerId"] = Constants.PLAYER_ID,
        //        ["requestaction"] = true,
        //        ["senderPlayerId"] = Constants.instance.FrinedInvitationJsonData["senderPlayerId"],
        //        ["roomName"] = Constants.instance.FrinedInvitationJsonData["roomName"],
        //        ["roomStake"] = Constants.instance.FrinedInvitationJsonData["roomStake"],
        //        ["roomType"] = Constants.instance.FrinedInvitationJsonData["roomType"]

        //    };

        //    MainNetworkManager.Instance.MainSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
        //    //NetworkManager_Poker.Instance.PokerSocket?.Emit("acceptFriendRequest", jsonnode.ToString());
        //    Debug.Log("Accept and Join from " + jsonnode["roomType"] + ": " + jsonnode.ToString());

        //}
        if (MainNetworkManager.Instance)
        {
            if (Constants.instance.FrinedInvitationJsonData["roomType"] == "poker")
            {
                Constants.PokerJoinRoom = true;
                Constants.isJoinByInvitation = true;
                //HomeScreenUIManager.Instance?.HomePanel.SetActive(false);
                //HomeScreenUIManager.Instance?.PokerSelection.SetActive(false);
                //HomeScreenUIManager.Instance?.TopPanel.SetActive(false);
                Debug.LogWarning("AcceptPoker " + Constants.instance.FrinedInvitationJsonData.ToString());
                Constants.GotoScene("Poker");
            }
            else if (Constants.instance.FrinedInvitationJsonData["roomType"] == "blackjack")
            {
                Constants.isJoinByInvitation = true;
                //HomeScreenUIManager.Instance?.HomePanel?.SetActive(false);
                //HomeScreenUIManager.Instance?.PokerSelection?.SetActive(false);
                //HomeScreenUIManager.Instance?.TopPanel?.SetActive(false);
                Debug.LogWarning("AcceptBlackJack " + Constants.instance.FrinedInvitationJsonData.ToString());
                Constants.GotoScene("BlackJack");
            }
        }

        CloseButtonClick();
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() =>
            {
                if (HomeScreenUIManager.Instance)
                {
                    HomeScreenUIManager.Instance.InvitationRequestPanel.SetActive(false);
                    Debug.Log("InvitationClose-Home");
                    BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                }
                else if (GameManager_Poker.Instance)
                {
                    GameManager_Poker.Instance.InvitationRequestPanel.SetActive(false);
                    Debug.Log("InvitationClose-Poker");
                    BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                }
                else if(BlackJackGameManager.Instance)
                {
                    BlackJackGameManager.Instance.InviteRequestPanel.SetActive(false);
                    Debug.Log("InvitationClose-BlackJack");
                    BG.GetComponent<RectTransform>().DOAnchorPosY(0, 0.1f);
                }
            });
    }

    private void SetPanelData(JSONNode jsonNode)
    {
        if (jsonNode["status"] == true)
        {
            Debug.Log("Set Invite Panel Data " + jsonNode.ToString());
            Constants.instance.FrinedInvitationJsonData = jsonNode;
            Constants.GetImageFrom64String(jsonNode["senderProfilePic"].Value, (Texture image) =>
            {
                profilePic.texture = image;
            });

            if (jsonNode["senderProfilePic"].Value != "null" && jsonNode["senderProfilePic"].Value != "")
            {
                Constants.GetImageFrom64String(jsonNode["senderProfilePic"].Value, (Texture image) =>
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

            senderName.text = jsonNode["message"].Value;
            messageText.text = "Invited you to play " + jsonNode["roomType"];
            tableAmount.text = "Join their " + jsonNode["roomStake"].Value + " / " + (jsonNode["roomStake"] * 2).ToString() + " table.";
        }
        else if (jsonNode["status"] == false)
        {
            Constants.ShowWarning(jsonNode["message"]);
        }
    }
}
