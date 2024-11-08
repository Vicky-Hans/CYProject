using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class BoosterPackItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public GameObject arrowNode;
		public DhImage arrowBg;
		public DhImage arrowImg;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton opBtn;
		public Image lockImg;
		public BtnPriceNode btnPriceNode;
		public DhText limitNums;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<BoosterPackItemView, BoosterPackItemViewModel>();
            
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(arrowNode).For(v => v.activeSelf).To(vm => vm.IsShowArrowNode);
			bindingSet.Bind(arrowBg).For(v=>v.sprite).To(vm=>vm.ArrowBgPath).WithConversion(this);
			bindingSet.Bind(arrowImg).For(v=>v.sprite).To(vm=>vm.ArrowImgPath).WithConversion(this);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtn.image).For(v=>v.sprite).To(vm=>vm.OpBtnImgPath).WithConversion(this);
			bindingSet.Bind(lockImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowLockPath);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
			bindingSet.Bind(limitNums).For(v => v.text).To(vm => vm.LimitNumsStr);

            bindingSet.Build();
        }
    }
}