
#if UNITY_EDITOR
using Assets.Scripts.AnimControllers;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editors
{
    [CustomPropertyDrawer(typeof(AnimParam), true)]
    class AnimParamDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUI.GetPropertyHeight(property, label) * 3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Get propertes
            var properties = new List<SerializedProperty>
            {
                property.FindPropertyRelative("m_Name"),
                property.FindPropertyRelative("m_Type")
            };

            if (GetValueName(properties[1]) is string name && name != string.Empty)
                properties.Add(property.FindPropertyRelative(name));

            // Calculate rects
            var rectPosition = new Vector2(position.x, position.y);
            var rectSize = new Vector2(position.width, position.height / properties.Count);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            for (int i = 0; i < properties.Count; i++)
                EditorGUI.PropertyField(new Rect(rectPosition + Vector2.up * rectSize * i, rectSize), properties[i], new GUIContent(properties[i].displayName));

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        private string GetValueName(SerializedProperty type)
        {
        getFieldName:
            switch ((AnimatorControllerParameterType)type.intValue)
            {
                case AnimatorControllerParameterType.Float: return "m_FloatValue";
                case AnimatorControllerParameterType.Int: return "m_IntValue";
                case AnimatorControllerParameterType.Bool: return "m_BoolValue";
                case AnimatorControllerParameterType.Trigger: return string.Empty;
                default: type.intValue = (int)AnimatorControllerParameterType.Bool; goto getFieldName;
            }
        }
    }
}
#endif