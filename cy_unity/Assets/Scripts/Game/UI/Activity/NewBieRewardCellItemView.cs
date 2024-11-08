using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class NewBieRewardCellItemView : BaseItemView
    {
        public override bool FullScreen => false;
		public TextMeshProUGUI titleText;

        public CellItemView CellItem1;
        public CellItemView CellItem2;
        public CellItemView CellItem3;
        public CellItemView CellItem4;

        public GameObject reward1;
        public GameObject reward2;
        private int rewardNums;

        public int RewardNums
        {
            get => rewardNums;
            set
            {
                rewardNums = value;
                switch (rewardNums)
                {
                    case 1:
                        CellItem2.gameObject.SetActive(false);
                        reward2.SetActive(false);
                        break;
                    case 2:
                        reward2.SetActive(false);
                        break;
                    case 3:
                        CellItem2.gameObject.SetActive(false);
                        break;
                    default:
                        reward1.SetActive(true);
                        reward2.SetActive(true);
                        break;
                }
            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<NewBieRewardCellItemView, NewBieRewardCellItemViewModel>();
			bindingSet.Bind(titleText).For(v => v.text).To(vm => vm.TitleTextStr);
			bindingSet.Bind(CellItem1.BindingContext).For(v => v.DataContext).To(vm => vm.CellItem1);
            bindingSet.Bind(CellItem2.BindingContext).For(v => v.DataContext).To(vm => vm.CellItem2);
            bindingSet.Bind(CellItem3.BindingContext).For(v => v.DataContext).To(vm => vm.CellItem3);
            bindingSet.Bind(CellItem4.BindingContext).For(v => v.DataContext).To(vm => vm.CellItem4);
            bindingSet.Bind(this).For(v => v.RewardNums).To(vm => vm.RewardNums);
            bindingSet.Build();
        }
    }
}