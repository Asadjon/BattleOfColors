using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class SecundamerView : UIBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;
        [SerializeField] private DisplayedTimeSpan m_MaxTime = TimeSpan.FromDays(365f);
        [SerializeField] private bool m_Inverse = false;
        [SerializeField] private float m_Speed = 1f;
        [SerializeField] UnityEvent<TimeSpan> m_OnValueChange = null;
        [SerializeField] UnityEvent<TimeSpan> m_OnValueLimited = null;

        private string mTextFormat = "hh\\:mm\\:ss";
        private bool mIsStarted = true;
        private TimeSpan mCurrenTime;
        private TimeSpan mMaxTime;
        private float mSpeed = 1f;

        public bool IsStarted => mIsStarted;
        public bool Inverse { get => m_Inverse; set { m_Inverse = value; Speed = m_Speed; } }
        public TimeSpan CurrentTime { get => mCurrenTime; set { mCurrenTime = value; UpdateUI(); } }
        public TimeSpan MaxTime { get => mMaxTime; set => mMaxTime = value; }
        public float Speed
        {
            get => m_Speed; 
            set 
            {
                m_Speed = Mathf.Clamp(value, 0, value);
                mSpeed = m_Speed * (m_Inverse ? -1 : 1);
            }
        }

        public string TextFormat { get => mTextFormat; set => mTextFormat = value; }
        public UnityEvent<TimeSpan> OnValueChange { get => m_OnValueChange; set => m_OnValueChange = value; }
        public UnityEvent<TimeSpan> OnValueLimited { get => m_OnValueLimited; set => m_OnValueLimited = value; }

        protected override void Awake()
        {
            base.Awake();
            mMaxTime = (TimeSpan) m_MaxTime;
        }

        public void StartTime() => mIsStarted = false;

        public void StopTime() => mIsStarted = true;

        public void ResetTime()
        {
            CurrentTime = m_Inverse ? mMaxTime : TimeSpan.Zero;
            UpdateUI();
            m_OnValueChange.Invoke(mCurrenTime);
        }

        public Color TextColor
        {
            get { if (m_Text) return m_Text.color; else return Color.white; }
            set { if (m_Text) m_Text.color = value; }
        }


        private void Update()
        {
            if (!mIsStarted) UpdataTime();
        }

        private void UpdateUI()
        {
            if (m_Text) m_Text.text = mCurrenTime.ToString(mTextFormat);
        }

        private void UpdataTime()
        {
            mCurrenTime = TimeSpan.FromSeconds(mCurrenTime.TotalSeconds + Time.deltaTime * mSpeed);

            if (m_Inverse ? mCurrenTime <= TimeSpan.Zero : mCurrenTime >= mMaxTime)
            {
                mCurrenTime = mMaxTime;
                StopTime();
                m_OnValueLimited.Invoke(mCurrenTime);
            }

            m_OnValueChange.Invoke(mCurrenTime);

            UpdateUI();
        }

        public override string ToString()
        {
            return mCurrenTime.ToString(@"hh\:mm\:ss");
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            mMaxTime = (TimeSpan)m_MaxTime;
            m_MaxTime = mMaxTime;
            Speed = m_Speed;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }

    [Serializable]
    public struct DisplayedTimeSpan
    {
        public int Days;
        public int Hours;
        public int Minutes;
        public int Seconds;
        public int MilliSeconds;

        public static implicit operator DisplayedTimeSpan(TimeSpan timeSpan) => new DisplayedTimeSpan
        {
            Days = timeSpan.Days,
            Hours = timeSpan.Hours,
            Minutes = timeSpan.Minutes,
            Seconds = timeSpan.Seconds,
            MilliSeconds = timeSpan.Milliseconds
        };

        public static explicit operator TimeSpan(DisplayedTimeSpan timeSpan) => new TimeSpan
            (timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.MilliSeconds);
    }
}
