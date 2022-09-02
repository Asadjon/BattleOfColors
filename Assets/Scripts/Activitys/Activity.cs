using Assets.Scripts.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Activitys
{
    public class Activity : UIBehaviour
    {
        [SerializeField] private SafeArea m_SafeArea;
        [SerializeField] private Transition m_TransitionAnim;

        public static bool IsBack { get; set; } = false;
        public int SceneId { get; private set; } = 0;

        protected override void Awake()
        {
            base.Awake();
            SceneId = ActivitesID.Instance.GetId(GetType());
        }

        protected virtual void Update()
        {
            IsBack = false;
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                IsBack = true;
                OnBackPressed();
            }
        }

        public virtual void OnBackPressed() => Finish();

        public virtual void PlayActivity()
        {
            if (!gameObject.active) gameObject.SetActive(true);
        }

        public virtual void StartActivity() {}

        public virtual void WaitActivity()
        {
            if (gameObject.active) gameObject.SetActive(false);
        }

        public virtual void Finish()
        {
            ActivityManager.GetActivityManager.UnLoadActivity(SceneId);
        }

        protected void StartTransitionAnim(int sceneId)
        {
            m_TransitionAnim.StartingEnd.AddListener(() => ActivityManager.GetActivityManager.LoadActivity(sceneId));
            m_TransitionAnim.StartTransition();
        }
    }
}
