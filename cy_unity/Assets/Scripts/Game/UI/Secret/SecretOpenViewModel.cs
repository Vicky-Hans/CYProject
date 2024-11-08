using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class SecretOpenViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private ObservableList<CellItemBaseViewModel> rewardsScrollviewList = new();

        [Preserve]
        public SecretOpenViewModel()
        {
            InitPanel();
        }

        private void InitPanel()
        {
            rewardsScrollviewList.Clear();
            var rewards = DataCenter.secretData.GetSecretRewardList();
            for (var i = 0; i < rewards.Count; i++)
            {
                var vm = new CellItemBaseViewModel(rewards[i], (int)RewardType.Item, 1, ECellItemSizeType.Size120X100, false, false);
                RewardsScrollviewList.Add(vm);
            }
        }

        [Command]
        private void OnClickOpBtn()
        {
            UIManager.Instance.CloseDialog<SecretOpenView>();
            // MainUiManager.Instance.CurTabType = ETabType.TabTypeActivity;
            UIManager.Instance.OpenDialog<SecretView, SecretViewModel>().Forget();
        }
    }
}