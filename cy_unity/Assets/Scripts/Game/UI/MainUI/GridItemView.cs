using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
namespace DH.Game.UIViews
{
    public partial class GridItemView: BaseItemView
    {
        public GameObject normalObj;
        public GameObject redObj;
        public GameObject validObj;
        public GameObject dashObj;
        public GameObject dashObj1;
        public GameObject dashObj2;
        public RectTransform gridRect;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<GridItemView, GridItemViewModel>();
            bindingSet.Bind(transform).For(v => v.localPosition).To(vm => vm.LocalPos);
            bindingSet.Bind(normalObj).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 0);
            bindingSet.Bind(redObj).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 1);
            bindingSet.Bind(validObj).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 2);
            bindingSet.Bind(dashObj).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 3);
            bindingSet.Bind(dashObj1).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 4);
            bindingSet.Bind(dashObj2).For(v => v.activeSelf).ToExpression(vm => vm.GridStateType == 5);
            bindingSet.Bind(this).For(v => v.gridRect).To(vm => vm.GridRect).OneWayToSource();
            bindingSet.Build();
        }
    }
}