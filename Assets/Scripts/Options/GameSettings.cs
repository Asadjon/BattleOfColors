using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    [Serialization(typeof(SerializeSettings))]
    class GameSettings : Singltone<GameSettings>
    {

        #region Constantas
        private const string mGameSettingsFileName = "Settings.json";
        public const float DefaultSwipeLimitSize = 0.03f;
        public const float DefaultAudioVolume = .5f;
        #endregion

        #region SerializeField Objects
        [SerializeField] private string m_SettingsJsonDataPath = "";
        [SerializeField, Range(0f, 1f)] private float m_AudioVolume = DefaultAudioVolume;
        [SerializeField, Range(1, GameOptions.MaxNumberOfArrays - 1)] private int m_DistanceOfSwiping = GameOptions.MaxNumberOfArrays - 1;
        [SerializeField] private bool m_IsMute = false;
        [SerializeField, Range(0f, 1f)] private float m_SwipeLimitSize = DefaultSwipeLimitSize;
        [SerializeField] private float m_SwipingSpeed = .2f;
        [SerializeField] private float m_ShuffleAnimDuration = .2f;
        [SerializeField] private Players.ItemView.State m_ItemState = Players.ItemView.State.Swipe;
        #endregion

        #region Getters And Setters
        public float SwipeLimitSize { get => m_SwipeLimitSize; set => m_SwipeLimitSize = Mathf.Clamp(value, 0f, 1f); }
        public int DistanceOfSwiping { get => m_DistanceOfSwiping; set => m_SwipeLimitSize = Mathf.Clamp(value, 1, GameOptions.MaxNumberOfArrays - 1); }
        public float SwipingSpeed { get => m_SwipingSpeed; set => m_SwipingSpeed = value; }
        public float ShuffleAnimDuration { get => m_ShuffleAnimDuration; set => m_ShuffleAnimDuration = value; }
        public float AudioVolume { get => m_AudioVolume; set => m_AudioVolume = Mathf.Clamp(value, 0f, 1f); }
        public bool IsMute { get => m_IsMute; set => m_IsMute = value; }
        public Players.ItemView.State ItemState { get => m_ItemState; set => m_ItemState = value; }
        #endregion

        protected override void LoadData()
        {
            JsonFileReader.CreateFolder(m_SettingsJsonDataPath);

            if (JsonFileReader.Read(m_SettingsJsonDataPath, mGameSettingsFileName, out SerializeSettings settings))
                this.SetSavedValue(settings);

            else JsonFileReader.Write(this.GetSavedValue(), m_SettingsJsonDataPath, mGameSettingsFileName);
        }

        public void SetSettingsData(float audioVolume, bool isMute, Players.ItemView.State itemState)
        {
            SetDatas(audioVolume, isMute, itemState);
            JsonFileReader.Write(this.GetSavedValue(), m_SettingsJsonDataPath, mGameSettingsFileName);
            AudioManager.Instance.UpdateSounds();
        }

        private void SetDatas(float audioVolume, bool isMute, Players.ItemView.State itemState)
        {
            AudioVolume = audioVolume;
            IsMute = isMute;
            ItemState = itemState;
        }
    }

    [Serializable] struct SerializeSettings
    {
        [SerializedMember("AudioVolume")] public float audioVolume;
        [SerializedMember("IsMute")] public bool isMute;
        [SerializedMember("ItemState")] public Players.ItemView.State itemState;
    }
}
