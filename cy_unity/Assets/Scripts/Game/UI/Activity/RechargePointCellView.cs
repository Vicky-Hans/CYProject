using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RechargePointCellView : BaseItemView
    {
        public override bool FullScreen => false;
		public DhText titleText;
		public DhText idText;
		public UICircularScrollView infoScrollView;
		[AssetPath]public string infoScrollViewCell;
		public DhButton opBtn;
		public BtnPriceNode btnPriceNode;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			infoScrollView.PrefabPath = infoScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<RechargePointCellView, RechargePointCellViewModel>();
			bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleTextStr);
			bindingSet.Bind(idText).For(v => v.text).To(vm => vm.IdTextStr);
			bindingSet.Bind(infoScrollView).For(v => v.Collection).To(vm => vm.InfoScrollViewList);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(btnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceNodeVm);
            bindingSet.Build();
        }
    }
}