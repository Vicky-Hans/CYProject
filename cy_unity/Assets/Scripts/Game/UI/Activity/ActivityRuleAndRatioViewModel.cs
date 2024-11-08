using System;
using System.Collections.Generic;
using DH.Config;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public enum EMagicDrawInfoType
	{
		Rule,
		Ratio
	}

	public partial class  LimitCellRatioData:ObservableObject
	{
		[AutoNotify] private Reward curReward;
		[AutoNotify] private string curRatioStr;
		[AutoNotify] private string bgPath;
		[AutoNotify] private PrayJackpotCfg curCfg;
		[AutoNotify] private int curIndex;

		public LimitCellRatioData()
		{
		}

		public LimitCellRatioData(PrayJackpotCfg cfg, int index = 0)
		{
			curCfg = cfg;
			CurIndex = index;
		}

		public LimitCellRatioData(Reward reward, string ratioStr, int index = 0 )
		{
			curReward = reward;
			curRatioStr = ratioStr;
			curIndex = index;
		}
	}
	public partial class ActivityRuleAndRatioData:ObservableObject
	{
		/// <summary>
		/// 标题
		/// </summary>
		[AutoNotify] private string titleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips03);
		/// <summary>
		/// 规则按钮上的文本
		/// </summary>
		[AutoNotify] private string ruleBtnStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_06);
		/// <summary>
		/// 概率按钮上的文本
		/// </summary>
		[AutoNotify] private string ratioBtnStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xingyun_07);
		/// <summary>
		/// 规则
		/// </summary>
		[AutoNotify] private string ruleStr;

		/// <summary>
		/// 概率  限定大奖 的标题
		/// </summary>
		/// <returns></returns>
		[AutoNotify] private string limitSubTitleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_14);
		/// <summary>
		/// 限定大奖
		/// </summary>
		[AutoNotify] private List<LimitCellRatioData> limitRatioList = new();
		/// <summary>
		/// 概率  限定大奖 的标题
		/// </summary>
		/// <returns></returns>
		[AutoNotify] private string normalSubTitleStr = LocalizeHelper.GetGlobal(GlobalLanguageId.MakeWish_13);
		/// <summary>
		/// 普通奖励
		/// </summary>
		[AutoNotify] private List<LimitCellRatioData> normalRatioList = new();

		public ActivityRuleAndRatioData()
		{
		}
		public ActivityRuleAndRatioData(string ruleText,List<LimitCellRatioData> limitList, List<LimitCellRatioData> normalList)
		{
			RuleStr = ruleText;
			LimitRatioList = limitList;
			NormalRatioList = normalList;
		}
	}

	public partial class ActivityRuleAndRatioViewModel : ViewModelBase
    {
		[AutoNotify] private string leftBgPath;
		[AutoNotify] private string rightBgPath;
		[AutoNotify] private ObservableList<ViewModelBase> ratioScrollViewList = new();
		[AutoNotify] private EMagicDrawInfoType curType;
		[AutoNotify] private Color leftTextStrColor;
		[AutoNotify] private Color rightTextStrColor;
		[AutoNotify] private ActivityRuleAndRatioData curData;
		[Preserve]
		public ActivityRuleAndRatioViewModel(ActivityRuleAndRatioData data)
		{
			CurType = EMagicDrawInfoType.Rule;
			curData = data;
			UpdateOpBtnText();
			// 抽奖奖励
			var cfgs = ConfigCenter.PrayJackpotCfgColl.GetDataByType(2);
			ratioScrollViewList.Clear();
			
			if (curData.LimitRatioList.Count > 0)
			{
				ratioScrollViewList.Add(new LimitRatioCellViewModel(curData.LimitRatioList));
			}
			foreach (var item in curData.NormalRatioList)
			{
				var tempRatio = item.CurCfg == null ? item.CurReward : item.CurCfg.Reward[0];
				var tempReward = new Reward(tempRatio.Type, tempRatio.Id, tempRatio.Count);
				UIHelper.GetRewardInfo(tempReward, out string nameStr, out string descStr);
				LuckDrawRatioCellViewModel tempVm = new(ratioScrollViewList.Count - 1, $"{nameStr}*{tempRatio.Count}", item.CurRatioStr);
				RatioScrollViewList.Add(tempVm);	
			}
		}
		
		[Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<ActivityRuleAndRatioView>();
	        CurType = EMagicDrawInfoType.Rule;
        }

        [Command]
        private void OnClickLeftBtn()
        {
	        CurType = EMagicDrawInfoType.Rule;
	        UpdateOpBtnText();
        }

        [Command]
        private void OnClickRightBtn()
        {
	        CurType = EMagicDrawInfoType.Ratio;
	        UpdateOpBtnText();
        }
        
        private void UpdateOpBtnText()
        {
	        if (CurType == EMagicDrawInfoType.Rule)
	        {
		        LeftTextStrColor = UIHelper.HexColorStrToColor("#FFEEB2");
		        RightTextStrColor = UIHelper.HexColorStrToColor("#D7BB93");
	        }
	        else
	        {
		        LeftTextStrColor = UIHelper.HexColorStrToColor("#D7BB93");
		        RightTextStrColor = UIHelper.HexColorStrToColor("#FFEEB2");
	        }
        }
    }
}