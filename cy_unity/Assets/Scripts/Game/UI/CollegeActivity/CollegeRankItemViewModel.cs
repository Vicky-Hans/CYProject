using DH.Config;
using DH.Game.UI;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Proto;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class CollegeRankItemViewModel : ViewModelBase
    {
	    public RankMember RankMember;
		[AutoNotify] private string rankTextStr;
		[AutoNotify] private string rankIconPath;
		[AutoNotify] private CommonHeadItemViewModel commonHeadItemViewVm;
		[AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
		[AutoNotify] private string levelTextStr;

        [Preserve]
        public CollegeRankItemViewModel(RankMember rankMember)
        {
	        RankMember = rankMember;
	        CommonHeadItemViewVm = CommonHeadItemViewModel.OnCreate(rankMember);
	        CommonPlayerNameVm = CommonPlayerNameViewModel.OnCreate(rankMember);
	        RefreshRankLv();
        }

        private void RefreshRankLv()
        {
	        rankTextStr = string.Empty;
	        rankIconPath = UIHelper.NoneImagePath();
	        if (RankMember.Rank is 1 or 2 or 3)
	        {
		        RankIconPath = $"school[school_number_{RankMember.Rank}]";
	        }else if (RankMember.Rank == 0)
	        {
		        rankTextStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips23);
	        }
	        else
	        {
		        rankTextStr = RankMember.Rank.ToString();
	        }
	        levelTextStr = RankMember.Score.ToString();
        }

        [Command]
        private void OnClick()
        {
	        CollegeActivityManager.Instance.SendPlayerInfo(RankMember).Forget();
        }
    }
}