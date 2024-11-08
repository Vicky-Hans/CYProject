using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class LuckDrawCellViewModel : ViewModelBase
    {
		[AutoNotify] private bool isShowChooseNode;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
        private int curIndex;
        private RandomReward curRandomReward;
        [Preserve]
        public LuckDrawCellViewModel(int index, RandomReward randomReward, bool isShowCount)
        {
            curIndex = index;
            curRandomReward = randomReward;
            Reward reward = new(curRandomReward.Type, curRandomReward.Id, curRandomReward.Count);
            CellItemBaseViewVm = new CellItemBaseViewModel(curRandomReward.Id, (int)curRandomReward.Type, curRandomReward.Count);
            CellItemBaseViewVm.SetSize(ECellItemSizeType.Size180X150);
            CellItemBaseViewVm.IsShowNum = isShowCount;
            IsShowChooseNode = false;
        }
    }
}