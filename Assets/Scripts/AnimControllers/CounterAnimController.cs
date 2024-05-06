using Assets.Scripts.AudioManagers;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.AnimControllers
{
    class CounterAnimController : AnimController
    {
        [SerializeField] private UnityEvent m_StartCounter = null;
        [SerializeField] private UnityEvent m_EndCounter = null;

        public UnityEvent StartCounter { get => m_StartCounter; set => m_StartCounter = value; }
        public UnityEvent EndCounter { get => m_EndCounter; set => m_EndCounter = value; }

        private bool mIsCounting = false;

        private void ChangeCounting() => ((mIsCounting = !mIsCounting) ? StartCounter : EndCounter).Invoke();

        private void PlaySound(EditorSoundObject soundObject) => soundObject.Play();
    }
}