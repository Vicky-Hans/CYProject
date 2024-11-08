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
using DH.UIFramework.Observables;
using DHFramework;
using Game.UI.MainStage;

namespace DH.Game.ViewModels
{
    public partial class SecretViewModel : ViewModelBase
    {
        
		[AutoNotify] private CommonTopViewModel commonTopItemsVm;
	    [AutoNotify] private ObservableList<LevelCellViewModel> levelScrollviewList = new();
		[AutoNotify] private string opBtnTextStr;
		[AutoNotify] private bool isShowCostItem;
		[AutoNotify] private ItemPriceNodeModel costItemViewVm;
		[AutoNotify] private string leftCountTextStr;
	    [AutoNotify] private ObservableList<int> subTitleScrollViewList = new();
		[AutoNotify] private BottomComponentViewModel bottomComponentVm;
		[AutoNotify] private int defaultIndex;
		[AutoNotify] private string timeTextStr;
		private LevelCellViewModel curChooseLevelCellVm;
        [Preserve]
        public SecretViewModel()
        {
	        InitPanel();
	        DataCenter.secretData.PropertyChanged += OnSecretDataChangee;
        }

        protected override void OnDispose()
        {
	        DataCenter.secretData.PropertyChanged -= OnSecretDataChangee;
	        base.OnDispose();
        }

        private void OnSecretDataChangee(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.secretData.Season))
	        {
		        UpdatePanel();
		        // 赛季更新，主动检查一次
		        CheckIsShowSeasonWish();
	        }
        }

        [Command]
        private void OnClickRankBtn()
        {
	        RankViewModel tempVm = new(ERankType.RankItemSecret);
	        UIManager.Instance.OpenDialog<RankView>(tempVm).Forget();
        }

        [Command]
        private void OnClickSeasonBtn()
        {
	        UIManager.Instance.OpenDialog<SeasonWishView,SeasonWishViewModel>().Forget();
	        
        }
        [Command]
        private void OnClickInfoBtn()
        {
	        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13);
	        var descStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_08);
	        UIHelper.OpenCommonRule(titleStr, descStr);
        }

        [Command]
        private void OnClickOpBtn()
        {
	        var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
	        if (leftTime < GameConst.SecretShowTipsTime)
	        {
		        var tipsStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_18);
		        ToastManager.Show(tipsStr);
		        return;
	        }

	        if (EquipManager.Instance.IsExistNoneSlots())
	        {
		        
		        var titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_Title);
		        var contentStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips32);
		        var cancleStr =  LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_CancelTxt);
		        var conformStr = LocalizeHelper.GetGlobal(GlobalLanguageId.CommonBox_ConfirmTxt);
		        Action cancleFunc = () =>
		        {
			        
		        };
		        Action confirmFunc = () =>
		        {
			        JumpManager.Instance.Jump(FunctionJumpCfgId.go_MainEquip);
		        };
		        CommonMessageBoxViewModel tempVm = new(titleStr,contentStr,cancleStr,conformStr,cancleFunc,confirmFunc, null);
		        UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget();
		        // ToastManager.ShowLanguage(GlobalLanguageId.Equip_16);
		        return;
	        }
	        
	        // 检查是否解锁
	        if (curChooseLevelCellVm.CurCfg.Id > DataCenter.secretData.CurrStage)
	        {
		        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.FunctionOpen_tips_2, curChooseLevelCellVm.CurCfg.Id - 1);
		        ToastManager.Show(str);
		        return;
	        }

	        RequestBattleBegin(curChooseLevelCellVm.CurCfg);
        }

        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<SecretView>();
        }

        private void OnClickLevelCellBtn(LevelCellViewModel vm)
        {
	        foreach (var item in LevelScrollviewList)
	        {
		        item.IsChoose = false;
	        }
	        curChooseLevelCellVm = vm;
	        curChooseLevelCellVm.IsChoose = true;
        }

        private async void RequestBattleBegin(CopySecretCfg cfg)
        {
	        // 请求进入战斗
	        ReqSecretFightBegin data = new ReqSecretFightBegin();
	        data.StageId = cfg.Id;
	        var result = await GameNetworkManager.Instance.SendAsync<RspSecretFightBegin>(data);
	        if (result.rsp ==null ||result.rsp.Status != 0)
	        {
		        DHLog.Debug($" 请求 进入 秘林 游戏失败 {result.rsp.Status}");
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        // 这里给提示
		        return;
	        }
	        // // 扣消耗
	        // Lodash.DealRewards(result.rsp.Cost.ToList(), false);
	        
	        GameManager.Instance.EnterGame(EStateType.StageTypeSecret,cfg.Id, result.rsp.Data);
	        
        }
        private void OnClickLevelCellBoxBtn(int index,LevelCellViewModel cellVm)
        {
	        // 检查状态 判断是可以领，还是预览
	        if(cellVm ==null) return;
	        if (cellVm.BoxState == EBoxState.Wait)
	        {
		        // 领取
		        RequestClaimBox(cellVm.CurCfg.Id, () =>
		        {
			        // 一键领取所有可领取的奖励
			        foreach (var item in LevelScrollviewList)
			        {
				        if (item.BoxState == EBoxState.Wait)
				        {
					        DataCenter.secretData.UpdateStageInfo(item.CurCfg.Id, true,true);
					        item.UpdateBoxState();
				        }
			        }
			        UpdateDefaultIndexAndChoose();
		        }).Forget();
	        }
	        else
	        {
		        var rewards = cellVm.CurCfg.CopyReward1;
		        var preModel = new ChapterBoxPreviewViewModel(rewards);
		        preModel.NodePos = cellVm.BoxRectTransform.position;
		        UIManager.Instance.OpenDialog<ChapterBoxPreviewView>(preModel).Forget();
	        }

        }

        private void InitPanel()
        {
			BottomComponentVm = new(OnClickCloseBtn);
			List<GameConst.ItemIdCode> resList = new() {GameConst.ItemIdCode.EnergyDrink, GameConst.ItemIdCode.Money,GameConst.ItemIdCode.Stone};
			CommonTopItemsVm = new(resList);
			InitLevelScrollView();
			SubTitleScrollViewList.Clear();
			LeftCountTextStr = "";
			var cost = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, 1000);
			CostItemViewVm = new(cost);
			IsShowCostItem = false;
			OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips01);
			UpdateDefaultIndexAndChoose();
			UpdateTimeStr();
			CheckIsShowSeasonWish();
        }

        private void InitLevelScrollView()
        {
	        foreach (var item in LevelScrollviewList)
	        {
		        item.Dispose();
	        }
	        LevelScrollviewList.Clear();
	        var cfgs = ConfigCenter.CopySecretCfgColl.DataItems;
	        var tempList = cfgs.ToList();
	        tempList.Sort((a, b) => a.Id.CompareTo(b.Id));
	        
	        var maxStageId = DataCenter.mainStageData.GetMaxPassChapter();
	        DataCenter.secretData.BeginStage = 1;
	        foreach (var item in tempList)
	        {
		        if (item.Id > DataCenter.secretData.MaxStage) continue;
		        LevelCellViewModel tempVm = new(item, OnClickLevelCellBtn, OnClickLevelCellBoxBtn);
		        tempVm.IsChoose = DataCenter.secretData.CurrStage == item.Id;
		        LevelScrollviewList.Add(tempVm);	
		        
	        }
        }

        private void UpdatePanel()
        {
	        foreach (var item in LevelScrollviewList)
	        {
		        item.Dispose();
	        }
	        InitLevelScrollView();
	        SubTitleScrollViewList.Clear();
	        LeftCountTextStr = "";
	        var cost = new Reward(RewardType.Lives, (int)GameConst.ItemIdCode.EnergyDrink, 1000);
	        CostItemViewVm = new(cost);
	        IsShowCostItem = false;
	        OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips01);

	        UpdateDefaultIndexAndChoose();
	        UpdateTimeStr();
	        
        }


        /// <summary>
        /// 一键领取所有可领取的奖励
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="callback"></param>
        private async UniTaskVoid RequestClaimBox(int chapterId,Action callback)
		{
			var sendData = new ReqSecretFightBox();
			// sendData.StageId = chapterId;
			var result = await GameNetworkManager.Instance.SendAsync<RspSecretFightBox>(sendData);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				if(result.rsp == null) return;
				DHLog.Debug($" 请求 领取 秘林 宝箱 失败 {result.rsp?.Status}");
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				// 这里给提示
				return;
			}
			Lodash.DealRewards(result.rsp.Reward.ToList());
			UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(),callback);
		}

		private void UpdateDefaultIndexAndChoose()
		{
			// 更新 默认的 当前选择
			defaultIndex = -1;
			foreach (var vm in LevelScrollviewList)
			{
				if (vm.BoxState == EBoxState.Wait)
				{
					DefaultIndex = vm.CurCfg.Id - DataCenter.secretData.BeginStage;
					break;
				}
			}

			if (defaultIndex == -1)
			{
				DefaultIndex = DataCenter.secretData.CurrStage - DataCenter.secretData.BeginStage;
			}

			var index = DataCenter.secretData.CurrStage - DataCenter.secretData.BeginStage;
			if(index >= LevelScrollviewList.Count)
			{
				index = LevelScrollviewList.Count - 1;
			}

			if (index >= 0)
			{
				curChooseLevelCellVm = LevelScrollviewList[index];
				curChooseLevelCellVm.IsChoose = true;
			}
		}
		
		private void UpdateTimeStr()
		{
			var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Secret_17);
			var leftTime = ServerTime.Instance.RemainTime(DataCenter.secretData.SeasonTime);
			if (leftTime < 0)
			{
				UIManager.Instance.CloseAllTopDialog();
				return;
			}

			var timeStr = UIHelper.ConvertTimeSecondToString(leftTime, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
			
			TimeTextStr = $"{str} {timeStr}" ;
		}

		/// <summary>
		/// 每个赛季第一次打开界面的时候，提示一次
		/// </summary>
		private void CheckIsShowSeasonWish()
		{
			int season = DHUnityUtil.PlayerPrefs.GetInt(GameConst.SecretSeasonPopUpKey, -1);
			if (season != DataCenter.secretData.Season)
			{
				UIManager.Instance.OpenDialog<SeasonWishView,SeasonWishViewModel>().Forget();
				DHUnityUtil.PlayerPrefs.SetInt(GameConst.SecretSeasonPopUpKey, DataCenter.secretData.Season);
			}
		}

		private float tempTIme;
		public override void Update()
		{
			base.Update();
			if (UIHelper.CalculateTime(ref tempTIme))
			{
				UpdateTimeStr();
			}
		}
    }
}