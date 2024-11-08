using System;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MagicDrawRewardCellViewModel : ViewModelBase
    {
        [AutoNotify] private Vector2 bgSize;
		[AutoNotify] private string bgPath;
        [AutoNotify] private Vector2 iconSize;
		[AutoNotify] private string iconPath;
		[AutoNotify] private bool isShowLimitNode;
        [AutoNotify] private bool isShowBg=true;
        [AutoNotify] private bool isGray;
        [AutoNotify] private Vector3 iconPos;
        private Vector2[] bgSizeArray = new Vector2[] {new Vector2(188, 188), new Vector2(130, 130)};
        private Vector2[] iconSizeArray = new Vector2[] {new Vector2(140, 140), new Vector2(100, 100)};
        private Vector3[] countPosArray = new Vector3[] {new Vector3(-10, -10, 0), new Vector3(-25, -46)};
        [AutoNotify] private bool isShowCount;
        private PrayJackpotCfg curCfg;
        [AutoNotify] private string itemCountStr;
        [AutoNotify] private Vector3 itemCountPos;
        [AutoNotify] private bool isShowEffectNode1;
        [AutoNotify] private bool isShowEffectNode2;
        
        [Preserve]
        public MagicDrawRewardCellViewModel(PrayJackpotCfg cfg)
        {
            curCfg = cfg;
            IsShowLimitNode = curCfg.Type == 1;
            var reward = curCfg.Reward[0];
            IconPath = UIHelper.GetRewardsIconPath(reward);
            ItemCountStr = reward.Count.ToString();
            var quality = UIHelper.GetRewardsQuality(reward.Type, reward.Id);
            if (quality < 2) quality = 2;
            if (quality > 5) quality = 5;
            BgPath = curCfg.Type ==1 ? $"wish[wish_limitpanel_{quality}]" : $"wish[wish_ordpanel_{quality}]";
            itemCountPos = curCfg.Type == 1 ? countPosArray[0] : countPosArray[1];
            IconPos = curCfg.Type == 1? new Vector3(0,9,0):Vector3.zero;
        }
        
        [Command]
        private void OnClickIconBtn(Tuple<Vector3, Vector3> info)
        {
            var baseData = new ResourceData()
            {
                Id = curCfg.Reward[0].Id,
                Count = curCfg.Reward[0].Count,
                Type = (int)curCfg.Reward[0].Type
            };
            UIHelper.OpenItemTips(baseData, info);
        }

        public void UpdatePanel()
        {
            var curCount =  curCfg.Frequency - DataCenter.magicDrawData.GetDrawRecord(curCfg.Id);
            IsGray = curCount <= 0;
            IsShowBg = curCount > 0 || curCfg.Type != 1;
            IsShowEffectNode1 = false;
            IsShowEffectNode2 = false;
        }

        public void PlayEffect()
        {
            if (curCfg.Type == 1)
            {
                IsShowEffectNode2 = true;
            }
            else
            {
                IsShowEffectNode1 = true;
            }
        }
    }
}