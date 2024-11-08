using Cysharp.Threading.Tasks;

namespace DH.Launch
{
    public class GameConfig
    {
        public string GameCode;
    }
    
    public class LoadConfigTask : TaskBase
    {
        public override async UniTask Start(TaskManager taskMgr, object userData)
        {
            base.Start(taskMgr, userData);
            ProgressTask();
        }

        public void ProgressTask()
        {
            SetResult(new GameConfig()
            {
                GameCode = StartupEntry.Instance.StartupConfig.GameCode
            });
            
            IsDone = true;
        }
    }
}
