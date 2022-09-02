using Assets.Scripts.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(NumberPicker))]
    internal class NumberPickerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var numberPicker = (NumberPicker)target;

            // Draw the Inspector widget for this property.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LeftArrow"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RightArrow"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TargetText"), true);

            var value = serializedObject.FindProperty("m_Value");
            value.intValue = EditorGUILayout.IntSlider(value.displayName, value.intValue, 0, numberPicker.Max - 1);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DisplayedValues"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IsSliding"), true);

            if (numberPicker.IsSliding)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SlidingArea"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ValueImage"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Direction"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnChangeValue"), true);

            // Commit changes to the property back to the component we're editing.
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}