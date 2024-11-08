using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class FunctionMenuView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton settingButton;
		public DhButton mailButton;
        public Transform content;
        public Transform menuTr;
        public GameObject mailRedGo;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            await base.Create();
            var bindingSet = this.CreateBindingSet<FunctionMenuView, FunctionMenuViewModel>();
			bindingSet.Bind(settingButton).For(v => v.onClick).To(vm => vm.OnClickSettingButtonCommand);
			bindingSet.Bind(mailButton).For(v => v.onClick).To(vm => vm.OnClickMailButtonCommand);
            bindingSet.Bind(menuTr).For(v => v.localPosition).ToExpression(vm => SetPosition(vm.Pos, vm.Offset));
            bindingSet.Bind(mailRedGo).For(v => v.activeSelf).To(vm => vm.ShowMailRedGo);
            bindingSet.Build();
        }
        
        public Vector3 SetPosition(Vector3 pos, Vector3 offset)
        {
            Vector3 localPos = content.InverseTransformPoint(pos);
            return new Vector3(localPos.x + offset.x, localPos.y + offset.y, localPos.z + offset.z);
        }
        
    }
}