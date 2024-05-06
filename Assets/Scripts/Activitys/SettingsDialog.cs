using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.ActivityManager;
using static Assets.Scripts.Players.ItemView;

namespace Assets.Scripts.Activitys
{
    class SettingsDialog : UILayout
    {
        [SerializeField] private AnimController m_AnimController = null;
        [SerializeField] private ScrollView m_AudioVolume = null;
        [SerializeField] private Toggle m_IsMute = null;
        [SerializeField] private Toggle m_IsSwipe = null;
        [SerializeField] private Toggle m_IsClick = null;

        private State mItemState = State.Swipe;

        protected override void Awake()
        {
            base.Awake();
            LoadData();
        }

        private void LoadData()
        {
            var settings = GameSettings.Instance;

            mItemState = settings.ItemState;

            m_AudioVolume.SlidingValue = settings.AudioVolume;

            m_IsMute.isOn = !settings.IsMute;

            m_IsSwipe.isOn = mItemState == State.Swipe;
            m_IsClick.isOn = mItemState == State.Click;

            m_IsSwipe.onValueChanged.AddListener(value => mItemState = value ? State.Swipe : mItemState);
            m_IsClick.onValueChanged.AddListener(value => mItemState = value ? State.Click : mItemState);
        }

        public void OnClickButton(bool isSaveButton)
        {
            ShowIs(false);
            if (isSaveButton)
                GameSettings.Instance.SetSettingsData(m_AudioVolume.SlidingValue, !m_IsMute.isOn, mItemState);
        }

        public void ShowIs(bool value)
        {
            if (ActivityManager.Instance)
                if (value) ActivityManager.Instance.AddUILayout(this);
                else ActivityManager.Instance.RemoveUILayout(this);

            m_AnimController.SetParam(0, value);
            if (value) LoadData();
        }

        public override void OnBackPressed() => ShowIs(false);
    }
}