using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.Config;
using DH.Data;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class ChallengeOpenViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private ObservableList<LuckDrawCellViewModel> challengeAwardList = new();//每日挑战奖励数据
        [Preserve]
        public ChallengeOpenViewModel()
        {
            var dailyFightAwardsCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_09);
            if (dailyFightAwardsCfg!= null && dailyFightAwardsCfg.Content.Count > 0)
            {
                for (var i = 0; i < dailyFightAwardsCfg.Content.Count; i++)
                {
                    var data = new RandomReward(RewardType.Item, dailyFightAwardsCfg.Content[i], 1, 1);
                    var vm = new LuckDrawCellViewModel(i, data, false);
                    ChallengeAwardList.Add(vm);
                }
            }
        }
        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<ChallengeOpenView>();
            MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
            UIManager.Instance.OpenDialog<ChallengeActivityView, ChallengeActivityViewModel>().Forget();
        }
    }
}