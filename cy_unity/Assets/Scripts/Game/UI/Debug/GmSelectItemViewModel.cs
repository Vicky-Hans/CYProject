using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;

namespace DH.Game.ViewModels
{
    public partial class GmSelectItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private ObservableList<ObservableObject> scrollViewList = new();
        [AutoNotify] private ObservableList<ObservableObject> scrollViewList1 = new();
		[AutoNotify] private string btnClosePath;

        public GmParamType ParamType;
        public Action<int> SelectInfoAction;
        [Preserve]
        public GmSelectItemViewModel(GmParamType type)
        {
            ParamType = type;
            InitSelectList();
        }

        private void InitSelectList()
        {
            if (ParamType == GmParamType.Item)
            {
                InitItemSelectList();
            }else if (ParamType == GmParamType.Talent)
            {
                InitTalentSelectList();
            }else if (ParamType == GmParamType.Equip)
            {
                InitEquipSelectList();
            }else if (ParamType == GmParamType.Clothes)
            {
                InitClothesSelectList();
            }

        }

        private void InitTalentSelectList()
        {
            scrollViewList1.Clear();
            var list = ConfigCenter.TalentCfgColl.DataItems.ToList();
            for (int i = list.Count-1; i >=0; i--)
            {
                ScrollViewList1.Add(new TalentChooseItemViewModel(0,list[i].Id, (index,Id) =>
                {
                    SelectInfoAction?.Invoke(Id);
                    OnClickClose();
                }));
            }
        }
        
        private void InitEquipSelectList()
        {
            scrollViewList.Clear();
            var list = ConfigCenter.EquipModelCfgColl.DataItems.ToList();
            
            for (int i = list.Count-1; i >=0; i--)
            {
                var cellItem = CellItemBaseViewModel.Create(new Reward(RewardType.Equip, list[i].Id, 1),ECellItemSizeType.Size166X150,false,false);
                cellItem.OnClickEvent = (info) =>
                {
                    SelectInfoAction?.Invoke(cellItem.BaseData.Id);
                    OnClickClose();
                };
                scrollViewList.Add(cellItem);
            }
        }

        private void InitItemSelectList()
        {
            scrollViewList.Clear();
            var allItem = ConfigCenter.ItemCfgColl.DataItems.ToList();
            foreach (var item in allItem)
            {
                if(item.Id == (int)GameConst.ItemIdCode.EnergyDrink || item.Id == (int)GameConst.ItemIdCode.Exp) continue;
                var cellItem = CellItemBaseViewModel.Create(new Reward(RewardType.Item, item.Id, 1),ECellItemSizeType.Size166X150,false,false);
                cellItem.OnClickEvent = (info) =>
                {
                    SelectInfoAction?.Invoke(cellItem.BaseData.Id);
                    OnClickClose();
                };
                ScrollViewList.Add(cellItem);
            }
        }

        private void InitClothesSelectList()
        {
            scrollViewList.Clear();
            var list = ConfigCenter.HeroEquipResourceCfgColl.DataItems.ToList();
            foreach (var item in list)
            {
                if(item.PartId==0) continue;
                var heroEquipData = new HeroEquipData()
                {
                    Uid = 0,
                    Id = item.Id,
                    QuaId = ClothesManager.Instance.GetBaseQua(item.Id),
                    Lv = 1,
                };
                    
                var cellItem = CellItemBaseViewModel.Create(heroEquipData);
                cellItem.OnClickEvent = (info) =>
                {
                    SelectInfoAction?.Invoke(cellItem.BaseData.Id);
                    OnClickClose();
                };
                scrollViewList.Add(cellItem);
            }
        }

        [Command]
        private void OnClickClose()
        {
            UIManager.Instance.CloseDialog<GmSelectItemView>();
        }

    }
}