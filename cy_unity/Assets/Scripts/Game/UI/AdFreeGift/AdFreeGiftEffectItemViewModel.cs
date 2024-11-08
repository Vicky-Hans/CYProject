using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class AdFreeGiftEffectItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string effectTextStr;
		[AutoNotify] private string adFreeDes;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();

	    [AutoNotify] private MonthCardEffectType effectType;
	    
	    [AutoNotify] private CommonPlayerNameViewModel playerNameVm;
	    
	    private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickIconBtn;
	    public ICommand OnClickIconBtn => clickIconBtn;
	    
	    [Preserve]
        public AdFreeGiftEffectItemViewModel(MonthCardEffectType effectType,string des)
        {
	        EffectType = effectType;
	        EffectTextStr = des;
	        if (EffectType == MonthCardEffectType.GoldenNickname)
	        {
		        playerNameVm = new CommonPlayerNameViewModel(DataCenter.charcaterData.Digest.Name,UIHelper.HexColorStrToColor("#412023"),true);
	        }
	        if (EffectType == MonthCardEffectType.AdRreeReward || EffectType == MonthCardEffectType.ADFreeForever)
	        {
		        AdFreeDes = des;
	        }
	        if (EffectType == MonthCardEffectType.DedicatedAvatar)
	        {
		        AwardScrollviewList.Clear();
		        var cfg = ConfigCenter.MonthlyVipMainCfgColl.GetDataById((int)MonthType.PermanentCard);
		        if (cfg.Reward == null)return;
		        for (int i = 0; i < cfg.Reward.Count; i++)
		        {
			        AwardScrollviewList.Add(CellItemBaseViewModel.Create( cfg.Reward[i],ECellItemSizeType.Size90X80));
		        }
	        }
	        
	        clickIconBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickAdInfoButton);
        }
        
		private void OnClickAdInfoButton(Tuple<Vector3, Vector3> info)
		{
			var define = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.MothlyVip_noAD).Reward;
			if (define == null || define.Count == 0) return;
			var des = string.Format(LocalizeHelper.GetGlobal(GlobalLanguageId.MonthlyVip_tips13),UIHelper.GetRewardName(define[0]),define[0].Count);
			UIHelper.OpenCommonTips (LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title),
				des,info.Item1,info.Item2);
		}

        
    }
}