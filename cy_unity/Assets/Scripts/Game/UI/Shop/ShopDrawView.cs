using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopDrawView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public Slider slider;
		public DhText progressValue;
		public DhButton btnProgressTips;
		public DhText lvValue;
		public ShopBoxItemView shopBoxItemView1;
		public ShopBoxItemView shopBoxItemView2;
		public ShopBoxItemView shopBoxItemView3;
		public DhText titleName;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopDrawView, ShopDrawViewModel>();
			bindingSet.Bind(slider).For(v => v.value).To(vm => vm.SliderValue);
			bindingSet.Bind(lvValue).For(v => v.text).To(vm => vm.LvValueStr);
			bindingSet.Bind(progressValue).For(v => v.text).To(vm => vm.ProgressValueStr);
			bindingSet.Bind(btnProgressTips).For(v => v.onClick).To(vm => vm.OnClickBtnProgressTipsCommand);
			bindingSet.Bind(shopBoxItemView1.BindingContext).For(v => v.DataContext).To(vm => vm.ShopBoxItemViewModels[0]);
			bindingSet.Bind(shopBoxItemView2.BindingContext).For(v => v.DataContext).To(vm => vm.ShopBoxItemViewModels[1]);
			bindingSet.Bind(shopBoxItemView3.BindingContext).For(v => v.DataContext).To(vm => vm.ShopBoxItemViewModels[2]);
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleName);
            bindingSet.Build();
        }
    }
}