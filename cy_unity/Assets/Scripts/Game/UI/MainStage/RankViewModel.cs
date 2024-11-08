using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.MainUi;
using UnityEngine;
using Object = System.Object;


namespace DH.Game.ViewModels
{
    public partial class RankViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<RankCellViewViewModel> scrollviewList = new();
		[AutoNotify] private RankCellViewViewModel selfRankCellViewVm;
		[AutoNotify] private string maxLevelTitleStr;
	    [AutoNotify] private ObservableList<SubTitleBtnCellViewModel> subTitleViewList = new();
	    [AutoNotify] private ObservableDictionary<int, RankTopCellViewModel> topRankDic = new();
	    [AutoNotify] private ERankPageType rankPageType = ERankPageType.MainStageRankTypeGlobal;
	    [AutoNotify] private bool isShowTopInfo;
	    [AutoNotify] private ERankType curRankType = ERankType.RankItemMainStage;
	    [AutoNotify] private CommonTopViewModel commonTopVm;
	    [AutoNotify] private BottomComponentViewModel bottomComponentVm;
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opScrollViewList = new();
	    [AutoNotify] private bool isNewcomer;
	    [AutoNotify] private bool isShowRuleBtn;
	    public Func<object, object> GetTopRankCellVmCallback => GetTopRankVmCallbackByIndex;
	    
	    private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickNewcomerInfoBtn;
	    public ICommand OnClickIconBtn => clickNewcomerInfoBtn;
	    
        [Preserve]
        public RankViewModel(ERankType rankType = ERankType.RankItemMainStage)
        {
	        List<int> list = new List<int>() {(int)GameConst.ItemIdCode.EnergyDrink, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
	        CommonTopVm = new(list, OnClickResItem);
	        MainUiManager.Instance.CurRankType = rankType;
	        InitRankInfo(new List<RankMember>());
	        InitOpBtnList();
	        switch (rankType)
	        {
		        case ERankType.RankItemMainStage:LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips18); break;
		        case ERankType.RankItemEndless:LocalizeHelper.GetGlobal(GlobalLanguageId.endless_16); break;
		        case ERankType.RankItemSecret :LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_04); break;
	        }
	        IsShowRuleBtn = (int)rankType == 3;
	        
	        InitSubTitleBtnList();
	        OnClickOpBtnCallback(rankType);
	        OnClickSubTitleBtnCallback(rankPageType);
	        BottomComponentVm = new(OnClickCloseBtn, opScrollViewList);
	        IsNewcomer = MainUiManager.Instance.IsNewcomer();
	        clickNewcomerInfoBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickNewcomerInfoBtn);
        }

        protected override void OnDispose()
        {
	        CommonTopVm.Dispose();
	        foreach (var item in TopRankDic)
	        {
		        item.Value.Dispose();
	        }
	        // TopRankDic.Clear();
	        foreach (var item in ScrollviewList)
	        {
		        item.Dispose();
	        }

	        foreach (var item in subTitleViewList)
	        {
		        item.Dispose();
	        }
	        base.OnDispose();
        }

        private void InitOpBtnList()
        {
	        OpScrollViewList.Clear();
	        foreach (ERankType type in Enum.GetValues(typeof(ERankType)))
	        {
		        if ((ERankType.RankItemEndless == type && !MainUiManager.Instance.CheckFunctionIsUnlockByMainUITab((int)ETabType.TabTypeActivity)) ||
		            ( MainUiManager.Instance.CurRankType != ERankType.RankItemEndless && MainUiManager.Instance.IsNewcomer())) continue;
		        if(ERankType.RankItemSecret == type &&!MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionSecret)) continue;
		        BottomOpCellData curData = new();
		        switch (type)
		        {
			        case ERankType.RankItemMainStage:
			        {
				        curData.ChooseBgPath = "common[common_panel_10]";
				        // curData.ChooseIconPath = "mainui[icon_home_1]";
				        curData.ChooseIconPath = "ranking[ranking_icon_6]";
				        curData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.ProPicture08);
				        curData.OnClickCallback = OnClickOpBtnCallback;
				        curData.OpType = type;
			        } break;
			        case ERankType.RankItemEndless:
			        {
				        curData.ChooseBgPath = "common[common_panel_10]";
				        curData.ChooseIconPath = "ranking[ranking_icon_5]";
				        curData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.endless_01);
				        curData.OnClickCallback = OnClickOpBtnCallback;
				        curData.OpType = type; 
			        } break;
			        case ERankType.RankItemSecret:
			        {
				        
				      
				        
				        curData.ChooseBgPath = "common[common_panel_10]";
				        curData.ChooseIconPath = "ranking[ranking_icon_7]";
				        curData.OpName = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_01);
				        curData.OnClickCallback = OnClickOpBtnCallback;
				        curData.OpType = type; 
			        } break;
			        default:
			        {
				        DHLog.Error($"InitOpBtnList 未处理的枚举类型 {type} 请及时处理");
			        } break;
		        }
		        BottomOpCellItemViewModel tempVm = new(curData);
		        OpScrollViewList.Add(tempVm);
	        }
        }

        private void InitSubTitleBtnList()
        {
	        SubTitleViewList.Clear();
	        foreach (ERankPageType type in Enum.GetValues(typeof(ERankPageType)))
	        {
		        SubTitleBtnCellViewModel tempVm = new(type,OnClickSubTitleBtnCallback);
		        SubTitleViewList.Add(tempVm);
	        }
        }

        private void InitRankInfo(List<RankMember> rankList)
        {
	        foreach (var item in TopRankDic)
	        {
		        item.Value.Dispose();
	        }
	        TopRankDic.Clear();
	        foreach (var item in ScrollviewList)
	        {
		        item.Dispose();
	        }
	        ScrollviewList.Clear();
	        DHLog.Debug($"count is rankList.count {rankList.Count}");
	        rankList.Sort((a,b)=>a.Rank.CompareTo(b.Rank));
	        for (int i = 0; i < 3; i++)
	        {
		        if (rankList.Count > i)
		        {
			        RankTopCellViewModel vm = new(rankList[i], OnClickPlayerInfo);
			        TopRankDic.Add(i, vm);
		        }
		        else
		        {
			        RankTopCellViewModel vm = new(null, null);
			        TopRankDic.Add(i, vm);   
		        }
	        }
	        // 初始化列表
	        for (int i = 3; i < rankList.Count; i++)
	        {
		        RankCellViewViewModel vm = new(rankList[i], OnClickPlayerInfo);
		        ScrollviewList.Add(vm);
	        }

	        IsShowTopInfo = true;
        }
        private void  OnClickResItem(ResourceData data, int tag)
        {
	        // 点击资源
	        DHLog.Debug($"点击了资源 itemId is {data.Id}");
        }

        [Command]
        private void OnClickRuleBtn()
        {
	        var content1 = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips11);
	        CommonRuleData rule1 = new("", content1, false);
	        var title = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13);
	        CommonRuleViewModel tempVm = new (title,content1);
	        tempVm.BgSize = new (950, 950);
	        UIManager.Instance.OpenDialog<CommonRuleView>(tempVm).Forget();
        }

		[Command]
		private void OnClickCloseBtn()
		{
			// UIManager.Instance.OpenDialog<MainUiView,MainUiViewModel>().Forget();
			UIManager.Instance.CloseDialog<RankView>();
		}
		
		private void OnClickNewcomerInfoBtn(Tuple<Vector3, Vector3> info)
		{
			var time = MainUiManager.Instance.NewcomerTimes();
			if (time == 0)return;
			string des = ServerTime.Instance.SecondsDHms(time) + LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips34);
			CommonRuleViewModel tempVm = new (LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13),des);
			UIManager.Instance.OpenDialog<CommonRuleView>(tempVm).Forget();
			// UIHelper.OpenCommonTips (LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title),
			// 	des,info.Item1,info.Item2);
		}
		private void OnClickOpBtnCallback(Object type)
		{
			foreach (var item in OpScrollViewList)
			{
				item.CurOpCellData.IsChoose = (int)item.CurOpCellData.OpType == (int)type;
			}
			CurRankType = (ERankType)type;
			MainUiManager.Instance.CurRankType = CurRankType;

			if (CurRankType == ERankType.RankItemEndless)
			{
				MaxLevelTitleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.endless_16);
			}
			else if (CurRankType == ERankType.RankItemSecret)
			{
				var itemCfg = ConfigCenter.ItemLanguageCfgColl.GetDataById((int)GameConst.ItemIdCode.SecretScore);
				if (itemCfg != null)
				{
					MaxLevelTitleStr = itemCfg.Name;
				}
				else
				{
					MaxLevelTitleStr = "";
				}
			}
			else
			{
				MaxLevelTitleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips18);
			}
			OnClickSubTitleBtnCallback(ERankPageType.MainStageRankTypeGlobal);
		}

		private void OnClickSubTitleBtnCallback(ERankPageType pageType)
		{
			foreach (var item in SubTitleViewList)
			{
				item.UpdatePanel(pageType);
			}
			
			RequestRankInfo(pageType).Forget();
		}

		private async UniTaskVoid RequestRankInfo(ERankPageType pageType)
		{
			var data = new ReqRankOp();
			data.Op = (int)CurRankType;
			data.Param = (int)pageType;
			var result = await GameNetworkManager.Instance.SendAsync<RspRankOp>(data);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				DHLog.Debug(" 请求 排行榜 失败");
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				// 清楚数据
				ClearRankInfo();
				// 这里给提示
				return;
			}
			InitRankInfo(result.rsp.Top.ToList());
			UpdateSelfRankInfo(result.rsp.Score, result.rsp.Rank);
		}

		private void ClearRankInfo()
		{
			foreach (var item in ScrollviewList)
			{
				item.Dispose();
			}

			foreach (var item in topRankDic)
			{
				item.Value.Dispose();
			}
			topRankDic.Clear();
			for (int i = 0; i < 3; i++)
			{
				RankTopCellViewModel vm = new(null, null);
				TopRankDic.Add(i, vm); 
			}
			
			ScrollviewList.Clear();
			SelfRankCellViewVm.Dispose();
			SelfRankCellViewVm = null;
		}

		private async void OnClickPlayerInfo(RankMember info)
		{
			ReqRankDigest data = new ReqRankDigest();
			data.RoleId = info.RoleId;
			var result = await GameNetworkManager.Instance.SendAsync<RspRankDigest>(data);
			if (result.rsp == null ||result.rsp.Status != 0)
			{
				DHLog.Debug(" 请求 玩家信息 失败");
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				// 这里给提示
				return;
			}
			DHLog.Debug($" 请求 玩家信息 {info.RoleId}");
			RankPlayerInfoViewModel tempVm = new (info, result.rsp.Data);
			UIManager.Instance.OpenDialog<RankPlayerInfoView>(tempVm).Forget();
			
		}

		private void UpdateSelfRankInfo(int score,long rank)
		{

			if (SelfRankCellViewVm != null)
			{
				SelfRankCellViewVm.Dispose();
				SelfRankCellViewVm = null;
			}

			RankMember info = new()
			{
				RoleId = DataCenter.charcaterData.Digest.RoleId,
				Name = DataCenter.charcaterData.Digest.Name,
				Logo = DataCenter.charcaterData.Digest.HeadId,
				HeadFrame = DataCenter.charcaterData.Digest.HeadFrame,
				Score = score,
				Rank = (int)rank,
				Stage = DataCenter.mainStageData.IsPassChapter(DataCenter.mainStageData.CurrChapter)?DataCenter.mainStageData.CurrChapter:DataCenter.mainStageData.CurrChapter-1,
				VipStatus = DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname) ? 3 : 0,
			};
			// 更新自己的排行榜信息 
			SelfRankCellViewVm = new(info, OnClickPlayerInfo);
		}
		
		private object GetTopRankVmCallbackByIndex(object index)
		{
			if (TopRankDic.TryGetValue((int)index, out RankTopCellViewModel ret))
			{
				return ret;
			}
			return null;
		}
    }
}