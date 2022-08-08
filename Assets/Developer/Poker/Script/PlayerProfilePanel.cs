using System;
using System.Collections;
using System.Collections.Generic;
using BalckJack;
using Casino_Poker;
using DG.Tweening;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfilePanel : MonoBehaviour
{
    public GameObject BG;
    public TextMeshProUGUI playerNameBox;
    public TextMeshProUGUI levelBox;
    public RawImage profilePic;
    public string friendId;
    public Button addFriendButton;

    [Header("GameStatPanels")]
    public GameObject GameStatDouble;
    public GameObject GameStatSingle;

    string playerActionGameStat;
    public static Action<JSONNode> SetGameStatPanel;

    public void OnEnable()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(710, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine);
        MainNetworkManager.SetPlayerProfilePanel += SetPlayerProfile;
        NetworkManager_Poker.GameStatAction += OpenGameStatPanel;
        BlackJack_NetworkManager.GameStatAction += OpenGameStatPanel;
    }

    private void OnDisable()
    {
        MainNetworkManager.SetPlayerProfilePanel -= SetPlayerProfile;
        NetworkManager_Poker.GameStatAction -= OpenGameStatPanel;
        BlackJack_NetworkManager.GameStatAction -= OpenGameStatPanel;
    }

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
        addFriendButton.interactable = false;

        //FriendsPanel.CloseFriendsPanel?.Invoke();

        //StartCoroutine(EnableInviteButton());
    }

    private void SetPlayerProfile(JSONNode jsonNode)
    {
        if (jsonNode["staus"] == true)
        {
            if (jsonNode["data"]["playerId"] == Constants.PLAYER_ID)
                addFriendButton.gameObject.SetActive(false);
            else if (jsonNode["data"]["playerId"] != Constants.PLAYER_ID)
                addFriendButton.gameObject.SetActive(true);


            Debug.Log("SetPlayerProfilePanel " + jsonNode.ToString());

            if (jsonNode["data"]["profilepic"].Value != "" && jsonNode["data"]["profilepic"].Value != "null")
            {
                Constants.GetImageFrom64String(jsonNode["data"]["profilepic"].Value, (Texture image) =>
                {
                    profilePic.texture = image;
                    Debug.Log("ProfilePanelPic" + jsonNode["data"]["profile_pic"].Value);
                });
            }
            else
            {
                Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
                {
                    profilePic.texture = image;
                    Debug.Log("DefaultImageSet");
                });
            }

            playerNameBox.text = jsonNode["data"]["name"];
            levelBox.text = jsonNode["data"]["level"];
            friendId = jsonNode["data"]["playerId"];
        }
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (GameStatDouble.activeInHierarchy)
        {
            GameStatDouble?.GetComponent<GameStatDoubleScript>().BG.GetComponent<RectTransform>().DOAnchorPosX(-1300, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    GameStatDouble.SetActive(false);
                    Debug.Log("DoubleStatClose");
                    CloseProfileAnimation();
                });
        }
        else if (GameStatSingle.activeInHierarchy)
        {
            GameStatSingle?.GetComponent<GameStatSingleScript>()?.BG.GetComponent<RectTransform>().DOAnchorPosX(-1300, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine)
               .OnComplete(() =>
               {
                   GameStatSingle.SetActive(false);
                   Debug.Log("SingleStatClose");
                   CloseProfileAnimation();
               });

            GameStatSingle?.GetComponent<GameStatSingleBlackjack>()?.BG.GetComponent<RectTransform>().DOAnchorPosX(-1300, 0.3f).From(new Vector2(0, 0)).SetEase(Ease.InSine)
              .OnComplete(() =>
              {
                  GameStatSingle.SetActive(false);
                  Debug.Log("SingleStatBJClose");
                  CloseProfileAnimation();
              });
        }
        else
        {
            CloseProfileAnimation();
        }
    }

    private void CloseProfileAnimation()
    {
        BG.GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f).From(new Vector2(710, 0)).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (GameManager_Poker.Instance)
            {
                Debug.Log("PokerProfileActive");
                GameManager_Poker.Instance.PlayerProfilePanel.SetActive(false);
            }
            else if (BlackJackGameManager.Instance)
            {
                Debug.Log("BlackJackProfileActive");
                BlackJackGameManager.Instance.PlayerProfilePanel.SetActive(false);
            }
        });
    }

    public void GameStatsButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (Constants.PLAYER_ID == friendId)
        {
            playerActionGameStat = "single";
        }
        else if (Constants.PLAYER_ID != friendId)
        {
            playerActionGameStat = "double";
        }

        JSONNode jsonnode = new JSONObject
        {
            ["ownerPlayerId"] = Constants.PLAYER_ID,
            ["oppositePlayerId"] = friendId,
            ["playerAction"] = playerActionGameStat
        };

        Debug.LogWarning("GameStatButtonClick " + jsonnode.ToString());
        NetworkManager_Poker.Instance.PokerSocket?.Emit("gameStats", jsonnode.ToString());
    }


    public void GameStatsBlackJackButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        playerActionGameStat = "single";
        JSONNode jsonnode = new JSONObject
        {
            ["ownerPlayerId"] = Constants.PLAYER_ID,
            ["oppositePlayerId"] = friendId,
            ["playerAction"] = playerActionGameStat
        };

        Debug.LogWarning("GameStatBlackJackButtonClick " + jsonnode.ToString());
        BlackJack_NetworkManager.Instance.BlackJackSocket?.Emit("gameStats", jsonnode.ToString());
    }


    private void OpenGameStatPanel(JSONNode jsonNode)
    {
        if (jsonNode["playerAction"] == "single")
        {
            Debug.Log("SingleStatPanelOpen");
            GameStatDouble?.SetActive(false);
            GameStatSingle?.SetActive(true);
        }
        else if (jsonNode["playerAction"] == "double")
        {
            Debug.Log("DoubleStatPanelOpen");
            GameStatSingle?.SetActive(false);
            GameStatDouble?.SetActive(true);
        }
        SetGameStatPanel?.Invoke(jsonNode);
    }
}
