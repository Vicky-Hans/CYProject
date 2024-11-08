using DH.Config;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MonthCardView : BaseView
    {
        public override bool FullScreen => false;
        public CommonTopView commonTopView; 
        public MonthCardItemView monthCardItem;
        public PrivilegeMonthCardItemView privilegeMonthCardItem;
        public DhButton closeBut;

        public GameObject timeMonthGo;
        public GameObject permanentMonthGo;
        public BottomComponentView bottomComponentView;
        public GameObject bottomComponentScrollviewGo;
        
        public UICircularScrollView awardScrollview;
        [AssetPath]public string awardScrollviewCell;
        public DhButton doubleAwardBtn;
        public GameObject doubleAwardGo;
        private int doubleAwardType;
        public int DoubleAwardType
        {
            get => doubleAwardType;
            set
            {
                doubleAwardType = value;
                doubleAwardGo.SetActive(doubleAwardType != 2);
                UIHelper.SetGray(doubleAwardBtn.gameObject,doubleAwardType==0,false);
               // bottomComponentScrollviewGo.SetActive(doubleAwardType!=1);
               doubleButText.text = LocalizeHelper.GetGlobal(doubleAwardType == 0 ? GlobalLanguageId.MonthlyVip_tips05 : GlobalLanguageId.Mail_buttonName_2);
            }
        }

        /// 永久卡 
        public ScrollRectExtend  permanentEffectScrollView;
        [AssetPath]public string permanentScrollViewCell;
        public BtnPriceNode permanentPrice;
        public DhButton permanentBtn;

        public GameObject timeCardBg;
        public GameObject permanentCardBg;

        public DhText doubleButText;

        public GameObject PermanentCardEffect;
        public GameObject PermanentCardButText;
        private bool isBuyPermanentCard;

        public bool IsBuyPermanentCard
        {
            get => isBuyPermanentCard;
            set
            {
                isBuyPermanentCard = value;
                UIHelper.SetGray(permanentBtn.gameObject,isBuyPermanentCard,false);
                PermanentCardEffect.SetActive(!isBuyPermanentCard);
                permanentPrice.gameObject.SetActive(!isBuyPermanentCard);
                PermanentCardButText.SetActive(isBuyPermanentCard);
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            awardScrollview.PrefabPath = awardScrollviewCell;
            permanentEffectScrollView.PrefabPath = permanentScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MonthCardView, MonthCardViewModel>();
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopVieVm);
            bindingSet.Bind(monthCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.MonthCardItem);
            bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
            bindingSet.Bind(privilegeMonthCardItem.BindingContext).For(v => v.DataContext).To(vm => vm.PrivilegeMonthCardItem);
            bindingSet.Bind(closeBut).For(v => v.onClick).To(vm => vm.CloseUI);
            bindingSet.Bind(timeMonthGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowType==MonthCardShowType.TimeCard);
            bindingSet.Bind(permanentMonthGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowType==MonthCardShowType.PermanentCard);
            bindingSet.Bind(bottomComponentView.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
            bindingSet.Bind(this).For(v => v.DoubleAwardType).ToExpression(vm => vm.Data.DoubleStatus);
            bindingSet.Bind(permanentPrice.BindingContext).For(v => v.DataContext).To(vm => vm.PriceNodeModel);
            bindingSet.Bind(permanentEffectScrollView).For(v => v.Collection).To(vm => vm.EffectDesList);
            bindingSet.Bind(doubleAwardBtn).For(v => v.onClick).To(vm => vm.OnClickGetDoubleAwardButtonCommand);
            bindingSet.Bind(permanentBtn).For(v => v.onClick).To(vm => vm.BuyPermanentBtnCommand);
            bindingSet.Bind(timeCardBg).For(v => v.activeSelf).ToExpression(vm => vm.ShowType==MonthCardShowType.TimeCard);
            bindingSet.Bind(permanentCardBg).For(v => v.activeSelf).ToExpression(vm => vm.ShowType==MonthCardShowType.PermanentCard);
            bindingSet.Bind(this).For(v => v.IsBuyPermanentCard).ToExpression(vm => vm.Data.LifetimeCard); 
            bindingSet.Build();
        }
    }
}