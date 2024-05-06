using UnityEngine;
using static Assets.Scripts.AudioManagers.AudioManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.AudioManagers
{
    public class EditorSoundObject : ScriptableObject
    {
        #region Serialize fields

        [SerializeField] private string m_SoundName;
        [SerializeField] private AudioClip m_Clip;
        [SerializeField, Range(0, 1)] private float m_Volume;
        [SerializeField, Range(0, 3)] private float m_Pitch;
        [SerializeField] private bool m_Mute;
        [SerializeField] private bool m_Loop;

        #endregion
        //private AudioManagerSingleton mSingleton;

        #region Getter and Setters

        public string SoundName { get => m_SoundName; set => m_SoundName = value ?? string.Empty; }
        public AudioClip Clip { get => m_Clip; set => m_Clip = value; }
        public float Volume { get => m_Volume; set => m_Volume = Mathf.Clamp(value, 0, 1); }
        public float Pitch { get => m_Pitch; set => m_Pitch = Mathf.Clamp(value, 0, 3); }
        public bool Mute { get => m_Mute; set => m_Mute = value; }
        public bool Loop { get => m_Loop; set => m_Loop = value; }

        #endregion

        public void Play() =>
            AudioManagerSingleton.Instance.Play(this);

        public void Pause() => 
            AudioManagerSingleton.Instance.Pause(this);

        public void Stop() =>
            AudioManagerSingleton.Instance.Stop(this);

        public void Action(State state) =>
            AudioManagerSingleton.Instance.Action(this, state);

        #region Editor

#if UNITY_EDITOR

        [ContextMenu("Remove Sound")]
        private void Remove()
        {
            typeof(AudioManagerSingleton).GetMethod("Remove", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(AudioManagerSingleton.Instance, new object[] { this });
        }

        private void Rename()
        {
            name = SoundName;
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
        }
#endif

        #endregion
    }
}