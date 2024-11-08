using System.ComponentModel;
using DH.Data;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;



namespace DH.Game.ViewModels
{
    public partial class CommonAdvIconViewModel : ViewModelBase
    {
        
		[AutoNotify] private string iconPath;

        private int id => (int)GameConst.ItemIdCode.AdFreeVouche;
        [Preserve]
        public CommonAdvIconViewModel()
        {
            InitUI();
            if (DataCenter.itemsData.ResourceDatas.TryGetValue(id, out var data))
            {
                data.PropertyChanged += ShardChangeEvent;
            }
            DataCenter.monthCardData.PropertyChanged  += ShardChangeEvent;
        }

        private void InitUI()
        {
            if (DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.AdRreeReward)||
                DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.ADFreeForever))
            {
                IconPath ="common[commom_icon_5]";
            }
            else
            {
                var isHave = DataCenter.itemsData.GetItemCount(id) > 0;
                IconPath =isHave ? DataCenter.itemsData.GetItemIconPathById(id) : "common[commom_icon_5]";
            }
        }

        private void ShardChangeEvent(object sender, PropertyChangedEventArgs e)
        {
            InitUI();
        }

        protected override void OnDispose()
        {
            if (DataCenter.itemsData.ResourceDatas.TryGetValue(id, out var data))
            {
                data.PropertyChanged -= ShardChangeEvent;
            }
            DataCenter.monthCardData.PropertyChanged  -= ShardChangeEvent;
            base.OnDispose();
        }
    }
}