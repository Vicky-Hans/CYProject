using System;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
    public partial class ItemPriceNodeView : BaseItemView,IViewKey
    {
        public Image icon;
        public DhText priceTxt;
        public DhImage bgNode;
        [NonSerialized]
        public Color startColor;

        public object Key => index;
        public int index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            startColor = priceTxt.color;
            var bindSet = this.CreateBindingSet<ItemPriceNodeView, ItemPriceNodeModel>();
            bindSet.Bind(this).For(v => v.startColor).To(vm => vm.StartColor).OneWayToSource();
            bindSet.Bind(icon).For(v => v.sprite).ToExpression(vm => UIHelper.GetRewardsIconPath(vm.Reward)).WithConversion(this);
            bindSet.Bind(priceTxt).For(v => v.text).ToExpression(vm => GetCountText(vm.ShowLimit,vm.OwnNum,vm.Reward,vm.Desc,vm.BuyNum));
            bindSet.Bind(priceTxt).For(v => v.color).ToExpression(vm => GetShowColor(vm.Deficiency,vm.IsEnough,vm.NoneColor,vm.BuyNum));
            bindSet.Bind(bgNode.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowBg);
            bindSet.Bind(bgNode).For(v => v.sprite).ToExpression(vm => GetBgPath(vm.IsShowBg,vm.BgPath)).WithConversion(this);
            bindSet.Build();
        }

        private string GetBgPath(bool isShowBg, string bgPath)
        {
            return isShowBg ? bgPath : "common[common_panel_01]";
        }

        public Color GetShowColor(bool show,bool isEnouth,Color none,int num)
        {
            if (show)
            {
                return isEnouth ? startColor : none;
            }
            else
            {
                return startColor;
            }
        }

        private string GetCountText(bool vmShowLimit,long vmOwnCnt, Reward vmItemData,string desc,int buyNum)
        {
            var descStr = vmShowLimit ? UIHelper.GetIsEnoughDesc(vmOwnCnt,vmItemData.Count*buyNum) : $"{vmItemData.Count*buyNum}";
            return desc == null ? descStr : string.Format(desc, descStr);
        }

        protected string GetIcon(Reward reward)
        {
            return UIHelper.GetRewardsIconPath(reward.Type, reward.Id);
        }

    }
}