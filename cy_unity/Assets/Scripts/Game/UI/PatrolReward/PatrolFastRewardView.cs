using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class PatrolFastRewardView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhButton close;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton fastBtn;
		public DhText adLastNums;
		public DhButton gettingBtn;
		public DhText goldLastNums;
		public CommonAdvIconView commonAdvIconView;
		public ItemPriceNodeView itemPriceNode;
		public ItemPriceNodeView adItemPriceNode;
		public DhText fastAwardText;
		
		public GameObject fastBtnGray;
		public GameObject gettingBtnGray;
		private bool adButIsGray;
		public bool ADButIsGray
		{
			get => adButIsGray;
			set
			{
				adButIsGray = value;
				//UIHelper.SetGray(fastBtn.gameObject,adButIsGray,false);
				fastBtnGray.SetActive(adButIsGray);
			}
		}
		
		private bool powerButIsGray;
		public bool PowerButIsGray
		{
			get => powerButIsGray;
			set
			{
				powerButIsGray = value;
				//UIHelper.SetGray(gettingBtn.gameObject,powerButIsGray,false);
				gettingBtnGray.SetActive(powerButIsGray);
			}
		}
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<PatrolFastRewardView, PatrolFastRewardViewModel>();
            
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(fastBtn).For(v => v.onClick).To(vm => vm.OnClickFastBtnCommand);
			bindingSet.Bind(adLastNums).For(v => v.text).To(vm => vm.AdLastNumsStr);
			bindingSet.Bind(gettingBtn).For(v => v.onClick).To(vm => vm.OnClickGettingBtnCommand);
			bindingSet.Bind(goldLastNums).For(v => v.text).To(vm => vm.GoldLastNumsStr);
			bindingSet.Bind(commonAdvIconView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonAdvIconVm);
			bindingSet.Bind(itemPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			bindingSet.Bind(adItemPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.ItemPriceNodeVm);
			bindingSet.Bind(this).For(v => v.ADButIsGray).ToExpression(vm => vm.AdButGray);
			bindingSet.Bind(this).For(v => v.PowerButIsGray).ToExpression(vm => vm.PowerButGray);
			bindingSet.Bind(fastAwardText).For(v => v.text).To(vm => vm.FastAwardText);
            bindingSet.Build();
        }
    }
}