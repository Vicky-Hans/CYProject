using System.Collections.Generic;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public ScrollRectExtend scrollViewTop;
		[AssetPath]public string scrollViewTopCell;

		public ScrollRectExtend scrollViewAttr;
		[AssetPath]public string scrollViewAttrCell;
		public DhButton btnTips;
		public DhButton btnSort;
		public DhText sortTitleName;
		public DhButton btnMerge;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public TabBtnGroupView tabBtnGroupView;
		public GameObject heroEquipNode;
		public GameObject roleSpineGo;
		public GameObject btnMergeRed;
		
		public RoleView roleView;
		public GameObject moveParent;
		
		
		public List<Texture2D> QltTextures;
        
		public RawImage qualityBg;
		public DhText noneTips;
		private int qlt;
		public int Qlt
		{
			get => qlt;
			set
			{
				qlt = value;
				if (qlt-3<0 || qlt-3 > QltTextures.Count-1)
				{
					qualityBg.texture = QltTextures[0]; 
					return;
				}
				qualityBg.texture = QltTextures[qlt-3];
			}
		}
		
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollViewTop.PrefabPath = scrollViewTopCell;
			scrollViewAttr.PrefabPath = scrollViewAttrCell;
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesView, ClothesViewModel>();
            bindingSet.Bind(this).For(v => v.Qlt).ToExpression(vm => vm.Quality);
			bindingSet.Bind(scrollViewTop).For(v => v.Collection).To(vm => vm.ScrollViewTopList);
			bindingSet.Bind(scrollViewAttr).For(v => v.Collection).To(vm => vm.ScrollViewAttrList);
			bindingSet.Bind(this).For(v => v.roleSpineGo).To(vm => vm.EffectParentNode).OneWayToSource();
			bindingSet.Bind(btnTips).For(v => v.onClick).To(vm => vm.OnClickBtnTipsCommand);
			bindingSet.Bind(btnSort).For(v => v.onClick).To(vm => vm.OnClickBtnSortCommand);
			bindingSet.Bind(sortTitleName).For(v => v.text).To(vm => vm.SortTitleNameStr);
			bindingSet.Bind(btnMerge).For(v => v.onClick).To(vm => vm.OnClickBtnMergeCommand);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(tabBtnGroupView.BindingContext).For(v => v.DataContext).To(vm => vm.TabBtnViewModel);
			bindingSet.Bind(roleView.BindingContext).For(v => v.DataContext).To(vm => vm.RoleVm);
			bindingSet.Bind(heroEquipNode).For(v => v.activeSelf).ToExpression(vm => vm.Manager.CurTabType == ClothesUI.HeroEquip);
			bindingSet.Bind(roleView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.Manager.CurTabType == ClothesUI.Hero);
			bindingSet.Bind(btnMergeRed).For(v => v.activeSelf).To(vm => vm.MergeRed);
			bindingSet.Bind(this).For(v => v.moveParent).To(vm => vm.MoveParent).OneWayToSource();
			bindingSet.Bind(noneTips).For(v => v.text).To(vm => vm.NoneTipsStr);
            bindingSet.Build();
        }
        
    }
}