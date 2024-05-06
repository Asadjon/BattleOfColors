using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class Transition : UIBehaviour
    {
        [SerializeField] private Animator m_Animator;
        [SerializeField] private float m_Duration = 1f;

        private bool mIsRunning
        {
            get
            {
                var state = m_Animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName("LoadingStart");
            }
        }

        public UnityAction StartingEnd = null;

        public void StartTransition()
        {
            if (!mIsRunning)
                StartAnim("start");
        }

        public void EndTransition()
        {
            if (mIsRunning)
                StartAnim("end");
        }

        private void StartAnim(string triggerName)
        {
            if (m_Animator)
                m_Animator.SetTrigger(triggerName);
        }

        public void StartAnimEnding() => StartingEnd.Invoke();

        protected override void Awake()
        {
            base.Awake();
            if (!m_Animator)
                m_Animator = GetComponentInChildren<Animator>();
        }
    }
}