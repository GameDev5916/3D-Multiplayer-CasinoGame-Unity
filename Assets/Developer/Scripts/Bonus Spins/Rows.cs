using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rows : MonoBehaviour
{
    public RectTransform transform;

    public int RowNumber;

    private void OnEnable()
    {
        BonusSlotManager.StartRolling += StartSpining;
    }

    private void OnDisable()
    {
        BonusSlotManager.StartRolling -= StartSpining;
    }

    public void StartSpining()
    {
        StartCoroutine(StartRowling());
    }

    IEnumerator StartRowling()
    {
        int n = Random.Range(100,200);
        float speed = 0.025f;
        float y = transform.anchoredPosition.y;
        float limit = 1890;// 2940;
        for (int i = 0; i < n; i++)
        {
            y += 42;
            transform.anchoredPosition = new Vector3(0, y, 0);
            yield return new WaitForSeconds(speed);

            if (y >= limit)
            {
                y = 0;
                transform.anchoredPosition = new Vector3(0, 0, 0);
            }

            //if (i > Mathf.RoundToInt(n * .95f))
            //{
            //    speed = 0.025f;
            //    Debug.LogError("3");
            //}
            //else if (i > Mathf.RoundToInt(n * .85f))
            //{
            //    speed = 0.020f;
            //    Debug.LogError("2");
            //}
            //else if (i > Mathf.RoundToInt(n * .75f))
            //{
            //    speed = 0.010f;
            //    Debug.LogError("1");
            //}
        }
        int a = n % 5;
        a = 5 - a;
        for (int i = 0; i < a; i++)
        {
            y += 42;
            transform.anchoredPosition = new Vector3(0, y, 0);

            if (y == limit)
            {
                y = 0;
                transform.anchoredPosition = new Vector3(0, 0, 0);
            }

            yield return new WaitForSeconds(speed);
        }

        SoundManager.Instance.PlaySound(SoundManager.SoundEnums.OneSpinComplete);
        BonusSlotManager.Instance.FindWin(RowNumber,(int)transform.anchoredPosition.y);
    }
}
