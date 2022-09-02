using Assets.Scripts.AnimControllers;
using UnityEngine.Events;

namespace Assets.Scripts
{
    class Transition : AnimController
    {
        [UnityEngine.SerializeField] private float m_Duration = 1f;
        public UnityEvent StartingEnd = null;

        public void OnEnable() => SetParam(0, m_Duration);

        public void StartTransition() => SetParam(1, default(char));

        public void StartAnimEnding() => StartingEnd.Invoke();
    }
}