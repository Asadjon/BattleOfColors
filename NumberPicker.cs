using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.UI
{
    class NumberPicker : MonoBehaviour
    {
        #region SerializeField Objects
        [SerializeField] private Button m_LeftArrow = null, m_RightArrow = null;
        [SerializeField] private TextMeshProUGUI m_TargetText = null;
        [SerializeField] private bool m_IsSliding = false;
        [SerializeField] private int m_Value = 0;
        [SerializeField] private UnityEvent<int> m_OnChangeValue = null;
        #endregion

        #region Local Objects
        private EventTrigger mSlidingArea = null;
        private RectTransform mSlidingAreaTransform = null;
        private Image mValueImage = null;
        private int mMax = 0;
        private List<string> m_DisplayedValues = new List<string>();
        private string mDisplayedValue = string.Empty;
        #endregion

        #region Getters And Setters
        public int Max { get => mMax; private set => mMax = value; }
        public int Value { get => m_Value; 
            set 
            {
                m_Value = value;
                mSlidingValue = Value / (Max - 1);
                mDisplayedValue = m_DisplayedValues[Value];
                UpdateUI();
            } 
        }
        public bool IsSliding { get => m_IsSliding; set => m_IsSliding = value; }
        public EventTrigger SlidingArea { get => mSlidingArea; set => mSlidingArea = value; }
        public Image ValueImage { get => mValueImage; set => mValueImage = value; }
        public SlidingDirection Direction { get; set; } = SlidingDirection.Horizontal;
        public UnityEvent<int> OnChangeValue { get => m_OnChangeValue; }
        public List<string> DisplayedValues { get => m_DisplayedValues;
            set
            {
                m_DisplayedValues = value;
                Max = m_DisplayedValues.Count;
                DisplayedValue = DisplayedValues[m_Value];
            }
        }
        public string DisplayedValue { get => mDisplayedValue; set => Value = m_DisplayedValues.IndexOf(value); }
        #endregion

        public enum SlidingDirection { Horizontal = 0, Vertical = 1 }

        private void Awake()
        {
            if(m_LeftArrow) m_LeftArrow.onClick.AddListener(LeftArrowClick);
            if (m_RightArrow) m_RightArrow.onClick.AddListener(RightArrowClick);

            if (mValueImage) mValueImage.type = Image.Type.Filled;
            if (!mSlidingArea) mSlidingArea = GetComponent<EventTrigger>();
            if (mSlidingArea)
            {
                var down = new Entry{ eventID = EventTriggerType.PointerDown };
                down.callback.AddListener(data =>
                {
                    mIsOnTouchDown = IsSliding;
                    mLastPosition = ((PointerEventData)data).position;
                });

                var up = new Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener(data => mIsOnTouchDown = false);

                var move = new Entry { eventID = EventTriggerType.Drag };
                move.callback.AddListener(data => OnMove(((PointerEventData)data).position));

                mSlidingArea.triggers.Add(down);
                mSlidingArea.triggers.Add(up);
                mSlidingArea.triggers.Add(move);

                mSlidingAreaTransform = mSlidingArea.GetComponent<RectTransform>();
            }

            if (m_DisplayedValues.Count > 0) DisplayedValue = m_DisplayedValues[Value];
        }

        public void LeftArrowClick() => Subtract();

        public void RightArrowClick() => Add();

        private void Add() => Value = Mathf.Clamp(Value + 1, 0, mMax - 1);

        private void Subtract() => Value = Mathf.Clamp(Value - 1, 0, mMax - 1);

        private void UpdateUI()
        {
            if (m_TargetText) m_TargetText.text = mDisplayedValue;

            if (m_RightArrow) m_RightArrow.interactable = Value < mMax-1;
            if (m_LeftArrow) m_LeftArrow.interactable = Value > 0;

            if (mValueImage) mValueImage.fillAmount = Value / Max;

            OnChangeValue.Invoke(Value);
        }


        private bool mIsOnTouchDown = false;
        private Vector2 mLastPosition;
        private float mSlidingValue = 0f;

        private void OnMove(Vector2 movingPosition)
        {
            if (mIsOnTouchDown)
            {
                mSlidingValue -= ((mLastPosition - movingPosition) / mSlidingAreaTransform.rect.size)[(int)Direction];

                mLastPosition = mSlidingValue < 1f && mSlidingValue > 0f ? movingPosition : mLastPosition;

                if ((int)Mathf.Lerp(0f, Max - 1f, mSlidingValue = Mathf.Clamp(mSlidingValue, 0f, 1f)) is var newValue && newValue != Value)
                {
                    m_Value = newValue;
                    mDisplayedValue = m_DisplayedValues[Value];
                    UpdateUI();
                }
            }
        }
    }
}
