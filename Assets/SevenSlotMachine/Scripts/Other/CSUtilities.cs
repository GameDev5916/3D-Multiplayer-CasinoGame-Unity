using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using TMPro;

[Flags]
public enum CSTimeProperty
{
    Hour = 1, Minutes = 2, Seconds = 4,
}

public class CSUtilities {
    private static float min = 999f;
    private static int idx = 0;

    private static string[] tokens = new string[]
        {"", "K", "M", "B", "T", "q", "Q", "s", "S", "o", "n", "d", "U", "D", "Qt", "Qd", "Sd", "St", "O", "N", "v", "c", "inf"};

    public static string FormatNumber(float value)
    {
        if (value <= min)
            return value.ToString("C0");

        idx = 0;

        while (value >= 1000)
        {
            value /= 1000;
            ++idx;
        }
        string format = (value % 1).Equals(0) ? "C0" : "C2";
        return value.ToString(format) + tokens[Mathf.Min(idx, tokens.Length - 1)];
    }

    public static T RandomSymbolValue<T>()
    {
        var values = Enum.GetValues(typeof(T));
        int random = Random.Range(1, values.Length);
        return (T)values.GetValue(random);
    }

    public static LTDescr AnimateWithFrames(Image image, CSAnimationData animationData, float multiplier = 1f, bool reverse = false)
    {
        int prev = -1;
        int count = animationData.frames.Length;

        return LeanTween.value(image.gameObject, 0f, 1f, animationData.duration * multiplier).setOnUpdate(delegate (float a) {
            for (int i = 0; i < count; i++)
            {
                float v = a;
                if (reverse) v = 1f - v;
                int index = (int)((count - 1) * v);
                if (index != prev)
                {
                    image.sprite = animationData.frames[index];
                    prev = index;
                }
            }
        });
    }


    public static float CalculateDuration(Vector3 start, Vector3 end, float speed)
    {
        return Vector3.Distance(start, end) / speed;
    }

    public static LTDescr LabelAction(TextMeshProUGUI label, float to, float time, bool withCoin = false, bool format = true)
    {
        return LabelAction(label, 0f, to, time, withCoin, format);
    }

    public static LTDescr LabelAction(TextMeshProUGUI label, float from, float to, float time, bool withCoin = false, bool format = true)
    {
        string coin = withCoin ? "<sprite=0> " : "";
        return LeanTween.value(label.gameObject, from, to, time).setOnUpdate((float value) => {
            label.text = coin + (format ? FormatNumber(value) : value.ToString("0"));
        });
    }

    public static string TimeFormat(float timer, CSTimeProperty property = CSTimeProperty.Hour | CSTimeProperty.Minutes | CSTimeProperty.Seconds)
    {
        timer = Mathf.Max(timer, 0f);
        float mod = timer % 3600f;
        int hours = Mathf.FloorToInt(timer / 3600f);
        int minutes = Mathf.FloorToInt(mod / 60f);
        int seconds = Mathf.FloorToInt(mod % 60f);
        string time = "";
        string separator = ":";

        if ((property & CSTimeProperty.Hour) == CSTimeProperty.Hour)
            time += (time == string.Empty ? "" : separator) + hours.ToString("00");

        if ((property & CSTimeProperty.Minutes) == CSTimeProperty.Minutes)
            time += (time == string.Empty ? "" : separator) + minutes.ToString("00");

        if ((property & CSTimeProperty.Seconds) == CSTimeProperty.Seconds)
            time += (time == string.Empty ? "" : separator) + seconds.ToString("00");

        return time;
    }
}
