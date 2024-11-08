using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class PatrolRewardView : BaseView
    {
        public override bool FullScreen => false;
        
		public DhText timesDes;
		public DhText goldNumsText;
		public DhText powerText;
		public UICircularScrollView scrollView;
		[AssetPath]public string scrollViewCell;
		public DhButton fastBtn;
		public DhButton gettingBtn;
		public GameObject gettingBtnGray;
		public GameObject cardView;
		public DhButton goTOCard;
		
		public UICircularScrollView MonthCardscrollView;
		public DhText allLastNums;
		//特权月卡收益描述
		public DhText monthCardAttrText;
		//最大贮存时长
		public DhText maxPatrolTime;
		
		public GameObject redGo;
		public DhButton closeBut;
		private bool butIsGray;
		public bool ButIsGray
		{
			get => butIsGray;
			set
			{
				butIsGray = value;
				//UIHelper.SetGray(gettingBtn.gameObject,butIsGray,false);
				gettingBtnGray.SetActive(butIsGray);
			}
		}
		
		private bool isShowMonthCard;
		public bool IsShowMonthCard
		{
			get => isShowMonthCard;
			set
			{
				isShowMonthCard = value;
				isShowMonthCard = false;
				cardView.SetActive(isShowMonthCard);
				TipsGo.SetActive(isShowMonthCard);
				TipsGo2.SetActive(!isShowMonthCard);
				var transform1 = cardView.transform.parent.transform;
				transform1.localPosition = new Vector3(0, isShowMonthCard ? -16f : -112, 0);
				TipsGo2.transform.localPosition = new Vector3(0, isShowMonthCard ? -674f : -771, 0);
			}
		}
		
		public GameObject TipsGo;
		public GameObject TipsGo2;
		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrollView.PrefabPath = scrollViewCell;
			MonthCardscrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<PatrolRewardView, PatrolRewardViewModel>();
			bindingSet.Bind(timesDes).For(v => v.text).To(vm => vm.TimesDesStr);
			bindingSet.Bind(goldNumsText).For(v => v.text).To(vm => vm.GoldNumsTextStr);
			bindingSet.Bind(powerText).For(v => v.text).To(vm => vm.PowerTextStr);
			bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
			bindingSet.Bind(MonthCardscrollView).For(v => v.Collection).To(vm => vm.MonthCardScrollViewList);
			bindingSet.Bind(fastBtn).For(v => v.onClick).To(vm => vm.OnClickFastBtnCommand);
			bindingSet.Bind(gettingBtn).For(v => v.onClick).To(vm => vm.OnClickGettingBtnCommand);
			bindingSet.Bind(this).For(v => v.IsShowMonthCard).ToExpression(vm => vm.IsShowCardView);
			bindingSet.Bind(goTOCard).For(v => v.onClick).To(vm => vm.OnClickGoTOCardCommand);
			bindingSet.Bind(this).For(v => v.ButIsGray).ToExpression(vm => vm.GetAwardGary);
			bindingSet.Bind(maxPatrolTime).For(v => v.text).To(vm => vm.MaxPatrolTime);
			bindingSet.Bind(allLastNums).For(v => v.text).To(vm => vm.AllLastNums);
			bindingSet.Bind(monthCardAttrText).For(v => v.text).To(vm => vm.MonthCardAttrText);
			bindingSet.Bind(redGo).For(v => v.activeSelf).ToExpression(vm => vm.RedGo);
			bindingSet.Bind(closeBut).For(v => v.onClick).To(vm => vm.CloseUI);
            bindingSet.Build();
        }
    }
}