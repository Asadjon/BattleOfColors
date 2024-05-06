using Assets.Scripts.Singletones;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.AudioManagers.AudioManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.AudioManagers
{
    public class AudioManagerSingleton : SingletoneForScriptableObject<AudioManagerSingleton>
    {
        [SerializeField] private List<EditorSoundObject> m_SoundObjects;
        private AudioManager mAudioManager;

        //[RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [MenuItem("Tools/Singletons/Audio Manager")]
#endif
        private static void Create() => Create("Assets/Resources/Audio Manager.asset");

        [RuntimeInitializeOnLoadMethod]
        private static void Load() => Instance.LoadData();

        private void LoadData() =>
            InitManager();

        private void InitManager()
        {
            mAudioManager = CreateManager();

            if (mAudioManager) 
                m_SoundObjects.ForEach(editorSound => mAudioManager.AddSound(editorSound));
        }

        private AudioManager CreateManager()
        {
            var audioManagerGameObject = new GameObject(nameof(AudioManager));
            var audioManager = audioManagerGameObject.AddComponent<AudioManager>();
            audioManagerGameObject.AddComponent<AudioListener>();

            DontDestroyOnLoad(audioManagerGameObject);

            return audioManager;
        }

        public void Play(EditorSoundObject soundObject)
        {
            if (mAudioManager)
                mAudioManager.Action(soundObject, State.Play);
        }

        public void Pause(EditorSoundObject soundObject)
        {
            if (mAudioManager)
                mAudioManager.Action(soundObject, State.Pause);
        }

        public void Stop(EditorSoundObject soundObject)
        {
            if (mAudioManager)
                mAudioManager.Action(soundObject, State.Stop);
        }

        public void Action(EditorSoundObject soundObject, State state)
        {
            if (mAudioManager)
                mAudioManager.Action(soundObject, state);
        }


        #region Editor
#if UNITY_EDITOR

        [ContextMenu("Load")]
        private void FindAllSounds()
        {
            var soundObjects = GetChilds<EditorSoundObject>();

            if (soundObjects.Length <= 0) return;

            m_SoundObjects.Clear();
            m_SoundObjects = soundObjects.ToList();
        }

        [ContextMenu("Add Sound")]
        [System.Obsolete]
        private void AddSound()
        {
            var soundObject = CreateInstance<EditorSoundObject>();
            soundObject.name = "Sound";
            //typeof(EditorSoundObject).GetField("mSingleton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(soundObject, this);
            
            AssetDatabase.AddObjectToAsset(soundObject, this);
            AssetDatabase.SaveAssets();
            SetDirty();
            EditorUtility.SetDirty(soundObject);

            m_SoundObjects.Insert(0, soundObject);
        }

        [ContextMenu("Remove Sound")]
        private void RemoveEditorSoundObject()
        {
            Remove(m_SoundObjects.LastOrDefault());
        }

        [ContextMenu("Clear")]
        private void Clear()
        {
            if (m_SoundObjects == null) m_SoundObjects = new List<EditorSoundObject>();

            else Enumerable.Range(0, m_SoundObjects.Count).ToList()
                    .ForEach(i => Remove(m_SoundObjects.Last()));
        }

        private void Remove(EditorSoundObject soundObject)
        {
            if (!m_SoundObjects.Contains(soundObject)) return;

            m_SoundObjects.Remove(soundObject);
            AssetDatabase.RemoveObjectFromAsset(soundObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
        #endregion
    }
}
