using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class TimerView : UIBehaviour
    {
        [SerializeField] private Image m_ImageOfValue = null;
        [SerializeField] private float m_GivenTime = 3f;
        [SerializeField] private float m_TimeLeft = 0f;
        [SerializeField] private float m_Speed = 1f;
        [SerializeField] private bool m_IsEvluateTime = false;
        [SerializeField] private Gradient m_EvluateTime;
        [SerializeField] private UnityEvent<float> m_OnUpdateTime = null;
        [SerializeField] private UnityEvent m_OnTimeOut = null;

        private bool mIsPause = false;
        private bool mIsTimeOut = true;

        public bool IsEvluateTime { get => m_IsEvluateTime; set => m_IsEvluateTime = value; }
        public bool IsPause { get => mIsPause; protected internal set => mIsPause = value; }
        public bool IsTimeOut { get => mIsTimeOut; protected internal set => mIsTimeOut = value; }
        public float GivenTime { get => m_GivenTime; }
        public float Speed { get => m_Speed; set => m_Speed = value; }
        public float TimeLeft { get => m_TimeLeft; }
        public Gradient EvluateTime { get => m_EvluateTime; set => m_EvluateTime = value; }
        public UnityEvent<float> OnUpdateTime { get => m_OnUpdateTime; set => m_OnUpdateTime = value; }
        public UnityEvent OnTimeOut { get => m_OnTimeOut; set => m_OnTimeOut = value; }

        public void StartTimer() => StartTimer(m_GivenTime);

        public void StartTimer(float givenTime)
        {
            mIsTimeOut = false;
            PlayTimer();
            m_TimeLeft = m_GivenTime = givenTime;
            UpdateUI();
        }

        public void ResetTimer() => ResetTimer(m_GivenTime);

        public void ResetTimer(float givenTime)
        {
            m_TimeLeft = m_GivenTime = givenTime;
            UpdateUI();
        }

        public void PauseTimer() => mIsPause = true;

        public void PlayTimer() => mIsPause = false;

        private void Update()
        {
            if (!mIsTimeOut && !mIsPause)
            {
                UpdateTime();
                UpdateUI();
            }
        }

        private void UpdateTime()
        {
            m_TimeLeft -= Time.deltaTime * m_Speed;

            if (m_TimeLeft <= 0f)
            {
                m_TimeLeft = 0f;
                IsTimeOut = true;
                m_OnTimeOut.Invoke();
                return;
            }

            m_OnUpdateTime.Invoke(m_TimeLeft);
        }

        protected virtual void UpdateUI()
        {
            if (m_ImageOfValue)
            {
                var value = m_TimeLeft / m_GivenTime;

                m_ImageOfValue.fillAmount = value;

                if (m_IsEvluateTime) m_ImageOfValue.color = m_EvluateTime.Evaluate(value);
            }
        }

    }
}
