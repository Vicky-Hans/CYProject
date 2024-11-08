using System;
using System.Collections;
using DHFramework;

namespace DH.Launch
{
    /// <summary>
    /// 负责管理启动配置相关
    /// </summary>
    public partial class StartupEntry
    {
        internal void StartCoroutineTask(IEnumerator task, Action completeAction, string taskName = "")
        {
            StartCoroutine(BeginStartCoroutineTask(task, completeAction, taskName));
        }
            
        private IEnumerator BeginStartCoroutineTask(IEnumerator task, Action loadComplete, string taskName = "")
        {
            using (new Benchmark(taskName))
            {
                if (task == null)
                {
                    loadComplete?.Invoke();
                    yield break;
                }
                
                yield return StartCoroutine(task);
                loadComplete?.Invoke();
            }
        }
    }
}
