using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class MonthCardAwardShowViewModel : ViewModelBase
    {
        
		 [AutoNotify] private string cardPath;
		 [AutoNotify] private ObservableList<MonthCardAwardShowItemPropertyViewModel> effectDesList = new();
		 [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
		 private MonthlyVipMainCfg cfg;
		 private bool isTodayAward;
		 private List<Resource> tempRewards;
		 [AutoNotify] private bool cardEffect;
		 [AutoNotify] private bool cardEffectPlus;
		 [AutoNotify] private bool cardEffectPermanent;
		 
		 private MonthCardData Data => DataCenter.monthCardData; 
        [Preserve]
        public MonthCardAwardShowViewModel(MonthlyVipMainCfg mCfg,List<Resource> resources,bool mIsTodayAward = false)
        {
	        cfg = mCfg;
	        isTodayAward = mIsTodayAward;
	        tempRewards = resources;
	        InitAward();
	        for (int i = 0; i < cfg.EffectId.Count; i++)
	        {
		        var effectId = cfg.EffectId[i];
		        var effectCfg = ConfigCenter.MonthlyVipEffectLanguageCfgColl.GetDataById(effectId);
		        var effectCfg2 = ConfigCenter.MonthlyVipEffectCfgColl.GetDataById(effectId);
		        
		        var desTemplate = Data.OutPutEffectDes(effectCfg.Dec, effectCfg2.Value);
		        EffectDesList.Add(new MonthCardAwardShowItemPropertyViewModel(desTemplate,i));
	        }
	        bool isMonth = cfg.Id == (int)MonthType.MonthCard;
	        bool isPlus = cfg.Id == (int)MonthType.PrivilegeMonthCard;
	        bool isPermanent = cfg.Id == (int)MonthType.PermanentCard;
	        if (isMonth)
		        CardPath = "monthly[monthly_card_3]"; 
	        else if (isPlus)
		        CardPath = "monthly[monthly_card_4]";
	        else if (isPermanent)
		        CardPath = "monthly[monthly_card_5]";
	        
	        CardEffect = isMonth;
	        CardEffectPlus = isPlus;
	        CardEffectPermanent = isPermanent;
        }


        [Command]
        private void OnClickClickToClose()
        {
	        UIManager.Instance.CloseDialog<MonthCardAwardShowView>();
        }

        private void InitAward()
        {
	        for (int i = 0; i < tempRewards.Count; i++)
	        {
		        var cellModel = CellItemBaseViewModel.Create(tempRewards[i]);
		        cellModel.SizeIcon = Vector2.one * 100;
		        cellModel.SizeBg = Vector2.one * 120;
		        AwardScrollviewList.Add(cellModel);
	        }
        }
    }
}