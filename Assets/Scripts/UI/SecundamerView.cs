using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    class SecundamerView : UIBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Text = null;
        [SerializeField] private double m_SecondValue = 0f;
        [SerializeField] private float m_Speed = 1f;
        [SerializeField] UnityEvent<TimeSpan> m_OnValueChange = null;

        private string mTextFormat = "hh\\:mm\\:ss";
        private bool mIsStarted = true;
        private TimeSpan mCurrenTime;

        public bool IsStarted => mIsStarted;
        public TimeSpan CurrentTime => mCurrenTime;
        public double Value { get => m_SecondValue; set { m_SecondValue = value; UpdateUI(); } }
        public float Speed { get => m_Speed; set => m_Speed = value; }

        public string TextFormat { get => mTextFormat; set => mTextFormat = value; }
        public UnityEvent<TimeSpan> OnValueChange { get => m_OnValueChange; set => m_OnValueChange = value; }


        public void StartTime() => mIsStarted = false;

        public void StopTime() => mIsStarted = true;

        public void ResetTime()
        {
            m_SecondValue = 0f;
            UpdataTime();
        }

        public void ChangeTextColor(Color newColor) { if(m_Text) m_Text.color = newColor; }


        private void Update()
        {
            if (!mIsStarted) UpdataTime();
        }

        private void UpdateUI()
        {
            if (TimeSpan.FromSeconds(m_SecondValue) is TimeSpan newTime && newTime.Seconds != mCurrenTime.Seconds)
            {
                if (m_Text) m_Text.text = newTime.ToString(mTextFormat)/*string.Format(m_TextFormat, newTime.Hours, newTime.Minutes, newTime.Seconds)*/;
                m_OnValueChange.Invoke(mCurrenTime = newTime);
            }
        }

        private void UpdataTime()
        {
            m_SecondValue += Time.deltaTime * m_Speed;
            UpdateUI();
        }

        public override string ToString()
        {
            return mCurrenTime.ToString(@"hh\:mm\:ss");
        }
    }
}
