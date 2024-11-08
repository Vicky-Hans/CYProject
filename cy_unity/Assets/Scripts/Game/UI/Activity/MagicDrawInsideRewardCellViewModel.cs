using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;


namespace DH.Game.ViewModels
{
    public partial class MagicDrawInsideRewardCellViewModel : ViewModelBase
    {
        
		[AutoNotify] private string bgPath;
		[AutoNotify] private string countTextStr;
		[AutoNotify] private MagicDrawRewardCellViewModel magicDrawRewardCellVm;
		[AutoNotify] private bool isShowChooseImg;
		[AutoNotify] private bool isShowOutMask;

		private string[] bgPathArray = new string[] { "wish[wish_turntable_9]",  "wish[wish_turntable_10]" };
		private PrayJackpotCfg curCfg;
        [Preserve]
        public MagicDrawInsideRewardCellViewModel(PrayJackpotCfg cfg,int indx)
        {
	        curCfg = cfg;
	        IsShowChooseImg = false;
	        magicDrawRewardCellVm = new MagicDrawRewardCellViewModel(cfg);
	        BgPath = bgPathArray[indx%2];
	        UpdatePanel();
        }

        public void UpdatePanel()
        {
	        var curCount =  curCfg.Frequency - DataCenter.magicDrawData.GetDrawRecord(curCfg.Id);
	        CountTextStr =curCount.ToString() ;
	        IsShowOutMask = curCount <= 0;
	        magicDrawRewardCellVm.UpdatePanel();
        }

        public void PlayEffect()
        {
	        magicDrawRewardCellVm?.PlayEffect();
        }
    }
}