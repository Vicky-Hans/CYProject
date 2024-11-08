
using Extend;
using UnityEditor;
using UnityEditor.UI;

namespace Editor.CommonTools.ExtendEdit
{
    [CustomEditor( typeof(DhImage), true )]
    [CanEditMultipleObjects]
    public class DhImageEditor : ImageEditor
    {
        SerializedProperty spriteLanguage;

        protected override void OnEnable()
        {
            // 获取自定义属性的SerializedProperty
            base.OnEnable();
            spriteLanguage = serializedObject.FindProperty("spriteLanguage");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            //
            serializedObject.Update();
            EditorGUILayout.PropertyField(spriteLanguage);
            serializedObject.ApplyModifiedProperties();
        }
    }
}