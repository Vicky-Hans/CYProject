using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class ClothesInfoView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText name;
		public DhButton btnClose;
		public CellItemBaseView cellItemBaseView;
		public DhText level;
		public DhImage qualityBg;
		public DhText qualityName;
		public GameObject qualityRareNode;
		public DhButton btnRefresh;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public StaticItemsBindComponent grid;
		public ScrollRectExtend scrollViewAttr;
		[AssetPath]public string scrollViewAttrCell;
		public DhButton btnUse;
		public DhButton btnUnUse;
		public DhButton btnUpLevle;
		public DhButton btnOneKeyUpLevel;
		public DhButton btnUpLevleUnUse;
		public DhButton btnOneKeyUpLevelUnUse;
		public GameObject maxTips;
		public ParticleSystem upEffect;
		public ParticleSystem upEffectAttr;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;
			scrollViewAttr.PrefabPath = scrollViewAttrCell;
			
            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesInfoView, ClothesInfoViewModel>();
            
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
			bindingSet.Bind(btnClose).For(v => v.onClick).To(vm => vm.OnClickBtnCloseCommand);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(level).For(v => v.text).To(vm => vm.LevelStr);
			bindingSet.Bind(qualityBg).For(v => v.sprite).To(vm => vm.QualityBgPath).WithConversion(this);
			bindingSet.Bind(qualityName).For(v => v.text).To(vm => vm.QualityNameStr);
			bindingSet.Bind(qualityRareNode).For(v => v.activeSelf).To(vm => vm.IsHightRare);
			bindingSet.Bind(btnRefresh).For(v => v.onClick).To(vm => vm.OnClickBtnRefreshCommand);
			bindingSet.Bind(btnRefresh.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && vm.IsShowResetBtn);
			
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(grid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetGridCellCallback);
			bindingSet.Bind(grid).For(v => v.Collection).To(vm => vm.GridDictionary);
			bindingSet.Bind(grid.gameObject).For(v => v.activeSelf).ToExpression(vm =>vm.IsOwn && !vm.IsMaxLevel);

			bindingSet.Bind(scrollViewAttr).For(v => v.Collection).To(vm => vm.AttrScrollViewList);
			
			bindingSet.Bind(btnUse).For(v => v.onClick).To(vm => vm.OnClickBtnUnUseCommand).CommandParameter(true);;
			bindingSet.Bind(btnUnUse).For(v => v.onClick).To(vm => vm.OnClickBtnUnUseCommand).CommandParameter(false);;
			bindingSet.Bind(btnUpLevle).For(v => v.onClick).To(vm => vm.OnClickBtnUpLevelCommand);
			bindingSet.Bind(btnOneKeyUpLevel).For(v => v.onClick).To(vm => vm.OnClickBtnOneKeyUpLevelCommand);

			bindingSet.Bind(btnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && !vm.IsUseIng);
			bindingSet.Bind(btnUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && vm.IsUseIng);
			bindingSet.Bind(btnUpLevle.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && !vm.IsMaxLevel && !vm.IsAllMaxLevel);
			bindingSet.Bind(btnOneKeyUpLevel.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && !vm.IsMaxLevel  && !vm.IsAllMaxLevel);
			
			
			bindingSet.Bind(btnUpLevleUnUse).For(v => v.onClick).To(vm => vm.OnClickBtnUpLevelCommand);
			bindingSet.Bind(btnOneKeyUpLevelUnUse).For(v => v.onClick).To(vm => vm.OnClickBtnOneKeyUpLevelCommand);
			bindingSet.Bind(btnUpLevleUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && vm.IsMaxLevel && !vm.IsAllMaxLevel);
			bindingSet.Bind(btnOneKeyUpLevelUnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsOwn && vm.IsMaxLevel  && !vm.IsAllMaxLevel);
			
			bindingSet.Bind(maxTips).For(v => v.activeSelf).ToExpression(vm =>vm.IsOwn && vm.IsAllMaxLevel);
			bindingSet.Bind(this).For(v => v.upEffect).To(vm => vm.UpEffect).OneWayToSource();
			bindingSet.Bind(this).For(v => v.upEffectAttr).To(vm => vm.UpEffectAttr).OneWayToSource();
			bindingSet.Bind(scrollView.GetComponent<RectTransform>()).For(v => v.sizeDelta).ToExpression(vm => GetScrollViewSize(vm.IsOwn));
            bindingSet.Build();
        }

        private Vector2 GetScrollViewSize(bool isShowInfo)
        {
	        return isShowInfo ? new Vector2(920, 531) : new Vector2(920, 825);
        }
    }
}