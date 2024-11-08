using Cysharp.Threading.Tasks;
using DHFramework;

namespace DH.Launch
{
    public class TaskBase
    {
        public int State { get; set; }
        public int Weight { get; set; }
        public bool IsDone { get; set; }
        public float Duration { get; set; }
        public object UserData { get; set; }
        public TaskManager Owner;
        
        private object resultData;
        
        public async UniTask Init(int state, int weight)
        {
            State = state;
            Weight = weight;
        }

        public virtual async UniTask Start(TaskManager taskMgr, object userData)
        {
            DHLog.Debug($"[startup]进入任务：{this}");
            
            Reset();
            
            this.Owner = taskMgr;
            UserData = userData;
        }

        public object GetResult()
        {
            return resultData;
        }
        
        public virtual async UniTask UpdateTask(float elapseSeconds, float realElapseSeconds)
        {
            Duration += realElapseSeconds;
        }

        protected void SetResult(object result)
        {
            resultData = result;
        }

        protected void ReStartTask()
        {
            Start(Owner, UserData).Forget();
        }

        private void Reset()
        {
            IsDone = false;
            Duration = 0f;
            UserData = null;
            resultData = null;
            Owner = null;
        }
    }
}
