using System.Collections.Generic;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class CommonRuleViewModel : ViewModelBase
    {
        [AutoNotify] private Vector2 bgSize = new Vector2(950, 1500) ;
        [AutoNotify] private ObservableList<CommonRuleCellViewModel> ruleInfo = new();
        [AutoNotify] private string titleStr;
        

        public CommonRuleViewModel(string title,string desc)
        {
            CommonRuleData ruleDate = new(null,desc,false);
            InitInfo(title, ruleDate);
        }
        public CommonRuleViewModel(string title,List<CommonRuleData> ruleDate)
        {
           InitInfo(title, ruleDate);
        }
        public CommonRuleViewModel(string title,CommonRuleData ruleDate)
        {
            InitInfo(title, ruleDate);
        }

        private void InitInfo(string title,List<CommonRuleData> ruleDate)
        {
            TitleStr = title;
            RuleInfo.Clear();
            foreach (var item in ruleDate)
            {
                CommonRuleCellViewModel tempVm = new(item);
                RuleInfo.Add(tempVm);
            }
           
        }
        private void InitInfo(string title,CommonRuleData ruleDate)
        {
            TitleStr = title;
            RuleInfo.Clear();
            CommonRuleCellViewModel tempVm = new(ruleDate);
            RuleInfo.Add(tempVm);
        }

        [Command]
        private void OnClickCloseBtn()
        {
            UIManager.Instance.CloseDialog<CommonRuleView>();
        }
    }
}