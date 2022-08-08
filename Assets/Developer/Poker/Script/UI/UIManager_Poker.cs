using System;
using DG.Tweening;
using UnityEngine;

namespace Casino_Poker
{
    public class UIManager_Poker : MonoBehaviour
    {
        public static UIManager_Poker Instance;

        public GameObject HomePanel, GamePlayePanel, RoomListPanel, RoomPanel;

        public GameObject TostCanvasPrefab;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance == this) Destroy(gameObject);
        }

        public void OpenPopUpAnimation(GameObject BG, Action OnAnimationComplete)
        {
            BG.transform.DOScale(new Vector3(1f, 1f, 1f), .3f)
                .SetEase(Ease.OutBack)
                .From(new Vector3(0f, 0f, 0f))
                .OnComplete(() => OnAnimationComplete());
        }

        public void ClosePopUpAnimation(GameObject BG, Action OnAnimationComplete)
        {
            BG.transform.DOScale(new Vector3(0f, 0f, 0f), .3f)
                .SetEase(Ease.InBack)
                .From(new Vector3(1f, 1f, 1f))
                .OnComplete(() => OnAnimationComplete());
        }
    }
}