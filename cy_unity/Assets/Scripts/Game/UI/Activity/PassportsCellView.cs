using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class PassportsCellView : BaseItemView
    {
        public override bool FullScreen => false;
		public UICircularScrollView normalScrollview;
		[AssetPath]public string normalScrollviewCell;
		public GameObject normalMaskNode;
		public UICircularScrollView plusScrollview;
		[AssetPath]public string plusScrollviewCell;
		public GameObject plusMaskNode;
		public DhText levelText;
		public DhButton unLockBtn;
		public DhImage normalBg;
		public DhImage plusBg;
		private void AddScrollRectListener(DragEventTriggerListener listener)
		{
			if (listener)
			{
				ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
				listener.BeginDragHandle = scrollRect.OnBeginDrag;
				listener.OnDragHandle = scrollRect.OnDrag;
				listener.EndDragHandle = scrollRect.OnEndDrag;
			}
		}
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {

	        // AddScrollRectListener(normalScrollview.GetComponent<DragEventTriggerListener>());
	        // AddScrollRectListener(plusScrollview.GetComponent<DragEventTriggerListener>());
			normalScrollview.PrefabPath = normalScrollviewCell;
			plusScrollview.PrefabPath = plusScrollviewCell;
			
            await base.Create();
            var bindingSet = this.CreateBindingSet<PassportsCellView, PassportsCellViewModel>();
            bindingSet.Bind(normalScrollview).For(v => v.Collection).To(vm => vm.NormalScrollviewList);
            bindingSet.Bind(normalScrollview).For(v => v.m_CanDragScrollView).To(vm => vm.IsCanDrag);
			bindingSet.Bind(normalMaskNode).For(v => v.activeSelf).To(vm => vm.IsShowNormalMaskNode);
			bindingSet.Bind(plusScrollview).For(v => v.Collection).To(vm => vm.PlusScrollviewList);
			bindingSet.Bind(plusScrollview).For(v => v.m_CanDragScrollView).To(vm => vm.IsCanDrag);
			bindingSet.Bind(plusMaskNode).For(v => v.activeSelf).To(vm => vm.IsShowPlusMaskNode);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
			bindingSet.Bind(unLockBtn).For(v => v.onClick).To(vm => vm.OnClickUnLockBtnCommand);
			bindingSet.Bind(unLockBtn.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowUnLockBtn);
			bindingSet.Bind(normalBg).For(v => v.sprite).To(vm => vm.NormalBgPath).WithConversion(this);
			bindingSet.Bind(plusBg).For(v => v.sprite).To(vm => vm.PlusBgPath).WithConversion(this);
            bindingSet.Build();
            normalScrollview.m_CanDragScrollView = false;
            plusScrollview.m_CanDragScrollView = false;
        }
    }
}