using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ShopBoxRewardView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton btnClose;
		public DhButton btnDel;
		public DhButton btnAdd;
		public DhText levelValue;
		public ScrollRectExtend scrollView;
		[AssetPath]public string scrollViewCell;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ShopBoxRewardView, ShopBoxRewardViewModel>();
            
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(btnDel).For(v => v.onClick).To(vm => vm.OnClickBtnDelCommand);
			bindingSet.Bind(btnAdd).For(v => v.onClick).To(vm => vm.OnClickBtnAddCommand);
			
			bindingSet.Bind(btnDel.GetComponent<DhImage>()).For(v => v.sprite).ToExpression(vm => GetBtnImagePath(vm.SelectLv>1)).WithConversion(this);
			bindingSet.Bind(btnAdd.GetComponent<DhImage>()).For(v => v.sprite).ToExpression(vm => GetBtnImagePath(!ShopManager.Instance.IsEquipChestMax(vm.SelectLv))).WithConversion(this);
			bindingSet.Bind(levelValue).For(v => v.text).ToExpression(vm => GetLvDesc(vm.SelectLv));
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);

            bindingSet.Build();
        }

        private string GetBtnImagePath(bool isOpen)
        {
	        return isOpen ? "shop[shop_button_1]" : "shop[shop_button_2]";
        }

        private string GetLvDesc(int lv)
        {
	        return $"Lv.{lv}";
        }
    }
}