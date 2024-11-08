using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine.UI;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class LuckTravelProgressAwardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public Slider slider;
        public DhText progressValue;
        public SelectCellItemEffectView selectItemView;
        public GameObject proBg;
        public GameObject proBgEnd;
        public DhImage fillIcon;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        await base.Create();
	        var bindSet = this.CreateBindingSet<LuckTravelProgressAwardItemView, LuckTravelProgressAwardItemViewModel>();
	        bindSet.Bind(slider).For(v => v.value).To(vm => vm.ProgressValue);
	        bindSet.Bind(progressValue).For(v => v.text).To(vm => vm.ScoreValue);
	        bindSet.Bind(selectItemView.BindingContext).For(v => v.DataContext).To(vm => vm.SelectItemViewModel);
	        bindSet.Bind(proBg).For(v => v.activeSelf).ToExpression(vm => !vm.IsEndPos);
	        bindSet.Bind(proBgEnd).For(v => v.activeSelf).To(vm => vm.IsEndPos);
	        bindSet.Bind(fillIcon).For(v => v.sprite).ToExpression(vm => GetFillIconPath(vm.IsEndPos)).WithConversion(this);
	        bindSet.Build();
        }
        
        protected string GetFillIconPath(bool isEnd)
        {
	        return !isEnd ? "equip[equip_progressbar_2_2]" : "equip[equip_progressbar_2_3]";
        }
    }
}