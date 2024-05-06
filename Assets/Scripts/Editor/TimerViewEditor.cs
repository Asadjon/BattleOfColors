using Assets.Scripts.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TimerView))]
    class TimerViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var timerView = (TimerView)target;

            // Draw the Inspector widget for this property.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ImageOfValue"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GivenTime"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_TimeLeft"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Speed"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IsEvluateTime"), true);

            if(timerView.IsEvluateTime) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_EvluateTime"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnUpdateTime"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnTimeOut"), true);

            // Commit changes to the property back to the component we're editing.
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
