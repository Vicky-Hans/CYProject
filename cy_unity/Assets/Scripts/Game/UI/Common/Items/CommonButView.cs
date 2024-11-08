using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class CommonButView : BaseItemView
    {
        public override bool FullScreen => false;

        public GameObject redGo; 
        public DhButton But;  
        public DhImage icon;
        public DhText name;
        public DhText time;
        public RectTransform recTr;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CommonButView, CommonButViewModel>();
            bindingSet.Bind(redGo).For(v => v.activeSelf).To(vm => vm.RedNote);
            bindingSet.Bind(But).For(v => v.onClick).To(vm => vm.OnClickButCommand);
            bindingSet.Bind(icon).For(v => v.sprite).ToExpression(vm =>vm.IconPath).WithConversion(this);
            
            bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameText);
            bindingSet.Bind(name.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsShowTimeDes);
            bindingSet.Bind(time).For(v => v.text).ToExpression(vm => vm.TimeText);
            bindingSet.Bind(time.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsShowTimeDes);
                        
            bindingSet.Bind(this).For(v => v.recTr).To(vm => vm.FunctionMenuTransform).OneWayToSource();
            bindingSet.Build();
        }
    }
}