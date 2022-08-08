using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using SimpleJSON;

public class ReciveChipsPanel : MonoBehaviour
{
    public TextMeshProUGUI ReciveChipText;
    public GameObject BG;

    private void OnEnable()
    {
        ReciveChipText.text = $"You won { Constants.NumberShow(5000000) } chips";
        BG.GetComponent<RectTransform>().DOAnchorPosY(0, .5f).From(new Vector2(0, 1300)).SetEase(Ease.InOutBack);
    }

    public void CollectButtonClick()
    {
        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.ButtonClick);

        JSONNode data = new JSONObject
        {
            ["unique_id"] = Constants.PLAYER_ID,
        };

        StartCoroutine(Constants.ApiCall(Constants.API_After_SavenDay_Dift_Collect, data.ToString(), (bool IsSuccess, string result) =>
        {
            if (IsSuccess)
            {
                JSONNode jsonNode = JSON.Parse(result)["data"];
                Constants.SetPlayerData(jsonNode);
                Debug.LogError("Gift Collect");
            }
            else
            {
                CollectButtonClick();
                return;
            }
        }));

        BG.GetComponent<RectTransform>().DOAnchorPosY(-1300, .5f).From(new Vector2(0, 0)).SetEase(Ease.InOutBack)
            .OnComplete(() => {
                Constants.CHIPS += 5000000;
                Constants.instance.Chips_Gold_Update();
                gameObject.SetActive(false);
            });
    }
}
