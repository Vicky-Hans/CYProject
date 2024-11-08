using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine.UI;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class PassportsView : BaseItemView
    {
        public DhText titleText;
		public DhText timeText;
		public Slider expProgress;
		public DhText expText;
		public DhText levelText;
		public DhButton opBtn;
		public DhText opBtnText;
		public BtnPriceNode opBtnPrice;
		public UICircularScrollView passportsScrollView;
		[AssetPath]public string passportsScrollViewCell;
		public CommonTopView commonTopItems;
		public Button expTipsBtn;
		public RawImage bannerImg;
		public Texture[] bannerTextures;
		public DhImage progressBg;
		public Sprite[] progressSprites;
		public RawImage bg;
		public Texture[] bgTextures;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        { 
			passportsScrollView.PrefabPath = passportsScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<PassportsView, PassportsViewModel>();
            bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleTestStr);
			bindingSet.Bind(timeText).For(v => v.text).To(vm => vm.TimeTextStr);
			bindingSet.Bind(expProgress).For(v => v.value).To(vm => vm.ExpProgressValue);
			bindingSet.Bind(expText).For(v => v.text).To(vm => vm.ExpTextStr);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(opBtn).For(v => v.interactable).To(vm => vm.IsCanClickOpBtn);
			bindingSet.Bind(opBtnText).For(v => v.text).To(vm => vm.OpBtnTextStr);
			bindingSet.Bind(opBtnPrice.BindingContext).For(v => v.DataContext).To(vm => vm.OpBtnPriceVm);
			bindingSet.Bind(opBtnPrice.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowCostNode);
			bindingSet.Bind(passportsScrollView).For(v => v.Collection).To(vm => vm.PassportsScrollViewList);
			bindingSet.Bind(passportsScrollView).For(v => v.DefaultJumpIndex).To(vm => vm.ShowIndex);
			bindingSet.Bind(this).For(v => v.ShowIndex).To(vm => vm.ShowIndex);
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(expTipsBtn).For(v => v.onClick).To(vm => vm.OnClickExpTipsBtnCommand);
			bindingSet.Bind(this).For(v => v.CurType).To(vm => vm.CurPassPortType);
            bindingSet.Build();
        }

        private int showIndex;
        public int ShowIndex
		{
			get => showIndex;
			set => DelayJumpIndex(value).Forget();
		}

		private async UniTaskVoid DelayJumpIndex(int index)
		{
			await UniTask.Delay(100);
			passportsScrollView.Jump2SpecificItem(index);
		}

		private EPassPortType curType;

		public EPassPortType CurType
		{
			get=> curType;
			set => UpdatePanelInfo(value);
		}

		private void UpdatePanelInfo(EPassPortType type)
		{
			var index = (int)type - 1;
			if(index < 0)return;
			bannerImg.texture = bannerTextures[index];
			progressBg.sprite = progressSprites[index];
			bg.texture = bgTextures[index];
		}
    }
}