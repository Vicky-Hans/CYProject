using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Game.UI.MainUi;


namespace DH.Game.ViewModels
{
    public partial class MagicEraViewModel : ViewModelBase
    {
        
		[AutoNotify] private string timesDesStr;
	    [AutoNotify] private ObservableList<MagicEraItemViewModel> scrollViewList = new();
        
	    [AutoNotify] private int scrollViewPos;
	    private MagicAgeData Data => DataCenter.magicAgeData;
	    
        [Preserve]
        public MagicEraViewModel()
        {
	        var datas = ConfigCenter.AgeMagicPackageCfgColl.DataItems;
	        ScrollViewList.Clear();
	        for (int i = 0; i < datas.Count; i++)
	        {
		        ScrollViewList.Add(new MagicEraItemViewModel(datas[i],i == datas.Count-1 ));
	        }

	        ScrollViewPos = Data.GetMowIndex();
	        TimesDes();
	        Data.PropertyChanged += OptionalChange;
        }


        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<MagicEraView>();
	        
        }

        private void OptionalChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Data.GetAwards))
	        {
		        if (Data.IsBuyOver())
		        {
			        MainUiManager.Instance.RemoveRightBut(MainStageInfoNodeRightButType.MagicEr);
			        OnClickClose();
		        }
	        }
        }
        
        public void TimesDes()
        {
	        var time = ServerTime.Instance.RemainTime(Data.EndStamp);
	        if (time < 0)
	        {
		        TimesDesStr =  string.Empty;
		        MainUiManager.Instance.RemoveRightBut(MainStageInfoNodeRightButType.MagicEr);
		        OnClickClose();
		        return;
	        }
	        if (time >= 86400)
	        {
		        TimesDesStr =   UIHelper.ConvertTimeSecondToString(time, ETimeFormatType.TimeFormatTypeHourMinuteWithUnit);
		        return;
	        }
	        TimesDesStr =   ServerTime.Instance.Seconds2Hhmmss(time);
	    }

        private float interval;
        public override void Update()
        {
	        if (UIHelper.CalculateTime(ref interval))
	        {
		        TimesDes();
	        }
            
        }
        protected override void OnDispose()
        {
	        foreach (var item in scrollViewList)
	        {
		        item.Dispose();
	        }
	        Data.PropertyChanged -= OptionalChange;
	        base.OnDispose();
        }
    }
}