using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;



namespace DH.Game.ViewModels
{
    public partial class MagicBingoGiftViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
        [AutoNotify] private CommonTopViewModel commonTopItemsVm;
        [AutoNotify] private ObservableList<MagicBingoGiftItemViewModel> scrollViewList = new();
        [AutoNotify] private string timeDes;
        private MagicBingoData Data => DataCenter.mgicBingoData;
        [Preserve]
        public MagicBingoGiftViewModel()
        {
            InitTopItems();
            InitUI();
            RefreshTimeDesc();
            Data.PropertyChanged += OptionalChange;
        }
        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Data.PackageRecord))
            {
                InitUI();
            }
        }
        protected override void OnDispose()
        {
            Data.PropertyChanged -= OptionalChange;
            base.OnDispose();
        }
        
        private void InitTopItems()
        {
            CommonTopItemsVm = new CommonTopViewModel(new List<GameConst.ItemIdCode>
            {
                GameConst.ItemIdCode.BinGoPoint,
                GameConst.ItemIdCode.Stone,
            });
        }
        
        
        void InitUI()
        {
            var list = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.MagicBingo);
            list = list.OrderByDescending(o => Data.IsCanGetAward(o.Id)).ThenBy(o => o.Id).ToList();
            ScrollViewList.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                ScrollViewList.Add(new MagicBingoGiftItemViewModel(list[i].Id));
            }
        }

        #region 倒计时
        
        private float interval;
        private void RefreshTimeDesc()
        {
            var times = Math.Max(0, Data.EndStamp - ServerTime.Instance.GetNowTime());
            TimeDes =  ServerTime.Instance.SecondsDHAndMS(times);
        }
        public override void Update()
        {
            if (UIHelper.CalculateTime(ref interval))
            {
                RefreshTimeDesc();
            }
        }
        

        #endregion
        
    }
}