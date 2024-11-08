using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class FreeBuyActivityItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage bg;
		public DhText desc;
		public StaticItemsBindComponent grid;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<FreeBuyActivityItemView, FreeBuyActivityItemViewModel>();
            
			bindingSet.Bind(bg).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
			bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);
			bindingSet.Bind(grid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetGridCellCallback);
			bindingSet.Bind(grid).For(v => v.Collection).To(vm => vm.GridDictionary);

            bindingSet.Build();
        }
    }
}