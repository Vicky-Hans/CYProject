using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class EquipItemViewModel : ViewModelBase
    {
        [AutoNotify] private bool isShow;
        [AutoNotify] private int id;
        [AutoNotify] private EquipCfg cfg;
        private EquipItemData itemData;

        [AutoNotify] private bool isLock;
        //数据状态
        [AutoNotify] private string name;
        [AutoNotify] private string iconPath;
        [AutoNotify] private string bgPath;
        [AutoNotify] private string levelBgPath;
        
        [AutoNotify] private int level;
        [AutoNotify] private string typeIconPath;
        [AutoNotify] private string attrIconPath;
        [AutoNotify] private bool isEnoughAttr;
        [AutoNotify] private int attrNum;
        [AutoNotify] private bool selectState;

        [AutoNotify] private bool isUse;
        [AutoNotify] private int needItemNum;
        [AutoNotify] private long ownItemNum;
        [AutoNotify] private bool isSelectIng;
        [AutoNotify] private bool isSelectAnimatorIng;

        [AutoNotify] private bool isReplace;
        [AutoNotify] private bool isMaxLevel;
        [AutoNotify] private bool isEnouthUpItem;
        public Action ClickItemAction;
        public EquipItemData ItemData
        {
            get => itemData;
            set
            {
                var old = itemData;
                Set(ref itemData, value);
                if (old != null)
                {
                    old.PropertyChanged -= EquipItemDataChanged;
                }
                
                if (itemData != null)
                {
                    itemData.PropertyChanged += EquipItemDataChanged;
                }
            }
        }

        private ResourceData needItemData;
        public ResourceData NeedItemData
        {
            get => needItemData;
            set
            {
                var old = needItemData;
                Set(ref needItemData, value);
                if (old != null)
                {
                    old.PropertyChanged -= NeedItemDataChanged;
                }
                
                if (needItemData != null)
                {
                    needItemData.PropertyChanged += NeedItemDataChanged;
                }
            }
        }
        

        [Preserve]
        public EquipItemViewModel(int cfgId,bool show = true,bool isReplace=false)
        {
            IsReplace = isReplace;
            IsShow = show;
            Id = cfgId;
            Cfg = ConfigCenter.EquipCfgColl.GetDataById(Id);
            ItemData = DataCenter.equipData.GetEquipItemData(Id);
            IsLock = DataCenter.equipData.IsOwn(Id);
            NeedItemData = DataCenter.itemsData.GetResourceDataById(Cfg?.ItemId ?? 0);
            BaseInfo();
            Refresh();
            RefreshSelect();
            EquipManager.Instance.PropertyChanged += SelectChange;
            DataCenter.itemsData.OnItemUpdate += UpdateItem;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ItemData = null;
            NeedItemData = null;
            EquipManager.Instance.PropertyChanged -= SelectChange;
            DataCenter.itemsData.OnItemUpdate -= UpdateItem;
        }

        private void UpdateItem(ResourceData data)
        {
            var reward = EquipManager.Instance.GetEquipLvNeedGoldItem(cfg);
            IsEnouthUpItem = Lodash.CheckRewardIsEnough(reward);
        }

        private void NeedItemDataChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        public void MergeInfo(int cfgId,bool show = true)
        {
            IsShow = show;
            var oldId = Id;
            Id = cfgId;
            Cfg = ConfigCenter.EquipCfgColl.GetDataById(Id);
            ItemData = DataCenter.equipData.GetEquipItemData(Id);
            IsLock = DataCenter.equipData.IsOwn(Id);
            NeedItemData = DataCenter.itemsData.GetResourceDataById(Cfg?.ItemId ?? 0);
            if(oldId!=Id)
                BaseInfo();
            Refresh();
            RefreshSelect();
        }

        private void SelectChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName==(nameof(EquipManager.Instance.CurSelectEquipId)))
            {
                RefreshSelect();
            }else if (e.PropertyName==(nameof(EquipManager.Instance.CurSelectEquipId)))
            {
                RefreshSelect();
            }
        }

        private void BaseInfo()
        {
            Name = EquipManager.Instance.GetEquipName(Id);
            BgPath =EquipManager.Instance.GetBgPath(Cfg);
            AttrIconPath = EquipManager.Instance.GetAttrIconPath(Cfg);
            TypeIconPath = EquipManager.Instance.GetEquipType(Cfg?.GridType??0);
            IconPath = EquipManager.Instance.GetIconPath(Cfg);
        }
        
        private void EquipItemDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EquipItemData.Lv))
            {
                BaseInfo();
            }
            Refresh();
        }

        private void Refresh()
        {
            if (ItemData!=null && Cfg!=null)
            {
                Level = itemData.Lv;
                NeedItemNum = EquipManager.Instance.GetEquipLvNeedItem(Id);
                OwnItemNum = DataCenter.itemsData.GetItemCountById(Cfg.ItemId);
                IsUse = DataCenter.equipData.IsUseIng(Id);
                IsEnoughAttr = !EquipManager.Instance.IsEnoughEquipAttr(Id);
                AttrNum = EquipManager.Instance.GetEquipAttrNum(Id);
                IsMaxLevel = EquipManager.Instance.IsMaxLevel(Id,ItemData?.Lv ?? 0);
                var reward = EquipManager.Instance.GetEquipLvNeedGoldItem(cfg)?[0];
                IsEnouthUpItem = UIHelper.CheckRewardIsEnough(reward);
            }
            else
            {
                Level = 0;
                NeedItemNum = 0;
                OwnItemNum = DataCenter.itemsData.GetItemCountById(Cfg?.ItemId ?? 0);
                IsUse = false;
                IsEnoughAttr = !EquipManager.Instance.IsEnoughEquipAttr(Id);
                AttrNum = EquipManager.Instance.GetEquipAttrNum(Id);
                IsMaxLevel = false;
                IsEnouthUpItem = false;
            }
        }

        private void RefreshSelect()
        {
            var showSelect = EquipManager.Instance.CurSelectEquipId == Id && !isReplace;
            if ( showSelect != IsSelectIng)
            {
                IsSelectIng = showSelect;
                isSelectAnimatorIng = IsSelectIng;
            }
            else
            {
                IsSelectAnimatorIng = IsSelectIng;
            }

        }

        [Command]
        private void OnClickInfo()
        {
            EquipManager.Instance.CurSelectEquipId = 0;
            UIManager.Instance.OpenDialog<EquipInfoView>(new EquipInfoViewModel(Id)).Forget();
        }
        
        [Command]
        private void OnClickUse()
        {
            EquipManager.Instance.CurSelectEquipId = 0;
            if (EquipManager.Instance.IsExistNoneSlots())
            {
                if (EquipManager.Instance.CheckEquipIsCanUse(Id))
                {
                    EquipManager.Instance.SendEquipBattle(Id).Forget();
                }
                else
                {
                    ToastManager.ShowLanguage(GlobalLanguageId.Equip_20);
                }

            }
            else
            {
                EquipManager.Instance.ReplaceEquipId = Id;
            }

        }
        
        [Command]
        private void OnClickDischarge()
        {
            EquipManager.Instance.CurSelectEquipId = 0;
            if (DataCenter.equipData.IsUseIng(Id))
            {
                EquipManager.Instance.SendEquipRemoveBattle(Id).Forget();
            }
        }

        [Command]
        private void OnClickOpenInfo()
        {
            if(ClickItemAction!=null)
            {
                ClickItemAction?.Invoke();
                return;
            }

            if (EquipManager.Instance.curSelectEquipId == Id)
            {
                IsSelectAnimatorIng = false;
                isSelectIng = false;
                EquipManager.Instance.CurSelectEquipId = 0;
            }
            else
            {
                IsSelectAnimatorIng = true;
                isSelectIng = true;
                EquipManager.Instance.CurSelectEquipId = Id;
            }
        }
    }
}