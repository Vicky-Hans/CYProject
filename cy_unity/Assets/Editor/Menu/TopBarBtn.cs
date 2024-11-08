
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityToolbarExtender
{

    [InitializeOnLoad]
    public static class TopBarBtn
    {
        private static readonly Type kToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject sCurrentToolbar;

        static TopBarBtn()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (sCurrentToolbar == null)
            {
                UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(kToolbarType);
                sCurrentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
                if (sCurrentToolbar != null)
                {
                    FieldInfo root = sCurrentToolbar.GetType()
                        .GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    VisualElement concreteRoot = root.GetValue(sCurrentToolbar) as VisualElement;

                    VisualElement toolbarZone = concreteRoot.Q("ToolbarZoneRightAlign");
                    VisualElement parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    IMGUIContainer container = new IMGUIContainer();
                    container.onGUIHandler += OnGuiBody;
                    parent.Add(container);
                    toolbarZone.Add(parent);
                }
            }
        }

        private static void OnGuiBody()
        {
            //自定义按钮加在此处
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("快速启动", EditorGUIUtility.FindTexture("PlayButton"))))
            {
                SceneHelper.StartScene("Assets/Scenes/Launcher.unity");
            }

            if (GUILayout.Button(new GUIContent("战斗测试", EditorGUIUtility.FindTexture("PlayButton"))))
            {
                SceneHelper.StartScene("Assets/GameAssets/Scenes/Level1.unity");
            }

            GUILayout.EndHorizontal();
        }
    }

    static class SceneHelper
    {
        static string sceneToOpen;

        public static void StartScene(string scene)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            sceneToOpen = scene;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(sceneToOpen);
                EditorApplication.isPlaying = true;
            }

            sceneToOpen = null;
        }
    }

}
