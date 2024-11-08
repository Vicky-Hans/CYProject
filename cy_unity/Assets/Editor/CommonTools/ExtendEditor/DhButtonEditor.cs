using Extend;
using UnityEditor;
using UnityEditor.UI;

namespace Editor.CommonTools.ExtendEdit
{
    [CustomEditor( typeof( DhButton ), true )]
    [CanEditMultipleObjects]
    public class DhButtonEditor:SelectableEditor
    {
        SerializedProperty audioType;
        SerializedProperty scaleEffect;
        SerializedProperty scaleEffectParent;
        protected override void OnEnable()
        {
            // 获取自定义属性的SerializedProperty
            base.OnEnable();
            audioType = serializedObject.FindProperty( "audioType" );
            scaleEffect = serializedObject.FindProperty( "scaleEffect" );
            scaleEffectParent = serializedObject.FindProperty( "scaleEffectParent" );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField( scaleEffect );
            if (scaleEffect.boolValue)
            {
                EditorGUILayout.PropertyField( scaleEffectParent );
            }
            EditorGUILayout.PropertyField( audioType );
            serializedObject.ApplyModifiedProperties();
        }
    }
}