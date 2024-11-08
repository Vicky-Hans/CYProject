using System;
using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class TriggerGiftItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhText name;
        public DhText tipsDesc;
        public ScrollRectExtend scrollViewItem;
        [AssetPath]public string scrollViewItemCell;
        public DhButton btnBuy;
        public DhButton btnFree;
        public BtnPriceNode btnPriceNode;
        public DhImage titleIcon;
        public DhText discountValue;
        public DhButton btnUnLock;
        public RectTransform titleTf;
        public GameObject discountBg;
        public DhImage titleBg;
        [NonSerialized] public bool isOpen;
        public DhText weekLimitText;
        public DhText monthLimitText;
        public DhText dayLimitText;
        public bool IsOpen
        {
            get => isOpen;
            set
            {
                isOpen = value;
                PlayCloseEffect(isOpen);
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            scrollViewItem.PrefabPath = scrollViewItemCell;
            var bindingSet = this.CreateBindingSet<TriggerGiftItemView, TriggerGiftItemViewModel>();
            bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
            bindingSet.Bind(scrollViewItem).For(v => v.Collection).To(vm => vm.ScrollViewItemList);
            bindingSet.Bind(btnBuy).For(v => v.onClick).To(vm => vm.OnClickBtnBuyCommand);
            bindingSet.Bind(btnFree).For(v => v.onClick).To(vm => vm.OnClickBtnBuyCommand);
            bindingSet.Bind(btnBuy.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsFree);
            bindingSet.Bind(btnFree.gameObject).For(v => v.activeSelf).To(vm => vm.IsFree);
            bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeModel);
            bindingSet.Bind(discountValue).For(v => v.text).To(vm => vm.DiscountValueStr);
            bindingSet.Bind(tipsDesc).For(v => v.text).To(vm => vm.EndTimeDesc);
            bindingSet.Bind(this).For(v => v.IsOpen).To(vm =>vm.IsOpen);
            bindingSet.Bind(titleIcon).For(v => v.sprite).To(vm => vm.TitleIconPath).WithConversion(this);
            bindingSet.Bind(titleBg).For(v => v.sprite).To(vm => vm.TitleBgPath).WithConversion(this);
            bindingSet.Bind(titleTf).For(v => v.localPosition).To(vm => vm.TitleIconPosition);
            bindingSet.Bind(titleTf).For(v => v.sizeDelta).To(vm => vm.TitleIconSize);
            bindingSet.Bind(discountBg).For(v => v.activeSelf).ToExpression(vm =>!vm.IsFree && !string.IsNullOrEmpty(vm.DiscountValueStr));
            
            bindingSet.Bind(weekLimitText).For(v => v.text).To(vm => vm.WeekLimitNumsDes);
            bindingSet.Bind(weekLimitText.transform.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsWeek);
            
            bindingSet.Bind(monthLimitText).For(v => v.text).To(vm => vm.MonthLimitNumsDes);
            bindingSet.Bind(monthLimitText.transform.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsMonth);
            
            bindingSet.Bind(dayLimitText).For(v => v.text).To(vm => vm.MonthLimitNumsDes);
            bindingSet.Bind(dayLimitText.transform.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsDay);
            
            bindingSet.Bind(tipsDesc.transform.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsWeek && !vm.IsMonth && !vm.IsDay);
            
            bindingSet.Build();
        }

        private void PlayCloseEffect(bool b)
        {
            if (b)
            {
                transform.localScale = Vector3.one;
            }
            else
            {
                transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InFlash);
            }
        }
    }
}