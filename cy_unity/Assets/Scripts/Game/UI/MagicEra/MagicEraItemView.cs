using DH.Data;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MagicEraItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton freeBtn;
		public DhButton CosBtn;
		public DhButton BuyOverBtn;
		
		public BtnPriceNode BtnPrice;
		
		public DhImage bgImg;
		public DhImage bgArrowBg;
		public DhImage bgArrow;
		public DhText limitDes;
		
		public GameObject lockImg1;
		public GameObject arrowGo;
		
	    private MagicEraItemState state;	
		public MagicEraItemState State
		{
			get => state;
			set
			{
				state = value;
				lockImg1.SetActive(state == MagicEraItemState.Lock);
				BuyOverBtn.gameObject.SetActive(state == MagicEraItemState.Get);
			}
		}

		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicEraItemView, MagicEraItemViewModel>();
            
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			
			bindingSet.Bind(freeBtn).For(v => v.onClick).To(vm => vm.FreeOnClickBtnCommand);
			bindingSet.Bind(CosBtn).For(v => v.onClick).To(vm => vm.CosOnClickBtnCommand);
			
			bindingSet.Bind(this).For(v => v.State).ToExpression(vm => vm.State);
			
			bindingSet.Bind(BtnPrice.BindingContext).For(v => v.DataContext).To(vm => vm.BtnPriceVM);
			
			bindingSet.Bind(limitDes).For(v => v.text).To(vm => vm.LimitDes);
			bindingSet.Bind(bgImg).For(v => v.sprite).To(vm => vm.BgImg).WithConversion(this);
			bindingSet.Bind(bgArrowBg).For(v => v.sprite).To(vm => vm.BgArrowBg).WithConversion(this);
			bindingSet.Bind(bgArrow).For(v => v.sprite).To(vm => vm.BgArrow).WithConversion(this);
			bindingSet.Bind(arrowGo).For(v => v.activeSelf).ToExpression(vm => vm.ShowArrowGo);
			
			bindingSet.Bind(freeBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.IsFree);
			bindingSet.Bind(CosBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsFree);
			
			
            bindingSet.Build();
        }
    }
}