using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class MagicBingoExchangeItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
        public DhText limitNum;
        public UICircularScrollView awardScrollview;
        [AssetPath]public string awardScrollviewCell;
        public DhButton btn;
		
		
        public GameObject buyOverGo;
        public GameObject bgGray;
        public GameObject bg;
        public CellItemBaseView cellItemBaseView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
            awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoExchangeItemView, MagicBingoExchangeItemViewModel>();
            
            bindingSet.Bind(limitNum).For(v => v.text).To(vm => vm.LimitNumStr);
            bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
            bindingSet.Bind(btn).For(v => v.onClick).To(vm => vm.OnClickBtnCommand);
            bindingSet.Bind(buyOverGo).For(v => v.activeSelf).ToExpression(vm => vm.IsBuyOver);
            bindingSet.Bind(bgGray).For(v => v.activeSelf).ToExpression(vm => vm.IsBuyOver);
            bindingSet.Bind(bg).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuyOver);
            bindingSet.Bind(btn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuyOver);
            bindingSet.Bind(cellItemBaseView.BindingContext).For(v => v.DataContext).ToExpression(vm =>vm.CellItemBaseVm);
            bindingSet.Build();
        }
    }
}