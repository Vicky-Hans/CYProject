using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.ViewModels;

namespace DH.Game.ViewModels
{

    public partial class PlayerInfoCostItemViewModel : ViewModelBase
    {

        [AutoNotify] private string iconPath;
        [AutoNotify] private int count;
        public PlayerInfoCostItemViewModel(Reward reward)
        {
            IconPath = DataCenter.itemsData.GetItemIconPathById(reward.Id);
            Count = (int)reward.Count;

        }

    }
}
