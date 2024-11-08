using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class CommonSelectRewardView : BaseView
    {
        public override bool FullScreen => false;
        public DhButton close;
        public DhButton BtnOK;
        public DhButton BtnClose;
        public CellItemBaseView cellItemBaseView;
        public GameObject notSelect;
        public UICircularScrollView scrollView;
        [AssetPath] public string prefabPath;
        public DhText tipsText;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = prefabPath;
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonSelectRewardView, CommonSelectRewardViewModel>();
            bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickClose);
            bindingSet.Bind(BtnClose).For(v => v.onClick).To(vm => vm.OnClickClose);
            bindingSet.Bind(BtnOK).For(v => v.onClick).To(vm => vm.OnClickBtnOKCommand);
            
            bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemBaseVm);
            bindingSet.Bind(notSelect).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelect);
            bindingSet.Bind(cellItemBaseView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsSelect);
            
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.itemList);
            bindingSet.Bind(this).For(v => v.scrollView).To(vm => vm.ScrollView).OneWayToSource();
            bindingSet.Bind(tipsText).For(v => v.text).To(vm => vm.TipsTextStr);
            bindingSet.Build();
        }
    }
}