using Assets.Scripts.Singletones;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Activitys
{
    [CreateAssetMenu]
    public class ActivitesID : SingletoneForScriptableObject<ActivitesID>
    {
        [SerializeField] private List<SceneField> m_Activities;

        public Type GetActivityType(int sceneID) => m_Activities.Find(a => a.Index.Equals(sceneID)) is SceneField scene ? scene.ActivityType : null;

        public int GetId(Type type, int defId = -1)
        {
            if (!typeof(Activity).IsAssignableFrom(type)) return defId;
            return m_Activities.Find(a => !string.IsNullOrEmpty(a._activityName) && a._activityName.ToUpper().Equals(type.Name.ToUpper())) is SceneField scene ? scene.Index : defId;
        }

        public int GetId<T>(int defId = -1) => GetId(typeof(T), defId);

#if UNITY_EDITOR
        [ContextMenu("Load")]
        private void LoadData()
        {
            if (Application.isPlaying) return;
            var scenes = EditorBuildSettings.scenes;
            m_Activities = Array.ConvertAll(scenes, s => new SceneField(s, Array.IndexOf(scenes, s))).ToList();
        }
#endif

        //private void OnValidate()
        //{
        //    m_Activities.ForEach(sceneField => sceneField.OnValidate());
        //}


        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [MenuItem("Tools/Singletons/Activites ID")]
#endif
        private static void Create() => Create("Assets/Resources/Activites ID.asset");

        [Serializable]
        public struct SceneField
        {
            [SerializeField] internal string _activityName;
            [SerializeField] private string _activityFullName;
            public readonly Type ActivityType => Type.GetType(_activityFullName);
            [HideInInspector] public Scene Scene;
            public int Index;
#if UNITY_EDITOR
            public SceneAsset SceneAsset;
            private EditorBuildSettingsScene _settingsScene;

            public SceneField(EditorBuildSettingsScene settingsScene, int index)
            {
                _settingsScene = settingsScene;
                Index = index;
                SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_settingsScene.path);
                Scene = SceneManager.GetSceneByPath(_settingsScene.path);

                var name = SceneAsset.name.ToUpper();
                var objs = "Scripts/Activitys/".GetAtPath<MonoScript>();
                var activityType = objs
                    .Select(mono => mono.GetClass())
                    .Where(type => typeof(Activity).IsAssignableFrom(type))
                    .FirstOrDefault(activity => activity.Name.ToUpper().Contains(name.ToUpper()));
                _activityName = activityType?.Name;
                _activityFullName = activityType?.FullName;
            }

            public void OnValidate()
            {
                var name = SceneAsset.name;
                _settingsScene = EditorBuildSettings.scenes.ToDictionary(ss => AssetDatabase.LoadAssetAtPath<SceneAsset>(ss.path).name)[name];
                Index = SceneManager.GetSceneByPath(_settingsScene.path).buildIndex;
            }
#endif
        }
    }
}
