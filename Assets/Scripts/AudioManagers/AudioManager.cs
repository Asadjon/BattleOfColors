using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AudioManagers
{
    public class AudioManager : SingltoneForBehaviour<AudioManager>
    {
        [SerializeField] private List<Sound> m_Sounds = new List<Sound>();

        public enum State { Play, Pause, Stop }

        protected override void LoadData() { }

        public void AddSound(EditorSoundObject soundObject)
        {
            var settings = GameSettings.Instance;
            var sound = new Sound(soundObject)
            {
                Source = gameObject.AddComponent<AudioSource>(),
                m_Volume = settings.AudioVolume,
                m_Mute = settings.IsMute
            };
            sound.Source.playOnAwake = false;
            sound.Update();
            m_Sounds.Add(sound);
        }

        public void UpdateSounds()
        {
            var settings = GameSettings.Instance;
            m_Sounds.ForEach(sound =>
            {
                sound.m_Volume = settings.AudioVolume;
                sound.m_Mute = settings.IsMute;
                sound.Update();
            });
        }

        public void Play(string soundName) => Action(soundName, State.Play);
        public void Play(EditorSoundObject soundObject) => Action(soundObject, State.Play);

        public void Pause(string soundName) => Action(soundName, State.Pause);
        public void Pause(EditorSoundObject soundObject) => Action(soundObject, State.Pause);

        public void Stop(string soundName) => Action(soundName, State.Stop);
        public void Stop(EditorSoundObject soundObject) => Action(soundObject, State.Stop);

        public void Action(string soundName, State state)
        {
            if (GetSound(soundName) is Sound sound) switch (state)
                {
                    case State.Play: sound.Play(); break;
                    case State.Pause: sound.Pause(); break;
                    case State.Stop: sound.Stop(); break;
                }
            else Debug.LogWarning("Sound: " + soundName + " not found!");
        }

        public void Action(EditorSoundObject soundObjec, State state)
        {
            if (GetSound(soundObjec) is Sound sound) switch (state)
                {
                    case State.Play: sound.Play(); break;
                    case State.Pause: sound.Pause(); break;
                    case State.Stop: sound.Stop(); break;
                }
            else Debug.LogWarning("Sound: " + soundObjec.name + " not found!");
        }

        private Sound GetSound(string name) => m_Sounds.FirstOrDefault(s => s.Name.Equals(name));
        private Sound GetSound(EditorSoundObject soundObject) => m_Sounds.FirstOrDefault(s => s.mSoundObject == soundObject);
    }

    [Serializable]
    public class Sound : ISerializationCallbackReceiver
    {
        [SerializeField] private string m_Name;
        //[SerializeField]
        private AudioClip m_Clip;
        //[SerializeField, Range(0, 1)] 
        internal float m_Volume = 1f;
        //[SerializeField, Range(0, 3)]
        internal float m_Pitch = 1f;
        //[SerializeField]
        internal bool m_Mute = false;
        //[SerializeField]
        internal bool m_Loop = false;

        internal EditorSoundObject mSoundObject;
        private AudioSource mSource;
        internal AudioSource Source
        {
            get => mSource; set
            {
                mSource = value;
                Update();
            }
        }
        internal string Name => m_Name;

        public Sound(EditorSoundObject soundObject)
        {
            m_Name = soundObject.SoundName;
            m_Clip = soundObject.Clip;
            m_Volume = soundObject.Volume;
            m_Pitch = soundObject.Pitch;
            m_Mute = soundObject.Mute;
            m_Loop = soundObject.Loop;
            mSoundObject = soundObject;
        }

        internal void Update()
        {
            if (mSource)
            {
                mSource.clip = m_Clip;
                mSource.volume = m_Volume;
                mSource.pitch = m_Pitch;
                mSource.mute = m_Mute;
                mSource.loop = m_Loop;
            }
        }

        internal void Play()
        {
            if (mSource) mSource.Play();
        }

        internal void Pause()
        {
            if (mSource) mSource.Pause();
        }

        internal void Stop()
        {
            if (mSource) mSource.Stop();
        }

        private string oldName = string.Empty;
        public void OnBeforeSerialize() => oldName = m_Name;

        public void OnAfterDeserialize() => m_Name = oldName;
    }
}