using UnityEngine;

namespace Assets.Scripts.AnimControllers
{
    class BackgroundAnimController : AnimController
    {
        [SerializeField] private float m_Speed = 0.1f;

        private void Awake()
        {
            SetParam(0, m_Speed);
        }

        public void IsPlay(bool value)
        {
            if (value) SetParam(0, m_Speed);
            else SetParam(0, 0f);
        }
    }
}