using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotSelectForPlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SlotName;
    [SerializeField] private TextMeshProUGUI SlotLevel;

    [SerializeField] private GameObject Lock;

    public int SlotNumber;

    private void OnEnable()
    {
        if (SlotNumber <= 8 || SlotNumber <= Constants.LEVEL)
            Lock.SetActive(false);
        else
            Lock.SetActive(true);

        SlotLevel.text = "Level " + SlotNumber;
    }

    public void SlotForPlay()
    {
        if (Lock.activeInHierarchy)
            return;

        Constants.GotoScene("20 Solt New");
        Constants.SelectedSlot = SlotNumber;
    }
}
