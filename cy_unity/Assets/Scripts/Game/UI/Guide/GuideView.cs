using System;
using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class GuideView : BaseItemView
    {
        public override bool FullScreen => false;
        public GameObject guideNode;
		public DhImage maskImg;
		public DhImage handImg;
		public GameObject descNode;
		public DhText descText;
		public RectTransform targetRect;
		
		public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<GuideView, GuideViewModel>();
            bindingSet.Bind(this).For(v => v.maskImg).To(vm => vm.MaskImg).OneWayToSource();
			bindingSet.Bind(handImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowHandImg);
			bindingSet.Bind(descNode).For(v => v.activeSelf).ToExpression(vm => GetIsShowDesc(vm.IsShowDescNode, vm.DescTextStr));
			bindingSet.Bind(descNode.transform).For(v=>v.localPosition).To(vm=>vm.DescNodePos);
			// bindingSet.Bind(descText).For(v => v.text).To(vm => vm.DescTextStr);
			bindingSet.Bind(guideNode).For(v => v.activeSelf).To(vm => vm.IsShowGuide);
			// bindingSet.Bind(maskImg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowMask);
			bindingSet.Bind(this).For(v => v.targetRect).To(vm => vm.TargetRectTransform).OneWayToSource();
			bindingSet.Bind(this).For(v => v.handImg).To(vm => vm.HandImg).OneWayToSource();
            bindingSet.Build();
        }

        private bool GetIsShowDesc(bool isShow, string desc)
        {
	        if (!isShow) return false;
	        if (desc != null)
	        {
		        var str = UIHelper.InsertLineBreaks(desc, 25);
		        descText.StartTextAnim(str,1.0f);   
	        }
	        // SetGuideText(desc).Forget();
	        return true;
        }

        private async UniTaskVoid SetGuideText(string desc)
        {
	        if(desc ==null)return;
	        await UniTask.Delay(200);
	        var str = UIHelper.InsertLineBreaks(desc, 25);
	        descText.StartTextAnim(str,1.0f);
        }

        public void CheckNextGuide()
        {
	        var allTrigger = FindObjectsOfType<GuideTrigger>();
	        foreach (var item in allTrigger)
	        {
		        item.StartTrigger();
	        }
        }
    }
}