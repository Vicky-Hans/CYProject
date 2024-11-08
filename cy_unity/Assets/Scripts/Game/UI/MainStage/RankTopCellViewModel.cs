
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
    public partial class RankTopCellViewModel : ViewModelBase
    {
		[AutoNotify] private bool isShowInfoNode;
		[AutoNotify] private CommonHeadItemViewModel commonHeadItemViewVm;
		[AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
		[AutoNotify] private string levelTextStr;
		private Action<RankMember> onClickPlayerInfoCallback;
		private RankMember curInfo;

		[Preserve]
		public RankTopCellViewModel(RankMember info, Action<RankMember> callback = null, bool isSchool = false)
        {
	        curInfo = info;	
	        onClickPlayerInfoCallback = callback;
	        IsShowInfoNode = info != null;
	        if (!IsShowInfoNode) return;
	        if (!isSchool)
	        {
		        if (curInfo.Stage <= 0) curInfo.Stage = 1;
		        var languageCfg = ConfigCenter.CopyLanguageCfgColl.GetDataById(curInfo.Stage);
		        if (languageCfg == null)
		        {
			        DHLog.Error($"未找到 CopyLanguageCfg id :{curInfo.Stage} 的配置");
			        if (MainUiManager.Instance.CurRankType == ERankType.RankItemEndless)
			        {
				        LevelTextStr = $"{curInfo.Score}"; 
			        }
			        else
			        {
				        LevelTextStr = $"{curInfo.Stage}"; 
			        }
		        }
		        else
		        {
			        switch (MainUiManager.Instance.CurRankType)
			        {
				        case ERankType.RankItemMainStage: LevelTextStr = $"{curInfo.Stage}.{languageCfg.Name}"; break;
				        case ERankType.RankItemEndless: LevelTextStr = $"{curInfo.Score}"; break;
						case ERankType.RankItemSecret: LevelTextStr = $"{curInfo.Score}"; break;
			        }
		        }
	        }
	        else
	        {
		        LevelTextStr = $"{curInfo.Score}"; 
	        }
	        
	        if (CommonPlayerNameVm ==null)
	        {
		        CommonPlayerNameVm = new CommonPlayerNameViewModel(curInfo.Name,UIHelper.HexColorStrToColor("#412023"), UIHelper.IsGoldName(curInfo.VipStatus));
	        }
	        else
	        {
		        CommonPlayerNameVm.InitUI(curInfo.Name,UIHelper.HexColorStrToColor("#412023"), UIHelper.IsGoldName(curInfo.VipStatus));
	        }
	        CommonHeadData tempDate = new(curInfo.Logo, curInfo.HeadFrame, OnClickPlayerInfo);
	        commonHeadItemViewVm = new(tempDate, false);
        }

        private void OnClickPlayerInfo()
        {
	        onClickPlayerInfoCallback?.Invoke(curInfo);
        }
    }
}