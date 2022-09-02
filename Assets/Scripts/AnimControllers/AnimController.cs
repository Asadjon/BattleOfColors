using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.AnimatorControllerParameterType;
using static System.Array;

namespace Assets.Scripts
{
    class AnimController : MonoBehaviour
    {
        [SerializeField] private Animator m_Animator = null;
        [SerializeField] private List<string> m_NameOfParametrs = new List<string>() { "isShow" };
        [SerializeField] private List<string> m_NameOfAnimations = new List<string>() { "Idle", "Show anim", "Hide anim" };

        public List<string> NameOfParametrs { get => m_NameOfParametrs; set => m_NameOfParametrs = value; }
        public List<string> NameOfAnimations { get => m_NameOfAnimations; set => m_NameOfAnimations = value; }

        private void Awake()
        {
            if (!m_Animator) m_Animator = GetComponent<Animator>();

            if (m_Animator && m_NameOfParametrs.Count == 0)
                    ForEach(m_Animator.parameters, param => m_NameOfParametrs.Add(param.name));
        }

        public void SetParam<T>(string paramName, T paramValue) where T : struct => Set<T>(paramName, paramValue);

        public void SetParam<T>(int paramNameIndex, T paramValue) where T : struct => Set<T>(m_NameOfParametrs[paramNameIndex], paramValue);

        public T GetParam<T>(string paramName) where T : struct => Get<T>(paramName);

        public T GetParam<T>(int paramNameIndex) where T : struct => Get<T>(m_NameOfParametrs[paramNameIndex]);

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

        private T Get<T>(string paramName) where T : struct
        {
            var type = GetParamType<T>();

            if (m_Animator && Exists(m_Animator.parameters, param => param.name.Equals(paramName) && param.type == type))
                switch (type)
                {
                    case Float: return (T)(object)m_Animator.GetFloat(paramName);
                    case Int: return (T)(object)m_Animator.GetInteger(paramName);
                    case Bool: return (T)(object)m_Animator.GetBool(paramName);
                    default: throw new TypeAccessException(nameof(type));
                }
            else if (!m_Animator) throw new ArgumentNullException(m_Animator.name);
            else throw new ArgumentException(paramName + " or " + nameof(type));
        }

        private void Set<T>(string paramName, T paramValue) where T : struct
        {
            var type = GetParamType<T>();
            if (m_Animator && Exists(m_Animator.parameters, param => param.name.Equals(paramName) && param.type == type))
                switch (type)
                {
                    case Float: m_Animator.SetFloat(paramName, (float)(object)paramValue); break;
                    case Int: m_Animator.SetInteger(paramName, (int)(object)paramValue); break;
                    case Bool: m_Animator.SetBool(paramName, (bool)(object)paramValue); break;
                    case Trigger: m_Animator.SetTrigger(paramName); break;
                }
            else if (!m_Animator) throw new ArgumentNullException(m_Animator.name);
            else throw new ArgumentException(paramName + " or " + nameof(type));
        }

        private AnimatorControllerParameterType GetParamType<T>() where T : struct
        {
            var paramType = typeof(T);
            return (paramType == typeof(float)) ? Float :
                   (paramType == typeof(int)) ? Int :
                   (paramType == typeof(bool)) ? Bool : Trigger;
        }
    }
}
