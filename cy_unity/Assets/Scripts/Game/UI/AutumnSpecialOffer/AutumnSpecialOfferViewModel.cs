using System;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;



namespace DH.Game.ViewModels
{
    public partial class AutumnSpecialOfferViewModel : ViewModelBase
    {
        public override bool AutoDispose => true;
		[AutoNotify] private string timesDesStr;
        [AutoNotify] private ObservableList<AutumnSpecialOfferItemViewModel> scrollViewList = new();
        private AutumnSpecialData Data => DataCenter.autumnSpecialData;
        [Preserve]
        public AutumnSpecialOfferViewModel()
        {
            InitScrollView();
            RefreshTimeDesc();
            Data.PropertyChanged += OnPropertyChanged;
        }
        
        private void InitScrollView()
        {
            var lists = ConfigCenter.PackageCfgColl.GetDataByRule((int)EPackageType.AutumnSpecial);
            lists = lists.OrderByDescending(o => Data.IsCanGetAward(o.Id)).ThenBy(o => o.Id).ToList();
            ScrollViewList.Clear();
            for (int i = 0; i < lists.Count; i++)
            {
                ScrollViewList.Add(new AutumnSpecialOfferItemViewModel(lists[i]));
            }
        }

        protected override void OnDispose()
        {
            Data.PropertyChanged -= OnPropertyChanged;
            base.OnDispose();
        }
        
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data.PackRecord))
            {
                if (Data.IsBuyAllOver() && UIManager.Instance.IsOpen<AutumnSpecialOfferView>())
                {
                    UIManager.Instance.CloseDialog<AutumnSpecialOfferView>();
                    return;
                }
                InitScrollView();
            }
        }
        
        #region 倒计时

        private float interval;
        private void RefreshTimeDesc()
        {
            var time = Data.EndStamp - ServerTime.Instance.GetNowTime();
            TimesDesStr = ServerTime.Instance.SecondsDHAndMS(Math.Max(time,0));
            if (time <= 0 && UIManager.Instance.IsOpen<AutumnSpecialOfferView>())
            {
                UIManager.Instance.CloseDialog<AutumnSpecialOfferView>();
            }
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