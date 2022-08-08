using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using SimpleJSON;

public class FreeSpinGameManager : MonoBehaviour
{
    public List<long> Prices;
    public float[] AnglesArray;

    public GameObject Wheel;

    public bool isStarted;
    public float StartAngle;
    public float CurrentLerpRotationTime;
    public float FinalAngle;

    public GameObject GoldButton, CentButton, CloseButton;

    public GameObject WinPanel;
    public Text WinText;
    public TextMeshProUGUI GoldAmount, ChipsAmount;

    [Header("Panel")]
    public GameObject GoldPopUpPanel;
    public GameObject PurchasePopUpPanel;
    public GameObject warningMsg;

    private void Awake()
    {
        for (int i = 0; i < Prices.Count; i++)
        {
            Wheel.transform.GetChild(i).GetChild(0).GetComponent<Text>().text = Constants.NumberShow(Prices[i]).ToString();
        }
    }

    private void OnEnable()
    {
        Constants.On_Chips_Gold_Update += UpdateGold;
        ;

        //UpdateChipsGold();
        //UpdateGold();
        Constants.instance.Chips_Gold_Update();
        //Button1.transform.DOLocalMoveX(0, 1f).SetRelative(true).From(-1500).SetEase(Ease.InOutBack);
        CloseButton.GetComponent<RectTransform>().DOAnchorPosX(-90, .5f).From(new Vector2(200, 0)).SetEase(Ease.InOutBack).SetDelay(1);
        GoldButton.GetComponent<RectTransform>().DOAnchorPosX(92, .5f).From(new Vector2(-500, 0)).SetEase(Ease.InOutBack).SetDelay(1);
        CentButton.GetComponent<RectTransform>().DOAnchorPosX(92, .5f).From(new Vector2(-500, 0)).SetEase(Ease.InOutBack).SetDelay(1.3f);
    }

    private void OnDisable()
    {

        Constants.On_Chips_Gold_Update -= UpdateGold;
    }

    public void UpdateChipsGold()
    {
        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_User_Info, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.SetPlayerData(jsonNode);
                ChipsAmount.text = Constants.NumberShow(Constants.CHIPS);
                GoldAmount.text = Constants.NumberShow(Constants.GOLDS); Debug.LogError("PokerChipsUpdatedFromGameplay " + jsonNode.ToString());
            }
            else
                Constants.Logout();
        }));
    }


    private void UpdateGold()
    {
        ChipsAmount.text = Constants.NumberShow(Constants.CHIPS);
        GoldAmount.text = Constants.NumberShow(Constants.GOLDS);
    }

    public void StartWheel()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        GoldButton.GetComponent<Button>().interactable = false;
        CentButton.GetComponent<Button>().interactable = false;
        int fullCircles = 5;
        float randomFinalAngle = AnglesArray[UnityEngine.Random.Range(0, AnglesArray.Length)];

        // Here we set up how many circles our wheel should rotate before stop
        FinalAngle = -(fullCircles * 360 + randomFinalAngle);
        CurrentLerpRotationTime = 0;
        //isStarted = true;

        StartCoroutine(WheelRotate());
    }

    IEnumerator WheelRotate()
    {
        float maxLerpRotationTime = 4f;

        // increment timer once per frame
        while (CurrentLerpRotationTime <= maxLerpRotationTime)
        {
            CurrentLerpRotationTime += Time.deltaTime;

            // Calculate current position using linear interpolation
            float t = CurrentLerpRotationTime / maxLerpRotationTime;

            // This formulae allows to speed up at start and speed down at the end of rotation.
            // Try to change this values to customize the speed
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            float angle = Mathf.Lerp(StartAngle, FinalAngle, t);
            Wheel.transform.eulerAngles = new Vector3(0, 0, angle);

            if (CurrentLerpRotationTime > maxLerpRotationTime)
            {
                SoundManager.Instance.PlaySound(SoundManager.SoundEnums.OneSpinComplete);

                StopCoroutine(WheelRotate());
                StartAngle = FinalAngle % 360;
                FindReward();
            }

            yield return new WaitForSeconds(0.01f);
        }
    }

    public void FindReward()
    {
        switch ((int)StartAngle)
        {
            case 0:
                GiveReward(Prices[0]);
                break;
            case -24:
                GiveReward(Prices[14]);
                break;
            case -48:
                GiveReward(Prices[13]);
                break;
            case -72:
                GiveReward(Prices[12]);
                break;
            case -96:
                GiveReward(Prices[11]);
                break;
            case -120:
                GiveReward(Prices[10]);
                break;
            case -144:
                GiveReward(Prices[9]);
                break;
            case -168:
                GiveReward(Prices[8]);
                break;
            case -192:
                GiveReward(Prices[7]);
                break;
            case -216:
                GiveReward(Prices[6]);
                break;
            case -240:
                GiveReward(Prices[5]);
                break;
            case -264:
                GiveReward(Prices[4]);
                break;
            case -288:
                GiveReward(Prices[3]);
                break;
            case -312:
                GiveReward(Prices[2]);
                break;
            case -336:
                GiveReward(Prices[1]);
                break;
        }
    }

    public void GiveReward(long Amount)
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.GameWin);

        Constants.CHIPS += Amount;
        Constants.instance.Chips_Gold_Update();
        WinPanel.SetActive(true);
        WinText.text = $"You win {Constants.NumberShow(Amount)} Chips";
        //Debug.LogError(Amount);
        //Debug.LogError("Total " + Constants.CHIPS);
    }

    public void CloseButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        Constants.GotoScene("Home");
    }

    public void PlayNowButtonCLick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        GoldButton.GetComponent<Button>().interactable = true;
        CentButton.GetComponent<Button>().interactable = true;
        //UpdateChipsGold();
        //UpdateGold();
        Constants.instance.Chips_Gold_Update();
        WinPanel.SetActive(false);
    }

    public void CentButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        PurchasePopUpPanel.SetActive(true);
    }

    public void GoldButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        GoldPopUpPanel.SetActive(true);
    }

    public void PurchessButtonClick()
    {
        StartWheel();
        PurchasePopUpPanel.SetActive(false);
        GoldButton.GetComponent<Button>().interactable = false;
        CentButton.GetComponent<Button>().interactable = false;
    }

    public void SpinButtonClickOnGold()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        if (Constants.GOLDS >= 500)
        {
            StartWheel();
            Constants.GOLDS -= 500;
            GoldPopUpPanel.SetActive(false);
            GoldButton.GetComponent<Button>().interactable = false;
            CentButton.GetComponent<Button>().interactable = false;
            //UpdateChipsGold();
            //UpdateGold();
            Constants.instance.Chips_Gold_Update();
        }
        else
        {
            StartCoroutine(ShowWarningMsg("Not Have Enough Gold"));
        }
    }

    public void CancalButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        GoldPopUpPanel.SetActive(false);
        PurchasePopUpPanel.SetActive(false);
    }

    public IEnumerator ShowWarningMsg(string msg)
    {
        warningMsg.SetActive(false);
        warningMsg.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = msg;
        warningMsg.SetActive(true);

        yield return new WaitForSeconds(2f);

        warningMsg.SetActive(false);
    }
}
