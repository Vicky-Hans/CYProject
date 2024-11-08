using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;


namespace DH.Game.ViewModels
{
    public partial class AdFreeGiftTriggerGiftItemViewModel : ViewModelBase
    {
	    [AutoNotify] private bool isShowSelf = true;
		[AutoNotify] private string tipsDescStr;
		[AutoNotify] private string titleIconPath;
		 [AutoNotify] private ObservableList<SelectCellItemViewModel> scrollViewItemList = new();
		[AutoNotify] private string discountValueStr;
		[AutoNotify] private BtnPriceNodeModel btnPriceNodeVm;
		private TriggerGiftCfg triggerGiftCfg;
		[AutoNotify] private string titleStr;
		private TriggerGiftData Data => DataCenter.triggerGiftData;
        [Preserve]
        public AdFreeGiftTriggerGiftItemViewModel()
        {
	        var items = ConfigCenter.TriggerGiftCfgColl.DataItems;
	        for (int i = 0; i < items.Count; i++)
	        {
		        if (items[i].Type == 6 && TriggerGiftManager.Instance.GiftIsCanBuy(items[i].Id))
		        {
			        triggerGiftCfg = ConfigCenter.TriggerGiftCfgColl.GetDataById(items[i].Id);
			        break;
		        }
	        }
	        if (triggerGiftCfg == null)
	        {
		       // DHLog.Error("广告券礼包获取错误");
		        IsShowSelf = false;
		        return;
	        }
	        TitleIconPath = TriggerGiftManager.Instance.GetTriggerTitleIcon(triggerGiftCfg.Id);
	        DiscountValueStr =  ShopManager.Instance.GetPackageDiscountDesc(triggerGiftCfg.Package);
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(triggerGiftCfg.Package);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        BtnPriceNodeVm = new BtnPriceNodeModel(priceStr);
	        TitleStr = ConfigCenter.TriggerGiftLanguageCfgColl.GetDataById(triggerGiftCfg.Id).Name;
	        InitRewardList();
	        RefreshTimeDesc();
	        Data.PropertyChanged += OptionalChange;
        }
        
        private void InitRewardList()
        {
	        ScrollViewItemList.Clear();
	        if (triggerGiftCfg.Reward is { Count: > 0 })
	        {
		        for (int i = 0; i<triggerGiftCfg.Reward.Count ; i++)
		        {
			        var reward = triggerGiftCfg.Reward[i];
			        var selectItemViewModel = SelectCellItemViewModel.Create(reward,ECellItemSizeType.Size120X100);
			        ScrollViewItemList.Add(selectItemViewModel);
		        }
	        }

	        if (triggerGiftCfg.OptionalReward is { Count: > 0 })
	        {
		        var selectItemViewModel = SelectCellItemViewModel.Create(triggerGiftCfg.OptionalReward,ECellItemSizeType.Size120X100,selectIndex:Data.GetSelectPacket(triggerGiftCfg.Id));
		        selectItemViewModel.ClickEvent = OnClickSelectReward;
		        ScrollViewItemList.Add(selectItemViewModel);
	        }
        }
        
        private void OnClickSelectReward()  
        {
	        if(triggerGiftCfg==null || triggerGiftCfg.OptionalReward ==null || triggerGiftCfg.OptionalReward.Count==0) return;
	        UIManager.Instance.OpenDialog<CommonSelectRewardView>(new CommonSelectRewardViewModel(triggerGiftCfg.OptionalReward,Data.GetSelectPacket(triggerGiftCfg.Id)
		        ,(selectIndex)=> {
			        Data.SetSelectPacket(triggerGiftCfg.Id,selectIndex);
		        })).Forget();
        }
        
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.OptionalRecord))
	        {
		        InitRewardList();
	        }
        }

        private void RefreshTimeDesc()
        {
	        if (triggerGiftCfg == null)return;
	        var endTime = DataCenter.triggerGiftData.GetTriggerGiftEndTime(triggerGiftCfg.Type);
	        var time = ServerTime.Instance.RemainTime(endTime);
	        if (time < 0) time = 0;
	        
	        TipsDescStr = ServerTime.Instance.Seconds2ShowTime(time);
        }
        private float interval;
        public override void Update()
        {
	        if (UIHelper.CalculateTime(ref interval))
	        {
		        RefreshTimeDesc();
	        }
        }
        
        [Command]
        private void OnClickBtnBuy()
        {
	        if (triggerGiftCfg == null)return;
	        if (triggerGiftCfg.Package == -1)
	        {
		        TriggerGiftManager.Instance.SendBuyTriggerGift(triggerGiftCfg.Id, (id) =>
		        {
			        IsShowSelf = false;
		        }).Forget();
	        }
	        else
	        {
		        if (triggerGiftCfg.OptionalReward != null && triggerGiftCfg.OptionalReward.Count >0 && Data.GetSelectPacket(triggerGiftCfg.Id) == -1)
		        {
			        ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.Trigger08));
			        return;
		        }
		        ShopManager.Instance.SendBuyPackageBuyRecharge(triggerGiftCfg.Package, (packId) =>
		        {
			        DataCenter.triggerGiftData.BuyTriggerGift(triggerGiftCfg.Id);
			        IsShowSelf = false;
		        },rewardIndex:Data.GetSelectPacket(triggerGiftCfg.Id));
	        }
	        
        }
        protected override void OnDispose()
        {
	        Data.PropertyChanged -= OptionalChange;
	        base.OnDispose();
        }
        
    }
}