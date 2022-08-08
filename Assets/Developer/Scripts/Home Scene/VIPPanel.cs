using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class VIPPanel : MonoBehaviour
{
    [SerializeField] private GameObject VipTierParentObject;
    [SerializeField] private Slider Slider;
    [SerializeField] private TextMeshProUGUI TierBanifitText;

    [SerializeField] private TextMeshProUGUI ChipBenefitText;
    [SerializeField] private TextMeshProUGUI GoldBenefitText;
    [SerializeField] private TextMeshProUGUI FriendBenefitText;
    [SerializeField] private TextMeshProUGUI LuckyBenefitText;


    [SerializeField] private GameObject TierObject;
    [SerializeField] private GameObject TireInfoObject;
    [SerializeField] private GameObject RedBox;


    [Header("VIPFQA Panel")]
    [SerializeField] private GameObject ParentOFData;

    public int n=1;

    private void OnEnable()
    {
        SetData();
        SetVIPBenifitData();
        SetDataForFQAPanel();

        TierObject.GetComponent<RectTransform>().DOAnchorPosX(0, .5f).From(new Vector2(2200, 0)).SetEase(Ease.InOutBack);
        TireInfoObject.GetComponent<RectTransform>().DOAnchorPosX(-307, .5f).From(new Vector2(-2200, 0)).SetEase(Ease.InOutBack).SetDelay(.4f);
        RedBox.GetComponent<RectTransform>().DOAnchorPosX(580, .5f).From(new Vector2(2200, 0)).SetEase(Ease.InOutBack).SetDelay(.8f);
    }

    private void SetDataForFQAPanel()
    {
        for (int i = 0; i < ParentOFData.transform.childCount; i++)
        {
            Transform a = ParentOFData.transform.GetChild(i).GetChild(1).transform;
            for (int j = 0; j < a.childCount; j++)
            {
                var name = j switch
                {
                    0 => "chips",
                    1 => "gold",
                    2 => "friend",
                    3 => "lucky",
                    _ => "null",
                };

                if(name != "null")
                    a.GetChild(j).GetComponent<TextMeshProUGUI>().text = $"+{HomeScreenUIManager.Instance.VIPData["data"][i]["benefites"][name]}%";
                else
                    a.GetChild(j).GetComponent<TextMeshProUGUI>().text = $"-";
            }
        }
    }

    public void SetVIPBenifitData()
    {
        TierBanifitText.text = $"TIER {n + 1} BENEFITS";

        ChipBenefitText.text = $"+{HomeScreenUIManager.Instance.VIPData["data"][n]["benefites"]["chips"]}%";
        GoldBenefitText.text = $"+{HomeScreenUIManager.Instance.VIPData["data"][n]["benefites"]["gold"]}%";
        FriendBenefitText.text = $"+{HomeScreenUIManager.Instance.VIPData["data"][n]["benefites"]["friend"]}%";
        LuckyBenefitText.text = $"+{HomeScreenUIManager.Instance.VIPData["data"][n]["benefites"]["lucky"]}%";
    }

    private void SetData()
    {
        VipTierParentObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-307*n,0);
        Slider.value = 0.064f;
        float a = .188f * n;
        Slider.value += a;

        for (int i = 0; i < 6; i++)
        {
            if(i == n)
            {
                VipTierParentObject.transform.GetChild(i).GetChild(0).transform.localScale = new Vector3(1,1,1);
                VipTierParentObject.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1,1,1,1f);
                VipTierParentObject.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                VipTierParentObject.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
            }
            else if(i < n)
            {
                VipTierParentObject.transform.GetChild(i).GetChild(0).transform.localScale = new Vector3(.75f, .75f, .75f);
                VipTierParentObject.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, 1f);
                VipTierParentObject.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
                VipTierParentObject.transform.GetChild(i).GetChild(2).gameObject.SetActive(true);
                VipTierParentObject.transform.GetChild(i).GetChild(2).GetChild(1).gameObject.SetActive(true);
            }
            else if(i > n)
            {
                VipTierParentObject.transform.GetChild(i).GetChild(0).transform.localScale = new Vector3(.75f, .75f, .75f);
                VipTierParentObject.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new Color(1, 1, 1, .5f);
                VipTierParentObject.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
                VipTierParentObject.transform.GetChild(i).GetChild(2).gameObject.SetActive(true);
                VipTierParentObject.transform.GetChild(i).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, .5f);
                VipTierParentObject.transform.GetChild(i).GetChild(2).GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    public void BackButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.VIPPanel.SetActive(false);
        HomeScreenUIManager.Instance.HomePanel.SetActive(true);
        HomeScreenUIManager.Instance.TopPanel.SetActive(true);
    }

    public void InfoButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.VIPFQAPanel.SetActive(true);

        ParentOFData.transform.GetChild(0).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(0f);
        ParentOFData.transform.GetChild(1).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(.1f);
        ParentOFData.transform.GetChild(2).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(.2f);
        ParentOFData.transform.GetChild(3).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(.3f);
        ParentOFData.transform.GetChild(4).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(.4f);
        ParentOFData.transform.GetChild(5).GetComponent<RectTransform>().DOAnchorPosY(-50f, .5f).From(new Vector2(0, 2200)).SetEase(Ease.InOutBack).SetDelay(.5f);
    }

    public void VIPFQABackButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);
        HomeScreenUIManager.Instance.VIPFQAPanel.SetActive(false);
    }
}
