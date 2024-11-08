using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UI.Control;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class DailySpecialitemViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<SelectCellItemViewModel> awardScrollviewList = new();
		 [AutoNotify] private bool isBuyOver;
		[AutoNotify] private string dimIconPath;
		[AutoNotify] private string numsStr;
		[AutoNotify] private string discountTextStr;
		public BtnPriceNodeModel BtnPriceNode;
		private DailySpecialPackageCfg cfg;

		public CellItemBaseViewModel EquipItemVm;
		private DailyPackData Data => DataCenter.dailyPackData;
		private int selectEquipId => Data.SelectEquip == 0 ? ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_defaltWeapon).Content[0]:Data.SelectEquip;
        [Preserve]
        public DailySpecialitemViewModel(DailySpecialPackageCfg mCfg)
        {
	        cfg = mCfg;
	        Data.PropertyChanged += OptionalChange;
	        PlayerInfoManager.Instance.PropertyChanged += OptionalChange;
	        InitUI();
        }

        private void InitUI()
        {
	        AwardScrollviewList.Clear();
	        if (cfg.OtherRewawds is { Count: > 0 })
	        {
		        for (int i = 0; i < cfg.OtherRewawds.Count; i++)
		        {
			        var vm = SelectCellItemViewModel.Create(cfg.OtherRewawds[i],ECellItemSizeType.Size117X76);
			        AwardScrollviewList.Add(vm);
		        }
	        }
	        
	        if (cfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemViewModel.Create(cfg.OptionalReward,ECellItemSizeType.Size117X76,selectIndex:Data.GetSelectPacket(cfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        selectItemViewModel.IsButEnabled = Data.IsCanBuy(cfg.Id);
		        AwardScrollviewList.Add(selectItemViewModel);
	        }

	        DimIconPath = UIHelper.GetRewardsIconPath(cfg.GemNum[0]);
	        NumsStr = cfg.GemNum[0].Count.ToString();
	        DiscountTextStr = cfg.Offer;
	        
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(cfg.PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNode = new BtnPriceNodeModel(priceStr);

	        InitEquipshard();
	        IsBuyOver = !Data.IsCanBuy(cfg.Id);
        }

        private void InitEquipshard()
        {
	        var gemvalue = cfg.GemValue;
	        var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(selectEquipId);
	        var itemCfg = ConfigCenter.ItemCfgColl.GetDataById(equipCfg.ItemId);
	        Reward reward = new Reward(RewardType.Item,equipCfg.ItemId,(long)(gemvalue/itemCfg.GemPrice));
	        EquipItemVm = CellItemBaseViewModel.Create(reward);
        }


        [Command]
        private void OnClickBuyBut()
        {
	        if (!Data.IsCanBuy(cfg.Id))return;

	        if (cfg.OptionalReward != null && cfg.OptionalReward.Count >0 && Data.GetSelectPacket(cfg.Id) == -1)
	        {
		        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
		        return;
	        }
	        
	        // PayController.Instance.Pay(cfg.PackageId,Data.GetSelectPacket(cfg.Id),callback:(result) =>
	        // {
		       //  if (result == null || result.Status != 0)
		       //  {
			      //   DHLog.Debug($"每日特惠购买失败 {result.Status}");
			      //   return;
		       //  }
		       //  Data.Buy(cfg.Id);
		       //  Lodash.DealRewards(result.Rewards.ToArray());
		       //  UIHelper.OpenCommonRewardView(result.Rewards.ToList());
	        // });
	        
	        ShopManager.Instance.SendBuyPackageBuyRecharge(cfg.PackageId, (packageId) =>
	        {
		        Data.Buy(cfg.Id);
	        },0,Data.GetSelectPacket(cfg.Id));
        }
        
        private void OnClickSelectReward()  
        {
	        if(cfg==null || cfg.OptionalReward ==null || cfg.OptionalReward.Count==0) return;
	        if (!Data.IsCanBuy(cfg.Id)) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(cfg.OptionalReward,Data.GetSelectPacket(cfg.Id)
		        ,selectEventCb)).Forget();
        }
        
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName is nameof(Data.OptionalRecord) or nameof(PlayerInfoManager.Instance.SecondDay))
	        {
		        InitUI();
	        }
        }

        private async void selectEventCb(int selectIndex)
        {
	        var req = new ReqDailyPackOptional();
	        req.Id = cfg.Id;
	        req.Index = selectIndex;
	        var result = await GameNetworkManager.Instance.SendAsync<RspDailyPackOptional>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Data.SetSelectPacket(cfg.Id,selectIndex);
	        }
        }

        protected override void OnDispose()
        {
	        foreach (var item in awardScrollviewList)
	        {
		        item.Dispose();
	        }
	        EquipItemVm.Dispose();
	        Data.PropertyChanged -= OptionalChange;
	        PlayerInfoManager.Instance.PropertyChanged -= OptionalChange;
	        base.OnDispose();
        }
    }
}