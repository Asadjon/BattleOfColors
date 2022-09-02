using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Players
{
    class TranslateAnimation : MonoBehaviour
    {
        [SerializeField] private UnityEvent m_EndAnim = null;

        private RectTransform mMyView;
        private Vector mFromDelta;
        private Vector2 mToDelta;
        private float mDuration;
        private float mMillisLeft;

        public UnityEvent EndAnim { get => m_EndAnim; set => m_EndAnim = value; }
        public bool IsRunning { get; private set; } = false;

        private void Awake() => mMyView = GetComponent<RectTransform>();

        private void Update()
        {
            if (Calc) Invalidate();
            else if (IsRunning) CancelAnim();
        }

        protected bool Calc => IsRunning && (mMillisLeft -= Time.deltaTime) >= 0f;

        public TranslateAnimation Set(Vector2 fromDelta, Vector2 toDelta, float duration)
        {
            mFromDelta = Vector.NewInstance(mMyView.anchorMin + fromDelta * 1f, mMyView.anchorMax + fromDelta * 1f);

            mToDelta = toDelta;
            mDuration = duration;
            mMillisLeft = mDuration;
            IsRunning = false;

            return this;
        }

        public TranslateAnimation StartAnim()
        {
            mMyView.anchorMin = mFromDelta.Min;
            mMyView.anchorMax = mFromDelta.Max;
            IsRunning = true;
            return this;
        }

        public void CancelAnim()
        {
            mMyView.anchorMin = mFromDelta.Min + mToDelta * 1f;
            mMyView.anchorMax = mFromDelta.Max + mToDelta * 1f;

            mMillisLeft = -1f;
            IsRunning = false;
            m_EndAnim.Invoke();
        }

        private void Invalidate()
        {
            var scaler = 1f - mMillisLeft / mDuration;

            mMyView.anchorMin = mFromDelta.Min + mToDelta * scaler;
            mMyView.anchorMax = mFromDelta.Max + mToDelta * scaler;
        }

        class Vector
        {
            public Vector2 Min { get; set; }
            public Vector2 Max { get; set; }

            public Vector(Vector2 min, Vector2 max)
            {
                Min = min;
                Max = max;
            }

            public static Vector NewInstance(Vector2 min, Vector2 max) => new Vector(min, max);
        }
    }
}
