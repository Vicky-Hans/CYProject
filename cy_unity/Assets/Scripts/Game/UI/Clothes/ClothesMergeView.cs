using System.Collections.Generic;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class ClothesMergeView : BaseView
    {
        public override bool FullScreen => false;
        
		public CellItemBaseView cellItemBaseView;
		public ScrollRectExtend scrollViewTop;
		[AssetPath]public string scrollViewTopCell;
		public DhButton btnSort;
		public DhText sortTitleName;
		public DhButton btnMergeOneKey;
		public DhButton btnMergeUnOneKey;
		public UICircularScrollView scrollViewList;
		[AssetPath]public string scrollViewListCell;
		public DhButton btnMerge;
		public BottomComponentView bottomComponent;
		public GameObject moveParent;
		public DhText mergeTips;
		
		public List<Texture2D> QltTextures;
		public RawImage qualityBg;
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
			scrollViewList.PrefabPath = scrollViewListCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<ClothesMergeView, ClothesMergeViewModel>();
            
            bindingSet.Bind(this).For(v => v.Qlt).ToExpression(vm => vm.Quality);
			bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemBaseViewVm);
			bindingSet.Bind(scrollViewTop).For(v => v.Collection).To(vm => vm.ScrollViewTopList);
			bindingSet.Bind(btnSort).For(v => v.onClick).To(vm => vm.OnClickBtnSortCommand);
			bindingSet.Bind(sortTitleName).For(v => v.text).To(vm => vm.SortTitleNameStr);
			bindingSet.Bind(btnMergeOneKey).For(v => v.onClick).To(vm => vm.OnClickBtnMergeOneKeyCommand);
			bindingSet.Bind(btnMergeUnOneKey).For(v => v.onClick).To(vm => vm.OnClickBtnMergeUnOneKeyCommand);
			bindingSet.Bind(btnMergeOneKey.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsCanOneKey && vm.SelectMergeData==null);
			bindingSet.Bind(btnMergeUnOneKey.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsCanOneKey || vm.SelectMergeData!=null);
			bindingSet.Bind(scrollViewList).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(this).For(v => v.scrollViewList).To(vm => vm.HeroEquipCollectionView).OneWayToSource();
			bindingSet.Bind(this).For(v => v.scrollViewList).To(vm => vm.UiScrollViewList).OneWayToSource();
			bindingSet.Bind(btnMerge).For(v => v.onClick).To(vm => vm.OnClickBtnMergeCommand);
			
			bindingSet.Bind(bottomComponent.BindingContext).For(v => v.DataContext).To(vm => vm.BottomComponentVm);
			bindingSet.Bind(btnMerge.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsCanMerge);
			bindingSet.Bind(this).For(v => v.moveParent).To(vm => vm.MoveParent).OneWayToSource();
			bindingSet.Bind(mergeTips).For(v => v.text).To(vm => vm.MergeTipsDesc);

            bindingSet.Build();
        }
    }
}