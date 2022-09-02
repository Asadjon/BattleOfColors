using System;
using UnityEngine;
using static System.Array;
using static UnityEngine.AnimatorControllerParameterType;

namespace Assets.Scripts.AnimControllers
{
    [Serializable, RequireComponent(typeof(AnimController))]
    class AnimParam
    {
        [SerializeField] private string m_Name = string.Empty;
        [SerializeField] private AnimatorControllerParameterType m_Type = Bool;

        [SerializeField, InspectorName("Value")] private float m_FloatValue = default;
        [SerializeField, InspectorName("Value")] private int m_IntValue = default;
        [SerializeField, InspectorName("Value")] private bool m_BoolValue = default;

        AnimController mParent = null;

        public AnimController Parent { set => mParent = value; }
        public string Name { get => m_Name; set => m_Name = value; }
        public AnimatorControllerParameterType Type { get => m_Type; set => m_Type = value; }
        
        public object Value
        {
            get
            {
                if (mParent.Animator && Exists(mParent.Animator.parameters, param => param.name.Equals(m_Name) && param.type == m_Type))
                    switch (m_Type)
                    {
                        case Float: return m_FloatValue = mParent.Animator.GetFloat(m_Name);
                        case Int: return m_IntValue = mParent.Animator.GetInteger(m_Name);
                        case Bool: return m_BoolValue = mParent.Animator.GetBool(m_Name);
                        default: throw new TypeAccessException(nameof(m_Type));
                    }
                else if (!mParent.Animator) throw new ArgumentNullException(mParent.Animator.name);
                else throw new ArgumentException(m_Name + " or " + nameof(m_Type));
            }
            set
            {
                if (!mParent) return;

                if (mParent.Animator && Exists(mParent.Animator.parameters, param => param.name.Equals(m_Name) && param.type == m_Type))
                    switch (m_Type)
                    {
                        case Float: mParent.Animator.SetFloat(m_Name, m_FloatValue = (float)value); break;
                        case Int: mParent.Animator.SetInteger(m_Name, m_IntValue = (int)value); break;
                        case Bool: mParent.Animator.SetBool(m_Name, m_BoolValue = (bool)value); break;
                        case Trigger: mParent.Animator.SetTrigger(m_Name); break;
                    }
                else if (!mParent.Animator) throw new ArgumentNullException(mParent.Animator.name);
                //else throw new ArgumentException(m_Name + " or " + m_Type);
            }
        }

        public AnimParam(AnimController parent, string name, AnimatorControllerParameterType type, object value)
        {
            mParent = parent;
            Name = name;
            Type = type;
            Value = value;
        }

        public AnimParam(AnimController parent, AnimatorControllerParameter param)
        {
            mParent = parent;
            Name = param.name;
            Type = param.type;

            switch (param.type)
            {
                case AnimatorControllerParameterType.Float: Value = param.defaultFloat; break;
                case AnimatorControllerParameterType.Int: Value = param.defaultInt; break;
                case AnimatorControllerParameterType.Bool: Value = param.defaultBool; break;
            }
        }
    }
}