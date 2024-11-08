using Cysharp.Threading.Tasks;
using DH.Launch;
using DHHybridCLR.Scripts;

public class LoadPreDllListTask : TaskBase
{
    private string loadTargetDllName;
    
    public override async UniTask Start(TaskManager taskMgr, object userData)
    {
        base.Start(taskMgr, userData);

        if (StartupEntry.Instance.StartupConfig.EnableHybridCLR)
        {
            PreloadDll();
        }
        else
        {
            OnComplete();
        }
    }

    public void SetLoadDllName(string dllName)
    {
        loadTargetDllName = dllName;
    }

    public void PreloadDll()
    {
        var task = GameRoot.Instance.LoadGameDll(loadTargetDllName, null);
        StartupEntry.Instance.StartCoroutineTask(task, OnComplete, $"[Startup]PreloadDll {loadTargetDllName}.dll");
    }

    private void OnComplete()
    {
        SetResult(UserData);
        IsDone = true;
    }
}
