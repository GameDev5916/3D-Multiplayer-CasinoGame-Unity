using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CSLFBGMill : MonoBehaviour {
    public System.Action<List<CSLFBGReward>> RotatetionEnded;

    public List<Transform> arrows;
    public List<Transform> rewards;
    public Transform temp;
    private List<CSLFBGReward> _winned;
    private int _millCount = 0;
    public int millCount {
        get { return _millCount; }
        set {
            value = Mathf.Min(value, arrows.Count - 1);
            if (value == _millCount)
                return;
            _millCount = value;
            for (int i = 0; i < arrows.Count; i++)
            {
                arrows[i].gameObject.SetActive(i <= value);
            }
        }
    }

    private int _stopIdx = 0;

    void Start () {
        _winned = new List<CSLFBGReward>();
	}

    public void Roate()
    {
        AnimateArrows(false);

        CSSoundManager.instance.Play("bicycle_wheel");
        Vector3 rotation = new Vector3();
        LeanTween.value(gameObject, transform.localEulerAngles.z, RandomAngle(), 3f).setOnUpdate((v) =>
        {
            rotation.z = v;
            transform.localEulerAngles = rotation;
        }).setEaseInOutSine().setOnComplete(RotateCompleted);
    }

    private float RandomAngle()
    {
        float angle;
        float offset = 8f;

        _stopIdx = Random.Range(0, rewards.Count);
        Transform random = rewards[_stopIdx];

        angle = Angle(random.position, transform.position);
        angle += offset + 360 * 5;
        return angle;
    }

    private float Angle(Vector3 f, Vector3 t)
    {
        Vector3 d = t - f;
        float angle = (Mathf.Atan2(d.y, d.x) + Mathf.PI) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    private void DefineWinners(int idx)
    {
        _winned.Clear();

        int count = 0;
        foreach (var item in arrows)
        {
            count += item.gameObject.activeSelf ? 1 : 0;
        }

        for (int i = 0; i < count; i++)
        {
            var reward = GetNextRewardForArrowIndex(idx, i);

            if (reward.type == CSLFBGRewardTypes.MillArrow_1)
                count = Mathf.Min(count + 1, arrows.Count);

            _winned.Add(reward);
        }
    }

    private CSLFBGReward GetNextRewardForArrowIndex(int idx, int i)
    {
        int curr = idx + i * 3;
        if (curr > rewards.Count - 1) curr -= rewards.Count;
        return rewards[curr].GetComponent<CSLFBGReward>();
    }

    private CSLFBGReward GetPrevRewardForArrowIndex(int idx, int i)
    {
        int curr = idx - i * 3;
        if (curr < 0) curr += rewards.Count;
        return rewards[curr].GetComponent<CSLFBGReward>();
    }

    private void RotateCompleted()
    {
        CSSoundManager.instance.Stop("bicycle_wheel");
        CSSoundManager.instance.Play("spin_stop");

        DefineWinners(_stopIdx);
        AnimateArrows(true);

        if (RotatetionEnded != null)
        {
            RotatetionEnded(_winned);
        }

        _winned.Clear();
    }

    public void AnimateArrows(bool value)
    {
        //foreach (var item in arrows)
        //{
        //    if (item.gameObject.activeSelf)
        //    {
        //        item.GetComponent<CSLFBGArrow>().animate = value;
        //    }
        //}
    }
}
