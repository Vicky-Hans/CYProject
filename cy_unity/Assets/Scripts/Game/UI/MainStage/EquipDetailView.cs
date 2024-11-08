using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipDetailView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText titleText;
		public DhImage equipTypeIcon;
		public DhText tipsText;
		public DhImage equipIcon;
		public UICircularScrollView infoScrollView;
		[AssetPath]public string infoScrollViewCell;
		public UICircularScrollView equipSkillScrollView;
		[AssetPath]public string equipSkillScrollViewCell;
		public DhButton opBtn;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
			infoScrollView.PrefabPath = infoScrollViewCell;
			equipSkillScrollView.PrefabPath = equipSkillScrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<EquipDetailView, EquipDetailViewModel>();
			bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleTextStr);
			bindingSet.Bind(equipTypeIcon).For(v=>v.sprite).To(vm=>vm.EquipTypeIconPath).WithConversion(this);
			bindingSet.Bind(equipTypeIcon.gameObject).For(v=>v.activeSelf).To(vm=>vm.IsShowEquipTypeIcon);
			bindingSet.Bind(tipsText).For(v => v.text).To(vm => vm.TipsTextStr);
			bindingSet.Bind(equipIcon).For(v => v.sprite).To(vm => vm.EquipIconPath).WithConversion(this);
			bindingSet.Bind(infoScrollView).For(v => v.Collection).To(vm => vm.InfoScrollViewList);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(equipSkillScrollView).For(v=>v.Collection).To(vm=>vm.EquipSkillScrollViewList);
            bindingSet.Build();
            infoScrollView.m_CanDragScrollView = false;
        }
    }
}