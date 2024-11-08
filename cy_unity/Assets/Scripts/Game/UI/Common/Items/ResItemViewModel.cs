using System;
using System.ComponentModel;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ResItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;
		[AutoNotify] private string addIconPath;
		[AutoNotify] private string countTextStr;
		[AutoNotify] private bool isShowAddIcon;
		[AutoNotify] private bool isShowLeftTime;
		[AutoNotify] private string leftTimeStr;
		[AutoNotify] private RectTransform bgRect;

		[AutoNotify] private Action<ResourceData,int> onClickItemCallback;
		[AutoNotify] private ResourceData curData;
		// 标签，用于区分不同的界面类型
		private int tag;

		[Preserve]
		public ResItemViewModel(ResourceData data, int tag)
		{
			curData = data;
			this.tag = tag;
			if(curData.Id == (int)GameConst.ItemIdCode.EnergyDrink)
			{
				DataCenter.livesData.PropertyChanged += OnLivesDataChanged;
			}
			else
			{
				curData.PropertyChanged += OnDataChanged;
			}
			UpdateCountTextStr();
	        IconPath = UIHelper.GetRewardsIconPath(curData);
	        UpdateLeftTime();
        }

        protected override void OnDispose()
        {
	        if(curData.Id == (int)GameConst.ItemIdCode.EnergyDrink)
	        {
		        DataCenter.livesData.PropertyChanged -= OnLivesDataChanged;
	        }
	        else
	        {
		        curData.PropertyChanged -= OnDataChanged;
	        }
	        base.OnDispose();
        }

        private void OnLivesDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        if(e.PropertyName == nameof(DataCenter.livesData.Curr)|| e.PropertyName == nameof(DataCenter.livesData.MaxLives))
	        {
		        UpdateCountTextStr();
		        if (!DataCenter.livesData.IsFull())
		        {		 
			        UpdateLeftTime();
		        }
	        }
        }

        private void OnDataChanged(object sender, PropertyChangedEventArgs e)
        {
	        UpdateCountTextStr();
        }
        
        [Command]
        private void OnClickBg()
        {
	        onClickItemCallback?.Invoke(curData,tag);
        }

        private void UpdateLeftTime()
        {
	        if (tag == 1 && curData.Id == (int)GameConst.ItemIdCode.EnergyDrink)
	        {
		        IsShowLeftTime = !DataCenter.livesData.IsFull();
		        LeftTimeStr = DataCenter.livesData.NextLiveTime();
		        CountTextStr = $"{DataCenter.livesData.Curr}/{DataCenter.livesData.GetMaxLives()}";
	        }
        }

        private void UpdateCountTextStr()
        {
	        if (curData.Id == (int)GameConst.ItemIdCode.EnergyDrink)
	        {
		        CountTextStr = $"{DataCenter.livesData.Curr}/{DataCenter.livesData.GetMaxLives()}";
	        }
	        else
	        {
		        CountTextStr = Lodash.ConvertNumToString(curData.Count, 1);
	        }
        }
        private float time;
        public override void Update()
        {
	        base.Update();
	        if (!UIHelper.CalculateTime(ref time)) return;
	        UpdateLeftTime();
        }
        
    }
}