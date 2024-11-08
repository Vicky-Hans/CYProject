using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class PassportsBuyConfirmViewModel : ViewModelBase
    {
        
		[AutoNotify] private ItemPriceNodeModel btnPriceNodeVm;
		private readonly List<int> unlockCostCfgIdList = new List<int>() {411, 412};
		private readonly Action onClickConfirmCallback;
		private EPassPortType curPassportType;
        [Preserve]
        public PassportsBuyConfirmViewModel(Action confirmCallback, EPassPortType type)
        {
	        onClickConfirmCallback	= confirmCallback;
	        curPassportType	= type;
	        
	        var tempId = unlockCostCfgIdList[(int)type - 1];
	        var cfg = ConfigCenter.DefinesCfgColl.GetDataById(tempId);
	        if (cfg == null || cfg.Reward == null || cfg.Reward.Count <= 0)
	        {
		        DHLog.Error($" 没有解锁消耗配置， 请检查配置 DefinesCfg id  {tempId}");
		        return;
	        }
	        BtnPriceNodeVm = new ItemPriceNodeModel(cfg.Reward[0],true);
	        
        }

        
        
        protected override void OnDispose()
        {
	        BtnPriceNodeVm.Dispose();
	        base.OnDispose();
        }

        [Command]
        private void OnClickConfirmBtn()
        {
	        var tempId = unlockCostCfgIdList[(int)curPassportType - 1];
	        var costCfg = ConfigCenter.DefinesCfgColl.GetDataById(tempId);
	        if (costCfg == null || costCfg.Reward == null || costCfg.Reward.Count <= 0)
	        {
		        DHLog.Error($" 没有解锁消耗配置， 请检查配置 DefinesCfg id  {tempId}");
		        return;
	        }
	        // 检查资产是否足够
	        if (!Lodash.CheckRewardIsEnough(costCfg.Reward))
	        {
		        ToastManager.ShowLanguage(GlobalLanguageId.Equip_15);
		        return;
	        }
	        
	        onClickConfirmCallback?.Invoke();
	        OnClickCloseBtn();
        }

		[Command]
		private void OnClickCloseBtn()
		{
			UIManager.Instance.CloseDialog<PassportsBuyConfirmView>();
		}

        
    }
}