using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace DH.UIFramework.Editor
{
    [CustomEditor(typeof(UICircularScrollView))]
    public class UICircularScrollViewEditor : UnityEditor.Editor
    {
        static readonly GUIContent cellIntervalLabel = new GUIContent("Interval", "变化步幅");
        static readonly GUIContent cellOffsetLabel = new GUIContent("Show Offset", "显示的偏移");
        static readonly GUIContent inertiaLabel = new GUIContent("Inertia", "是否吸附在偏移处");
        
        static readonly GUIContent k_DirectionLabel = new GUIContent("Direction", "滑动的方向");
        static readonly GUIContent m_PreloadItemCountLabel = new GUIContent("预加载个数", "提前预加载的item个数");
        static readonly GUIContent m_CanAutoScrollLabel = new GUIContent("AutoScroll", "是否自动滚动");
        static readonly GUIContent m_SpacingLabel = new GUIContent("Item 间距", "item之间的间距");
        static readonly GUIContent m_SpacingXLabel = new GUIContent("X");
        static readonly GUIContent m_SpacingYLabel = new GUIContent("Y");
        static readonly GUIContent m_PaddingLabel = new GUIContent("Padding");
        static readonly GUIContent m_PaddingLeftLabel = new GUIContent("Left");
        static readonly GUIContent m_PaddingRightLabel = new GUIContent("Right");
        static readonly GUIContent m_PaddingTopLabel = new GUIContent("Top");
        static readonly GUIContent m_PaddingBottomLabel = new GUIContent("Bottom");
        static readonly GUIContent m_RowLabel = new GUIContent("单行的个数", "一行显示多少个item");
        static readonly GUIContent m_DefaultBottomLabel = new GUIContent("默认滑到底部", "显示的时候是否默认滑到底部");
        static readonly GUIContent m_ShowTipOffsetLabel = new GUIContent("上下提示的距离", "滑过超过该数值时才显示底部，顶部提示");
        static readonly GUIContent m_CellGameObjectLabel = new GUIContent("Item Prefab", "默认的Item的prefab，一般在代码里自动获取");
        static readonly GUIContent m_CanDragInViewLabel = new GUIContent("一页显示时可拖拽", "如果一页即可显示全部内容时，是否可拖拽");
        static readonly GUIContent childAlignmentLabel = new GUIContent("布局对齐方案", "");
        static readonly GUIContent cellSizeTypeLabel = new GUIContent("显示的Item的Size类型", "");
        protected static readonly GUIContent m_CanDragScrollViewLabel = new GUIContent("可拖动ScrollView", "是否可拖动ScrollView");
        protected GUIContent m_AutoScrollSpeedLabel = new GUIContent("AutoScroll Speed", "滚动速度，对于flip是翻页时间间隔");
        
        protected SerializedProperty cellIntervalProp;
        protected SerializedProperty cellOffsetProp;
        protected SerializedProperty inertiaProp;
        
        protected SerializedProperty m_DirectionProp;
        protected SerializedProperty m_PreloadItemCountProp;
        protected SerializedProperty m_CanAutoScrollProp;
        protected SerializedProperty m_AutoScrollSpeedProp;
        protected SerializedProperty m_RowProp;
        protected SerializedProperty m_SpacingXProp;
        protected SerializedProperty m_SpacingYProp;
        protected SerializedProperty m_DefaultBottomProp;
        protected SerializedProperty m_ShowTipOffsetProp;
        protected SerializedProperty m_CellGameObjectProp;
        protected SerializedProperty m_PaddingLeftProp;
        protected SerializedProperty m_PaddingRightProp;
        protected SerializedProperty m_PaddingTopProp;
        protected SerializedProperty m_PaddingBottomProp;
        protected SerializedProperty m_CanDragInViewProp;
        protected SerializedProperty childAlignmentProp;
        protected SerializedProperty cellSizeTypeProp;
        protected SerializedProperty m_CanDragScrollViewProp;
        
        protected bool m_HavePropertiesChanged;
        protected UICircularScrollView m_CircularScrollView;
        
        protected virtual void OnEnable()
        {
            cellIntervalProp = serializedObject.FindProperty("cellInterval");
            cellOffsetProp = serializedObject.FindProperty("cellOffset");
            inertiaProp = serializedObject.FindProperty("inertia");
            
            m_DirectionProp = serializedObject.FindProperty("m_Direction");
            m_CanAutoScrollProp = serializedObject.FindProperty("m_CanAutoScroll");
            m_AutoScrollSpeedProp = serializedObject.FindProperty("m_AutoScrollSpeed");
            m_PreloadItemCountProp = serializedObject.FindProperty("m_PreloadItemCount");
            m_RowProp = serializedObject.FindProperty("m_Row");
            m_SpacingXProp = serializedObject.FindProperty("m_SpacingX");
            m_SpacingYProp = serializedObject.FindProperty("m_SpacingY");
            m_DefaultBottomProp = serializedObject.FindProperty("m_DefaultBottom");
            m_ShowTipOffsetProp = serializedObject.FindProperty("m_ShowTipOffset");
            m_CellGameObjectProp = serializedObject.FindProperty("m_CellGameObject");
            m_PaddingLeftProp = serializedObject.FindProperty("m_PaddingLeft");
            m_PaddingRightProp = serializedObject.FindProperty("m_PaddingRight");
            m_PaddingTopProp = serializedObject.FindProperty("m_PaddingTop");
            m_PaddingBottomProp = serializedObject.FindProperty("m_PaddingBottom");
            m_CanDragInViewProp = serializedObject.FindProperty("m_CanDragInView");
            childAlignmentProp = serializedObject.FindProperty("childAlignment");
            cellSizeTypeProp = serializedObject.FindProperty("m_CellSizeType");
            m_CanDragScrollViewProp = serializedObject.FindProperty("m_CanDragScrollView");
            
            m_CircularScrollView = (UICircularScrollView)target;
        }

        public override void OnInspectorGUI()
        {
            bool refreshPos = false;
            
            EditorGUILayout.PropertyField(cellIntervalProp, cellIntervalLabel); 
            EditorGUILayout.PropertyField(cellOffsetProp, cellOffsetLabel); 
            EditorGUILayout.PropertyField(inertiaProp, inertiaLabel); 
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_DirectionProp, k_DirectionLabel); //方向
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
                refreshPos = true;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_RowProp, m_RowLabel); 
            EditorGUILayout.PropertyField(m_PreloadItemCountProp, m_PreloadItemCountLabel); 
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
                refreshPos = true;
            }
            
            EditorGUI.BeginChangeCheck();
            ShowAutoScrollProp();
            refreshPos = refreshPos || DrawSpacing();
            refreshPos = refreshPos || DrawPadding();
            refreshPos = refreshPos || DrawChildAssignment();

            ShowInverseProp();
            ShowCanDragProp();
            EditorGUILayout.PropertyField(m_ShowTipOffsetProp, m_ShowTipOffsetLabel);

            if (Application.isPlaying && m_CellGameObjectProp != null)
            {
                EditorGUILayout.PropertyField(m_CellGameObjectProp, m_CellGameObjectLabel);
            }

            ShowCellItemSizeProp();
            
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
            
            if (serializedObject.ApplyModifiedProperties() || m_HavePropertiesChanged)
            {
                m_HavePropertiesChanged = false;
                EditorUtility.SetDirty(target);
            }

            if (!Application.isPlaying && (GUILayout.Button("刷新位置") ||refreshPos))
            {
                RefreshChildPosition();
            }
        }
        
        bool DrawSpacing()
        {
            bool bChanged = false;
            EditorGUI.BeginChangeCheck();

            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            EditorGUI.PrefixLabel(rect, m_SpacingLabel);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float currentLabelWidth = EditorGUIUtility.labelWidth;
            rect.x += currentLabelWidth;
            rect.width = (rect.width - currentLabelWidth - 20f) / 2f;

            EditorGUIUtility.labelWidth = Mathf.Min(rect.width * 0.55f, 20f);

            EditorGUI.PropertyField(rect, m_SpacingXProp, m_SpacingXLabel);
            rect.x += rect.width + 20f;
            EditorGUI.PropertyField(rect, m_SpacingYProp, m_SpacingYLabel);

            EditorGUIUtility.labelWidth = currentLabelWidth;
            EditorGUI.indentLevel = oldIndent;

            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
                bChanged = true;
            }

            EditorGUILayout.Space();
            return bChanged;
        }
        
        bool DrawPadding()
        {
            bool bChanged = false;
            EditorGUI.BeginChangeCheck();

            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            EditorGUI.PrefixLabel(rect, m_PaddingLabel);

            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float currentLabelWidth = EditorGUIUtility.labelWidth;
            rect.x += currentLabelWidth;
            rect.width = (rect.width - currentLabelWidth - 3f) / 2f;

            EditorGUIUtility.labelWidth = Mathf.Min(rect.width * 0.55f, 80f);

            EditorGUI.PropertyField(rect, m_PaddingLeftProp, m_PaddingLeftLabel);
            rect.x += rect.width + 3f;
            EditorGUI.PropertyField(rect, m_PaddingRightProp, m_PaddingRightLabel);

            rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

            rect.x += currentLabelWidth;
            rect.width = (rect.width - currentLabelWidth -3f) / 2f;
            EditorGUIUtility.labelWidth = Mathf.Min(rect.width * 0.55f, 80f);

            EditorGUI.PropertyField(rect, m_PaddingTopProp, m_PaddingTopLabel);
            rect.x += rect.width + 3f;
            EditorGUI.PropertyField(rect, m_PaddingBottomProp, m_PaddingBottomLabel);

            EditorGUIUtility.labelWidth = currentLabelWidth;
            EditorGUI.indentLevel = oldIndent;

            if (EditorGUI.EndChangeCheck())
            {
                bChanged = true;
                m_HavePropertiesChanged = true;
            }

            EditorGUILayout.Space();

            return bChanged;
        }
        
        bool DrawChildAssignment()
        {
            bool bChanged = false;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(childAlignmentProp, childAlignmentLabel);

            if (EditorGUI.EndChangeCheck())
            {
                bChanged = true;
                m_HavePropertiesChanged = true;
            }
            return bChanged;
        }

        protected virtual void RefreshChildPosition()
        {
            var scrollRect = m_CircularScrollView.GetComponent<ScrollRect>();
            var contentTrans = scrollRect.content;
            var childCount = contentTrans.childCount;

            if (childCount > 0)
            {
                m_CircularScrollView.m_CellGameObject = null;
                m_CircularScrollView.ResetInit(contentTrans.GetChild(0).gameObject);

                for (int i = 0; i < childCount; ++i)
                {
                    var child = contentTrans.GetChild(i) as RectTransform;
                    child.anchoredPosition = m_CircularScrollView.GetCellOriPos(i);
                }
            }
        }

        protected virtual void ShowInverseProp()
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_DefaultBottomProp, m_DefaultBottomLabel);
            
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }

        protected virtual void ShowCellItemSizeProp()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(cellSizeTypeProp, cellSizeTypeLabel);
            
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }

        protected virtual void ShowCanDragProp()
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_CanDragScrollViewProp, m_CanDragScrollViewLabel); 
            EditorGUILayout.PropertyField(m_CanDragInViewProp, m_CanDragInViewLabel);
            
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }

        protected virtual void ShowAutoScrollProp()
        {
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_CanAutoScrollProp, m_CanAutoScrollLabel); //是否自动滚动
            
            if (m_CanAutoScrollProp.boolValue)
            {
                EditorGUILayout.PropertyField(m_AutoScrollSpeedProp, m_AutoScrollSpeedLabel);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                m_HavePropertiesChanged = true;
            }
        }
    }
}
