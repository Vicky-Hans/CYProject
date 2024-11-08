using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class PullDownItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText titleName;
        public DhButton btnSelect;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<PullDownItemView, PullDownItemViewModel>();
            
			bindingSet.Bind(titleName).For(v => v.text).To(vm => vm.TitleNameStr);
            bindingSet.Bind(btnSelect).For(v => v.onClick).To(vm => vm.OnClickSelectCommand);
            
            bindingSet.Build();
        }
    }
}