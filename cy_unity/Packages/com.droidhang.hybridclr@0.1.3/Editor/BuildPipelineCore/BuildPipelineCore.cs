using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Il2Cpp;
using UnityEditor.UnityLinker;
using UnityEngine.SceneManagement;
using UnityEngine;

#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace DHHybridCLR.Editor.BuildPipeline.BuildPipelineCore
{
#if !WECHAT_MINI

    public class BuildPipelineCore : IPreprocessBuildWithReport, IProcessSceneWithReport, IFilterBuildAssemblies,
        IPostBuildPlayerScriptDLLs,
#if !UNITY_2021_1_OR_NEWER
        IIl2CppProcessor,
#endif
#if UNITY_ANDROID
        IPostGenerateGradleAndroidProject,
#endif
        IPostprocessBuildWithReport
    {
        private static MethodInfo _sBuildReportAddMessage = null;

        int IOrderedCallback.callbackOrder => 10;

        private readonly List<BuildPipelineCallbackBase> _buildPipelineCallback =
            new List<BuildPipelineCallbackBase>(16);

        private BuildReport _reporter = null;

        private void CheckPipelineCallback(BuildReport report)
        {
            if (_sBuildReportAddMessage == null)
            {
                var flag = BindingFlags.Instance | BindingFlags.NonPublic;
                _sBuildReportAddMessage = typeof(BuildReport).GetMethod("AddMessage", flag);
            }

            if (report != null)
            {
                _reporter = report;
            }

            BuildPipelineException.Init((successful, willCancel) =>
            {
                if (Application.isPlaying)
                {
                    return;
                }

                if (!willCancel)
                {
                    return;
                }

                EditorUtility.DisplayDialog("错误", "有异常发生，请根据控制台提示修正对应错误!", "确定");
                _sBuildReportAddMessage.Invoke(_reporter,
                    new object[] {LogType.Exception, "用户取消", "BuildFailedException"});
            });

            if (_buildPipelineCallback.Count > 0)
            {
                return;
            }

            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                .ToArray();
            var baseType = typeof(BuildPipelineCallbackBase);
            foreach (var callbackType in allTypes)
            {
                if (!baseType.IsAssignableFrom(callbackType))
                {
                    continue;
                }

                var typeInstance = (BuildPipelineCallbackBase) Activator.CreateInstance(callbackType);
                if (typeInstance != null)
                {
                    _buildPipelineCallback.Add(typeInstance);
                }
            }
        }

        private void ForeachCall(Action<BuildPipelineCallbackBase> callback)
        {
            foreach (var node in _buildPipelineCallback)
            {
                callback?.Invoke(node);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log($"[BuildPipelineCore] OnPreprocessBuild");
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPreprocessBuild(report); });
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            Debug.Log($"[BuildPipelineCore] OnProcessScene");
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnProcessScene(scene, report); });
        }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            Debug.Log($"[BuildPipelineCore] OnFilterAssemblies");
            CheckPipelineCallback(null);

            ForeachCall(node => { assemblies = node.OnFilterAssemblies(buildOptions, assemblies); });

            return assemblies;
        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            Debug.Log($"[BuildPipelineCore] OnPostBuildPlayerScriptDLLs");
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPostBuildPlayerScriptDLLs(report); });
        }
        
        public void OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {
            Debug.Log($"[BuildPipelineCore] OnBeforeConvertRun");
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnBeforeConvertRun(report, data); });
        }

#if UNITY_ANDROID
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            Debug.Log($"[BuildPipelineCore] OnPostGenerateGradleAndroidProject");
            CheckPipelineCallback(null);

            ForeachCall(node => { node.OnPostGenerateGradleAndroidProject(path); });
        }
#endif

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log($"[BuildPipelineCore] OnPostprocessBuild");
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPostprocessBuild(report); });

#if UNITY_2021_3_OR_NEWER
            OnBeforeConvertRun(report, new Il2CppBuildPipelineData(EditorUserBuildSettings.activeBuildTarget, ""));
#endif
        }
    }
#endif

}
