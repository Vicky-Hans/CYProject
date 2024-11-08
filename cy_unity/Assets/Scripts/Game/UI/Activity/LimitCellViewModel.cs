using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class LimitCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private MagicDrawRewardCellViewModel magicDrawRewardCellVm;
		[AutoNotify] private CellItemBaseViewModel cellBaseViewVm;
		[AutoNotify] private string nameTextStr;
		[AutoNotify] private string ratioTextStr;
		[AutoNotify] private bool isShowCellItem;

		private PrayJackpotCfg curCfg;
        [Preserve]
        public LimitCellViewModel(PrayJackpotCfg cfg,string nameStr, string ratioStr)
        {
	        curCfg = cfg;
	        NameTextStr = nameStr;
	        RatioTextStr = ratioStr;
	        MagicDrawRewardCellVm = new MagicDrawRewardCellViewModel(cfg);
	        MagicDrawRewardCellVm.IsGray = false;
	        MagicDrawRewardCellVm.IsShowBg = true;
	        isShowCellItem = false;
        }

        public LimitCellViewModel(Reward reward, string nameStr, string ratioStr)
        {
	        curCfg = null;
	        NameTextStr = nameStr;
	        RatioTextStr = ratioStr;
	        cellBaseViewVm = CellItemBaseViewModel.Create(reward,ECellItemSizeType.Size180X150);
	        isShowCellItem = true;
        }
    }
}