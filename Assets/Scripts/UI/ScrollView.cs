using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    public class ScrollView : Selectable
    {
        [SerializeField] private EventTrigger m_SlidingArea = null;
        [SerializeField] private Image m_ValueImage = null;
        [SerializeField] private SlidingDirection m_Direction = SlidingDirection.Horizontal;
        [SerializeField] private UnityEvent<float> m_OnChangeSlidingValue = null;

        private RectTransform mSlidingAreaTransform;
        private bool mIsOnTouchDown;
        private Vector2 mLastPosition;
        private float mSlidingValue;

        public EventTrigger SlidingArea { get => m_SlidingArea; set => m_SlidingArea = value; }
        public Image ValueImage { get => m_ValueImage; set => m_ValueImage = value; }
        public SlidingDirection Direction { get => m_Direction; set => m_Direction = value; }
        public float SlidingValue { get => mSlidingValue;
            set
            {
                mSlidingValue = Mathf.Clamp(value, 0f, 1f);
                OnSlidingValueChange(SlidingValue);
            }
        }
        public UnityEvent<float> OnChangeSlidingValue { get => m_OnChangeSlidingValue; set => m_OnChangeSlidingValue = value; }

        protected internal float slidingValue { get => mSlidingValue;
            set
            {
                mSlidingValue = Mathf.Clamp(value, 0f, 1f);
                if (m_ValueImage) m_ValueImage.fillAmount = SlidingValue;
            }
        }

        public enum SlidingDirection { Horizontal, Vertical }

        protected override void Awake() => LoadData();

        protected virtual void LoadData()
        {
            if (m_ValueImage) m_ValueImage.type = Image.Type.Filled;
            if (!m_SlidingArea) m_SlidingArea = GetComponent<EventTrigger>();
            if (m_SlidingArea)
            {
                var down = new Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener(data =>
                {
                    mIsOnTouchDown = true;
                    mLastPosition = ((PointerEventData)data).position;
                });

                var up = new Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener(data => mIsOnTouchDown = false);

                var move = new Entry { eventID = EventTriggerType.Drag };
                move.callback.AddListener(data => OnMove(((PointerEventData)data).position));

                m_SlidingArea.triggers.Add(down);
                m_SlidingArea.triggers.Add(up);
                m_SlidingArea.triggers.Add(move);

                mSlidingAreaTransform = m_SlidingArea.GetComponent<RectTransform>();
            }
        }

        private void OnMove(Vector2 movingPosition)
        {
            if (IsInteractable() && mIsOnTouchDown)
            {
                var value = SlidingValue - ((mLastPosition - movingPosition) / mSlidingAreaTransform.rect.size)[(int)Direction];

                mLastPosition = value < 1f && value > 0f ? movingPosition : mLastPosition;

                if(Mathf.Clamp(value, 0f, 1f) is var newValue && newValue != SlidingValue)
                    SlidingValue = value;
            }
        }

        protected virtual void OnSlidingValueChange(float value)
        {
            OnChangeSlidingValue.Invoke(value);
            if (m_ValueImage) m_ValueImage.fillAmount = SlidingValue;
        }
    }
}