using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ChapterFundViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<PassportsCellViewModel> passportsScrollViewList = new();
	    [AutoNotify] private ObservableList<BottomOpCellItemViewModel> opBtnScrollviewList = new();
	    [AutoNotify] private BtnPriceNodeModel btnPriceNodeMod;
	    [AutoNotify] private int showIndex;
	    private EPassPortType curPassPortType = EPassPortType.PassportTypeChapter;
	    public ChapterFundData Data => DataCenter.chapterFundData;
	    [AutoNotify] private CommonTopViewModel commonTopItemsVm;
	    private int PackageId => 46;
	    
        [Preserve]
        public ChapterFundViewModel()
        {
	        List<GameConst.ItemIdCode> list = new(){GameConst.ItemIdCode.Stone};
	        CommonTopItemsVm = new(list);
	        var packageCfg = ConfigCenter.PackageCfgColl.GetDataById(PackageId);
	        var cfgInfo = ConfigCenter.RechargeCfgColl.GetDataById(packageCfg.RechargeId);
	        string priceStr = "";
	        if (cfgInfo != null) priceStr = PayController.Instance.GetLocalizePrice(cfgInfo.PayId);
	        btnPriceNodeMod = new BtnPriceNodeModel(priceStr);
	        InitScrollViewList();
	        ParseShowIndex();
        }

        private void InitScrollViewList()
        {
	        PassportsScrollViewList.Clear();
	        var items =  ConfigCenter.FundRewardsCfgColl.DataItems.Where(o=>o.Type == (int)EPassPortType.PassportTypeChapter).ToList();
	        for (int i = 0; i < items.Count; i++)
	        {
		        PassportsCellViewModel tempVm = new(items[i], OnClickUnLockBtn, OnClickClaimBtn, EPassPortType.PassportTypeChapter);
		        PassportsScrollViewList.Add(tempVm);
	        }
        }

        public void UpdateOpBtnState()
        {
	        foreach (var item in opBtnScrollviewList)
	        {
		        if ((EPassPortType)item.CurOpCellData.OpType == EPassPortType.PassportTypeChapter)
					item.CurOpCellData.IsShowRedDot = Data.IsRed();
	        }
        }
        
        private void OnClickUnLockBtn(FundRewardsCfg cfg)
        {
			
        }
        private async void OnClickClaimBtn(FundRewardsCfg cfg)
        {
	        var data = new ReqPassportClaim();
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
	        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
	        Lodash.DealRewards(result.rsp.Reward.ToList());
	        Data.GetAward();
	        InitScrollViewList();
	        UpdateOpBtnState();
	        ParseShowIndex();
        }
        [Command]
        private void OnClickOpBtn()
        {
	        if (Data.Plus)return;
	        
	        ShopManager.Instance.SendBuyPackageBuyRecharge(PackageId, (packId) =>
	        {
		        Data.Plus = true;
		        InitScrollViewList();
	        });
        }
        public void ParseShowIndex()
        {
	        showIndex = -2;

	        ShowIndex = Data.NowIndex();
        }
        protected override void OnDispose()
        {
	        foreach (var item in PassportsScrollViewList)
	        {
		        item.Dispose();
	        }
	        btnPriceNodeMod?.Dispose();
	        base.OnDispose();
        }
    }
}