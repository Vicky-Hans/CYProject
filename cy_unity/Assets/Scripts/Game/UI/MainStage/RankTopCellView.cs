using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RankTopCellView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
        
		public GameObject noRank;
		public GameObject infoNode;
		public CommonHeadItemView commonHeadItemView;
		public DhText levelText;
		public int index;
		public CommonPlayerNameView commonPlayerNameView;
		public object Key => index;
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RankTopCellView, RankTopCellViewModel>();
			bindingSet.Bind(noRank).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowInfoNode);
			bindingSet.Bind(infoNode).For(v => v.activeSelf).To(vm => vm.IsShowInfoNode);
			bindingSet.Bind(commonHeadItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonHeadItemViewVm);
			bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
			bindingSet.Bind(levelText).For(v => v.text).To(vm => vm.LevelTextStr);
            bindingSet.Build();
        }
    }
}