using Assets.Scripts.Activitys;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.SceneManager;
using static UnityEngine.Object;

namespace Assets.Scripts
{
    class ActivityManager
    {
        public static ActivityManager NewInstanse() => GetActivityManager ?? (GetActivityManager = new ActivityManager());

        public static ActivityManager GetActivityManager { get; private set; } = null;

        public enum ActionScene { Loaded, Unloaded }

        public List<int> LoadingActivityes;

        private ActivityManager()
        {
            LoadingActivityes = new List<int>();

            var mainScene = GetSceneByBuildIndex(0);
            LoadScene(mainScene.buildIndex, LoadSceneMode.Single);
        }


        public void Start() => LoadData();

        private void LoadData()
        {
            sceneLoaded += SceneLoaded;
            sceneUnloaded += SceneUnLoaded;
        }

        public void LoadActivity(int id) => LoadScene(id, LoadSceneMode.Additive);

#pragma warning disable CS0618 // Тип или член устарел
        public void UnLoadActivity(int id) => UnloadScene(id);
#pragma warning restore CS0618 // Тип или член устарел

        Activity[] AllActivities;

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AllActivities = FindObjectsOfType<Activity>();
            if (LoadingActivityes.Count > 0)
            {
                var a1 = AllActivities.FirstOrDefault(activity => activity.SceneId == LoadingActivityes[LoadingActivityes.Count - 1]);

                if (a1 != null) a1.WaitActivity();
            }

            var a = AllActivities.FirstOrDefault(activity => activity.SceneId == scene.buildIndex);

            if (a)
            {
                a.StartActivity();
                LoadingActivityes.Add(scene.buildIndex);
            }
        }

        private void SceneUnLoaded(Scene scene)
        {
            if (LoadingActivityes.Count > 0) LoadingActivityes.Remove(scene.buildIndex);

            var a = AllActivities.FirstOrDefault(activity => activity != null && LoadingActivityes.Count > 0 && activity.SceneId == LoadingActivityes[LoadingActivityes.Count - 1] );

            if (a) a.PlayActivity();

            AllActivities = FindObjectsOfType<Activity>();
        }
    }

    interface ISceneAction { void Action(int sceneIndex); }
}
