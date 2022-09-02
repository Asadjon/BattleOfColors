using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Array;

namespace Assets.Scripts.AnimControllers
{
    class AnimController : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator = null;
        [SerializeField] private List<AnimParam> m_NameOfParametrs = new List<AnimParam>();
        [SerializeField] private List<string> m_NameOfAnimations = new List<string>() { "Idle", "Show anim", "Hide anim" };

        public Animator Animator => m_Animator;
        public List<AnimParam> Parametrs => m_NameOfParametrs;
        public List<string> NameOfAnimations { get => m_NameOfAnimations; set => m_NameOfAnimations = value; }

        private void Awake()
        {
            if (!m_Animator) m_Animator = GetComponent<Animator>();

            if (m_Animator)
            {
                if (m_NameOfParametrs.Count == 0) InitParams();
                else m_NameOfParametrs.ForEach(param => param.Parent = this);

                if (m_NameOfAnimations.Count == 0)
                    ForEach(m_Animator.runtimeAnimatorController.animationClips, param => m_NameOfAnimations.Add(param.name));
            }
        }

        [ContextMenu("Find params")]
        private void InitParams()
        {
            m_NameOfParametrs.Clear();
            if (m_Animator) ForEach(m_Animator.parameters, param => m_NameOfParametrs.Add(new AnimParam(this, param)));
        }


        public void SetParam<T>(string paramName, T paramValue) where T : struct => m_NameOfParametrs.FirstOrDefault(param => param.Name == paramName).Value = paramValue;

        public void SetParam<T>(int paramNameIndex, T paramValue) where T : struct => SetParam(m_NameOfParametrs[paramNameIndex].Name, paramValue);

        public T GetParam<T>(string paramName) where T : struct => (T)m_NameOfParametrs.FirstOrDefault(param => param.Name == paramName).Value;

        public T GetParam<T>(int paramNameIndex) where T : struct => GetParam<T>(m_NameOfParametrs[paramNameIndex].Name);

        public void PlayAnim(string animName)
        {
            if (m_Animator && !string.IsNullOrEmpty(animName))
            {
                List<AnimatorClipInfo> animatorClips = m_Animator.GetCurrentAnimatorClipInfo(0).ToList();
                if (animatorClips.Exists(anim => anim.clip.name.Equals(animName)))
                {
                    m_Animator.Play(animName);
                }
            }
        }

        public void PlayAnim(int animNameIndex) => PlayAnim(m_NameOfAnimations[animNameIndex]);
    }
}
