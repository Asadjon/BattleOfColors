using UnityEngine;
using UnityEditor;

namespace Assets.Scripts.UI.Editor
{
    [CustomEditor(typeof(CustomGridLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class CustomGridLatyoutGroupEditor : UnityEditor.Editor
    {
        SerializedProperty m_Padding;
        SerializedProperty m_Spacing;
        SerializedProperty m_ChildAlignment;
        SerializedProperty m_ChildControlWidth;
        SerializedProperty m_ChildControlHeight;
        SerializedProperty m_ReverseArrangement;
        SerializedProperty m_AxisDirection;
        SerializedProperty m_Constraint;
        SerializedProperty m_ConstraintCount;

        protected virtual void OnEnable()
        {
            m_Padding = serializedObject.FindProperty("m_Padding");
            m_Spacing = serializedObject.FindProperty("m_Spacing");
            m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
            m_ReverseArrangement = serializedObject.FindProperty("m_ReverseArrangement");
            m_AxisDirection = serializedObject.FindProperty("m_AxisDirection");
            m_Constraint = serializedObject.FindProperty("m_Constraint");
            m_ConstraintCount = serializedObject.FindProperty("m_ConstraintCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _ = EditorGUILayout.PropertyField(m_Padding, true);
            _ = EditorGUILayout.PropertyField(m_Spacing, true);
            _ = EditorGUILayout.PropertyField(m_ChildAlignment, true);
            _ = EditorGUILayout.PropertyField(m_AxisDirection, true);
            _ = EditorGUILayout.PropertyField(m_Constraint, true);
            if (m_Constraint.enumValueIndex == 1)
            {
                _ = EditorGUILayout.PropertyField(m_ConstraintCount, true);
                m_ConstraintCount.intValue = m_ConstraintCount.intValue > 1 ? m_ConstraintCount.intValue : 1;
            }

            _ = EditorGUILayout.PropertyField(m_ReverseArrangement, true);

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, -1, EditorGUIUtility.TrTextContent("Control Child Size"));
            rect.width = Mathf.Max(50, (rect.width - 4) / 3);
            EditorGUIUtility.labelWidth = 50;
            ToggleLeft(rect, m_ChildControlWidth, EditorGUIUtility.TrTextContent("Width"));
            rect.x += rect.width + 2;
            ToggleLeft(rect, m_ChildControlHeight, EditorGUIUtility.TrTextContent("Height"));
            EditorGUIUtility.labelWidth = 0;

            _ = serializedObject.ApplyModifiedProperties();
        }

        private void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool toggle = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            toggle = EditorGUI.ToggleLeft(position, label, toggle);
            EditorGUI.indentLevel = oldIndent;
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = property.hasMultipleDifferentValues ? true : !property.boolValue;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}
