using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CSReels))]
public class CSReelAutoSpin : MonoBehaviour {
    public Sprite normalSprite;
    public SpriteState state;
    public float holdDuration = 1.1f;

    private float _startTime;
    private bool _start;
    private CSReels _reels;

    private void Awake()
    {
        _reels = GetComponent<CSReels>();
    }

    private void Update()
    {
        if (!_start || _reels.autoSpin)
            return;

        float delta = Time.time - _startTime;
        if (delta < holdDuration)
            return;

        _reels.SetAutoSpin(true);
        _start = false;
    }

    public void SwapSpinSprite(CSBottomPanel basePanel)
    {
        Button button = basePanel.spinButton;
        Image image = button.GetComponent<Image>();

        Sprite n_sprite = image.sprite;
        image.sprite = normalSprite;
        normalSprite = n_sprite;

        SpriteState n_state = button.spriteState;
        button.spriteState = state;
        state = n_state;
    }

    public void OnSpinDown()
    {
        //if (_reels.autoSpin)
        //{
        //    _start = false;
        //    _reels.SetAutoSpin(false);
        //}
        //else
        //{
        //    _start = true;
        //    _startTime = Time.time;
        //}
    }

    public void OnSpinUp()
    {
        //_start = false;

        ////if (!_start || _reels.autoSpin)
        ////    return;

        ////float delta = Time.time - _startTime;
        ////_reels.SetAutoSpin(delta >= holdDuration);
    }
}
