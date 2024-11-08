using System.Collections.Generic;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class RoleTierDownViewModel : ViewModelBase
    {
        
		[AutoNotify] private string desStr;
		[AutoNotify] private string quilityBgPath;
		[AutoNotify] private string heroHeadPath;
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> awardScrollviewList = new();
	    public int RoleId;
	    private HeroMainCfg cfg;
	    private RoleData Data => DataCenter.roleData;
        [Preserve]
        public RoleTierDownViewModel(int roleId)
        {
	        RoleId = roleId;
	        cfg = ConfigCenter.HeroMainCfgColl.GetDataById(RoleId);
	        var lv = Data.GetHeroLevel(RoleId);
	        if (lv < 2)return;
	        
	        QuilityBgPath = $"common[commom_equipbg_{cfg.Qlt}]";
	        HeroHeadPath= cfg.HeadIcon;
	        
	        var res = new List<Reward>();
	        for (int i = 1; i < lv; i++)
	        {
		        var mcfg = ConfigCenter.HeroLevelCfgColl.GetDataById(lv - i);
		       res = UIHelper.MergeLists(res,mcfg.LevelCost[cfg.Qlt switch
		       {
			       3 => 0,
			       4 => 1,
			       _ => 2
		       }]);
	        }

	        var strList = new List<string>();
	        AwardScrollviewList.Clear();
	        for (int i = 0; i < res.Count; i++)
	        {
		        AwardScrollviewList.Add(CellItemBaseViewModel.Create(res[i]));
		        strList.Add(UIHelper.GetRewardName(res[i]));
	        }
	        DesStr = Des(ConfigCenter.GlobalLanguageCfgColl
		        .GetDataById(GlobalLanguageId.hero_tips_19).Name,strList);
        }


        [Command]
        private async void OnClickSureButton()
        {
	        if (Data.GetHeroLevel(RoleId)<2)return;
	        
	        var req = new ReqHeroReset();
	        req.HeroId = RoleId;
	        var result = await GameNetworkManager.Instance.SendAsync<RspHeroReset>(req);
	        if (NetHelper.CheckNetErrorMessage(result.rsp, true))
	        {
		        Lodash.DealRewards(new List<Resource>(result.rsp.Rewards));
		        if (Data.Heroes.ContainsKey(RoleId))
			        Data.Heroes[RoleId].Lv = 1;
		        Data.HeroLevelUp(RoleId);
		        UIHelper.OpenCommonRewardView(new List<Resource>(result.rsp.Rewards));
		        UIManager.Instance.CloseDialog<RoleTierDownView>();
	        }
	        
        }

        [Command]
        private void OnClickCloseBut()
        {
	        UIManager.Instance.CloseDialog<RoleTierDownView>();
        }
        private string Des(string Des,List<string> value)
        {

	        for (int i = 0; i < value.Count; i++)
	        {
		        var temp = "{" + i + "}";
		        Des = Des.Replace(temp, value[i]);
	        }

	        return Des;
        }

        protected override void OnDispose()
        {
	        foreach (var item in awardScrollviewList)
	        {
		        item.Dispose();
	        }
	        base.OnDispose();
        }
    }
}