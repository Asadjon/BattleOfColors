using Assets.Scripts.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Assets.Scripts.Editors
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ToggleForEnum<>), true)]
    public class ToggleForEnum  : ToggleEditor
    {
        protected override void OnEnable()
        {
            try
            {
                base.OnEnable();
            }
            catch { }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject != null)
            {

                if (serializedObject.FindProperty("m_Label") is SerializedProperty m_Label && m_Label != null) EditorGUILayout.PropertyField(m_Label, true);
                if (serializedObject.FindProperty("m_Enum") is SerializedProperty m_Enum && m_Enum != null) EditorGUILayout.PropertyField(m_Enum, true);

                // Commit changes to the property back to the component we're editing.
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}