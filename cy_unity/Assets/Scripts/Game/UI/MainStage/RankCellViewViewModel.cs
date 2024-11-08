using System;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class RankCellViewViewModel : ViewModelBase
    {
        
	    [AutoNotify] private string bgPath;
	    [AutoNotify] private string rankBgPath;
		[AutoNotify] private string rankTextStr;
		[AutoNotify] private CommonHeadItemViewModel commonHeadItemViewVm;
		[AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
		[AutoNotify] private string levelTextStr;
		private Action<RankMember> onClickPlayerInfoCallback;
		private RankMember curInfo;
        [Preserve]
        public RankCellViewViewModel(RankMember info, Action<RankMember> callback)
        {
	        curInfo = info;	
	        onClickPlayerInfoCallback = callback;
	        UpdatePanel();
        }

        [Command]
        private void OnClickOpBtn()
        {
	        if(curInfo == null) return;
	        onClickPlayerInfoCallback?.Invoke(curInfo);
        }


        private void UpdatePanel()
        {
	        if(curInfo == null) return;
	        CommonHeadData tempDate = new(curInfo.Logo, curInfo.HeadFrame, null);
	        commonHeadItemViewVm = new(tempDate, false);
	        RankTextStr = curInfo.Rank == 0 ? LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips23):curInfo.Rank.ToString();
	        if (CommonPlayerNameVm ==null)
	        {
		        CommonPlayerNameVm = new CommonPlayerNameViewModel(curInfo.Name,UIHelper.HexColorStrToColor("#6D4F3A"), UIHelper.IsGoldName(curInfo.VipStatus));
	        }
	        else
	        {
		        CommonPlayerNameVm.InitUI(curInfo.Name,UIHelper.HexColorStrToColor("#6D4F3A"), UIHelper.IsGoldName(curInfo.VipStatus));
	        }

	        switch (MainUiManager.Instance.CurRankType)
	        {
		        
		        case ERankType.RankItemMainStage:
		        {
			        var languageCfg = ConfigCenter.CopyLanguageCfgColl.GetDataById(curInfo.Stage);
			        LevelTextStr = $"<color=#3A456D>{curInfo.Stage}.{languageCfg.Name}</color>";
			        BgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_5]": "equip[equip_panel_12]";
			        RankBgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_7]" : "ranking[ranking_panel_6]";
			        break;
		        }
		        case ERankType.RankItemEndless:
		        {
			        LevelTextStr = $"<color=#5b3265>{curInfo.Score}</color>";
			        BgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_11]": "equip[equip_panel_12]";
			        RankBgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_13]" : "ranking[ranking_panel_6]";
			        break;
		        }
		        case ERankType.RankItemSecret:
		        {
			        LevelTextStr = $"<color=#3A456D>{curInfo.Score}</color>";
			        BgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_12]": "equip[equip_panel_12]";
			        RankBgPath = curInfo.RoleId == DataCenter.charcaterData.Digest.RoleId ? "ranking[ranking_panel_10]" : "ranking[ranking_panel_6]";
			        break;
		        }
		        default:
		        {
			        DHLog.Error($"排行榜cell 没处理的类型 请及时处理 {MainUiManager.Instance.CurRankType}");
			        break;
		        }
	        }
        }
    }
}