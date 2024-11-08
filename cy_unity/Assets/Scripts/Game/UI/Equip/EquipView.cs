using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class EquipView : BaseItemView
    {
		public DhText battleNum;
		public ScrollRectExtend battleGrid;
		[AssetPath]public string battleGridCell;
		public ScrollRectExtend scrollView;
		[AssetPath]public string scrollViewCell;
		public ScrollRectExtend scrollViewLock;
		[AssetPath]public string scrollViewLockCell;
		
		public DhText progressValue;
		public GameObject infoNode;
		public GameObject replaceNode;

		public EquipItemView replaceEquipItemView;
		public GameObject replaceGrid;

		public GridLayoutGroup equipListGrid;
		public DhButton btnCloseReplace;
		public TabBtnGroupTitleView tabBtnGroupTitleView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			battleGrid.PrefabPath = battleGridCell;
			scrollView.PrefabPath = scrollViewCell;
			scrollViewLock.PrefabPath = scrollViewLockCell;

            await base.Create();
            var bindSet = this.CreateBindingSet<EquipView, EquipViewModel>();
            
			bindSet.Bind(battleNum).For(v => v.text).To(vm => vm.BattleNumStr);
			bindSet.Bind(battleGrid).For(v => v.Collection).To(vm => vm.BattleGridList);
			bindSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindSet.Bind(scrollViewLock).For(v => v.Collection).To(vm => vm.ScrollViewLockList);
			bindSet.Bind(progressValue).For(v => v.text).ToExpression(vm => GetProgress(vm.BattleNum,vm.AllBattleNum));
			
			bindSet.Bind(infoNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsReplace);
			bindSet.Bind(replaceNode).For(v => v.activeSelf).To(vm => vm.IsReplace);
			bindSet.Bind(replaceEquipItemView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipItemViewModel);
			bindSet.Bind(equipListGrid).For(v => v.padding).ToExpression(vm => GetEquipListSize(vm.IsOpenState));
			for (int i = 0; i < replaceGrid.transform.childCount; i++)
			{
				bindSet.Bind(replaceGrid.transform.GetChild(i).GetComponent<DhButton>()).For(v => v.onClick).To(vm => vm.OnClickReplaceCommand).CommandParameter(i+1);
			}
			bindSet.Bind(btnCloseReplace).For(v => v.onClick).To(vm => vm.OnClickCloseReplaceCommand);
			bindSet.Bind(tabBtnGroupTitleView.BindingContext).For(v => v.DataContext).To(vm => vm.TabBtnGroupTitle);
            bindSet.Build();
        }

        public RectOffset GetEquipListSize(bool open)
        {
	        return new RectOffset(10, 0, 125, open?140:60);
        }

        private string GetProgress(int vmBattleNumStr, int vmAllBattleNum)
        {
	        return $"{vmBattleNumStr}/{vmAllBattleNum}";
        }
    }
}