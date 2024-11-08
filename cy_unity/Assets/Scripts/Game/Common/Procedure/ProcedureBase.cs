
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using SceneManager = DH.UIFramework.SceneManager;

namespace DH.Game
{
    public class ProcedureBase : IEquatable<ProcedureBase>
    {
        private string procedureConfigKey;
        public string ProcedureConfigKey => procedureConfigKey;
        private TransitionState progress = TransitionState.None;
        private ProcedureState curState = ProcedureState.None;
        private float sceneLoadingProgress;

        public TransitionState Progress
        {
            get => progress;
            set => progress = value;
        }
        
        public ProcedureBase()
        {
            var curType = GetType();
            procedureConfigKey = curType.Name;
        }

        public async UniTask ChangeState(ProcedureState state, Action<TransitionState> onStateChange = null)
        {
            if (curState == state)
                return;

            progress = TransitionState.None;
            curState = state;
            if (state == ProcedureState.Enter)
            {
                await EnterTransition(onStateChange);
            }
            else if (state == ProcedureState.Active)
            {
                await ActiveTransition(onStateChange);
            }
            else if (state == ProcedureState.DeActive)
            {
                progress = TransitionState.UnLoad;
                DoInactive();
                progress = TransitionState.End;
            }
            else if (state == ProcedureState.Exit)
            {
                progress = TransitionState.UnLoad;
                DoInactive();
                DoExit();
                progress = TransitionState.End;
            }
        }

        private async UniTask EnterTransition(Action<TransitionState> onStateChange = null)
        {
            progress = TransitionState.LoadScene;
            onStateChange?.Invoke(progress);
            await DoLoadScene();
            await BeforeEnter();
            progress = TransitionState.Enter;
            onStateChange?.Invoke(progress);
            await DoEnter();
            await DoActive();
            progress = TransitionState.End;
            onStateChange?.Invoke(progress);
        }

        private async UniTask ActiveTransition(Action<TransitionState> onStateChange = null)
        {
            progress = TransitionState.LoadScene;
            onStateChange?.Invoke(progress);
            await DoLoadScene();
            progress = TransitionState.Enter;
            onStateChange?.Invoke(progress);
            await DoActive();
            progress = TransitionState.End;
            onStateChange?.Invoke(progress);
        }

        public async UniTask DoLoadScene()
        {
            var tcs = new UniTaskCompletionSource();
            var sceneAddress = GetSceneConfig();
            if (!string.IsNullOrEmpty(sceneAddress))
            {
                SceneManager.Instance.LoadScene(sceneAddress, LoadSceneMode.Single,
                    delegate(bool b) { tcs.TrySetResult(); }, delegate(float f) { sceneLoadingProgress = f; });
            }
            else
            {
                tcs.TrySetResult();
            }

            await tcs.Task;
        }


        public async UniTask DoEnter()
        {
            await Enter();
        }

        public async UniTask DoActive()
        {
            await Active();
        }

        public void DoInactive()
        {
            DeActive();
        }

        public void DoExit()
        {
            Exit();
        }

        public TransitionState GetProgress()
        {
            return progress;
        }

        public virtual string GetSceneConfig()
        {
            return "";
        }

        protected virtual async UniTask BeforeEnter()
        {
            
        }
        
        protected virtual async UniTask Enter()
        {

        }

        protected virtual async UniTask Active()
        {
            await UniTask.CompletedTask;
        }

        protected virtual void DeActive()
        {

        }

        protected virtual void Exit()
        {

        }


        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        public bool Equals(ProcedureBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return procedureConfigKey == other.procedureConfigKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcedureBase)obj);
        }

        public override int GetHashCode()
        {
            return (procedureConfigKey != null ? procedureConfigKey.GetHashCode() : 0);
        }
    }
}