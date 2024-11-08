    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(DH.Game.DebugDataAttribute))]
    public class DebugDataPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            label.text = $"{label.text}[调试]";
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
