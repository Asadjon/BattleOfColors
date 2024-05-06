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

        public Type GetActivityType(int sceneID) => m_Activities.Find(a => a.index.Equals(sceneID)) is SceneField scene ? scene.activityType : null;

        public int GetId(Type type, int defId = -1)
        {
            if (!typeof(Activity).IsAssignableFrom(type)) return defId;
            return m_Activities.Find(a => !string.IsNullOrEmpty(a.activityName) && a.activityName.ToUpper().Equals(type.Name.ToUpper())) is SceneField scene ? scene.index : defId;
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
            [SerializeField] internal string activityName;
            [SerializeField] private string activityFullName;
            public Type activityType => Type.GetType(activityFullName);
            [HideInInspector] public Scene scene;
            public int index;
#if UNITY_EDITOR
            public SceneAsset sceneAsset;
            private EditorBuildSettingsScene settingsScene;

            public SceneField(EditorBuildSettingsScene settingsScene, int index)
            {
                this.settingsScene = settingsScene;
                this.index = index;
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(this.settingsScene.path);
                scene = SceneManager.GetSceneByPath(this.settingsScene.path);

                var name = sceneAsset.name.ToUpper();
                var objs = "Scripts/Activitys/".GetAtPath<MonoScript>();
                var activityType = objs
                    .Select(mono => mono.GetClass())
                    .Where(type => typeof(Activity).IsAssignableFrom(type))
                    .FirstOrDefault(activity => activity.Name.ToUpper().Contains(name.ToUpper()));
                activityName = activityType?.Name;
                activityFullName = activityType?.FullName;
            }

            public void OnValidate()
            {
                var name = sceneAsset.name;
                settingsScene = EditorBuildSettings.scenes.ToDictionary(ss => AssetDatabase.LoadAssetAtPath<SceneAsset>(ss.path).name)[name];
                index = SceneManager.GetSceneByPath(settingsScene.path).buildIndex;
            }
#endif
        }
    }
}
