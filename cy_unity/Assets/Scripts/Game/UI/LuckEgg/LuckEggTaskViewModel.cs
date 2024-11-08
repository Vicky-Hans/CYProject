using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class LuckEggTaskViewModel : ViewModelBase
    {
        
		 [AutoNotify] private ObservableList<LuckEggTaskItemViewModel> scrollViewList = new();
         [AutoNotify] private CommonTopViewModel commonTopItemsVm;
         public LuckyEggData Data => DataCenter.luckyEggData;
        [Preserve]
        public LuckEggTaskViewModel()
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
            InitTaskList();
            Data.TaskClaimed.CollectionChanged += TaskChanged;
            Data.TaskProgress.CollectionChanged += TaskChanged;
            PlayerInfoManager.Instance.PropertyChanged  += SecondDay;
        }
        protected override void OnDispose()
        {
            base.OnDispose();
            Data.TaskClaimed.CollectionChanged -= TaskChanged;
            Data.TaskProgress.CollectionChanged -= TaskChanged;
            PlayerInfoManager.Instance.PropertyChanged  -= SecondDay;
            foreach (var item in scrollViewList)
            {
                item.Dispose();
            }
        }
        private void InitTaskList()
        {
            ScrollViewList.Clear();
            var temp = Data.GetAllRewardList((int)ETaskType.LuckEgg);
            var tempList = new List<LuckEggTaskItemViewModel>();
            for (int i = 0; i < temp.Count; i++)
            {
                tempList.Add(new LuckEggTaskItemViewModel(temp[i]));
            }

            tempList = tempList.OrderBy(o => o.State == LuckEggTaskItemState.Finish)
                .ThenByDescending(o => o.State == LuckEggTaskItemState.NotGetAward).ToList();
            ScrollViewList.AddRange(tempList);
        }
        private void TaskChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InitTaskList();
        }

        private void SecondDay(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            InitTaskList();
        }

    }
}