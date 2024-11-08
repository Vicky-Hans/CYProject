using System;
using UnityEngine;
using UnityEditor;

namespace DH.UIFramework.Editor
{
    [CustomEditor(typeof(FlipPageCircularScrollView))]
    public class FlipPageCircularScrollViewEditor : UICircularScrollViewEditor
    {
        static readonly GUIContent m_OnePageCountLabel = new GUIContent("显示行数", "一页里显示多少行");
        static readonly GUIContent m_SlideSpeedLabel = new GUIContent("滑动的速度", "翻页过程中滑动的速度");
        static readonly GUIContent m_IsOpenNavIconLabel = new GUIContent("显示导航", "是否显示导航按钮");
        static readonly GUIContent m_ObjNavigationParentLabel = new GUIContent("导航父物体", "导航按钮父物体");
        static readonly GUIContent m_NavSpacingLabel = new GUIContent("导航间距", "导航按钮之间的间距");
        static readonly GUIContent m_NavNormalPrefabLabel = new GUIContent("正常按钮", "导航的正常按钮");
        static readonly GUIContent m_NavSelectedPrefabLabel = new GUIContent("选中时按钮", "导航的选中时的按钮");
        static readonly GUIContent m_CurNavAlignmentLabel = new GUIContent("导航对齐方式", "导航的对齐方式");

        protected SerializedProperty m_OnePageCountProp;
        protected SerializedProperty m_SlideSpeedProp;
        protected SerializedProperty m_IsOpenNavIconProp;
        protected SerializedProperty m_ObjNavigationParentProp;
        protected SerializedProperty m_NavSpacingProp;
        protected SerializedProperty m_NavNormalPrefabProp;
        protected SerializedProperty m_NavSelectedPrefabProp;
        protected SerializedProperty m_CurNavAlignmentProp;

        protected FlipPageCircularScrollView m_FlipCircularScrollView;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_AutoScrollSpeedLabel = new GUIContent("翻页的间隔", "翻页的时间间隔");
            m_OnePageCountProp = serializedObject.FindProperty("m_OnePageCount");
            m_SlideSpeedProp = serializedObject.FindProperty("m_SlideSpeed");
            m_IsOpenNavIconProp = serializedObject.FindProperty("m_IsOpenNavIcon");
            m_ObjNavigationParentProp = serializedObject.FindProperty("m_ObjNavigationParent");
            m_NavSpacingProp = serializedObject.FindProperty("m_NavSpacing");
            m_NavNormalPrefabProp = serializedObject.FindProperty("m_NavNormalPrefab");
            m_NavSelectedPrefabProp = serializedObject.FindProperty("m_NavSelectedPrefab");
            m_CurNavAlignmentProp = serializedObject.FindProperty("m_CurNavAlignment");
            
            m_FlipCircularScrollView = (FlipPageCircularScrollView)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_OnePageCountProp, m_OnePageCountLabel); 
            EditorGUILayout.PropertyField(m_SlideSpeedProp, m_SlideSpeedLabel); 
            
            //显示导航按钮
            EditorGUILayout.PropertyField(m_IsOpenNavIconProp, m_IsOpenNavIconLabel);

            if (m_IsOpenNavIconProp.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_ObjNavigationParentProp, m_ObjNavigationParentLabel);
                EditorGUILayout.PropertyField(m_NavSpacingProp, m_NavSpacingLabel);
                EditorGUILayout.PropertyField(m_CurNavAlignmentProp, m_CurNavAlignmentLabel);
                
                if (EditorGUI.EndChangeCheck())
                {
                    m_FlipCircularScrollView.m_CurNavAlignment = (NavAlignment) m_CurNavAlignmentProp.enumValueIndex;
                    RefreshChildPosition();
                }
                
                if (Application.isPlaying)
                {
                    EditorGUILayout.PropertyField(m_NavNormalPrefabProp, m_NavNormalPrefabLabel);
                    EditorGUILayout.PropertyField(m_NavSelectedPrefabProp, m_NavSelectedPrefabLabel);
                }
            }
                
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
            
            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }
        }

        protected override void RefreshChildPosition()
        {
            base.RefreshChildPosition();
            m_FlipCircularScrollView.ResetNavPosForEditor();
        }

        protected override void ShowCanDragProp()
        {
            EditorGUILayout.PropertyField(m_CanDragScrollViewProp, m_CanDragScrollViewLabel); 
        }
    }
}
