using UnityEngine;
using DG.Tweening;
using Casino_Poker;
using System;
using UnityEngine.UI;
using SimpleJSON;
using TMPro;

public class FriendsPanel : MonoBehaviour
{
    [SerializeField] private RectTransform friendsPanel;
    public GameObject FriendPrefab;
    public Transform friendsContent;

    public GameObject Bottom, NoFriendsText;

    public static Action CloseFriendsPanel;

    public RectTransform contentFriends;
    public Button NextButton;
    public Button PrivousButton;

    public GameObject SideParent;
    public GameObject ContentsParent;

    public Color SelectedFunctionColor;
    public Color UnselectedFunctionColor;
    public Color SelectedTypeColor;
    public Color UnselectedTypeColor;

    public GameObject BuddiesList, BuddiesAlt;
    public Button BuddiesNextButton;
    public Button BuddiesPreviousButton;
    public RectTransform contentBuddies;

    public GameObject BuddiesTypeButtonParent;
    public TMP_InputField BuddiesSearchText;
    public Text BuddiesResultText;

    public GameObject InvitesList, InvitesAlt;
    public Button InvitesNextButton;
    public Button InvitesPreviousButton;
    public RectTransform contentInvites;

    public GameObject RecentList, RecentAlt;
    public Button RecentNextButton;
    public Button RecentPreviousButton;
    public RectTransform contentRecent;

    private void OnEnable()
    {
        CheckCurrentGame();
        MainNetworkManager.SetFriendsPanel += DisplayFriends;
        CloseFriendsPanel += CloseClick;
        friendsPanel.DOAnchorPosX(0, .35f).From(new Vector2(1300, 0)).SetEase(Ease.Linear);
        SelectBuddiesTypeButtonClick(0);
    }

    private void OnDisable()
    {
        CloseFriendsPanel -= CloseClick;
        MainNetworkManager.SetFriendsPanel -= DisplayFriends;
    }

    private void DisplayFriends(GameObject parent, RectTransform contents, GameObject alt, JSONNode friends)
    {
        parent.SetActive(false);
        alt.SetActive(false);

        if (friends.Count > 0)
        {
            parent.SetActive(true);

            int childs = contents.childCount;

            for (int i = 0; i < childs; i++)
            {
                Destroy(contents.GetChild(i).gameObject);
            }

            for (int i = 0; i < friends.Count; i++)
            {
                JSONNode friend = friends[i];
                JSONNode user = friend["user"];
                if (user.Count == 0)
                    continue;
                user = user[0];
                GameObject _friend = Instantiate(FriendPrefab, contents.transform);
                _friend.transform.SetParent(contents);
                FriendScript friendScript = _friend.GetComponent<FriendScript>();
                friendScript.srNumber.text = (i + 1).ToString();
                friendScript.friendId = user["unique_id"];
                friendScript.friendName.text = friend["count"] <= 1 ? user["name"].Value : string.Format("{0} ({1})", user["name"].Value, friend["count"]);

                if (user["profile_pic"].Value != "null" && user["profile_pic"].Value != "")
                {
                    Constants.GetImageFrom64String(user["profile_pic"].Value, (Texture image) =>
                    {
                        friendScript.profilePic.texture = image;
                    });
                }
                else
                {
                    Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
                    {
                        friendScript.profilePic.texture = image;
                    });
                }

                bool isOnline = user["playerison"];
                friendScript.onlineIndicator.SetActive(isOnline);
                friendScript.offlineIndicator.SetActive(!isOnline);
                friendScript.inviteButton.gameObject.SetActive(/*(!friend.HasKey("invited") || friend["invited"] == 0)
                    &&*/ isOnline && !GameManager_Poker.Instance.IsPlayerInRoom(user["unique_id"]));
            }
        }
        else
        {
            Debug.Log("No Friends Available");
            alt.SetActive(true);
        }
    }

    private void DisplayFriends(JSONNode jsonNode)
    {
        SelectFunctionButtonClick(0);

        // Buddies
        Debug.Log("Buddies JSON Getting Status :" + jsonNode.ToString());
        BuddiesList.SetActive(false);
        BuddiesAlt.SetActive(false);

        if (jsonNode["data"].Count > 0 && jsonNode["status"] == 1)
        {
            BuddiesList.SetActive(true);

            int childs = contentBuddies.childCount;

            for (int i = 0; i < childs; i++)
            {
                Destroy(contentBuddies.GetChild(i).gameObject);
                Debug.Log("FriendChildRemoveOnEnable");
            }

            JSONNode buddies = jsonNode["data"];
            BuddiesResultText.text = String.Format("Active Buddies ({0})", buddies.Count);
            for (int i = 0; i < buddies.Count; i++)
            {
                GameObject _friend = Instantiate(FriendPrefab, contentBuddies.transform);
                _friend.transform.SetParent(contentBuddies);
                FriendScript friendScript = _friend.GetComponent<FriendScript>();
                friendScript.srNumber.text = (i + 1).ToString();
                friendScript.friendId = buddies[i]["unique_id"];
                friendScript.friendName.text = buddies[i]["name"];

                if (buddies[i]["profile_pic"].Value != "null" && buddies[i]["profile_pic"].Value != "")
                {
                    Constants.GetImageFrom64String(buddies[i]["profile_pic"].Value, (Texture image) =>
                    {
                        friendScript.profilePic.texture = image;
                    });
                }
                else
                {
                    Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
                    {
                        friendScript.profilePic.texture = image;
                    });
                }

                bool isOnline = buddies[i]["playerison"];
                friendScript.onlineIndicator.SetActive(isOnline);
                friendScript.offlineIndicator.SetActive(!isOnline);
                friendScript.inviteButton.gameObject.SetActive(isOnline
                    && !GameManager_Poker.Instance.IsPlayerInRoom(buddies[i]["unique_id"]));
            }
        }
        else
        {
            Debug.Log("No Friends Available");
            BuddiesAlt.SetActive(true);
        }

        // Invite Requests
        DisplayFriends(InvitesList, contentInvites, InvitesAlt, jsonNode["invites"]);

        // Recent Requests
        DisplayFriends(RecentList, contentRecent, RecentAlt, jsonNode["recent"]);
    }

    public void CloseClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        friendsPanel.DOAnchorPosX(1300, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            int childs = friendsContent.childCount;

            for (int i = 0; i < childs; i++)
            {
                Destroy(friendsContent.GetChild(0).gameObject);
                Debug.Log("FriendChildRemoveOnClose");
            }
            gameObject.SetActive(false);

        });
    }

    private void CheckCurrentGame()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == "Poker")
            Constants.RoomType = "poker";
        else if (currentScene == "BlackJack")
            Constants.RoomType = "blackjack";
        else
            Debug.Log("No Any Game Running");


        Debug.Log("Room Type: " + Constants.RoomType);
    }

    public void NextButtonClick()
    {
        NextButton.interactable = false;
        PrivousButton.interactable = false;

        contentFriends.DOAnchorPosX(contentFriends.anchoredPosition.x - 835f, .2f).From(new Vector2(contentFriends.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                NextButton.interactable = true;
                PrivousButton.interactable = true;
            });
    }

    public void PreviousButtonClick()
    {
        NextButton.interactable = false;
        PrivousButton.interactable = false;

        contentFriends.DOAnchorPosX(contentFriends.anchoredPosition.x + 835f, .2f).From(new Vector2(contentFriends.anchoredPosition.x, 0)).SetEase(Ease.Linear)
       .OnComplete(() =>
       {
           NextButton.interactable = true;
           PrivousButton.interactable = true;
       });
    }

    public void SelectFunctionButtonClick(int number)
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        GameManager_Poker.Instance.FriendsSelectedFunction = number;

        for (int i = 0; i < SideParent.transform.childCount; i++)
        {
            if (i == number)
            {
                SideParent.transform.GetChild(i).GetComponent<Image>().color = SelectedFunctionColor;
            }
            else
            {
                SideParent.transform.GetChild(i).GetComponent<Image>().color = UnselectedFunctionColor;
            }

            if (i < ContentsParent.transform.childCount)
            {
                ContentsParent.transform.GetChild(i).gameObject.SetActive(i == number);
            }
        }
    }

    public void SelectBuddiesTypeButtonClick(int number)
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        for (int i = 0; i < BuddiesTypeButtonParent.transform.childCount; i++)
        {
            if (i == number)
            {
                BuddiesTypeButtonParent.transform.GetChild(i).GetComponent<Image>().color = SelectedTypeColor;
            }
            else
            {
                BuddiesTypeButtonParent.transform.GetChild(i).GetComponent<Image>().color = UnselectedTypeColor;
            }
        }

        int childs = contentBuddies.childCount;
        for (int i = 0; i < childs; i++)
        {
            FriendScript friendScript = contentBuddies.GetChild(i).GetComponent<FriendScript>();
            bool bActive = false;
            switch (number)
            {
                case 0:
                    bActive = true;
                    break;
                case 1:
                    break;
                case 2:
                    bActive = friendScript.onlineIndicator.activeSelf;
                    break;
            }
            friendScript.gameObject.SetActive(bActive);
        }
    }

    public void BuddiesNextButtonClick()
    {
        BuddiesNextButton.interactable = false;
        BuddiesPreviousButton.interactable = false;

        contentBuddies.DOAnchorPosX(contentBuddies.anchoredPosition.x - 835f, .2f).From(new Vector2(contentBuddies.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                BuddiesNextButton.interactable = true;
                BuddiesPreviousButton.interactable = true;
            });
    }

    public void BuddiesPreviousButtonClick()
    {
        BuddiesNextButton.interactable = false;
        BuddiesPreviousButton.interactable = false;

        contentBuddies.DOAnchorPosX(contentBuddies.anchoredPosition.x + 835f, .2f).From(new Vector2(contentBuddies.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                BuddiesNextButton.interactable = true;
                BuddiesPreviousButton.interactable = true;
            });
    }

    public void InvitesNextButtonClick()
    {
        InvitesNextButton.interactable = false;
        InvitesPreviousButton.interactable = false;

        contentInvites.DOAnchorPosX(contentInvites.anchoredPosition.x - 835f, .2f).From(new Vector2(contentInvites.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                InvitesNextButton.interactable = true;
                InvitesPreviousButton.interactable = true;
            });
    }

    public void InvitesPreviousButtonClick()
    {
        InvitesNextButton.interactable = false;
        InvitesPreviousButton.interactable = false;

        contentInvites.DOAnchorPosX(contentInvites.anchoredPosition.x + 835f, .2f).From(new Vector2(contentInvites.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                InvitesNextButton.interactable = true;
                InvitesPreviousButton.interactable = true;
            });
    }

    public void RecentNextButtonClick()
    {
        RecentNextButton.interactable = false;
        RecentPreviousButton.interactable = false;

        contentRecent.DOAnchorPosX(contentRecent.anchoredPosition.x - 835f, .2f).From(new Vector2(contentRecent.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                RecentNextButton.interactable = true;
                RecentPreviousButton.interactable = true;
            });
    }

    public void RecentPreviousButtonClick()
    {
        RecentNextButton.interactable = false;
        RecentPreviousButton.interactable = false;

        contentRecent.DOAnchorPosX(contentRecent.anchoredPosition.x + 835f, .2f).From(new Vector2(contentRecent.anchoredPosition.x, 0)).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                RecentNextButton.interactable = true;
                RecentPreviousButton.interactable = true;
            });
    }

    public void BuddiesSearchTextChanged(String text)
    {
        text = BuddiesSearchText.text;
        int childs = contentBuddies.childCount;
        for (int i = 0; i < childs; i++)
        {
            FriendScript friendScript = contentBuddies.GetChild(i).GetComponent<FriendScript>();
            if (text == "")
                friendScript.gameObject.SetActive(true);
            else if (!friendScript.friendName.text.Contains(text, StringComparison.CurrentCultureIgnoreCase))
                friendScript.gameObject.SetActive(false);
        }
    }
}
