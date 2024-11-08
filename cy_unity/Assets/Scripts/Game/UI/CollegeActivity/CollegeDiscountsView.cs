using DH.Game;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using Cysharp.Threading.Tasks;
using DH.Game.UI;
using DH.UIFramework;
using Extend;
using UnityEngine;
namespace Game.UI.UIViews
{
    public partial class CollegeDiscountsView : BaseItemView
    {
        public UICircularScrollView levelDiscountsScroll;
        [AssetPath] public string itemPrefab;
        public CommonTopView topItems;
        public GameObject levelBought;
        public DhButton levelBtn;
        public BtnPriceNode BtnPriceNode;
        private int index;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            levelDiscountsScroll.PrefabPath = itemPrefab;
            await base.Create();
            var bindSet = this.CreateBindingSet<CollegeDiscountsView, CollegeDiscountsModel>();
            bindSet.Bind(topItems.BindingContext).For(v => v.DataContext).To(vm => vm.TopItemsModel);
            bindSet.Bind(levelBought.gameObject).For(v => v.activeSelf).To(vm => vm.IsBuyState);
            bindSet.Bind(levelBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsBuyState);
            bindSet.Bind(BtnPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.PriceNodeModel);
            bindSet.Bind(levelBtn).For(v => v.onClick).To(vm => vm.BuyLevelCommand);
            bindSet.Bind(levelDiscountsScroll).For(v => v.Collection).To(vm => vm.CollegeDiscountsItems);
            bindSet.Bind(this).For(v => v.Index).To(vm => vm.Manager.CurTab);
            bindSet.Bind(this).For(v => v.MovePos).To(vm => vm.IsMoveScroll);
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
                    levelDiscountsScroll.Jump2SpecificItem(lvPos-1);
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
                // if (index == (int)ActivityBtnType.LevelDiscounts)
                // {
                //     DelayScrollToPos().Forget();
                // }
            }
        }
    }
}