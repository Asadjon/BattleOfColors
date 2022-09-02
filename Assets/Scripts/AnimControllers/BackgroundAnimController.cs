using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AnimControllers
{
    class BackgroundAnimController : AnimController
    {
        [SerializeField] private float m_Speed = 0.1f;

        private void Awake()
        {
            Speed = m_Speed;
        }

        public float Speed { get => m_Speed; set { m_Speed = value; SetParam(0, m_Speed); } }

        public void IsPlay(bool value) => SetParam(1, value);
    }
}