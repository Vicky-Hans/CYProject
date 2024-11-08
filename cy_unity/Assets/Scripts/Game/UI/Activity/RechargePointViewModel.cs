using System.Collections.Generic;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class RechargePointViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<RechargePointCellViewModel> infoScrollViewList = new();
		 [AutoNotify] private CommonTopViewModel topVm;

        [Preserve]
        public RechargePointViewModel()
        {
	        
	        // 初始化顶部信息
	        List<GameConst.ItemIdCode> topList = new() { GameConst.ItemIdCode.GameCoin };
	        TopVm = new CommonTopViewModel(topList);
	        // 初始化列表信息
	        var cfg = ConfigCenter.RechargeCfgColl.DataItems;
	        foreach (var item in cfg)
	        {
		        var tempVm = new RechargePointCellViewModel(item);
		        InfoScrollViewList.Add(tempVm);
	        }
        }

        [Command]
        private void OnClickClsoeBtn()
        {
	        UIManager.Instance.CloseDialog<RechargePointView>();
        }

        
    }
}