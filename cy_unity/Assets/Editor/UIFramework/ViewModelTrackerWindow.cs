using System.Linq;
using DH.UIFramework;
using UnityEditor;
using UnityEngine;

namespace Editor.UIFramework
{
    public class ViewModelTrackerWindow : EditorWindow
    {
        static int interval;
        static ViewModelTrackerWindow window;

        [MenuItem("Window/ViewModel Tracker")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }
            ViewModelTracker.EnableTracking = EditorPrefs.GetBool(ViewModelTracker.EnableTrackingKey, false);
            // will called OnEnable(singleton instance will be set).
            GetWindow<ViewModelTrackerWindow>("ViewModel Tracker").Show();
        }

        static readonly GUILayoutOption[] EmptyLayoutOption = new GUILayoutOption[0];
        static readonly GUIContent EnableTrackingHeadContent = EditorGUIUtility.TrTextContent("Enable Tracking", "Start to track view model lifecycle", (Texture)null);
        ViewModelTrackerTreeView treeView;
        object splitterState;

        void OnEnable()
        {
            window = this; // set singleton.
            splitterState = SplitterGUILayout.CreateSplitterState(new float[] { 75f, 25f }, new int[] { 32, 32 }, null);
            treeView = new ViewModelTrackerTreeView();
        }

        void OnGUI()
        {
            // Head
            RenderHeadPanel();

            // Splittable
            SplitterGUILayout.BeginVerticalSplit(this.splitterState, EmptyLayoutOption);
            {
                // Column Tabble
                RenderTable();

                // StackTrace details
                RenderDetailsPanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        #region HeadPanel
        static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View.", (Texture)null);
        // [Enable Tracking] | [Enable StackTrace]
        void RenderHeadPanel()
        {
            EditorGUILayout.BeginVertical(EmptyLayoutOption);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, EmptyLayoutOption);

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Toggle(ViewModelTracker.EnableTracking, EnableTrackingHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption) != ViewModelTracker.EnableTracking)
            {
                ViewModelTracker.EnableTracking = !ViewModelTracker.EnableTracking;
            }

            if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton, EmptyLayoutOption))
            {
                ViewModelTracker.CheckAndResetDirty();
                treeView.ReloadAndSort();
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region TableColumn

        Vector2 tableScroll;
        GUIStyle tableListStyle;

        void RenderTable()
        {
            if (tableListStyle == null)
            {
                tableListStyle = new GUIStyle("CN Box");
                tableListStyle.margin.top = 0;
                tableListStyle.padding.left = 3;
            }

            EditorGUILayout.BeginVertical(tableListStyle, EmptyLayoutOption);

            this.tableScroll = EditorGUILayout.BeginScrollView(this.tableScroll, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000f)
            });
            var controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true)
            });


            treeView?.OnGUI(controlRect);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (interval++ % 120 == 0)
            {
                if (ViewModelTracker.CheckAndResetDirty())
                {
                    treeView.ReloadAndSort();
                    Repaint();
                }
            }
        }

        #endregion

        #region Details

        static GUIStyle detailsStyle;
        Vector2 detailsScroll;

        void RenderDetailsPanel()
        {
            if (detailsStyle == null)
            {
                detailsStyle = new GUIStyle("CN Message");
                detailsStyle.wordWrap = false;
                detailsStyle.stretchHeight = true;
                detailsStyle.margin.right = 15;
            }

            string message = "";
            var selected = treeView.state.selectedIDs;
            if (selected.Count > 0)
            {
                var first = selected[0];
                if (treeView.CurrentBindingItems.FirstOrDefault(x => x.id == first) is ViewModelTrackerViewItem item)
                {
                    message = item.Position;
                }
            }

            detailsScroll = EditorGUILayout.BeginScrollView(this.detailsScroll, EmptyLayoutOption);
            var vector = detailsStyle.CalcSize(new GUIContent(message));
            EditorGUILayout.SelectableLabel(message, detailsStyle, new GUILayoutOption[]
            {
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinWidth(vector.x),
                GUILayout.MinHeight(vector.y)
            });
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}