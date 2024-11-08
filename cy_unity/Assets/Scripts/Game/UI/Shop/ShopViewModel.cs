using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI.MainUi;

namespace DH.Game.ViewModels
{
    public partial class ShopViewModel : ViewModelBase
    {

        [AutoNotify] private TabBtnGroupViewModel tabBtnViewModel;
        [AutoNotify] private ShopDrawViewModel shopDrawViewModel = new();
        [AutoNotify] private ShopChapterViewModel shopChapterViewModel = new();
        [AutoNotify] private ShopDailyViewModel shopDailyViewModel = new();
        [AutoNotify] private ShopDiamondViewModel shopDiamondViewModel = new();
        [AutoNotify] private ShopGoldViewModel shopGoldViewModel = new();
        [AutoNotify] private ShopDrawClothesViewModel shopDrawClothesViewModel = new();

        [AutoNotify] private ShopTitle shopDraw=ShopTitle.None;
        [AutoNotify] private ShopTitle shopChapter=ShopTitle.None;
        [AutoNotify] private ShopTitle shopDaily=ShopTitle.None;
        [AutoNotify] private ShopTitle shopDiamond=ShopTitle.None;
        [AutoNotify] private ShopTitle shopGold=ShopTitle.None;
        [AutoNotify] private ShopTitle shopClothes=ShopTitle.None;

        [AutoNotify] private Dictionary<int,int>  showPosDic=new ();

        private ShopTitle selectShopTitle;
        public ShopTitle SelectShopTitle
        {
            get => selectShopTitle;
            set
            {
                if(!Set(ref selectShopTitle, value)) return;
                    tabBtnViewModel?.RefreshSetSelect((int)SelectShopTitle);
            }
        }

        [Preserve]
        public ShopViewModel()
        {
            RefreshInitSelect();
            InitTabBtn();
            RefreshShopRed();
            DataCenter.shopData.Recruit.PropertyChanged += RecruitChanged;
            DataCenter.shopData.PropertyChanged += RecruitChanged;
            DataCenter.shopData.ChapterGift.CollectionChanged += CollectionChapterChanged;
            ShopManager.Instance.PropertyChanged += ManagerChanged;
            MainUiManager.Instance.PropertyChanged += OnMainUiChanged;
        }

     

        protected override void OnDispose()
        {
            base.OnDispose();
            tabBtnViewModel.Dispose();
            shopDrawViewModel.Dispose();
            shopChapterViewModel.Dispose();
            shopDailyViewModel.Dispose();
            shopDiamondViewModel.Dispose();
            shopGoldViewModel.Dispose();
            DataCenter.shopData.Recruit.PropertyChanged -= RecruitChanged;
            DataCenter.shopData.PropertyChanged -= RecruitChanged;
            DataCenter.shopData.ChapterGift.CollectionChanged -= CollectionChapterChanged;
            ShopManager.Instance.PropertyChanged -= ManagerChanged;
            MainUiManager.Instance.PropertyChanged -= OnMainUiChanged;
    
        }

        private void CollectionChapterChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshChapterShow();
        }

        private void ManagerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ShopManager.Instance.SelectShopTitle))
            {
                SelectShopTitle = ShopManager.Instance.SelectShopTitle;
            }
            else if (e.PropertyName is nameof(ShopManager.Instance.JumpSelectShopTile))
            {
                RefreshInitSelect();
            }
        }

        private void OnMainUiChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MainUiManager.Instance.CurTabType))
            {
                if (MainUiManager.Instance.CurTabType != ETabType.TabTypeShop)
                {
                    RefreshInitSelect();
                    tabBtnViewModel.CurPos = (int)SelectShopTitle;
                }
            }
        }
        
        private void RecruitChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshShopRed();
        }

        private void InitTabBtn()
        {
            ShowPosDic.Clear();
            var allTab = ConfigCenter.ShopCfgColl.DataItems.ToList();
            for (int i = 0; i < allTab.Count; i++)
            {
                ShowPosDic.Add(allTab[i].Id,allTab[i].Id);
            }

            int pos = 0;
            List<TabBtnInfo> btnInfos = new List<TabBtnInfo>();

            var list = ConfigCenter.ShopTabCfgColl.DataItems.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if(GameConst.IsIosAuditState && list[i].Id== 3) continue;
                var cfgL = ConfigCenter.ShopTabLanguageCfgColl.GetDataById(list[i].Id);
                var btnInfo = new TabBtnInfo()
                {
                    Pos = list[i].Id,
                    Name = cfgL.Name,
                }; 
                btnInfos.Add(btnInfo);

                foreach (var item in list[i].Content)
                {
                    if (item.Type == ElementType.Shop)
                    {
                        ShowPosDic[item.Id] = pos;
                        pos++;
                        switch (item.Id)
                        {
                            case 3: shopDraw = (ShopTitle)list[i].Id; break;
                            case 1:
                            {
                                if (ShopManager.Instance.IsShowChapterGift())
                                {
                                    shopChapter = (ShopTitle)list[i].Id; 
                                }
                                break;
                            }
                            case 2: shopDaily = (ShopTitle)list[i].Id; break;
                            case 4: shopDiamond = (ShopTitle)list[i].Id; break;
                            case 5: shopGold = (ShopTitle)list[i].Id; break;
                            case 7:
                            {
                                if(MainUiManager.Instance.CheckFunctionIsUnlock(EFunctionOpenType.FunctionClothes))
                                    shopClothes = (ShopTitle)list[i].Id; 
                                break;
                            }
                        }
                    }
                }
            }
            tabBtnViewModel = new TabBtnGroupViewModel(btnInfos,(int)SelectShopTitle,ClickSelect);
        }

        private void RefreshChapterShow()
        {
            if (!ShopManager.Instance.IsShowChapterGift())
            {
                ShopChapter = ShopTitle.None;
            }
        }

        private void ClickSelect(int index)
        {
            SelectShopTitle = (ShopTitle)index;
        }

        private void RefreshInitSelect()
        {
            if (GameConst.IsIosAuditState)
            {
                SelectShopTitle = ShopTitle.Draw;
                return;
            }
            if (ShopManager.Instance.JumpSelectShopTile != ShopTitle.None)
            {
                SelectShopTitle = ShopManager.Instance.JumpSelectShopTile;
                ShopManager.Instance.jumpSelectShopTile = ShopTitle.None;
                return;
            }

            if (ShopManager.Instance.CheckDrawRed())
            {
                SelectShopTitle = ShopTitle.Draw;
            }else if (ShopManager.Instance.CheckDailyRed())
            {
                SelectShopTitle = ShopTitle.DisCount;
            }else if (ShopManager.Instance.CheckGoldRed())
            {
                SelectShopTitle = ShopTitle.Currency;
            }
            else
            {
                SelectShopTitle = ShopTitle.Draw;
            }

           
        }

        private void RefreshShopRed()
        {
            //分别刷新红点
            tabBtnViewModel.RefreshRedDot((int)ShopTitle.Draw,ShopManager.Instance.CheckDrawRed());
            tabBtnViewModel.RefreshRedDot((int)ShopTitle.DisCount,ShopManager.Instance.CheckDailyRed());
            tabBtnViewModel.RefreshRedDot((int)ShopTitle.Currency,ShopManager.Instance.CheckGoldRed());
        }
        
        private void RefreshShopLock()
        {
            //分别刷新锁定
        }
    }
}