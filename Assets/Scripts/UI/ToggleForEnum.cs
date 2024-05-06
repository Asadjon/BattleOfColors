using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI
{
    public abstract class ToggleForEnum<TEnum> : Toggle where TEnum : Enum
    {
        [SerializeField] private TextMeshProUGUI m_Label;
        [SerializeField] private TEnum m_Enum;
        public TEnum Enum { get => m_Enum;
            set
            {
                m_Enum = value;
                if (m_Label) m_Label.text = ConvertEnumValueToString(m_Enum);
            }
        }

        protected abstract string ConvertEnumValueToString(TEnum @enum);
    }
}