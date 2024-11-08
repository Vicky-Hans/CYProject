using Cysharp.Threading.Tasks;
using DH.Game.UI;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using UnityEngine;
using Extend;

namespace DH.Game.UIViews
{
    public partial class LuckEggFundView : BaseItemView
    {
        public override bool FullScreen => false;

        public UICircularScrollView levelDiscountsScroll;
        [AssetPath] public string itemPrefab;
        public CommonTopView topItems;
        public GameObject levelBought;
        public DhButton levelBtn;
        public BtnPriceNode BtnPriceNode;
        private int index;

        public DhText times;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            levelDiscountsScroll.PrefabPath = itemPrefab;
            await base.Create();
            var bindSet = this.CreateBindingSet<LuckEggFundView, LuckEggFundViewModel>();
            bindSet.Bind(topItems.BindingContext).For(v => v.DataContext)
                .To(vm => vm.TopItemsModel);
            bindSet.Bind(levelBought.gameObject).For(v => v.activeSelf).To(vm => vm.IsBuyState);
            bindSet.Bind(levelBtn.gameObject).For(v => v.activeSelf)
                .ToExpression(vm => !vm.IsBuyState);
            bindSet.Bind(BtnPriceNode.BindingContext).For(v => v.DataContext)
                .To(vm => vm.PriceNodeModel);
            bindSet.Bind(levelBtn).For(v => v.onClick).To(vm => vm.BuyLevelCommand);
            bindSet.Bind(levelDiscountsScroll).For(v => v.Collection)
                .To(vm => vm.CollegeDiscountsItems);
           // bindSet.Bind(this).For(v => v.Index).To(vm => vm.Manager.CurTab);
            bindSet.Bind(this).For(v => v.MovePos).To(vm => vm.IsMoveScroll);
            bindSet.Bind(times).For(v => v.text).To(vm => vm.TimeDes);
            bindSet.Build();
        }

        protected override void OnShow()
        {
            base.OnShow();
            DelayScrollToPos().Forget();
        }

        public bool MovePos
        {
            get => false;
            set
            {
                if (value)
                {
                    int lvPos = CollegeActivityManager.Instance.GetScrollIdx();
                    levelDiscountsScroll.Jump2SpecificItem(lvPos - 1);
                }
            }
        }


        private async UniTaskVoid DelayScrollToPos()
        {
            await UniTask.Delay(300);
            // int lvPos = DataCenter.stageFundData.GetScrollIdx();
            // levelDiscountsScroll.Jump2SpecificItem(lvPos-1);
        }

        public int Index
        {
            get => index;
            set
            {
                index = value;
            }
        }
    }
}