using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class RandomItemView : BaseItemView
    {
        public DhImage adIcon;
        public DhImage randomIcon;
        public GameObject adLeftTopObj;
        public GameObject adRightTopObj;
        public CanvasGroup canvasGroupObj;
        public RectTransform rootNode;
        public RectTransform randomIconNode;
        public RectTransform adIconNode;
        /// <summary>
        /// 装备模型id
        /// </summary>
        public int EquipModelId;
        public CommonAdvIconView rCommonAdv;
        public CommonAdvIconView lCommonAdv;
        private int curModelId;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RandomItemView, RandomItemViewModel>();
            bindingSet.Bind(canvasGroupObj).For(v => v.alpha).ToExpression(vm => vm.Alpha);
            bindingSet.Bind(adIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.AdState == 1);
            bindingSet.Bind(rootNode).For(v => v.sizeDelta).To(vm => vm.RandomSize);
            bindingSet.Bind(randomIconNode).For(v => v.sizeDelta).To(vm => vm.RandomIconSize);
            bindingSet.Bind(adIconNode).For(v => v.sizeDelta).To(vm => vm.AdIconSize);
            bindingSet.Bind(adLeftTopObj).For(v => v.activeSelf).ToExpression(vm => vm.AdShowType == 1);
            bindingSet.Bind(adRightTopObj).For(v => v.activeSelf).ToExpression(vm => vm.AdShowType == 0);
            bindingSet.Bind(this).For(v => v.rootNode).To(vm => vm.RandomNodeRect).OneWayToSource();
            bindingSet.Bind(this).For(v => v.randomIconNode).To(vm => vm.RandomIconRect).OneWayToSource();
            bindingSet.Bind(adIcon).For(v => v.sprite).To(vm => vm.AdIconPathStr).WithConversion(this);
            bindingSet.Bind(randomIcon).For(v => v.sprite).To(vm => vm.IconPathStr).WithConversion(this);
            bindingSet.Bind(this).For(v => v.EquipModelId).To(vm => vm.RandomItemID);
            bindingSet.Bind(rCommonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.RCommonAdvVm);
            bindingSet.Bind(lCommonAdv.BindingContext).For(v => v.DataContext).To(vm => vm.LCommonAdvVm);
            bindingSet.Bind(this).For(v => v.CurModelId).ToExpression(vm => GetModelId(vm.WeaponData, vm.GridAddData));
            bindingSet.Build();
        }

        public int CurModelId
        {
            get => curModelId;
            set=> curModelId = value;
        }
        
        public int GetModelId(BackpackWeaponData data, GridAddData grid)
        {
            if (data!= null)
            {
               return data.WeaponId;
            }

            if (grid != null)
            {
                return grid.GridId;
            }

            return 0;
        }
    }
}