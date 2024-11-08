using System;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class LuckEggExchangeViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private ObservableList<LuckEggExchangeItemViewModel> scrollViewList = new();
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
        private LuckyEggData Data => DataCenter.luckyEggData;
        
        [AutoNotify] private string timeDes;
        [AutoNotify] private string cosImgPath;
        [AutoNotify] private string cosNums;
        
        [Preserve]
        public LuckEggExchangeViewModel()
        {
            InitTopItems();
            InItShopItems();
            RefreshTimeDesc();
            Data.PropertyChanged += OptionalChange;
        }
        private void InitTopItems()
        {
            CommonTopItemsVm = UIHelper.GetTopModel(GameConst.ItemIdCode.EggCoin,GameConst.ItemIdCode.EggRedHeart,GameConst.ItemIdCode.Stone);
            CommonTopItemsVm.ClickItemAction = (data, id) =>
            {
                if (data.Id == (int)GameConst.ItemIdCode.EggCoin && !DataCenter.luckyEggData.IsTimeOver())
                {
                    ActivityUIManager.Instance.OpenBuyEggCoin();
                }

                if (data.Id == (int)GameConst.ItemIdCode.EggRedHeart && !DataCenter.luckyEggData.IsTimeOver())
                {
                    ActivityUIManager.Instance.EggTabType = LuckEggShowView.Main;
                }
            };

            CosImgPath = DataCenter.itemsData.GetItemIconPathById((int)GameConst.ItemIdCode.EggRedHeart);
            UpDataCosNums(null,null);
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.EggRedHeart, out var data))
            {
                data.PropertyChanged += UpDataCosNums;
            }
        }
        private void UpDataCosNums(object sender, PropertyChangedEventArgs e)
        {
            CosNums = DataCenter.itemsData.GetItemCount((int)GameConst.ItemIdCode.EggRedHeart).ToString();
        }

        private void InItShopItems()
        {
            ScrollViewList.Clear();
            var items = ConfigCenter.ExchangeShopCfgColl.DataItems;
            var list = new List<ExchangeShopCfg>(items.Where(o => o.TypeId == (int)ExchangeShopType.LuckEgg));
            list = list.OrderByDescending(o => DataCenter.luckyEggData.GetExchangeNums(o.Id) < o.BuyLimit).ThenBy(o => o.Num).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                ScrollViewList.Add(new LuckEggExchangeItemViewModel(list[i]));
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
            var times = Math.Max(0,
                DataCenter.luckyEggData.EndExchangeStamp - ServerTime.Instance.GetNowTime());
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
            if (DataCenter.itemsData.ResourceDatas.TryGetValue((int)GameConst.ItemIdCode.EggRedHeart, out var data))
            {
                data.PropertyChanged -= UpDataCosNums;
            }
            base.OnDispose();
        }
    }
}