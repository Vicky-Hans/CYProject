using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.Login;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DHFramework.Localization;
using UnityEngine;

namespace DH.Game
{
    [ProcedureDeep(2)]
    public class LoginProcedure : ProcedureBase
    {
        protected override async UniTask Enter()
        {
            // Localization.ChangeLanguage("en", null);
            GameNetworkManager.Instance.Init();
            LoginManager.Instance.Init();
            GlobalSchedule.Instance.Clear();

            // 第一次进入游戏时为解决切换黑屏问题
            if (UIManager.Instance.GetDialog<StartGameMenu>()) return;
        }

        protected override void DeActive()
        {
            UIManager.Instance.CloseDialog<StartGameMenu>();
        }

        protected override async UniTask Active()
        {
            var view = await UIManager.Instance.OpenDialog<StartGameMenu, StartGameViewModel>();
        }

        protected override void Exit()
        {
            // int enterCount = PlayerPrefs.GetInt(GameConst.EnterGameCount, 0);
            // if (enterCount == 0)
            // {
            //     DelayExit().Forget();
            // }
            // else
            // {
                GameNetworkManager.Instance.Destroy();
                LoginManager.Instance.Destroy();
                GlobalSchedule.Instance.Destroy();

                UIManager.Instance.CloseDialog<StartGameMenu>();
            // }

            
        }

        private async  UniTaskVoid DelayExit()
        {
            await UniTask.Delay(5000);
            GameNetworkManager.Instance.Destroy();
            LoginManager.Instance.Destroy();
            GlobalSchedule.Instance.Destroy();

            UIManager.Instance.CloseDialog<StartGameMenu>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (ProcedureManager.Instance.IsCurrent(ProcedureConfigKey))
            {
                LoginManager.Instance.Update(elapseSeconds, realElapseSeconds);
            }
            GlobalSchedule.Instance.Update(elapseSeconds, realElapseSeconds);
        }

        public override string GetSceneConfig()
        {
            return "Scenes/Login";
        }
    }
}