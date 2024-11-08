using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using Extend;
namespace DH.Game.UIViews
{
    public partial class CollegeRankRewardItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public TextMeshProUGUI rankText;
		public DhImage rankIcon;
		public DhButton opBtn;
		public StaticItemsBindComponent itemGrid;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            

            await base.Create();
            var bindingSet = this.CreateBindingSet<CollegeRankRewardItemView, CollegeRankRewardItemViewModel>();
            
			bindingSet.Bind(rankText).For(v => v.text).To(vm => vm.RankTextStr);
			bindingSet.Bind(rankIcon).For(v => v.sprite).To(vm => vm.RankIconPath).WithConversion(this);
			bindingSet.Bind(opBtn).For(v => v.onClick).To(vm => vm.OnClickOpBtnCommand);
			bindingSet.Bind(itemGrid).For(v => v.BindDictionaryGetValueFunc).To(vm => vm.GetItemGridCellCallback);
			bindingSet.Bind(itemGrid).For(v => v.Collection).To(vm => vm.ItemGridDictionary);

            bindingSet.Build();
        }
    }
}