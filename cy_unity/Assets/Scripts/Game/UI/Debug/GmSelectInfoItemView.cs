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
    public partial class GmSelectInfoItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhText desc;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<GmSelectInfoItemView, GmSelectInfoItemViewModel>();
            
			bindingSet.Bind(desc).For(v => v.text).To(vm => vm.DescStr);

            bindingSet.Build();
        }
    }
}