using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSZLBGZombieHand : MonoBehaviour {
    public CSAnimationData animationData;
    public GameObject[] rewards;
    private Image _image;
    private int _animationId = 0;
    private CSZLBGReward _rewardText;

    public CSZLBGRewardTypes reward {
        get { return _rewardText.type; }
    }

    private bool _appear;
    public bool appear {
        get { return _appear; }
        set {
            if (_appear == value)
                return;
            _appear = value;
            LeanTween.cancel(_animationId);

            _rewardText.animate = value;
            if (value)
                _animationId = CSUtilities.AnimateWithFrames(_image, animationData).id;
            else
            {
                _animationId = CSUtilities.AnimateWithFrames(_image, animationData, reverse:true).setDestroyOnComplete(true).id;
            }
        }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void OnDestroy()
    {
        LeanTween.cancel(_animationId);
    }

    public CSZLBGZombieHand Instantiate()
    {
        _rewardText = CreateReward();
        appear = true;
        return this;
    }

    private CSZLBGReward CreateReward()
    {
        return Instantiate(rewards[Random.Range(0, rewards.Length)], transform).GetComponent<CSZLBGReward>();
    }

    public CSZLBGRewardTypes RewardType()
    {
        return _rewardText.type;
    }
}
