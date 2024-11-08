using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.U2D;

namespace DH.Game.ViewModels
{
    public partial class CanClaimItemViewModel:ViewModelBase
    {
        [AutoNotify] private string bgImgPath;
        [AutoNotify] private string iconPath;
        [AutoNotify] private string countStr;
        [AutoNotify] private bool isClaimed;
        [AutoNotify] private bool isShowPartIcon;
        [AutoNotify] private string partIconPath;
        [AutoNotify] private Vector3 localScale = Vector3.one;
        [AutoNotify] private bool isShowEffect;
        [AutoNotify] private SpriteAtlas seasonAtlas;
        [AutoNotify] private string tipsIconPath = "common[commom_icon_4]";
        [AutoNotify] private bool isShowTips;
        [AutoNotify] private bool isShowCount = true;
        private Transform iconEffectParent;

        public Transform IconEffectParent
        {
            get => null;
            set
            {
                iconEffectParent = value;
                UpDateIconEffect();
            }
        }
        private Transform seasonParent;

        public Transform SeasonParent
        {
            get => null;
            set
            {
                seasonParent = value;
                UpdateSeasonInfo();
            }
        }
        
        private Reward reward;
        public List<long> param = new();
        private readonly SimpleCommand<Tuple<Vector3, Vector3>> clickIconBtn;
        public ICommand OnClickIconBtn => clickIconBtn;
        /// <summary>
        ///  Tips偏移量
        /// </summary>
        private Vector2 tipsOffset;
        public CanClaimItemViewModel(MailRewards rewardsInfo, bool isClaimed, Vector2 tipsOffset = default)
        {
            reward = new Reward((RewardType)rewardsInfo.type, rewardsInfo.id, rewardsInfo.count);
            param = rewardsInfo.param;
            this.tipsOffset = tipsOffset;
            clickIconBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickItem);
            IsClaimed =  isClaimed;
            // if ((RewardType)rewardsInfo.type == RewardType)
            // {
            //     IsShowPartIcon = true;
            //     var chipId = EquipManager.Instance.GetChipIdById(rewardsInfo.id);
            //     var cfg = ConfigCenter.EquipChipResourceCfgColl.GetDataById(chipId);
            //     var partCfg = ConfigCenter.EquipPartCfgColl.GetDataById(cfg.ChipPart);
            //     PartIconPath =$"itemicon[{partCfg.EquipIconMini}]"; 
            // }
            // else
            {
                IsShowPartIcon = false;
            }

            UpdateMailInfo();
            UpDateIconEffect();
            UpdateTipsInfo();
        }

        public void UpdatePanel()
        {
            UpdateMailInfo();
            UpDateIconEffect();
            UpdateTipsInfo();
        }

        public CanClaimItemViewModel(Reward rewardsInfo, bool isClaimed)
        {
            reward = rewardsInfo;
            UpdateMailInfo();
            IsClaimed =  isClaimed;
            clickIconBtn = new SimpleCommand<Tuple<Vector3, Vector3>>(OnClickItem);
            
        }

        private void UpdateMailInfo()
        {
            
            BgImgPath = UIHelper.GetRewardsBgPath(reward.Type, reward.Id);
            if (reward.Type == RewardType.Equip)
            {
                 IconPath = EquipManager.Instance.GetIconPath(reward.Id);
                 BgImgPath = EquipManager.Instance.GetBgPathByEquipId(reward.Id);
            }
            else
            {
                IconPath = UIHelper.GetRewardsIconPath((int)reward.Type, reward.Id);
            }
            CountStr = Lodash.ConvertNumToString(reward.Count);
        }

        public void OnClickItem(Tuple<Vector3, Vector3> info)
        {
            bool isOpOffset = false;
            var offset = info.Item2;
            if (tipsOffset != default)
            {
                offset.x += tipsOffset.x;
                offset.y += tipsOffset.y;
                isOpOffset = true;
            }
            CommonItemTipsData data = new CommonItemTipsData(reward,info.Item1, offset);
            // if (reward.Type == RewardType.Chip && param is { Count: > 0 })
            // {
            //     var chipId = EquipManager.Instance.GetChipIdById(reward.Id);
            //     var newId = (int)param[0] * 1000 + chipId;
            //     Reward tempReward = new Reward(reward.Type, newId, reward.Count);
            //
            //     data = new CommonItemTipsData(tempReward,info.Item1, tipsOffset);
            // }
            UIHelper.OpenCommonItemTipsVIew (data,isOpOffset);
        }

        private void UpDateIconEffect()
        {
            if(iconEffectParent == null) return;
            while (iconEffectParent.childCount>0)
            {
                var tempNode = iconEffectParent.GetChild(0).gameObject;
                tempNode.transform.SetParent(null);
                AssetsManager.ReleaseInstance(tempNode);
            }
            if(isClaimed)return;
            // 添加IconD动效
            var effectName = UIHelper.GetRewardsIconEffectPath(reward.Type, reward.Id);
            iconEffectParent.gameObject.SetActive(effectName != null);
            if(effectName == null)return;
            UIEffectManager.Instance.AddItemIconEffect(effectName, iconEffectParent).Forget();
        }
        
        private void UpdateSeasonInfo() {
            
            if(seasonAtlas == null )return;
            if(seasonParent == null) return;
            for (int i = 0; i < seasonParent.childCount; i++)
            {
                var tempNode = seasonParent.GetChild(i).gameObject;
                AssetsManager.ReleaseInstance(tempNode);
            }
            if(reward.Type!= RewardType.Head) return;

            var seasonId = DataCenter.charcaterData.GetHeadSeasonById(reward.Id);

            if (seasonId > 0)
            {
                UIEffectManager.Instance.AddSeason(seasonId,seasonParent,seasonAtlas,"g_").Forget(); 
            }
        }
        public bool CheckIsSameReward(Resource rewards)
        {
            if(reward.Type != (RewardType)rewards.Type 
               || reward.Id != rewards.Id)
                return false;
            return true;
        }
        public void DoDoubleAction(int count)
        {
            var starValue = Vector3.one * 1.3f;
            var endValue = Vector3.one;
            LocalScale = starValue;
            float animationDuration = 1f;
            
            DOVirtual.Vector3(starValue, endValue, animationDuration,v => { LocalScale =v; });  
            CountStr = GetShowNum(count) ;
        }
        private string GetShowNum(int count=0)
        {
            if (reward.Count + count >= 0)
            {
                return (reward.Count + count).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void UpdateTipsInfo()
        {
            if(reward == null) return;
            switch (reward.Type)
            {
                case RewardType.Item:
                {
                  var cfg = ConfigCenter.ItemCfgColl.GetDataById(reward.Id);
                  if (cfg!=null && cfg.Type == 2)
                  {
                      isShowTips = true;
                  }
                  else
                  {
                      IsShowTips = false;
                  }
                } break;
                
            }
        }
    }
}