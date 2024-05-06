using Assets.Scripts.Activitys;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using Assets.Scripts.UI;
using static UnityEngine.SceneManagement.SceneManager;
using static Assets.Scripts.Activitys.Activity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    internal static class Initializer
    {
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //private static void Initialize() =>
        //    LoadScene(0, LoadSceneMode.Single);
    }

    public class ActivityManager : SingltoneForBehaviour<ActivityManager>
    {
        [SerializeField] private Transition m_Transition;
        [SerializeField] private ActivitesID m_ActivitesID;

        private readonly List<UILayout> mAllLoadedUILayouts = new List<UILayout>();
        private KeyValuePair<Scene, Activity> mActiveSceneActivity;
        private readonly Dictionary<Scene, Activity> mWaitingSceneActivitys = new Dictionary<Scene, Activity>();
        private readonly Dictionary<Type, Bundle> mBundleList = new Dictionary<Type, Bundle>();
        private TransitionAnimLoadActions mTransitionAnimLoadActions;
        private TransitionAnimUnloadActions mTransitionAnimUnloadActions;

        protected ActivityManager() : base()
        {
            mTransitionAnimLoadActions = new TransitionAnimLoadActions(this);
            mTransitionAnimUnloadActions = new TransitionAnimUnloadActions(this);
        }

        protected override void LoadData()
        {
            _ = m_ActivitesID.GetId<MenuActivity>();
            m_Transition = CreateTransition();
            m_Transition.StartingEnd += mTransitionAnimLoadActions.OnLoadActivityTransitionEnd;
            m_Transition.StartingEnd += mTransitionAnimUnloadActions.OnUnloadActivityTransitionEnd;

            //void action(Scene scene)
            //{
            //    if (scene.buildIndex != 0) m_Transition.EndTransition();
            //}

            //sceneLoaded += (scane, __) => action(scane);
            //sceneUnloaded += (scane) => action(scane);
        }

        private Transition CreateTransition()
        {
            if (!m_Transition) return null;

            var transitionAnim = Instantiate(m_Transition);
            DontDestroyOnLoad(transitionAnim);
            return transitionAnim;
        }

        private void Update()
        {
            if (
#if UNITY_EDITOR
                Input.GetButtonUp("Cancel")
#else
                Input.GetKeyUp(KeyCode.Escape)
#endif
                )
            {
                var lastLoadedUILayout = mAllLoadedUILayouts.LastOrDefault();
                if (lastLoadedUILayout)
                    lastLoadedUILayout.OnBackPressed();
                else
                    ExitApp();
            }
        }

        public List<T> GetActivitys<T>() where T : Activity
        {
            var allActivity = new List<Activity>(mWaitingSceneActivitys.Values.ToList()) { mActiveSceneActivity.Value };
            return allActivity.FindAll(activity => activity as T).ConvertAll(activity => (T)activity);
        }

        public void LoadActivity<T>() where T : Activity =>
            LoadActivity(typeof(T));

        public void LoadActivity<T>(Bundle bundle) where T : Activity =>
            LoadActivity(typeof(T), bundle);

        public void LoadActivity(Type activityType) =>
            LoadActivity(activityType, new Bundle());

        private class TransitionAnimLoadActions
        {
            private readonly ActivityManager mManager;
            private readonly List<Type> mActivityTypes = new List<Type>();

            internal TransitionAnimLoadActions(ActivityManager manager)
            {
                mManager = manager;
            }

            public static TransitionAnimLoadActions operator +(TransitionAnimLoadActions actions, Type loadedActivityType)
            {
                if (actions == null) throw new ArgumentNullException(nameof(actions));
                actions.mActivityTypes.Add(loadedActivityType);
                return actions;
            }

            internal void OnLoadActivityTransitionEnd()
            {
                if (mActivityTypes.Count == 0) return;

                mActivityTypes.ForEach(activityType =>
                {
                    int sceneID = mManager.m_ActivitesID.GetId(activityType);
                    LoadSceneAsync(sceneID, LoadSceneMode.Additive).completed += (asyncOperation) =>
                    {
                        mActivityTypes.Remove(activityType);
                        mManager.m_Transition.EndTransition();
                        _ = SetActiveScene(GetSceneByBuildIndex(sceneID));
                    };
                });
            }
        }

        public void LoadActivity(Type activityType, Bundle bundle)
        {
            mTransitionAnimLoadActions += activityType;

            mBundleList.Add(activityType, bundle);

            m_Transition.StartTransition();
        }

        private class TransitionAnimUnloadActions
        {
            private readonly ActivityManager mManager;
            internal Scene unloadingScene;

            internal TransitionAnimUnloadActions(ActivityManager manager)
            {
                mManager = manager;
            }

            internal void OnUnloadActivityTransitionEnd()
            {
                if (unloadingScene == default) return;

                UnloadSceneAsync(unloadingScene).completed += (asyncOperation) =>
                {
                    mManager.m_Transition.EndTransition();
                    unloadingScene = default;
                };
            }
        }

        public void UnloadActivity(Scene unloadingScene)
        {
            if (mWaitingSceneActivitys.ContainsKey(unloadingScene))
                _ = UnloadSceneAsync(unloadingScene);
            else
            {
                mTransitionAnimUnloadActions.unloadingScene = unloadingScene;
                m_Transition.StartTransition();
            }
        }

        public void AddActivity(Activity activity)
        {
            if (!AddUILayout(activity)) return;

            if (mActiveSceneActivity.Value)
            {
                mWaitingSceneActivitys.Add(mActiveSceneActivity.Key, mActiveSceneActivity.Value);
                mActiveSceneActivity.Value.OnPause();
            }

            var scene = activity.gameObject.scene;
            mActiveSceneActivity = new KeyValuePair<Scene, Activity>(scene, activity);

            var bundle = new Bundle();
            var typeForBundleKey = mActiveSceneActivity.Value.GetType();
            if (mBundleList.ContainsKey(typeForBundleKey))
            {
                bundle = mBundleList[typeForBundleKey];
                mBundleList.Remove(typeForBundleKey);
            }
            mActiveSceneActivity.Value.OnCreate(bundle);
            mActiveSceneActivity.Value.OnPlay();
        }

        public void RemoveActivity(Activity removedActivity)
        {
            var scene = removedActivity.gameObject.scene;

            if (mWaitingSceneActivitys.ContainsKey(scene))
                _ = mWaitingSceneActivitys.Remove(scene);

            if (mActiveSceneActivity.Key == scene)
            {
                mActiveSceneActivity.Value.OnPause();
                mActiveSceneActivity = default;

                var oldLoadedSceneActivity = mWaitingSceneActivitys.LastOrDefault();

                if (oldLoadedSceneActivity.Value)
                {
                    mActiveSceneActivity = new KeyValuePair<Scene, Activity>(oldLoadedSceneActivity.Key, oldLoadedSceneActivity.Value);
                    mWaitingSceneActivitys.Remove(oldLoadedSceneActivity.Key);
                    mActiveSceneActivity.Value.OnPlay();
                    _ = SetActiveScene(mActiveSceneActivity.Key);
                }
                else ExitApp();
            }

            RemoveUILayout(removedActivity);
        }

        public bool AddUILayout(UILayout uiLayout)
        {
            if (!uiLayout || mAllLoadedUILayouts.Contains(uiLayout)) return false;

            mAllLoadedUILayouts.Add(uiLayout);
            return true;
        }

        public void RemoveUILayout(UILayout uiLayout)
        {
            if (!mAllLoadedUILayouts.Contains(uiLayout) && !uiLayout) return;

            mAllLoadedUILayouts.Remove(uiLayout);
        }

        private void ExitApp()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }


        [DisallowMultipleComponent]
        public abstract class UILayout : UIBehaviour
        {
            public abstract void OnBackPressed();
        }
    }
}
