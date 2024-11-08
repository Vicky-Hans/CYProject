using Cysharp.Threading.Tasks;
using DH.Launch;
using DHHybridCLR.Scripts;

public class LoadMetadataTask : TaskBase
{
    public override async UniTask Start(TaskManager taskMgr, object userData)
    {
        base.Start(taskMgr, userData);
        
        LoadMetadataForAotAssembly();
    }

#if UNITY_WEBGL || WECHAT_MINI

    private void LoadMetadataForAotAssembly()
    {
        IsDone = true;
        return;
    }

#else

    private void LoadMetadataForAotAssembly()
    {
        if (!StartupEntry.Instance.StartupConfig.EnableHybridCLR)
        {
            IsDone = true;
            return;
        }

#if !UNITY_EDITOR
        var task = GameRoot.Instance.LoadMetadataForAotAssembly("startupBase.bytes", "startupBase");
        StartupEntry.Instance.StartCoroutineTask(task, () => IsDone = true, "[Startup]LoadMetadataForAotAssembly");
#else
        StartupEntry.Instance.StartCoroutineTask(null, () => IsDone = true, "[Startup]LoadMetadataForAotAssembly");
#endif
    }
    
#endif
}
