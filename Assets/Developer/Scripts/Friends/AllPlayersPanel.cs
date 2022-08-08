using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;
using SimpleJSON;

public class AllPlayersPanel : MonoBehaviour
{
    [SerializeField] private RectTransform allPlayerPanel;
    public GameObject CasinoPlayerPrefab;
    public Transform allPlayerContent;

    public GameObject Bottom, NoPlayerText;

    public RectTransform contentAllPlayer;
    public Button NextButton;
    public Button PrivousButton;


    private void OnEnable()
    {
        MainNetworkManager.SetAllPlayerPanel += DisplayAllPlayers;
        allPlayerPanel.DOAnchorPosX(0, 0.35f).From(new Vector2(1300, 0)).SetEase(Ease.Linear);
        //NextPreviousButtonCheck();
    }

    private void OnDisable()
    {
        MainNetworkManager.SetAllPlayerPanel -= DisplayAllPlayers;
    }

    private void DisplayAllPlayers(JSONNode jsonNode)
    {
        Debug.Log("All Player List JSON Getting Status :" + jsonNode["status"].Value);
        Debug.Log("DataCount " + jsonNode["data"].Count);

        if (jsonNode["data"].Count > 0 && jsonNode["status"] == true)
        {
            Bottom.SetActive(true);
            NoPlayerText.SetActive(false);

            int childs = allPlayerContent.childCount;

            for (int i = 0; i < childs; i++)
            {
                Destroy(allPlayerContent.GetChild(0).gameObject);
                //Debug.Log("AllPlayerChildRemoveOnEnable");
            }

            for (int i = 0; i < jsonNode["data"].Count; i++)
            {
                //Debug.Log("DisplayPlayer " + i);

                GameObject _casinoPlayer = Instantiate(CasinoPlayerPrefab, allPlayerContent.transform);
                _casinoPlayer.transform.SetParent(allPlayerContent);

                CasinoPlayerScript casinoPlayerScript = _casinoPlayer.GetComponent<CasinoPlayerScript>();

                casinoPlayerScript.srNumber.text = (i + 1).ToString();
                casinoPlayerScript.friendId = jsonNode["data"][i]["unique_id"];
                casinoPlayerScript.playerName.text = jsonNode["data"][i]["name"] + " # " + jsonNode["data"][i]["shortuser_id"];

                if (jsonNode["data"][i]["profile_pic"].Value != "" && jsonNode["data"][i]["profile_pic"].Value != "null")
                {
                    Constants.GetImageFrom64String(jsonNode["data"][i]["profile_pic"].Value, (Texture image) =>
                    {
                        casinoPlayerScript.profilePic.texture = image;
                        Debug.Log("ProfilePIC" + jsonNode["data"][i]["profile_pic"].Value);
                    });
                }
                else
                {
                    Constants.GetImageFrom64String(Constants.PLAYER_PHOTO_64STRING, (Texture image) =>
                    {
                        casinoPlayerScript.profilePic.texture = image;
                        Debug.Log("DefaultImageSet");
                    });
                }

                bool isOnline = jsonNode["data"][i]["playerison"];
                casinoPlayerScript.onlineIndicator.SetActive(isOnline);
                casinoPlayerScript.offlineIndicator.SetActive(!isOnline);
                //casinoPlayerScript.addfriendButton.interactable = true;
            }
        }
        else
        {
            Debug.Log("No Players Available ");
            Bottom.SetActive(false);
            NoPlayerText.SetActive(true);
        }
    }

    public void AllPlayerPanelCloseButton()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        allPlayerPanel.DOAnchorPosX(1300, .35f).SetEase(Ease.Linear).OnComplete(() =>
        {
            for (int i = 0; i < allPlayerContent.childCount; i++)
            {
                Destroy(allPlayerContent.GetChild(i).gameObject);
                //Debug.Log("allPlayerChildRemoveOnClose");
            }
            gameObject.SetActive(false);
        });
    }

    public void NextButtonClick()
    {
        NextButton.interactable = false;
        PrivousButton.interactable = false;

        contentAllPlayer.DOAnchorPosX(contentAllPlayer.anchoredPosition.x - 835f, .2f).From(new Vector2(contentAllPlayer.anchoredPosition.x, 0)).SetEase(Ease.Linear)
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

        contentAllPlayer.DOAnchorPosX(contentAllPlayer.anchoredPosition.x + 835f, .2f).From(new Vector2(contentAllPlayer.anchoredPosition.x, 0)).SetEase(Ease.Linear)
       .OnComplete(() =>
        {
            NextButton.interactable = true;
            PrivousButton.interactable = true;
        });
    }



    //public void NextPreviousButtonCheck()
    //{
    //    if (contentAllPlayer.anchoredPosition.x >= 0)
    //    {
    //        NextButton.interactable = true;
    //        PrivousButton.interactable = false;
    //    }

    //    if(contentAllPlayer.anchoredPosition.x < 0)
    //    {
    //        NextButton.interactable = false;
    //        PrivousButton.interactable = true;
    //    }
    //}
}
