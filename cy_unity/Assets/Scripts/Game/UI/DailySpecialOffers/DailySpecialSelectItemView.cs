using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class DailySpecialSelectItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhImage equipIcon;
        public DhImage qualityImg;
		public DhText name;
        public DhButton But;
        public GameObject select;
        public GameObject selectGray;
        public GameObject notSelect;
        public GameObject lockImg;
        private bool isGray;

        public bool IsGray
        {
            get => isGray;
            set
            {
                isGray = value;
                UIHelper.SetGray(lockImg,isGray);
                UIHelper.SetGray(selectGray,true);
                UIHelper.SetGray(equipIcon.gameObject,isGray);
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<DailySpecialSelectItemView, DailySpecialSelectItemViewModel>();
            
			bindingSet.Bind(equipIcon).For(v => v.sprite).To(vm => vm.EquipIconPath).WithConversion(this);
			bindingSet.Bind(name).For(v => v.text).To(vm => vm.NameStr);
            bindingSet.Bind(qualityImg).For(v => v.sprite).To(vm => vm.QualityImg).WithConversion(this);
            bindingSet.Bind(But).For(v => v.onClick).To(vm => vm.OnClickSelectBuyCommand);
            bindingSet.Bind(this).For(v => v.IsGray).ToExpression(vm => vm.IsGray);
            bindingSet.Bind(select).For(v => v.activeSelf).ToExpression(vm => vm.IsSelect && !IsGray);
            bindingSet.Bind(selectGray).For(v => v.activeSelf).ToExpression(vm => vm.IsSelect && IsGray);
            bindingSet.Bind(notSelect).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelect);
            bindingSet.Bind(lockImg).For(v => v.activeSelf).ToExpression(vm => vm.IsGray);
            bindingSet.Build();
        }
    }
}