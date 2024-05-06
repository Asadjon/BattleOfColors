using UnityEngine;
using Assets.Scripts.AudioManagers;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(EditorSoundObject))]
    public class EditorSoundObjectEditor : Editor
    {
        private EditorSoundObject mSoundObject;

        private void OnEnable()
        {
            mSoundObject = (EditorSoundObject)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            var name = EditorGUILayout.TextField("Sound name", mSoundObject.SoundName);
            if (name != mSoundObject.SoundName)
            {
                mSoundObject.SoundName = name;
                typeof(EditorSoundObject).GetMethod("Rename", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, new object[0]);
            }
            mSoundObject.Clip = (AudioClip)EditorGUILayout.ObjectField("Clip", mSoundObject.Clip, typeof(AudioClip), true);
            mSoundObject.Volume = EditorGUILayout.Slider("Volume", mSoundObject.Volume, 0f, 1f);
            mSoundObject.Pitch = EditorGUILayout.Slider("Pitch", mSoundObject.Pitch, 0f, 3f);
            mSoundObject.Mute = EditorGUILayout.Toggle("Mute", mSoundObject.Mute);
            mSoundObject.Loop = EditorGUILayout.Toggle("Loop", mSoundObject.Loop);


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}