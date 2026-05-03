using UnityEngine;
using UnityEditor;

namespace DH.UIFramework.Editor
{
    [CustomEditor(typeof(GroupExpandCircularScrollView))]
    public class GroupExpandCircularScrollViewEditor : UICircularScrollViewEditor
    {
        static readonly GUIContent m_IsExpandLabel = new GUIContent("默认展开", "是否默认展开button");
        static readonly GUIContent m_ExpandButtonLabel = new GUIContent("默认展开", "是否默认展开button");

        protected SerializedProperty m_IsExpandProp;
        protected SerializedProperty m_ExpandButtonProp;

        protected GroupExpandCircularScrollView m_GroupCircularScrollView;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_IsExpandProp = serializedObject.FindProperty("m_IsExpand");
            m_ExpandButtonProp = serializedObject.FindProperty("expandButtonPrefab");
            
            m_GroupCircularScrollView = (GroupExpandCircularScrollView)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.PropertyField(m_IsExpandProp, m_IsExpandLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.PropertyField(m_ExpandButtonProp, m_ExpandButtonLabel);
            } 

            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        protected override void ShowInverseProp()
        {
        }

        protected override void ShowAutoScrollProp()
        {
        }
    }
}
