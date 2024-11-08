using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class PassportsViewModel : ViewModelBase
    {
	    [AutoNotify] private string titleTestStr;
		[AutoNotify] private string timeTextStr;
		[AutoNotify] private float expProgressValue;
		[AutoNotify] private string expTextStr;
		[AutoNotify] private string levelTextStr;
		[AutoNotify] private string opBtnTextStr;
		[AutoNotify] private BtnPriceNodeModel opBtnPriceVm;
		[AutoNotify] private bool isShowCostNode;
	    [AutoNotify] private ObservableList<PassportsCellViewModel> passportsScrollViewList = new();
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opBtnScrollviewList = new();
	    [AutoNotify] private CommonTopViewModel commonTopItemsVm;
		[AutoNotify] private int showIndex;
		[AutoNotify] private int passportsCfgId;
		[AutoNotify] private bool isCanClickOpBtn;
		// [AutoNotify] private bool isCanClickOpBtn = true;
		private List<int> passportPackageIdList = new List<int>(){ 22 };
		private Passport curPassport;
		[AutoNotify] private EPassPortType curPassPortType;
		private List<FundRewardsCfg> cfgs = new();
        [Preserve]
        public PassportsViewModel()
        {
	        CurPassPortType = DataCenter.allPassportData.GetDefaultPassportType();

	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.Stone};
	        CommonTopItemsVm = new(list);
	        UpdatePanel();
        }

        public void UpdatePanel()
        {
	        
	        if (OpBtnPriceVm != null)
	        {
		        OpBtnPriceVm.Dispose();
	        }

	        foreach (var item in PassportsScrollViewList)
	        {
		        item.Dispose();
	        }
	        PassportsScrollViewList.Clear();

	        TitleTestStr = curPassPortType switch
	        {
		        EPassPortType.PassportTypeDiscount =>LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_01),
		        EPassPortType.PassportTypeStone =>LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_08),
	        };
	        curPassport = DataCenter.allPassportData.GetPassportData(curPassPortType);
	        if (curPassPortType == EPassPortType.PassportTypeDiscount)
	        {
		        PassportsCfgId = passportPackageIdList[0];
	        }
	        else
	        {
		        // 砖石通行证
		        var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.FundRewards_PackageId);
		        if (defCfg == null || defCfg.Content.Count < curPassport.Version)
		        {
			        if (defCfg == null)
			        {
				        DHLog.Error($"配置 DefinesCfg 的 没有 通行证礼包 配置");
				        return;
			        }
			        DHLog.Error($"配置 DefinesCfg 的通行证礼包id 不正确，请及时检查  礼包数组长度 跟版本不匹配 长度{defCfg.Content} 当前版本 {curPassport.Version}");
			        return;
		        }
		        PassportsCfgId = defCfg.Content[curPassport.Version - 1];
	        }
	        cfgs = DataCenter.allPassportData.GetShowConfigs(curPassPortType);
	        foreach (var cfg in cfgs)
	        {
		        PassportsCellViewModel tempVm = new(cfg, OnClickUnLockBtn, OnClickClaimBtn, curPassPortType);
		        PassportsScrollViewList.Add(tempVm);
	        }
	        UpdateExpValue();
	        UpdatePassportsBtnState();
	        UpdateTime();
	        OpBtnPriceVm = new(passportsCfgId);
	        ParseShowIndex();
        }

        public void UpdateOpBtnState()
        {
	        foreach (var item in opBtnScrollviewList)
	        {
		        if ((EPassPortType)item.CurOpCellData.OpType != EPassPortType.PassportTypeChapter)
					item.CurOpCellData.IsShowRedDot = DataCenter.allPassportData.CheckIsShowRedDot((EPassPortType)item.CurOpCellData.OpType);
	        }
        }

        private void UpdateExpValue()
        {
	        if(curPassport ==null) return;
	        LevelTextStr = curPassport.Lv.ToString();
	        if (curPassport.Lv >= cfgs.Count)
	        {
		        ExpProgressValue = 1;
		        ExpTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_06);
	        }
	        else
	        {
		        var curCfg = cfgs[curPassport.Lv - 1];
		        ExpProgressValue = curPassport.Exp/(float)curCfg.Exp;
		        ExpTextStr = $"{curPassport.Exp}/{curCfg.Exp}";
	        }
        }

        private void ParseShowIndex()
        {
	        showIndex = -2;
	        if(curPassport == null) return;
            // 检查是否有没领取的
            var isShowRedDot = DataCenter.allPassportData.CheckIsShowRedDot();
            if (!isShowRedDot)
            {
	            // 都领取了，显示到最后一个
	            ShowIndex = curPassport.Lv -1;
            }
            else
            {
	            if (curPassport.Plus)
	            {
		            ShowIndex = curPassport.FreeClaimLv < curPassport.PlusClaimLv ? curPassport.FreeClaimLv -1: curPassport.PlusClaimLv -1;
	            }
	            else
	            {
		            ShowIndex = curPassport.FreeClaimLv - 1;  
	            }
            }
        }

        protected override void OnDispose()
        {
	        foreach (var item in PassportsScrollViewList)
	        {
		        item.Dispose();
	        }
	        OpBtnPriceVm.Dispose();
	        base.OnDispose();
        }

        private void UpdateTime()
        {
	        if(curPassport == null) return;
	        
	        var endTime = ServerTime.Instance.RemainTime(curPassport.EndTime);
	        if (endTime <= 0)
	        {
		        UIManager.Instance.CloseDialog<PassportsView>();
	        }
	        if (endTime >= 86400)
	        {
		        TimeTextStr = UIHelper.ConvertTimeSecondToString(endTime, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
	        }
	        else
	        {
		        TimeTextStr =  ServerTime.Instance.Seconds2Hhmmss(endTime); 
	        }
        }

        private float tempTime;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref tempTime)) return;
	        UpdateTime();
        }

        [Command]
        private void OnClickOpBtn()
        {
	        if(curPassport == null) return;
	        if(curPassport.Plus) return;
	        if( !IsCanClickOpBtn) return;
	        ShopManager.Instance.SendBuyPackageBuyRecharge(passportsCfgId,null,0,-1,1, (rewardList,costList) =>
	        {
		        Lodash.DealRewards(rewardList,costList);
		        curPassport.Plus = true;
		        UpdatePassportsCellState();
		        UpdatePassportsBtnState();
		        ToastManager.ShowLanguage(GlobalLanguageId.ConfRecharge0);
	        });
        }

        [Command]
        private void OnClickExpTipsBtn()
        {
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_05);
	        ToastManager.Show(str);
        }

		private void OnClickUnLockBtn(FundRewardsCfg cfg)
		{
			if(curPassport ==null) return;
			PassportsBuyConfirmViewModel tempVm = new(() =>
			{
				RequestUnLockPassport(cfg, curPassport);
			},curPassPortType);
			UIManager.Instance.OpenDialog<PassportsBuyConfirmView>(tempVm).Forget();
		}

		private async void RequestUnLockPassport(FundRewardsCfg cfg, Passport tempPassport)
		{
			ReqPassportUnlock data = new ReqPassportUnlock();
			data.Id = cfg.Id;
			data.Typ = (int)curPassPortType;
			var result = await GameNetworkManager.Instance.SendAsync<RspPassportUnlock>(data);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				DHLog.Debug(" 请求解锁失败");
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return ;
			}
			tempPassport.Lv = result.rsp.Lv;
			Lodash.DealRewards(result.rsp.Cost.ToList(),false);
			UpdatePassportsCellState();
			UpdateExpValue();
			UpdateOpBtnState();
		}

		private async void OnClickClaimBtn(FundRewardsCfg cfg)
		{
			var curType = curPassPortType;
			// DHLog.Debug("muzili  请求战斗结算 ");
			ReqPassportClaim data = new ReqPassportClaim();
			data.Typ = (int)curPassPortType;
			var result = await GameNetworkManager.Instance.SendAsync<RspPassportClaim>(data);
			if (result.rsp ==null ||result.rsp.Status != 0)
			{
				DHLog.Debug(" 领取奖励失败");
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return ;
			}
			UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), UpdatePassportsCellState);
			Lodash.DealRewards(result.rsp.Reward.ToList());
			DataCenter.allPassportData.UpdateClaimedInfo(curType);
			UpdateOpBtnState();
		}

		private void UpdatePassportsCellState()
		{
			foreach (var item in PassportsScrollViewList)
			{
				item.UpdateState();
			}
			if(curPassport ==null) return;
			LevelTextStr = curPassport.Lv.ToString();
		}

		private void UpdatePassportsBtnState()
		{
			if(curPassport ==null) return;
			if (!curPassport.Plus)
			{
				IsCanClickOpBtn = true;
				IsShowCostNode = true;
				OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_02);
			}
			else
			{
				IsCanClickOpBtn = false;
				IsShowCostNode = false;
				OpBtnTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Tehuijijin_04);
			}
		}
		
    }
}