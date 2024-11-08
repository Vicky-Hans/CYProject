using System;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.ViewModels;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class EquipSkillItemViewModel : ViewModelBase
    {
        [AutoNotify] private EquipSkillCfg skillCfg;
        [AutoNotify] private int skillId;
        [AutoNotify] private string bgPath;
        [AutoNotify] private string targetIconPath;
        [AutoNotify] private string desc;
        [AutoNotify] private bool isOwn;
        [AutoNotify] private bool isLock;

        private EquipItemData itemData;
        [AutoNotify]private int equipId;

        public EquipItemData ItemData
        {
            get => itemData;
            set
            {
                var old = itemData;
                Set(ref itemData, value);
                if (old != null) old.PropertyChanged -= ItemDataChange;
                if (itemData != null) itemData.PropertyChanged += ItemDataChange;
            }
        }

        
        private ClickTextComponent clickTextCmp;

        public ClickTextComponent ClickTextCmp
        {
            get => null;
            set { 
                clickTextCmp = value;
                if (clickTextCmp != null)
                {
                    clickTextCmp.ClickCallback = OnClickLinkCallback;
                }
            }
        }

        public Action<int> ClickTmpAction;
        [Preserve]
        public EquipSkillItemViewModel(int skillId,int equipId=0)
        {
            SkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(skillId);
            SkillId = skillId;
            EquipId = equipId;
            ItemData = DataCenter.equipData.GetEquipItemData(equipId);
            RefreshInfo();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ItemData = null;
        }

        private void RefreshInfo()
        {
            IsOwn = equipId==0 || DataCenter.equipData.IsOwn(equipId);
            IsLock =equipId!=0  && (ItemData == null || ItemData.Lv < skillCfg.LvUnlockId);
            if (IsOwn)
            {
                BgPath = IsLock ? "equip[equip_panel_13]" : "equip[equip_panel_12]";
            }
            else
            {
                BgPath = "equip[equip_panel_14]";
            }
            
            TargetIconPath = EquipManager.Instance.GetEquipSkillIcon(SkillId);
            ItemData = DataCenter.equipData.GetEquipItemData(equipId);
            if (IsOwn)
            {
                if (IsLock)
                {
                    Desc = $"<color=#FFE6A4> {LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_08,skillCfg.LvUnlockId)}</color>{EquipManager.Instance.GetEquipSkillDesc(SkillId)}";
                }
                else
                {
                    Desc = EquipManager.Instance.GetEquipSkillDesc(SkillId);
                }
            }
            else
            {
                Desc = $"<color=#CBD3EB>{LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_08,skillCfg.LvUnlockId)}</color><color=#303238>{EquipManager.Instance.GetEquipSkillDesc(SkillId)}</color>";
            }
        }
        
        private void ItemDataChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemData.Lv))
            {
                RefreshInfo();
            }
        }
        
        private void OnClickLinkCallback(string info, Vector3 arg2)
        {
            UIHelper.OnClickDescLink(info,arg2);
        }
    }
}