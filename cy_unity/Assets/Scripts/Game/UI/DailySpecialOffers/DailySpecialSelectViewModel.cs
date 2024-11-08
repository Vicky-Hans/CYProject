using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;



namespace DH.Game.ViewModels
{
    public partial class DailySpecialSelectViewModel : ViewModelBase
    {
        
		[AutoNotify] private string titileStr;
		[AutoNotify] private string equipIconPath;
	    [AutoNotify] private ObservableList<DailySpecialSelectItemViewModel> equipScrollviewList = new();
	    [AutoNotify] private bool isShowSelect;
	    [AutoNotify] private bool lockGo;
	    private DailyPackData Data => DataCenter.dailyPackData;
	    private int nowEquipId;
	    private int selectEquipId => Data.SelectEquip == 0 ? ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_defaltWeapon).Content[0]:Data.SelectEquip;
	    
        [Preserve]
        public DailySpecialSelectViewModel()
        {
	        nowEquipId = selectEquipId;
	        InitUI();
        }
        
        private void InitUI()
        {
	        EquipScrollviewList.Clear();
	        var items = ConfigCenter.JackpotCfgColl.GetDataById(ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_WeaponJackpot).Content[0]);
	        var lists = items.RandomReward;
	        lists = lists.OrderByDescending(o => DataCenter.equipData.IsOwn(o.Id))
		        .ThenBy(o => ConfigCenter.EquipCfgColl.GetDataById(o.Id).Quality).ToList();
	        for (int i = 0; i <lists.Count; i++)
	        {
		        EquipScrollviewList.Add(new DailySpecialSelectItemViewModel(lists[i].Id,SelectEquip));
	        }
	        SelectEquip(nowEquipId);
	        RefreshTopEquip();
        }

        private void RefreshTopEquip()
        {
	        var cfg = ConfigCenter.EquipCfgColl.GetDataById(nowEquipId);
	        var qlt = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.DailySpecial_WeaponQua).Content[0]-1;
	        EquipIconPath = EquipManager.Instance.GetModelIconPath(cfg.Model[qlt][0]);
	        TitileStr = EquipManager.Instance.GetModelName(cfg.Model[qlt][0]);
        }


        public void SelectEquip(int id)
        {
	        nowEquipId = id;
	        var isLock = !DataCenter.equipData.IsOwn(id);
	        IsShowSelect = !isLock && selectEquipId == nowEquipId;
	        LockGo = isLock;
	        for (int i = 0; i < EquipScrollviewList.Count; i++)
	        {
		        EquipScrollviewList[i].IsSelect = EquipScrollviewList[i].EquipId == nowEquipId;
	        }

	        RefreshTopEquip();
        }
        
        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<DailySpecialSelectView>();
        }

        [Command]
        private void OnClickInfoBut()
        {
	        UIManager.Instance.OpenDialog<EquipInfoView>(new EquipInfoViewModel(nowEquipId,true)).Forget();
        }

		[Command]
		private async void OnClickButton()
		{
			var req = new ReqDailyPackSelect();
			req.Id = nowEquipId;
			var result = await GameNetworkManager.Instance.SendAsync<RspDailyPackSelect>(req);
			if (NetHelper.CheckNetErrorMessage(result.rsp, true))
			{
				Data.SelectEquip = nowEquipId;
				OnClickClose();
			}
		}
		protected override void OnDispose()
		{
			foreach (var item in equipScrollviewList)
			{
				item.Dispose();
			}

			base.OnDispose();
		}
        
    }
}