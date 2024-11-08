using System;
using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
using Spine;
using Spine.Unity;

namespace DH.Game.UIViews
{
    public partial class ClothesMergeSucceedView : BaseView
    {
        public override bool FullScreen => false;
        
		public GameObject mergeAnimation;
		public GameObject mergeSucceed;
		public DhText itemName;
		public CellItemBaseView cellItemBaseView;
		public DhText startLevel;
		public DhText nextLevel;
		public DhText attrName;
		public DhText startAtk;
		public DhText nextAtk;
		public DhText skillDesc;
		public GameObject skillNode;
		public DhButton btnClose;
		[NonSerialized] public int startAnimation;

		public ParticleSystem mergeEffect;
		public ParticleSystem mergeEffect1;
		public SkeletonGraphic mergeTitleSpine;
		public int StartAnimation
		{
			get => startAnimation;
			set
			{
				startAnimation = value;
				StartAnimationPlay(startAnimation);
			}
		}

		public SkeletonGraphic spineSucceedBg;
		[NonSerialized] private int mergeNum;
		public Action ResultAction;
		public StaticItemsBindComponent grid;
		public ClickTextComponent clickText;
		public DhButton skipBtn;

		private TrackEntry bgAnimation;
		private long audioId;
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesMergeSucceedView, ClothesMergeSucceedViewModel>();
			bindingSet.Bind(itemName).For(v => v.text).To(vm => vm.ItemNameStr);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(startLevel).For(v => v.text).To(vm => vm.StartLevelStr);
			bindingSet.Bind(nextLevel ).For(v => v.text).To(vm => vm.NextLevelStr);
			bindingSet.Bind(startAtk).For(v => v.text).To(vm => vm.StartAtkStr);
			bindingSet.Bind(nextAtk).For(v => v.text).To(vm => vm.NextAtkStr);
			bindingSet.Bind(skillDesc).For(v => v.text).To(vm => vm.SkillDescStr);
			bindingSet.Bind(skillNode).For(v => v.activeSelf).ToExpression(vm => vm.SkillDescStr != string.Empty);
			bindingSet.Bind(attrName).For(v => v.text).To(vm => vm.AttrName);
			bindingSet.Bind(this).For(v => v.StartAnimation).To(vm => vm.ShowNum);
			// bindingSet.Bind(this).For(v => v.startAnimation).ToExpression(vm => StartAnimationOpen(vm.IsShowMergeSucceed,vm.ShowNum));
			bindingSet.Bind(this).For(v => v.ResultAction).To(vm=>vm.ResultAction);
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(grid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetGridCellCallback);
			bindingSet.Bind(grid).For(v => v.Collection).To(vm => vm.GridDictionary);
			bindingSet.Bind(this).For(v => v.clickText).To(vm => vm.ClickTextCmp).OneWayToSource();
			
            bindingSet.Build();
            skipBtn.onClick.AddListener(SkipBtnClick);
        }
		
        public void SkipBtnClick()
        {
	        AnimatorEnd();
        }


        public bool StartAnimationOpen(bool isSucceed,int num)
        {
	        // StartAnimationPlay(num);
	        return isSucceed;
        }
        
        private void StartAnimationPlay(int num = 5)
        {
	        if (num == 0) return;
	        mergeNum = num;
	        btnClose.gameObject.SetActive(false);
	        mergeAnimation.gameObject.SetActive(true);
	        mergeSucceed.gameObject.SetActive(false);
	        AssetLoaded();
        }
        
        private void AssetLoaded()
        {
	         
			audioId = AudioManager.Instance.PlayUIAudio("ui_clothing_mergeAll");
	         // AudioManager.Instance.PlayAudio("SFX_UI/ui_chip_merge01");
	         bgAnimation = spineSucceedBg.AnimationState.SetAnimation(0, "hecheng_"+mergeNum, false);
	         spineSucceedBg.AnimationState.Complete += (trackentry) =>
	         {
		         AnimatorEnd();
	         };
        }
        
        private void AnimatorEnd()
        {
            AudioManager.Instance.StopAudio(audioId);
            if (mergeNum == 2)
            {
	            audioId = AudioManager.Instance.PlayUIAudio("ui_clothing_merge2");
            }else if (mergeNum == 3)
            {
	            audioId = AudioManager.Instance.PlayUIAudio("ui_clothing_merge3");
            }
            spineSucceedBg.AnimationState.ClearTrack(bgAnimation.TrackIndex);
            if (mergeNum != 5)
            {
	            mergeSucceed.gameObject.SetActive(true);
	            mergeAnimation.gameObject.SetActive(false);
	            UIHelper.PlayEffect(mergeEffect);
	            UIHelper.PlayEffect(mergeEffect1);
	            mergeTitleSpine.AnimationState.SetAnimation(0, "animation",false);
            }
            ResultShow().Forget();
        }
         private async UniTask ResultShow()
        {
            ResultAction?.Invoke();
            btnClose.gameObject.SetActive(true);
        }  
 
        
    }
}