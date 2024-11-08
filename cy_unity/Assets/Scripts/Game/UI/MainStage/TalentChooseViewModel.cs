using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class TalentChooseViewModel : ViewModelBase
    {
        
		[AutoNotify] private string pageTextStr;
		[AutoNotify]  private ObservableList<TalentCellItemViewModel> chooseScrollViewList = new();
	    [AutoNotify] private ObservableList<TalentChooseItemViewModel> talentScrollViewList = new();
		[AutoNotify] private bool isShowAdIcon;
		[AutoNotify] private string opBtnTextStr;
		[AutoNotify] private string leftCountTextStr;
		[AutoNotify] private bool isCanClickRefreshBtn;
		[AutoNotify] private bool isShowRefreshBtn;
		[AutoNotify] private bool isShowCanClose;
		[AutoNotify] private bool isShowEffectNode;
		public CommonAdvIconViewModel CommonAdvVm;
		private Action<int,Action<List<int>>>  selectCallback; 
        [Preserve]
        public TalentChooseViewModel(List<int> talentList, Action<int,Action<List<int>>>  selectedCallback)
        {
	        selectCallback = selectedCallback;
	        RefreshTalentList(talentList);
	        // 检查是否可以刷新
	        IsCanClickRefreshBtn = GameDataManager.Instance.TalentAdReFreshCount > 0;
	        CheckAndRefreshAdRefresh();
	        CommonAdvVm = new CommonAdvIconViewModel();
	        if (GameDataManager.Instance.CheckIsCanRandomTalent())
	        {
		        TalentCellItemViewModel tempVm = new(-1,0);
		        tempVm.SetSize(Vector2.one * 106);
		        ChooseScrollViewList.Add(tempVm);
		        var totalCout = GameDataManager.Instance.GetCanChooseTalentCount();
		        PageTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips33,totalCout);
	        }
	        else
	        {
		        PageTextStr = "";
	        }
	        IsShowEffectNode = false;
	        // IsShowCanClose = !GameDataManager.Instance.CheckIsCanRandomTalent();
        }

        [Command]
        private void OnClickRefreshBtn()
        {
	        UIHelper.ShowRewardAds(() =>
	        {
		        GameManager.Instance.RequestRefreshTalent(ERefreshType.RefreshTypeAd, GameDataManager.Instance.ChooseTalentCount,
			        (talentList) =>
			        {
				        UpdateTalentList(talentList);
				        GameDataManager.Instance.TalentAdReFreshCount -= 1;
				        CheckAndRefreshAdRefresh();
			        }).Forget();
	        });
	        // 检查是否可以刷新
	     
        }
        [Command]
		private void OnClickCloseBtn()
		{
			GameTime.Instance.Pause = GameDataManager.Instance.WaveEnd;
			UIManager.Instance.CloseDialog<TalentChooseView>();	
			selectCallback?.Invoke(-1,null);
		}

		private async void OnChooseTalent(int index, int talentId)
		{
			// 这里还可以做选中后的动画
			ReqBattleTalentSelect req = new();
			req.Uid = GameDataManager.Instance.Uid;
			req.Index = index;
			var result = await GameNetworkManager.Instance.SendAsync<RspBattleTalentSelect>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return;
			}
			// 这里选择了天赋
			foreach (var item in TalentScrollViewList)
			{
				item.IsCanOp = false;
			}
			// 加钱
			GameDataManager.Instance.GameCoin += result.rsp.GotMoney;
			GameDataManager.Instance.EquipRefreshMoney = result.rsp.EquipRefreshMoney;
			// 清理未选择的天赋
			GameDataManager.Instance.ClearUnChooseTalent();
			// 选择的天赋
			for (var i = 0; i < result.rsp.Talent.Count; i++)
			{
				GameDataManager.Instance.AddChooseTalent(result.rsp.Talent[i]);
				BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddTalent(result.rsp.Talent[i]);
			}
			// 处理特殊天赋
			GameManager.Instance.DealSpecialTalent(talentId);;
			
			if (result.rsp.Talent.Count != 1)
			{
				IsShowEffectNode = true;
				DoAllInAction();
			}
			else
			{
				DoOtherChooseAction(index);
				// IsShowEffectNode = true;
				// DoAllInAction();
			}

			await UniTask.Delay(600);
			selectCallback?.Invoke(talentId,UpdateTalentList);
		}


		private void DoOtherChooseAction(int index)
		{
			foreach (var item in TalentScrollViewList)
			{
				item.DoChooseAction(index);
			}
		}
		private void DoAllInAction()
		{
			for (int i = 0; i < TalentScrollViewList.Count; i++)
			{
				TalentScrollViewList[i].DoChooseAction(i);
			}
		}
		private void RefreshTalentList(List<int> talentList)
		{
			foreach (var item in TalentScrollViewList)
			{
				item.Dispose();
			}
			DHLog.Debug($"更新天赋列表 {talentList.Count}");
			TalentScrollViewList.Clear();
			for (int i = 0; i < talentList.Count; i++)
			{
				TalentChooseItemViewModel tempVm = new(i,talentList[i], OnChooseTalent);
				TalentScrollViewList.Add(tempVm);    
			}
			UpdateChooseTalentList();
		}

		private void UpdateTalentList(List<int> talentList)
		{
			foreach (var item in TalentScrollViewList)
			{
				item.Dispose();
			}

			IsShowEffectNode = false;
			// DHLog.Debug($"更新天赋列表 {talentList.Count}");
			TalentScrollViewList.Clear();
			for (int i = 0; i < talentList.Count; i++)
			{
				TalentChooseItemViewModel tempVm = new(i,talentList[i], OnChooseTalent);
				TalentScrollViewList.Add(tempVm);    
			}
			UpdateChooseTalentList();
			
			var totalCout = GameDataManager.Instance.GetCanChooseTalentCount();
			PageTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips33,totalCout);
		}

		private void UpdateChooseTalentList()
		{
			foreach (var item in ChooseScrollViewList)
			{
				item.Dispose();
			}
			ChooseScrollViewList.Clear();
			var chooseTalentDic = GameDataManager.Instance.GetChooseTalent(ETalentType.TalentTypeNone);
			foreach (var item in chooseTalentDic)
			{
				TalentCellItemViewModel tempVm = new(item.Key,item.Value);
				tempVm.SetSize(Vector2.one * 106);
				ChooseScrollViewList.Add(tempVm);
			}
			
		}
		
		public void CheckAndRefreshAdRefresh()
		{
			var defCfg = ConfigCenter.DefinesCfgColl.GetDataById(305);
			var adTotalCount = 0;
			adTotalCount = defCfg.Content[0];
			IsShowAdIcon = true;
			var curCount = GameDataManager.Instance.TalentAdReFreshCount;
			IsCanClickRefreshBtn = curCount > 0;
			var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips39);
			LeftCountTextStr = $"<color=#FFF8E4>{str}：</color><color=#64D32E>{curCount}/{adTotalCount}</color>";	
			IsCanClickRefreshBtn = curCount > 0;
		}
		protected override void OnDispose()
		{
			CommonAdvVm?.Dispose();
			base.OnDispose();
		}
    }
}