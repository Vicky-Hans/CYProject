using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class LevelCellView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton levelBtn;
		public DhText unChooseLevelInfoText;
		public DhText chooseLevelInfoText;
		public DhButton boxBtn;
		public GameObject effectNode;
		public Animator boxAni;
		public DhImage boxImg;
		public GameObject chooseNode;
		public RectTransform boxRect;
		public GameObject tipsNode;
		private bool alreadyOpen = true;
		

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<LevelCellView, LevelCellViewModel>();
			bindingSet.Bind(levelBtn).For(v => v.onClick).To(vm => vm.OnClickLevelBtnCommand);
			bindingSet.Bind(levelBtn.image).For(v=>v.sprite).To(vm=>vm.LevelOpBtnImgPath).WithConversion(this);
			bindingSet.Bind(unChooseLevelInfoText).For(v => v.text).To(vm => vm.LevelInfoTextStr);
			bindingSet.Bind(unChooseLevelInfoText.gameObject).For(v=>v.activeSelf).ToExpression(vm=>!vm.IsChoose);
			bindingSet.Bind(unChooseLevelInfoText).For(v=>v.color).To(vm=>vm.LevelInfoTextColor);
			bindingSet.Bind(chooseLevelInfoText).For(v => v.text).To(vm => vm.LevelInfoTextStr);
			bindingSet.Bind(chooseLevelInfoText).For(v => v.color).To(vm => vm.LevelInfoTextColor);
			bindingSet.Bind(chooseLevelInfoText.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsChoose);
			bindingSet.Bind(boxBtn).For(v => v.onClick).To(vm => vm.OnClickBoxBtnCommand);
			bindingSet.Bind(chooseNode).For(v=>v.activeSelf).To(vm=>vm.IsChoose);
			bindingSet.Bind(this).For(v=>v.BoxSate).To(vm=>vm.BoxState);
			bindingSet.Bind(this).For(v => v.boxRect).To(vm => vm.BoxRectTransform).OneWayToSource();
			bindingSet.Bind(tipsNode).For(v => v.activeSelf).To(vm => vm.IsShowTipsNode);
            bindingSet.Build();
        }

        private EBoxState boxState;

        public EBoxState BoxSate
        {
	        get=> boxState;
	        set=>UpdateBoxState(value);
        }
        private void UpdateBoxState(EBoxState state)
        {
	        switch (state)
	        {
		        case EBoxState.Close:
		        {
			        boxImg.gameObject.SetActive(true);
			        effectNode.SetActive(false);
			        boxAni.gameObject.SetActive(false);
			        var path = "mainui[mainui_icon_8]";
			        ConvertDirectly(path, boxImg);
		        } break;
		        case EBoxState.Wait:
		        {
			        boxImg.gameObject.SetActive(false);
			        effectNode.SetActive(true);
			        boxAni.gameObject.SetActive(true);
			        boxAni.Play("Idle");
			        alreadyOpen = false;
		        } break;
		        case EBoxState.Open:
		        {
			        effectNode.SetActive(false);
			        if (!alreadyOpen)
			        {
				        boxImg.gameObject.SetActive(false);
				        boxAni.gameObject.SetActive(true);
				        boxAni.StopPlayback();
				        boxAni.Play("Open");
				        alreadyOpen = true;
				        // boxAni.Complete += AniNodeOnComplete;
			        }
			        else
			        {
				        boxImg.gameObject.SetActive(true);
				        boxAni.gameObject.SetActive(false);
				        var path = "mainui[mainui_icon_9]";
				        ConvertDirectly(path, boxImg);
			        }
		        } break;
	        }
        }
    }
}