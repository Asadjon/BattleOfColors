using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Assets.Scripts.UI
{
    public abstract class ToggleGroupForEnum<TEnum> : ToggleGroup where TEnum : Enum
    {
        [SerializeField] private ToggleForEnum<TEnum> m_TogglePrefab;

        public UnityEvent<TEnum> OnChangeEnumValue;
        public Dictionary<TEnum, ToggleForEnum<TEnum>> Toggles;

        protected override void Awake()
        {
            Toggles = new Dictionary<TEnum, ToggleForEnum<TEnum>>();
            var togglesForEnum = GetComponentsInChildren<ToggleForEnum<TEnum>>();
            foreach (var toggle in togglesForEnum)
                if (!Toggles.ContainsKey(toggle.Enum)) Toggles.Add(toggle.Enum, toggle);

            foreach (var toggle in Toggles)
                toggle.Value.onValueChanged.AddListener(isSelected =>
                { if (isSelected) OnChangeEnumValue.Invoke(toggle.Key); });
        }

        [ContextMenu("Initialize")]
        protected virtual void Initialize()
        {
#if UNITY_EDITOR
            if (!m_TogglePrefab) return;

            var toggles = GetComponentsInChildren<ToggleForEnum<TEnum>>().ToDictionary(toggleForEnum => toggleForEnum.Enum);

            foreach (TEnum @enum in Enum.GetValues(typeof(TEnum)))
            {
                var toggleParent = transform;
                if (toggles.TryGetValue(@enum, out ToggleForEnum<TEnum> value) && value)
                {
                    toggleParent = value.transform.parent;
                    DestroyImmediate(value.gameObject);
                }

                var toggle = PrefabUtility.InstantiatePrefab(m_TogglePrefab, toggleParent) as ToggleForEnum<TEnum>;

                toggle.Enum = @enum;
                toggle.name = "Toggle " + GetName(@enum);
                toggle.group = this;
            }
#endif
        }

        protected abstract string GetName(TEnum @enum);
    }
}