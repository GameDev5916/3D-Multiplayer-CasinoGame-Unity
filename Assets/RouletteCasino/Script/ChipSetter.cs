using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ChipSetter : MonoBehaviour
{
    public List<GameObject> chipsPrefab, chips;
    private int selectedChip = 0;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null)
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.forward);
            Debug.DrawRay(Input.mousePosition, Vector3.forward, Color.green);
            if (hit)
            {
                GameObject go = Instantiate(chipsPrefab[selectedChip], hit.transform, false);
                go.transform.position = hit.transform.position;
                go.transform.eulerAngles = Vector3.zero;
                GameManager_Roulette.instance.chips.Push(go);
                GameManager_Roulette.instance.CountBetAmount(int.Parse(go.GetComponentInChildren<Text>().text));
            }
        }
    }

    public void ChipSelect(int index)
    {
        selectedChip = index;

        for (int i = 0; i < chips.Count; i++)
        {
            if (i == index)
                chips[i].GetComponent<Image>().color = new Color32(120, 120, 120, 255);
            else
                chips[i].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        }
    }
}

