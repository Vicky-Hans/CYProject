using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game;
using DH.Game.UI;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using Game.UI.MainUi;
using UnityEngine.Scripting;
namespace Game.UI.CommonView
{
    public partial class JumpViewModel : ViewModelBase
    {
        public ICommand OnClickCloseBtn { get; private set; }
        [AutoNotify] private string iconPath;
        [AutoNotify] private string iconBgPath;
        [AutoNotify] private string itemNameTxt;
        [AutoNotify] private string itemDesTxt;
        [AutoNotify] private ObservableDictionary<int, JumpItemViewModel> jumpItemDic = new();
        public Action JumpToCallback;
        [Preserve]
        public JumpViewModel(List<Reward> itemList)
        {
            OnClickCloseBtn = new AsyncCommand(ClickCloseBtn);
            if (itemList == null || itemList.Count == 0) return;
            Reward item = null;
            foreach (var reward in itemList)
            {
                if (!Lodash.CheckRewardIsEnough(reward))
                {
                    item = reward;
                    break;
                }
            }
            ParseRewardInfo(item);
        }
        
        public JumpViewModel(Reward reward)
        {
            OnClickCloseBtn = new AsyncCommand(ClickCloseBtn);
            ParseRewardInfo(reward);
        }

        private void ParseRewardInfo(Reward reward)
        {
            if (reward == null) return;
            
            var itemConfig = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
            if (itemConfig == null) 
            {
                DHLog.Error($"当前 ItemID 不存在==== {reward.Id}");
                return;
            }
            IconBgPath = UIHelper.GetRewardsBgPath(RewardType.Item, itemConfig.Id);
            var itemLanguageConfig = ConfigCenter.ItemLanguageCfgColl.GetDataById(reward.Id);
            if (itemLanguageConfig!= null)
            {
                ItemNameTxt = itemLanguageConfig.Name;
                ItemDesTxt = itemLanguageConfig.Dec;
            }
            IconPath = UIHelper.GetRewardsIconPath((int)RewardType.Item, reward.Id);
            if (itemConfig.Obtain != null && itemConfig.Obtain.Count > 0)
            {
                for (int i = 0; i < itemConfig.Obtain.Count; i++)
                {
                    JumpItemViewModel tempVm = new JumpItemViewModel(itemConfig.Obtain[i], OnClickJumpItemOPBtn);
                    jumpItemDic[itemConfig.Obtain[i]] = tempVm;
                }
            }
        }

        private async UniTask ClickCloseBtn()
        {
            UIManager.Instance.CloseDialog<JumpView>();
        }
        //跳转回调按钮
        private void OnClickJumpItemOPBtn(int jumpKey)
        {
            var cfg = ConfigCenter.FunctionJumpCfgColl.GetDataById(jumpKey);
            if (cfg == null)
            {
                DHLog.Error("FunctionJump找不到配置"+jumpKey);
                return;
            }
            bool isUnlock = MainUiManager.Instance.CheckFunctionIsUnlock((EFunctionOpenType)cfg.FunctionId);
            if (isUnlock)
            {
                JumpManager.Instance.Jump(jumpKey);
                JumpToCallback?.Invoke();
                UIManager.Instance.CloseDialog<JumpView>();
            }
            else
            {
                var str = MainUiManager.Instance.GetFunctionUnLockTips((EFunctionOpenType)cfg.FunctionId);
                ToastManager.Show(str);
            }

        }
    }
}