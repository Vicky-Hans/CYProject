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
    public partial class TemplateView : BaseView
    {
        public override bool FullScreen => true;
        //##property_start##//
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            //##scrollview_cell_start##//
            await base.Create();
            var bindingSet = this.CreateBindingSet<TemplateView, TempLateViewModel>();
            //##binding_start##//
            bindingSet.Build();
        }
    }
}