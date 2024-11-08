using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;



namespace DH.Game.ViewModels
{
    public partial class MagicBingoExchangeViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private ObservableList< MagicBingoExchangeItemViewModel> scrollViewList = new();
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
        private MagicBingoData Data => DataCenter.mgicBingoData;
        
        [AutoNotify] private string timeDes;
        [AutoNotify] private string cosImgPath;
        [AutoNotify] private string cosNums;
        
        [Preserve]
        public MagicBingoExchangeViewModel()
        {
            InitTopItems();
            InItShopItems();
            RefreshTimeDesc();
            Data.PropertyChanged += OptionalChange;
        }
        private void InitTopItems()
        {
            CommonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.BinGoPoint,
                GameConst.ItemIdCode.Stone,
            });
            // CommonTopItemsVm.ClickItemAction = (data, id) =>
            // {
            //     if (data.Id == (int)GameConst.ItemIdCode.EggCoin && !DataCenter.luckyEggData.IsTimeOver())
            //     {
            //         ActivityUIManager.Instance.OpenBuyEggCoin();
            //     }
            //
            //     if (data.Id == (int)GameConst.ItemIdCode.EggRedHeart && !DataCenter.luckyEggData.IsTimeOver())
            //     {
            //         ActivityUIManager.Instance.EggTabType = LuckEggShowView.Main;
            //     }
            // };

            CosImgPath = DataCenter.itemsData.GetItemIconPathById((int)GameConst.ItemIdCode.BinGoCoin);
            UpDataCosNums(null,null);
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.BinGoCoin, out var data))
            {
                data.PropertyChanged += UpDataCosNums;
            }
        }
        private void UpDataCosNums(object sender, PropertyChangedEventArgs e)
        {
            CosNums = DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.BinGoCoin).ToString();
        }

        private void InItShopItems()
        {
            ScrollViewList.Clear();
            var items = ConfigCenter.ExchangeShopCfgColl.DataItems;
            var list = new List<ExchangeShopCfg>(items.Where(o => o.TypeId == (int)ExchangeShopType.MagicBingo));
            list = list.OrderByDescending(o => Data.GetExchangeNums(o.Id) < o.BuyLimit).ThenBy(o => o.Num).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                ScrollViewList.Add(new MagicBingoExchangeItemViewModel(list[i]));
            }
        }
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Data.ExchangeRecord))
            {
                InItShopItems();
            }
        }
        
                
        private float interval;
        private void RefreshTimeDesc()
        {
            var times = Math.Max(0, Data.EndExchangeStamp - ServerTime.Instance.GetNowTime());
            TimeDes =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        

        protected override void OnDispose()
        {
            CommonTopItemsVm.Dispose();
            foreach (var item in scrollViewList)
            {
                item.Dispose();
            }
            Data.PropertyChanged -= OptionalChange;
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.BinGoCoin, out var data))
            {
                data.PropertyChanged -= UpDataCosNums;
            }
            base.OnDispose();
        }
    }
}