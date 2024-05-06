using Assets.Scripts.SaveGameDatas.Attributes;
using Assets.Scripts.Singletones;
using System;
using UnityEngine;
using static Assets.Scripts.Players.ItemView;
using Assets.Scripts.AudioManagers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts
{
    [Serialization(typeof(SerializeSettings))]
    internal sealed class GameSettings : SingletoneForScriptableObject<GameSettings>
    {

        #region Constantas
        private const string mGameSettingsFileName = "Settings.json";
        public const float DefaultSwipeLimitSize = 0.03f;
        public const float DefaultAudioVolume = .5f;
        #endregion

        #region SerializeField Objects
        [SerializeField] private string m_SettingsJsonDataPath = "/";
        [SerializeField, Range(0f, 1f)] private float m_AudioVolume = DefaultAudioVolume;
        [SerializeField, Range(1, GameOptions.MaxSizeOfSquare - GameOptions.MinSizeOfSquare)] private int m_DistanceOfSwiping = GameOptions.MaxSizeOfSquare - GameOptions.MinSizeOfSquare;
        [SerializeField] private bool m_IsMute = false;
        [SerializeField, Range(0f, 1f)] private float m_SwipeLimitSize = DefaultSwipeLimitSize;
        [SerializeField] private float m_SwipingSpeed = .2f;
        [SerializeField] private float m_ShuffleAnimDuration = .2f;
        [SerializeField] private State m_ItemState = State.Swipe;
        #endregion

        #region Getters And Setters
        public float SwipeLimitSize { get => m_SwipeLimitSize; set => m_SwipeLimitSize = Mathf.Clamp(value, 0f, 1f); }
        public int DistanceOfSwiping { get => m_DistanceOfSwiping; set => m_SwipeLimitSize = Mathf.Clamp(value, 1, GameOptions.MaxSizeOfSquare - GameOptions.MinSizeOfSquare); }
        public float SwipingSpeed { get => m_SwipingSpeed; set => m_SwipingSpeed = value; }
        public float ShuffleAnimDuration { get => m_ShuffleAnimDuration; set => m_ShuffleAnimDuration = value; }
        public float AudioVolume { get => m_AudioVolume; set => m_AudioVolume = Mathf.Clamp(value, 0f, 1f); }
        public bool IsMute { get => m_IsMute; set => m_IsMute = value; }
        public State ItemState { get => m_ItemState; set => m_ItemState = value; }
        #endregion

        private void LoadData()
        {
            JsonFileReader.CreateFolder(m_SettingsJsonDataPath);

            if (JsonFileReader.Read(m_SettingsJsonDataPath, mGameSettingsFileName, out SerializeSettings settings))
                this.SetSavedValue(settings);

            else JsonFileReader.Write(this.GetSavedValue(), m_SettingsJsonDataPath, mGameSettingsFileName);
        }

        public void SetSettingsData(float audioVolume, bool isMute, State itemState)
        {
            SetDatas(audioVolume, isMute, itemState);
            JsonFileReader.Write(this.GetSavedValue(), m_SettingsJsonDataPath, mGameSettingsFileName);
            AudioManager.Instance.UpdateSounds();
        }

        private void SetDatas(float audioVolume, bool isMute, State itemState)
        {
            AudioVolume = audioVolume;
            IsMute = isMute;
            ItemState = itemState;
        }

        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [MenuItem("Tools/Singletons/Game Settings")]
#endif
        private static void Create() => Create("Assets/Resources/Game Settings.asset");

        [RuntimeInitializeOnLoadMethod]
        private static void Load() => Instance.LoadData();
    }

    [Serializable] struct SerializeSettings
    {
        [SerializedMember("AudioVolume")] public float audioVolume;
        [SerializedMember("IsMute")] public bool isMute;
        [SerializedMember("ItemState")] public State itemState;
    }
}
