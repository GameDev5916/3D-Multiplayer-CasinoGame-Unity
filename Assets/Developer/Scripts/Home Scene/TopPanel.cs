using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopPanel : MonoBehaviour
{
    public static TopPanel Instance;

    public RawImage Profile_Pic;
    public Text PlayerName;
    public Text ChipsText;
    public Text GoldText;
    public Text LevelText;
    public Image LevelSlider;
    public Image BG;

    private void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        SetPlayerData();
        Constants.ProfilePicUpadate += SetProfileImage;
        Constants.On_Level_Or_Percentage_Update += LevelUpdate;
        Constants.On_Chips_Gold_Update += ChipAndGoldUpdate;
    }

    private void OnDisable()
    {
        Constants.ProfilePicUpadate -= SetProfileImage;
        Constants.On_Level_Or_Percentage_Update -= LevelUpdate;
        Constants.On_Chips_Gold_Update -= ChipAndGoldUpdate;
    }

    public void SetProfileImage()
    {
        Profile_Pic.texture = Constants.PROFILE_PIC_TEXTURE;
    }

    public void ChipAndGoldUpdate()
    {
        ChipsText.text = Constants.NumberShow(Constants.CHIPS);
        GoldText.text = Constants.NumberShow(Constants.GOLDS);
    }

    public void SetPlayerData()
    {
        PlayerName.text = Constants.NAME;
        //SetProfileImage();
        ChipAndGoldUpdate();

        if (Constants.PROFILE_PIC_TEXTURE)
            SetProfileImage();

        LevelUpdate();
    }

    public void LevelUpdate()
    {
        LevelText.text = $"LEVEL {Constants.LEVEL}";
        LevelSlider.fillAmount = Constants.LEVEL_PERCENTAGE/100f ;
    }

    public void ProfileButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.HomePanel.SetActive(false);
        HomeScreenUIManager.Instance.ShopPanel.SetActive(false);
        HomeScreenUIManager.Instance.SlotSelectionPanel.SetActive(false);
        HomeScreenUIManager.Instance.TopPanel.SetActive(false);
        HomeScreenUIManager.Instance.PokerSelection.SetActive(false);
        HomeScreenUIManager.Instance.AllPlayersPanel.SetActive(false);
        HomeScreenUIManager.Instance.ProfilePanel.SetActive(true);
    }

    public void ShopButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        HomeScreenUIManager.Instance.ShopPanel.SetActive(true);
        HomeScreenUIManager.Instance.HomePanel.SetActive(false);
        HomeScreenUIManager.Instance.TopPanel.SetActive(true);
        HomeScreenUIManager.Instance.PokerSelection.SetActive(false);
        HomeScreenUIManager.Instance.SlotSelectionPanel.SetActive(false);
        //BG.enabled = true;
    }

    public void GiftButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.GiftPanel.SetActive(true);
    }

    public void ManuButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.ManuPanel.SetActive(true);
    }

    public void VIPButtonCLick()
    {
        //SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        //HomeScreenUIManager.Instance.VIPPanel.SetActive(true);
        //HomeScreenUIManager.Instance.HomePanel.SetActive(false);
        //HomeScreenUIManager.Instance.ShopPanel.SetActive(false);
        //HomeScreenUIManager.Instance.TopPanel.SetActive(false);
    }
}
