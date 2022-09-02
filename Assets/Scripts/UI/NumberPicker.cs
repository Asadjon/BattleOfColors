using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class NumberPicker : ScrollView, ISerializationCallbackReceiver
    {
        #region SerializeField Objects
        [SerializeField] private Button m_LeftArrow = null, m_RightArrow = null;
        [SerializeField] private TextMeshProUGUI m_TargetText = null;
        [SerializeField] private bool m_IsSliding = false;
        [SerializeField] private List<string> m_DisplayedValues = new List<string> { "1", "2", "3" };
        [SerializeField] private int m_Value = 0;
        [SerializeField] private UnityEvent<int> m_OnChangeValue = null;
        #endregion

        #region Local Objects
        private string mDisplayedValue = string.Empty;
        #endregion

        #region Getters And Setters
        public int Max { get => m_DisplayedValues.Count; }
        public int Value { get => m_Value; 
            set 
            {
                m_Value = Mathf.Clamp(value, 0, Max - 1);
                slidingValue = Value / (Max - 1f);
                mDisplayedValue = m_DisplayedValues[Value];
                UpdateUI();
            } 
        }
        public bool IsSliding { get => m_IsSliding; set => m_IsSliding = value; }
        public UnityEvent<int> OnChangeValue { get => m_OnChangeValue; }
        public List<string> DisplayedValues { get => m_DisplayedValues;
            set
            {
                m_DisplayedValues = value;
                mDisplayedValue = DisplayedValues[m_Value];
            }
        }
        #endregion

        protected internal int value { get => m_Value; set => m_Value = value; }

        protected override void LoadData()
        {
            base.LoadData();

            if (m_LeftArrow) m_LeftArrow.onClick.AddListener(() => Value -= 1);
            if (m_RightArrow) m_RightArrow.onClick.AddListener(() => Value += 1);

            if (m_DisplayedValues.Count > 0) mDisplayedValue = m_DisplayedValues[Value];
        }

        private void UpdateUI()
        {
            if (m_TargetText) m_TargetText.text = mDisplayedValue;

            if (m_RightArrow) m_RightArrow.interactable = Value < Max - 1;
            if (m_LeftArrow) m_LeftArrow.interactable = Value > 0;

            OnChangeValue.Invoke(Value);
        }

        protected override void OnSlidingValueChange(float value)
        {
            if (IsSliding && (int)Mathf.Lerp(0f, Max - 1f, value) is var newValue && newValue != Value)
            {
                m_Value = newValue;
                mDisplayedValue = m_DisplayedValues[Value];
                UpdateUI();
            }
        }

        public void OnBeforeSerialize()
        {
            Value = m_Value;
        }

        public void OnAfterDeserialize() { }
    }
}
