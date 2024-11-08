using Cysharp.Threading.Tasks;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class EquipUnOwnItemView : BaseItemView
    {
        public override bool FullScreen => false;
        public DhImage icon;
        public DhImage typeIcon;
        public DhImage attrIcon;
        public GameObject lvBg;
        public GameObject frigateBg;
        public DhText progress;
        public override async UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<EquipUnOwnItemView, EquipUnOwnItemViewModel>();
            
            bindSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindSet.Bind(typeIcon).For(v => v.sprite).To(vm => vm.TypeIconPath).WithConversion(this);
            bindSet.Bind(attrIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.AttrNum>1);
            bindSet.Bind(progress).For(v => v.text).To(vm => vm.ProgressDesc);
            bindSet.Build();
            UIHelper.SetGray(attrIcon.gameObject,true,false);
            UIHelper.SetGray(typeIcon.gameObject,true,false);
            UIHelper.SetGray(icon.gameObject,true,false);
            UIHelper.SetGray(lvBg,true,false);
            UIHelper.SetGray(frigateBg,true,false);
        }
    }
}