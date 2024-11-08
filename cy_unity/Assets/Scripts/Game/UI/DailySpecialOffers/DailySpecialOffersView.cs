using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class DailySpecialOffersView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton infoBut;
		public DhText timeDes;
		public DhText discountText;
		public DhButton dayFreeBuy;
		public DhImage selectEquipIcon;
		public DhButton selectEquipBut;
		public DhText selectEquipName;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;
		public DhButton close;
		public DhButton allButton;

		public GameObject dayFreeBuyFree;
		public GameObject allButBuyOver;
		public BtnPriceNode allBuyPriceNode;
		public CommonTopView topView;
		
		private bool dayButGray;
		public bool DayButGray
		{
			get => dayButGray;
			set
			{
				dayButGray = value;
				UIHelper.SetGray(dayFreeBuy.gameObject,dayButGray);
				dayFreeBuyFree.SetActive(!dayButGray);
			}
		}

		#region 一键购买置灰售罄逻辑
		
		private bool allButGray;
		public bool AllButGray
		{
			get => allButGray;
			set
			{
				allButGray = value;
				UIHelper.SetGray(allButton.gameObject,allButGray);
			}
		}
		private bool allButOver;
		public bool AllButOver
		{
			get => allButOver;
			set
			{
				allButOver = value;
				allButton.gameObject.SetActive(!allButOver);
				allButBuyOver.SetActive(allButOver);
			}
		}

		#endregion

		
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<DailySpecialOffersView, DailySpecialOffersViewModel>();
            
			bindingSet.Bind(infoBut).For(v => v.onClick).To(vm => vm.OnClickInfoButCommand);
			bindingSet.Bind(timeDes).For(v => v.text).To(vm => vm.TimeDesStr);
			bindingSet.Bind(dayFreeBuy).For(v => v.onClick).To(vm => vm.OnClickDayFreeBuyCommand);
			bindingSet.Bind(dayFreeBuy.GetComponent<DhImage>()).For(v => v.sprite).To(vm => vm.DayButGrayString).WithConversion(this);
			bindingSet.Bind(selectEquipIcon).For(v => v.sprite).To(vm => vm.SelectEquipIcon).WithConversion(this);
			bindingSet.Bind(selectEquipBut).For(v => v.onClick).To(vm => vm.OnClickSelectEquipButCommand);
			bindingSet.Bind(selectEquipName).For(v => v.text).To(vm => vm.SelectEquipNameStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
	        bindingSet.Bind(discountText).For(v => v.text).To(vm => vm.DiscountText);
			bindingSet.Bind(this ).For(v => v.DayButGray).ToExpression(vm => vm.DayButGray);
			bindingSet.Bind(this ).For(v => v.AllButGray).ToExpression(vm => vm.AllButGray);
			bindingSet.Bind(this ).For(v => v.AllButOver).ToExpression(vm => vm.AllButOver);
			bindingSet.Bind(allBuyPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.AllBtnPriceNode);
			bindingSet.Bind(topView.BindingContext).For(v => v.DataContext).To(vm => vm.TopView);
			bindingSet.Bind(allButton).For(v => v.onClick).To(vm => vm.OnClickAllBuyButCommand);
            bindingSet.Build();
        }
    }
}