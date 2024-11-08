using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
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
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class DailySpecialOffersViewModel : ViewModelBase
    {
	    #region 组件 属性
	    
	    [AutoNotify] private string timeDesStr;
	    [AutoNotify] private string selectEquipIcon;
	    [AutoNotify] private string selectEquipNameStr;
	    [AutoNotify] private string discountText;
	    [AutoNotify] private ObservableList<DailySpecialitemViewModel> awardScrollviewList = new();
	    [AutoNotify] private bool dayButGray;
	    [AutoNotify] private string dayButGrayString;
	    [AutoNotify] private bool allButGray;
	    [AutoNotify] private bool allButOver;
	    public BtnPriceNodeModel AllBtnPriceNode;
	    public CommonTopViewModel TopView;
	    private DailyPackData Data => DataCenter.dailyPackData;

	    #endregion
        


        [Preserve]
        public DailySpecialOffersViewModel()
        {
	        List<int> list = new List<int>() {(int)GameConst.ItemIdCode.EnergyDrink, (int)GameConst.ItemIdCode.Money, (int)GameConst.ItemIdCode.Stone};
	        TopView = new(list);
	        InitUI();
	        TimeDes();
	        Data.PropertyChanged += SelectEuipEvent;
	        PlayerInfoManager.Instance.PropertyChanged += SecondDay;
        }

        private void InitUI()
        {
	        //选择武器
	        var selectEquipId = Data.SelectEquip == 0 ? ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_defaltWeapon).Content[0]:Data.SelectEquip;
	        var qlt = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_WeaponQua).Content[0]-1;
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(selectEquipId);
	        SelectEquipIcon =EquipManager.Instance.GetModelIconPath(cfg.Model[qlt][0]);
	        SelectEquipNameStr =EquipManager.Instance.GetModelName(cfg.Model[qlt][0]);

	        //每日免费按钮
	        DayButGray = Data.FreeDayIsGet();
	        DayButGrayString = $"gratia[gratia_icon_1{(DayButGray ? "_2" : "")}]";
	        
	        //购买所有
	        AllButGray = !Data.IsButAllType(1) && !Data.IsCanBuy(Data.AllBuyCfg().Id);

	        AllButOver = Data.IsButAllType(2) || Data.IsButAllType(1);
	        
	        
	        //一键购买展示
	        DiscountText = Data.AllBuyCfg().Offer;
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(Data.AllBuyCfg().PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        AllBtnPriceNode = new BtnPriceNodeModel(priceStr);
	        
	        AwardScrollviewList.Clear();
	        var items = ConfigCenter.DailySpecialPackageCfgColl.DataItems;
	        for (int i = 0; i < items.Count; i++)
	        {
		        if (items[i].Type == 1)
		        {
			        AwardScrollviewList.Add(new DailySpecialitemViewModel(items[i]));
		        }
	        }
        }    

        #region 按钮事件

        [Command]
        private void OnClickInfoBut()
        {
        
	        var content1 = LocalizeHelper.GetGlobal(GlobalLanguageId.dailySpecialPackage_tips_05);
	        CommonRuleData rule1 = new("", content1, true);
	        var title = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips13);
	        CommonRuleViewModel tempVm = new (title,content1);
	        UIManager.Instance.OpenDialog<CommonRuleView>(tempVm).Forget();
        }

        [Command]
        private async void OnClickDayFreeBuy()
        {
	        if ( Data.FreeDayIsGet())return;
	        
	        var req = new ReqDailyPackClaim();
	        req.Id = Data.FreeCfg().Id;
	        var result = await GameNetworkManager.Instance.SendAsync<RspDailyPackClaim>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Data.Buy(Data.FreeCfg().Id);
		        DayButGray = Data.FreeDayIsGet();
		        Lodash.DealRewards(result.rsp.Reward.ToArray());
		        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
	        }
        }

        [Command]
        private void OnClickSelectEquipBut()
        {
	        if (Data.IsBuyGift(1) || Data.IsBuyGift(2))
	        {
		        ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.dailySpecialPackage_tips_06).Name);
		        return;
	        }
	        
	        UIManager.Instance.OpenDialog<DailySpecialSelectView>(
		        new DailySpecialSelectViewModel()).Forget();
        }

        [Command]
        private void OnClickAllBuyBut()
        {
	        if (!Data.IsCanBuy(Data.AllBuyCfg().Id))
	        {
		        ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.dailySpecialPackage_tips_06).Name);
		        return;
	        }

	        var items = ConfigCenter.DailySpecialPackageCfgColl.DataItems;
	        for (int i = 0; i < items.Count; i++)
	        {
		        var temp = items[i];
		        if (temp.OptionalReward!=null && temp.OptionalReward.Count>0 && temp.Type ==1 && !Data.CheckSelectPacket(temp.Id))
		        {
			        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
			        return; 
		        }
	        }
	        
	        // PayController.Instance.Pay(Data.AllBuyCfg().PackageId, callback:(result) =>
	        // {
		       //  if (result == null || result.Status != 0)
		       //  {
			      //   DHLog.Debug($"每日特惠购买失败 {result.Status}");
			      //   return;
		       //  }
		       //  Data.Buy(Data.AllBuyCfg().Id);
		       //  Lodash.DealRewards(result.Rewards.ToArray());
		       //  UIHelper.OpenCommonRewardView(result.Rewards.ToList());
	        // });
	        
	        ShopManager.Instance.SendBuyPackageBuyRecharge(Data.AllBuyCfg().PackageId, (packageId) =>
	        {
		        Data.Buy(Data.AllBuyCfg().Id);
	        });
        }
        
        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<DailySpecialOffersView>();
        }

        #endregion


		#region 倒计时

		public void TimeDes()
		{
			var time = Data.RefreshStamp - ServerTime.Instance.GetNowTime();
			time =  Math.Max(0, time);
			TimeDesStr = ServerTime.Instance.SecondsDHms(time);
		}
		private float time;
		public override void Update()
		{
			base.Update();
			if (!UIHelper.CalculateTime(ref time)) return;
			TimeDes();
		}

		#endregion
		
		private void SelectEuipEvent(object sender, PropertyChangedEventArgs e)
		{
			InitUI();
		}
		private void SecondDay(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(PlayerInfoManager.Instance.SecondDay))
			{
				InitUI();
			}	
		}

		protected override void OnDispose()
		{
			Data.PropertyChanged -= SelectEuipEvent;
			PlayerInfoManager.Instance.PropertyChanged -= SecondDay;
			foreach (var item in awardScrollviewList)
			{
				item.Dispose();
			}
			AllBtnPriceNode.Dispose();
			TopView.Dispose();
			base.OnDispose();
		}

    }
}