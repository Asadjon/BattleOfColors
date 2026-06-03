using Assets.Scripts.Activitys;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static Assets.Scripts.Activitys.Activity;

namespace Assets.Scripts
{
    public class ActivityManager : SingltoneForBehaviour<ActivityManager>
    {
        [SerializeField] private Transition _transition;
        [SerializeField] private ActivitesID _activitesID;

        private readonly List<UILayout> _allLoadedUILayouts = new();
        private KeyValuePair<Scene, Activity> _activeSceneActivity;
        private readonly Dictionary<Scene, Activity> _waitingSceneActivities = new();
        private readonly Dictionary<Type, Bundle> _bundles = new();
        private ITransitionAnimActions _transitionAnimLoadActions;
        private ITransitionAnimActions _transitionAnimUnloadActions;

        protected override void LoadData()
        {
            _ = _activitesID.GetId<MenuActivity>();
            _transition = CreateTransition();
            _transitionAnimLoadActions = new TransitionAnimLoadActions(_transition);
            _transitionAnimUnloadActions = new TransitionAnimUnloadActions(_transition);
        }

        private Transition CreateTransition()
        {
            if (!_transition) return null;

            var transitionAnim = Instantiate(_transition);
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
                var lastLoadedUILayout = _allLoadedUILayouts.LastOrDefault();
                if (lastLoadedUILayout)
                    lastLoadedUILayout.OnBackPressed();
                else
                    ExitApp();
            }
        }

        public void LoadActivity<T>() where T : Activity =>
            LoadActivity(typeof(T));

        public void LoadActivity<T>(Bundle bundle) where T : Activity =>
            LoadActivity(typeof(T), bundle);

        public void LoadActivity(Type activityType) =>
            LoadActivity(activityType, null);

        public void LoadActivity(Type activityType, Bundle bundle)
        {
            _transitionAnimLoadActions.SetValue(_activitesID.GetId(activityType));
            _bundles[activityType] = bundle ?? new Bundle();
            _transitionAnimLoadActions.StartTransition();
        }

        public void UnloadActivity(Scene unloadingScene)
        {
            if (_waitingSceneActivities.ContainsKey(unloadingScene))
                _ = SceneManager.UnloadSceneAsync(unloadingScene);
            else
            {
                _transitionAnimUnloadActions.SetValue(unloadingScene);
                _transitionAnimUnloadActions.StartTransition();
            }
        }

        public void AddActivity(Activity activity)
        {
            if (!AddUILayout(activity)) return;

            if (_activeSceneActivity.Value)
            {
                _waitingSceneActivities[_activeSceneActivity.Key] = _activeSceneActivity.Value;
                _activeSceneActivity.Value.OnPause();
            }

            var scene = activity.gameObject.scene;
            _activeSceneActivity = new KeyValuePair<Scene, Activity>(scene, activity);

            var bundle = new Bundle();
            var typeForBundleKey = _activeSceneActivity.Value.GetType();
            if (_bundles.TryGetValue(typeForBundleKey, out var savedBundle))
            {
                bundle = savedBundle;
                _bundles.Remove(typeForBundleKey);
            }
            _activeSceneActivity.Value.OnCreate(bundle);
            _activeSceneActivity.Value.OnPlay();
        }

        public void RemoveActivity(Activity removedActivity)
        {
            if (!removedActivity) return;

            var scene = removedActivity.gameObject.scene;

            if (_waitingSceneActivities.ContainsKey(scene))
                _ = _waitingSceneActivities.Remove(scene);

            if (_activeSceneActivity.Key == scene)
            {
                _activeSceneActivity.Value.OnPause();
                _activeSceneActivity = default;

                var oldLoadedSceneActivity = _waitingSceneActivities.LastOrDefault();

                if (oldLoadedSceneActivity.Value)
                {
                    _activeSceneActivity = new KeyValuePair<Scene, Activity>(oldLoadedSceneActivity.Key, oldLoadedSceneActivity.Value);
                    _waitingSceneActivities.Remove(oldLoadedSceneActivity.Key);
                    _activeSceneActivity.Value.OnPlay();
                    _ = SceneManager.SetActiveScene(_activeSceneActivity.Key);
                }
                else ExitApp();
            }

            RemoveUILayout(removedActivity);
        }

        public bool AddUILayout(UILayout uiLayout)
        {
            if (!uiLayout || _allLoadedUILayouts.Contains(uiLayout)) return false;

            _allLoadedUILayouts.Add(uiLayout);
            return true;
        }

        public void RemoveUILayout(UILayout uiLayout)
        {
            if (!uiLayout || !_allLoadedUILayouts.Contains(uiLayout)) return;

            _allLoadedUILayouts.Remove(uiLayout);
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

        private interface ITransitionAnimActions
        {
            void SetValue(object value);
            void StartTransition();
            void EndTransition();
        }

        private abstract class TransitionAnimAction<T> : ITransitionAnimActions
        {
            protected readonly Transition _transition;
            protected bool _isTransitionStarting;

            public TransitionAnimAction(Transition transition)
            {
                if (!transition) return;

                _transition = transition;
                _transition.StartingEnd += OnActivityTransitionEnd;
            }

            private void OnActivityTransitionEnd()
            {
                if (_isTransitionStarting)
                    ActivityTransitionEnd();
            }

            protected abstract void ActivityTransitionEnd();

            public void StartTransition()
            {
                if (_isTransitionStarting) return;

                if (_transition)
                {
                    _transition.StartTransition();
                    _isTransitionStarting = true;
                }
                else ActivityTransitionEnd();
            }

            public void EndTransition()
            {
                if (_isTransitionStarting && _transition)
                {
                    _transition.EndTransition();
                    _isTransitionStarting = false;
                }
            }

            public abstract void SetValue(T value);

            public void SetValue(object value)
            {
                if (value is T t)
                    SetValue(t);
            }
        }

        private class TransitionAnimLoadActions : TransitionAnimAction<int>
        {
            private readonly HashSet<int> _activitySceneBuildIndexes = new();

            public TransitionAnimLoadActions(Transition transition) : base(transition) { }

            public override void SetValue(int loadedActivityBuildIndex)
            {
                if (loadedActivityBuildIndex < 0 || loadedActivityBuildIndex >= SceneManager.sceneCountInBuildSettings)
                    throw new IndexOutOfRangeException(nameof(loadedActivityBuildIndex));
                _activitySceneBuildIndexes.Add(loadedActivityBuildIndex);
            }

            protected override void ActivityTransitionEnd()
            {
                if (_activitySceneBuildIndexes.Count == 0) return;

                foreach (var sceneBuildIndex in _activitySceneBuildIndexes)
                    SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive).completed += (asyncOperation) =>
                    {
                        _ = SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex));
                        _activitySceneBuildIndexes.Remove(sceneBuildIndex);
                        if (_activitySceneBuildIndexes.Count == 0) EndTransition();
                    };
            }
        }

        private class TransitionAnimUnloadActions : TransitionAnimAction<Scene>
        {
            private Scene _unloadingScene;

            public TransitionAnimUnloadActions(Transition transition) : base(transition) { }

            protected override void ActivityTransitionEnd()
            {
                if (_unloadingScene == default) return;

                SceneManager.UnloadSceneAsync(_unloadingScene).completed += (asyncOperation) =>
                {
                    EndTransition();
                    _unloadingScene = default;
                };
            }

            public override void SetValue(Scene unloadingScene)
            {
                _unloadingScene = unloadingScene;
            }
        }
    }
}