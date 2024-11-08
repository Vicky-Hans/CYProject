using System;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class TriggerGiftView : BaseView
    {
        public override bool FullScreen => false;

        public ScrollRectExtend scrollRectList;
        [AssetPath] public string prefabPath;
        public DhButton btnClose;
        public GameObject noneTips;
        public CommonTopView commonTopView;
        public UICircularScrollView scrollView;
        [AssetPath]public string scrollViewCell;
        public GameObject giftBanner;
        public GameObject weekBanner;
        public GameObject monthBanner;
        public GameObject dayBanner;
        
        public DhText dayTimeDes;
        public DhText weekTimeDes;
        public DhText monthTimeDes;
        
        public DhText timeCountdown;
        public DhText TriggerGiftProgressText;
        public UICircularScrollView ProgressScrollview;
        [AssetPath]public string ProgressScrollviewCell;
        public DhButton progressInfoBut;
        
        private TriggerGiftViewShowType showType;

        public TriggerGiftViewShowType ShowType
        {
            get => showType;
            set
            {
                showType = value;
                if (scrollRectList!=null)
                {
                    var RtPos = scrollRectList.content.localPosition;
                    scrollRectList.content.localPosition = new Vector3(RtPos.x,10,RtPos.z);
                }
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            scrollView.PrefabPath = scrollViewCell;
            scrollRectList.PrefabPath = prefabPath;
            var bindingSet = this.CreateBindingSet<TriggerGiftView, TriggerGiftViewModel>();
            bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
            bindingSet.Bind(scrollRectList).For(v => v.Collection).To(vm => vm.TriggerGiftList);
            bindingSet.Bind(noneTips).For(v => v.activeSelf).ToExpression(vm => vm.TriggerGiftList.Count==0);		
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.WeekGiftList);
            bindingSet.Bind(giftBanner).For(v => v.activeSelf).ToExpression(vm => vm.BShowType == TriggerGiftViewShowType.Gift);
            bindingSet.Bind(weekBanner).For(v => v.activeSelf).ToExpression(vm => vm.BShowType == TriggerGiftViewShowType.WeekGift);
            bindingSet.Bind(monthBanner).For(v => v.activeSelf).ToExpression(vm => vm.BShowType == TriggerGiftViewShowType.MonthGift);
            bindingSet.Bind(dayBanner).For(v => v.activeSelf).ToExpression(vm => vm.BShowType == TriggerGiftViewShowType.DayGift);
            bindingSet.Bind(this).For(v => v.ShowType).ToExpression(vm => vm.BShowType);
            bindingSet.Bind(commonTopView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopViewModel);
            bindingSet.Bind(weekTimeDes).For(v => v.text).To(vm => vm.WeekTimeDes);
            bindingSet.Bind(monthTimeDes).For(v => v.text).To(vm => vm.MonthTimeDes);
            bindingSet.Bind(dayTimeDes).For(v => v.text).To(vm => vm.DayTimeDes);
            
            ProgressScrollview.PrefabPath = ProgressScrollviewCell;
            bindingSet.Bind(ProgressScrollview).For(v => v.Collection).To(vm => vm.ProgressGiftList);
            bindingSet.Bind(ProgressScrollview).For(v => v.DefaultJumpIndex).To(vm => vm.ProgressGiftNowIndex);
            bindingSet.Bind(TriggerGiftProgressText).For(v => v.text).To(vm => vm.ProgressText);
            bindingSet.Bind(timeCountdown).For(v => v.text).To(vm => vm.TimeCountdown);
            bindingSet.Bind(progressInfoBut).For(v => v.onClick).To(vm => vm.OnClickIconBtn)
                .CommandParameter(() => new Tuple<Vector3, Vector3>(progressInfoBut.transform.position, new Vector3(0,20,0)));
            
            bindingSet.Build();
        }
    }
}