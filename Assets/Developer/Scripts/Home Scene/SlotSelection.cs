using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotSelection : MonoBehaviour
{
    public static SlotSelection Instance;

    public ScrollSnap1 ss;
    [SerializeField] Button NextButton;
    [SerializeField] Button PriviousButton;

    [SerializeField] GameObject MostPlayed;
    [SerializeField] GameObject NormalPlayed;

    [SerializeField] GameObject MostPlayedSlotParent;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(this);
    }

    private void OnEnable()
    {
        SwithBetweenMostPlayAndNormal(0);
        ss.MoveToIndex(0);
        NextButton.interactable = true;
        PriviousButton.interactable = false;
    }

    public void BackButtonClick()
    {
        Constants.ShowSelectSlot = false;
        HomePanel.Instance.ScrollView.SetActive(true);
        HomeScreenUIManager.Instance.SlotSelectionPanel.SetActive(false);
        HomePanel.Instance.BG.sprite = HomePanel.Instance.allBGSprites[HomePanel.Instance.bgnumber];
        //TopPanel.Instance.BG.enabled = true;
    }

    public void SetDataOfMostPlayeSlot()
    {
        for (int i = 0; i < MostPlayedSlotParent.transform.childCount; i++)
        {
            SlotSelectForPlay ss = MostPlayedSlotParent.transform.GetChild(i).GetComponent<SlotSelectForPlay>();

            ss.SlotNumber = HomeScreenUIManager.Instance.SoltData[i]["slot_number"].AsInt;
        }
    }

    public void NextButtonClick()
    {
        ss.SnapToIndex(1);
    }

    public void PriviousButtonCLick()
    {
        ss.SnapToIndex(0);
    }

    public void SetButtons(int index)
    {
        if(index == 0)
        {
            NextButton.interactable = true;
            PriviousButton.interactable = false;
        }
        else
        {
            NextButton.interactable = false;
            PriviousButton.interactable = true;
        }
    }

    public void SwithBetweenMostPlayAndNormal(int n)
    {
        if (n == 0)
        {
            NormalPlayed.SetActive(true);
            MostPlayed.SetActive(false);
        }
        else
        {
            SetDataOfMostPlayeSlot();
            MostPlayed.SetActive(true);
            NormalPlayed.SetActive(false);
        }
    }
}
