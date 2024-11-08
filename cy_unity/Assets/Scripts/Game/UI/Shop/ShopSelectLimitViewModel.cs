using System;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;
using UnityEngine.UI;


namespace DH.Game.ViewModels
{
    public partial class ShopSelectLimitViewModel : ViewModelBase
    {
        
		[AutoNotify] private string nameStr;
		[AutoNotify] private CellItemBaseViewModel cellItemViewVm;
		[AutoNotify] private string limitDescValueStr;
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeModel;
		private int selectNum;
		[AutoNotify] private int allLimitNum;
		[AutoNotify] private bool isLimitOwn;
		[AutoNotify] private bool isMinPos;
		[AutoNotify] private bool isMaxPos;

		[AutoNotify] public Action<int> selectAction;

		[AutoNotify] private int maxBuyNum;
		[AutoNotify] private string buyDesc;
		public int SelectNum
		{
			get => selectNum;
			set
			{
				Set(ref selectNum, value);
				RefreshLimit();
			}
		}
		
		private Slider slider;

		public Slider Slider
		{
			get => slider;
			set
			{
				slider = value;
				if(slider==null) return;
				slider.onValueChanged.RemoveAllListeners();
				slider.onValueChanged.AddListener(SliderOnValueChanged);
				SetSliderValue();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reward">购买物品</param>
		/// <param name="price">购买价格物品</param>
		/// <param name="selectAction">购买返回事件</param>
		/// <param name="limit">限购数量</param>
		/// <param name="selectNum">购买初始数量</param>
		/// <param name="name">购买物品名称</param>
		/// <param name="isLimitOwn">是否根据根据拥有数量限制</param>
		[Preserve]
        public ShopSelectLimitViewModel(Reward reward,Reward price, Action<int> selectAction,int limit=-1, int selectNum=1,string name = null,bool isLimitOwn = true)
        {
	        CellItemViewVm = CellItemBaseViewModel.Create(reward);
	        ItemPriceNodeModel = new ItemPriceNodeModel(price,true);
	        if (string.IsNullOrEmpty(name))
	        {
		        NameStr = UIHelper.GetRewardName(reward);
	        }
	        AllLimitNum = limit;
	        IsLimitOwn = isLimitOwn;
	        SelectAction = selectAction;
	        SelectNum = selectNum;
	        MaxBuyNum = UIHelper.GetMaxBuyNum(AllLimitNum, price);
	        BuyDesc = string.Empty; //$"是否花费{UIHelper.GetRewardName(price)}购买{UIHelper.GetRewardName(reward)}";
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        ItemPriceNodeModel?.Dispose();
	        cellItemViewVm?.Dispose();
        }

        private void RefreshLimit()
        {
	        ItemPriceNodeModel.BuyNum = SelectNum;
	        if (AllLimitNum == -1)
	        {
		        LimitDescValueStr = string.Empty;
	        }
	        else
	        {
		        LimitDescValueStr = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.Shop_tips18,selectNum,AllLimitNum)}";
	        }

	        if (AllLimitNum != -1)
	        {
		        IsMinPos = SelectNum <= 1;
	        
		        if (SelectNum < AllLimitNum)
		        {
			        if (IsLimitOwn)
			        {
				        IsMaxPos = !DataCenter.itemsData.CheckItemIsEnough(ItemPriceNodeModel.Reward, SelectNum + 1);
			        }
		        }
		        else
		        {
			        IsMaxPos = true;
		        }
	        }
	        else
	        {
		        IsMinPos = false;
		        IsMaxPos = false;
	        }

	   

        }

        public void SliderOnValueChanged(float value)
        {
	        var num = Mathf.RoundToInt(value * (AllLimitNum-1))+1;
	        num = Mathf.Min(num, MaxBuyNum);
	        num = Mathf.Max(num, 1);
	        SelectNum = num;
	        SetSliderValue();
        }

        private void SetSliderValue()
        {
	        if(Slider==null) return;
	        if (AllLimitNum == 1)
	        {
		        Slider.value = 1;
	        }
	        else
	        {
		        Slider.value = (SelectNum - 1f) / (AllLimitNum - 1);
	        }
        }


        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ShopSelectLimitView>();
        }

        [Command]
        private void OnClickBtnDel()
        {
	        if(IsMinPos) return;
	        if (SelectNum > 1)
	        {
		        SelectNum -= 1;
	        }
	        RefreshLimit();
	        SetSliderValue();
        }

        [Command]
        private void OnClickBtnAdd()
        {
	        if(IsMaxPos) return;
	        if (IsLimitOwn)
	        {
		        if (DataCenter.itemsData.CheckItemIsEnough(ItemPriceNodeModel.Reward, SelectNum + 1))
		        {
			        SelectNum += 1;
		        }
	        }
	        else
	        {
		        SelectNum += 1;
	        }

	        SetSliderValue();
        }

        [Command]
        private void OnClickBtnConfirm()
        {
	        OnClickBtnClose();
	        SelectAction?.Invoke(SelectNum);
        }
        
    }
}