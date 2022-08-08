using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CSSliderGroup : MonoBehaviour
{
    public Slider[] sliders;
    private float startValue = 0f;

    private void Start()
    {
        foreach (var item in sliders)
        {
            item.onValueChanged.AddListener(delegate { onValueChanged(item); });
        }
    }

	private void OnDestroy()
	{
        foreach (var item in sliders)
        {
            item.onValueChanged.RemoveListener(delegate { onValueChanged(item); });
        }
	}

	private void onValueChanged(Slider slider)
    {
        for (int i = 1; i < sliders.Length; i++)
        {
            float delta = slider.normalizedValue - startValue;

            Slider item = sliders[i];
            //if (item == slider)
            //{
            //    Debug.Log("cont");
            //    continue;
            //}
            //else
            //{
            //    Debug.Log("delta: " + delta);
            //    item.normalizedValue += -1 * delta;
            //}

            item.normalizedValue += -1 * delta;


            startValue = slider.normalizedValue;
        }
    }
}
