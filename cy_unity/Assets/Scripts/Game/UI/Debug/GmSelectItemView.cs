using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class GmSelectItemView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView scrollView;
		[AssetPath]public string[] scrollViewCell;
		
		public UICircularScrollView scrollView1;
		[AssetPath]public string[] scrollViewCell1;
		public DhButton btnClose;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.GetPrefabPathFunction = (pos, model) =>
			{
				if (model is CellItemBaseViewModel) return scrollViewCell[0];
				return string.Empty;
			};

			scrollView1.GetPrefabPathFunction = (pos, model) =>
			{
				if (model is TalentChooseItemViewModel) return scrollViewCell1[0];
				return string.Empty;
			};

            await base.Create();
            var bindingSet = this.CreateBindingSet<GmSelectItemView, GmSelectItemViewModel>();
            
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(scrollView1).For(v => v.Collection).To(vm => vm.ScrollViewList1);
			
			bindingSet.Bind(scrollView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ParamType!=GmParamType.Talent);
			bindingSet.Bind(scrollView1.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ParamType==GmParamType.Talent);
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);

            bindingSet.Build();
        }
    }
}