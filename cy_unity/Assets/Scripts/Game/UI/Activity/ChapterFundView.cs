using Cysharp.Threading.Tasks;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ChapterFundView : BaseItemView
    {
		public DhButton opBtn;
		public UICircularScrollView passportsScrollView;
		[AssetPath]public string passportsScrollViewCell;
		public BtnPriceNode btnPriceNode;
		public GameObject buyBtn;
		public GameObject buyDes;
		public CommonTopView commonTopItems;
		private int showIndex;
		public int ShowIndex
		{
			get => showIndex;
			set => DelayJumpIndex(value).Forget();
		}
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			passportsScrollView.PrefabPath = passportsScrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ChapterFundView, ChapterFundViewModel>();
            
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(passportsScrollView).For(v => v.Collection).To(vm => vm.PassportsScrollViewList);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeMod);
			bindingSet.Bind(buyBtn).For(v => v.activeSelf).ToExpression(vm => !vm.Data.Plus);
			bindingSet.Bind(buyDes).For(v => v.activeSelf).ToExpression(vm => vm.Data.Plus);
			bindingSet.Bind(this).For(v => v.ShowIndex).To(vm => vm.ShowIndex);
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
            bindingSet.Build();
        }
        
        private async UniTaskVoid DelayJumpIndex(int index)
        {
	        await UniTask.Delay(100);
	        passportsScrollView.Jump2SpecificItem(index);
        }
        
    }
}