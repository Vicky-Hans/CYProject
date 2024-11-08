using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.ComponentModel;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class ClothesSkillItemViewModel : ViewModelBase
    {
	    [AutoNotify] private HeroEquipSkillCfg skillCfg;
	    [AutoNotify] private int skillId;
		[AutoNotify] private string ownIconPath;
		[AutoNotify] private string lockPath;
		[AutoNotify] private string descStr;
		[AutoNotify] private bool isLock;
		[AutoNotify] private GameObject selfNode;

		private HeroEquipData data;
		public HeroEquipData Data
		{
			get => data;
			set
			{
				var old = data;
				Set(ref data, value);
				if (old != null)
				{
					old.PropertyChanged -= HeroEquipDataChange;
				}
                
				if (data != null)
				{
					data.PropertyChanged += HeroEquipDataChange;
				}
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

		[Preserve]
        public ClothesSkillItemViewModel(int skillId,long uid)
        {
	        SkillId = skillId;
	        skillCfg = ConfigCenter.HeroEquipSkillCfgColl.GetDataById(SkillId);
	        if(uid!=0)
				Data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
	        DescStr = ClothesManager.Instance.GetClothesSkillDesc(skillId);
	        SetStateIconPath();
	        Refresh();
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        Data = null;
        }
        
        private void HeroEquipDataChange(object sender, PropertyChangedEventArgs e)
        {
	        Refresh();
        }

        private void Refresh()
        {
		    IsLock = Data.IsNull() || skillCfg.QuaId > ClothesManager.Instance.GetQuaSmallByQuaId(Data.QuaId);
		    // BgPath = IsLock ? "equip[equip_panel_13]" : "equip[equip_panel_12]";
        }

        private void SetStateIconPath()
        {
	        switch (skillCfg.QuaId)
	        {
		        case 3:
		        {
			        OwnIconPath = "item[item_icon_blue2]";
			        LockPath = "item[item_icon_blue]";
		        } break;
		        case 2:
		        {
			        OwnIconPath = "item[item_icon_green2]";
			        LockPath = "item[item_icon_green]";
		        } break;
		        case 4:
		        {
			        OwnIconPath = "item[item_icon_purple2]";
			        LockPath = "item[item_icon_purple]";
		        } break;
		        case 5:
		        {
			        OwnIconPath = "item[item_icon_yellow2]";
			        LockPath = "item[item_icon_yellow]";
		        } break;
		        case 6:
		        {
			        OwnIconPath = "item[item_icon_red2]";
			        LockPath = "item[item_icon_red]";
		        } break;
		        default:
		        {
			        OwnIconPath = "item[item_icon_green2]";
			        LockPath = "item[item_icon_green]";
		        } break;
	        }

	        
        }
        
        private void OnClickLinkCallback(string info, Vector3 arg2)
        {
	        UIHelper.OnClickDescLink(info,arg2);
        }
        
    }
}