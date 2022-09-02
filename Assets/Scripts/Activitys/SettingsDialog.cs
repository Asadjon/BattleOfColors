using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.Scripts.Players.ItemView;

namespace Assets.Scripts.Activitys
{
    class SettingsDialog : UIBehaviour
    {
        [SerializeField] private AnimController m_AnimController = null;
        [SerializeField] private ScrollView m_AudioVolume = null;
        [SerializeField] private Toggle m_IsMute = null;
        [SerializeField] private Toggle m_IsSwipe = null;
        [SerializeField] private Toggle m_IsClick = null;
        [SerializeField] private Button m_Save = null;
        [SerializeField] private Button m_Cancel = null;
        [SerializeField] private string m_SoundName = string.Empty;

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

            m_IsSwipe.onValueChanged.AddListener(value => { mItemState = value ? State.Swipe : mItemState; PlaySound(m_SoundName); });
            m_IsClick.onValueChanged.AddListener(value => { mItemState = value ? State.Click : mItemState; PlaySound(m_SoundName); });
            m_Save.onClick.AddListener(() => OnClickButton(true));
            m_Cancel.onClick.AddListener(() => OnClickButton(false));
        }

        private void OnClickButton(bool isSaveButton)
        {
            PlaySound(m_SoundName);
            ShowIs(false);
            if (isSaveButton)
                GameSettings.Instance.SetSettingsData(m_AudioVolume.SlidingValue, !m_IsMute.isOn, mItemState);
        }

        private void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        public void ShowIs(bool value) {
            m_AnimController.SetParam(0, value);
            if (value) LoadData();
        }
    }
}