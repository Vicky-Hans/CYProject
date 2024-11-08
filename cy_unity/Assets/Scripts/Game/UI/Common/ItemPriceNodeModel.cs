using System.Collections.Specialized;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class ItemPriceNodeModel: ViewModelBase
    {
        [AutoNotify] private Reward reward;

        [AutoNotify] private Color startColor;
        public Color NoneColor;
        public bool Deficiency;

        [AutoNotify] private long rewardCount;

        private ResourceData data;
        [AutoNotify] private bool showLimit;
        [AutoNotify] private bool isEnough;
        [AutoNotify] private long ownNum;
        [AutoNotify] private bool isShowBg;
        [AutoNotify] private string bgPath;
        [AutoNotify] private string desc;
        [AutoNotify] private int buyNum;
        public ResourceData Data
        {
            get => data;
            set
            {
                var old = data;
                Set(ref data, value);
                if (old != null)
                {
                    old.PropertyChanged -= ItemPropertyChanged;
                }
                
                if (data != null)
                {
                    data.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        [Preserve]
        public ItemPriceNodeModel(Reward reward,bool deficiency = false,string noneColor = null,bool showLimit=false,string desc=null)
        {
            Reward = reward;
            RewardCount = Reward.Count; 
            Deficiency = deficiency;
            ShowLimit = showLimit;
            if (noneColor == null || !ColorUtility.TryParseHtmlString(noneColor, out NoneColor))
            {
                NoneColor = Color.red;
            }

            if (reward.Type == RewardType.Lives)
            {
                DataCenter.livesData.PropertyChanged += LivesPropertyChanged;
                OwnNum = DataCenter.livesData.Curr;
            }
            else
            {
                Data = DataCenter.itemsData.ResourceDatas[reward.Id];
                DataCenter.itemsData.ResourceDatas.CollectionChanged += ResourceDatasChange;
                DataCenter.itemsData.PropertyChanged += ItemPropertyChanged;
                OwnNum = Data?.Count ?? 0;
            }
            
            CostColor();
            Desc = desc;
            isShowBg = false;
            BuyNum = 1;
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            if (reward.Type == RewardType.Lives)
            {
                DataCenter.livesData.PropertyChanged -= LivesPropertyChanged;
            }
            else
            {
                DataCenter.itemsData.ResourceDatas.CollectionChanged -= ResourceDatasChange;
                DataCenter.itemsData.PropertyChanged -= ItemPropertyChanged;
            }

            Data = null;
        }

        private void LivesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CostColor();
            OwnNum =  DataCenter.livesData.Curr;
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CostColor();
            OwnNum = Data?.Count ?? 0; 
        }

        private void ResourceDatasChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            CostColor();
            OwnNum =  Data?.Count ?? 0; 
        }

        public void CostColor()
        {
            IsEnough = !(Deficiency && !Lodash.CheckRewardIsEnough(Reward));
        }

    }
}