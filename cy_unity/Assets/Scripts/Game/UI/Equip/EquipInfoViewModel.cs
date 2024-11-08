using System.Collections.Generic;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class EquipInfoViewModel : ViewModelBase
    {
	    [AutoNotify] private EquipCfg cfg;
	    [AutoNotify] private bool isOwn;
		[AutoNotify] private string nameStr;
		[AutoNotify] private EquipItemViewModel equipItemViewVm;
		[AutoNotify] private EquipUnOwnItemViewModel equipUnOwnItemViewModel;
		[AutoNotify] private ObservableList<EquipAttrItemViewModel> equipAttrItemViewModelList = new();
		[AutoNotify] private ObservableList<EquipSkillItemViewModel> equipSkillItemList = new();
		[AutoNotify] private ItemPriceNodeModel itemPriceNodeVm;
		[AutoNotify] private bool isEnoughItem;
		[AutoNotify] private CommonTopViewModel commonTopViewModel;
		private EquipItemData itemData;

		public ParticleSystem UpLevelSucceedEffect;
		[AutoNotify] private bool isMaxLevel;
		[AutoNotify] private string equipTypePath;

		[AutoNotify] private bool isCloseButton ;
		private EquipItemData ItemData
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
		
		[Preserve]
        public EquipInfoViewModel(int id,bool isShowBase = false)
        {
	        IsCloseButton = isShowBase;
	        Cfg = ConfigCenter.EquipCfgColl.GetDataById(id);
	        IsOwn = DataCenter.equipData.IsOwn(id);
	        NameStr = EquipManager.Instance.GetEquipName(Cfg.Id);
	        ItemData = DataCenter.equipData.GetEquipItemData(Cfg.Id);
	        EquipTypePath = EquipManager.Instance.GetEquipAtkTypeIcon(Cfg);
	        EquipItemViewVm = new EquipItemViewModel(Cfg.Id);
	        EquipUnOwnItemViewModel = new EquipUnOwnItemViewModel(Cfg.Id);
	        EquipItemViewVm.ClickItemAction=()=>{
		        //点击事件
	        };
	        InitAttrList();
	        InitSkillList();
	        RefreshItem();
	        CommonTopViewModel = new CommonTopViewModel(new List<GameConst.ItemIdCode>()
	        {
		        GameConst.ItemIdCode.Money,
	        }, (data,pos) =>
	        {
		        OnClickClose();
	        });
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        UIHelper.ViewModelBaseOnDisposes(equipAttrItemViewModelList);
	        UIHelper.ViewModelBaseOnDisposes(equipSkillItemList);
	        ItemData = null;
	        ItemPriceNodeVm?.Dispose();
        }

        private void ItemDataChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(ItemData.Lv))
	        {
		        InitAttrList();
		        RefreshItem();
	        }
        }

        private void InitAttrList()
        {
	        EquipAttrItemViewModelList.ClearAndDispose();
	        if(cfg.Attr==null) return;
	        EquipAttrItemViewModelList.Add(new EquipAttrItemViewModel("equip[common_icon_target]",LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_09),EquipManager.Instance.GetTargetDesc(Cfg.Id),IsOwn));
	        var allAttrList = EquipManager.Instance.GetEquipAttrList(cfg.Id);
	        var nextAttrList =!IsCloseButton?EquipManager.Instance.GetEquipNextAddAttrList(cfg.Id):new Dictionary<string, int>();
	        foreach (var item in allAttrList)
	        {
		        if (nextAttrList.ContainsKey(item.Key))
		        {
			        EquipAttrItemViewModelList.Add(new EquipAttrItemViewModel(new WeaponAttr(item.Key,item.Value),IsOwn,new WeaponAttr(item.Key,nextAttrList[item.Key])));
		        }
		        else
		        {
			        EquipAttrItemViewModelList.Add(new EquipAttrItemViewModel(new WeaponAttr(item.Key,item.Value),IsOwn));
		        }
	        }
        }
        
        private void InitSkillList()
        {
	        equipSkillItemList.ClearAndDispose();
	        if(cfg.EquipSkillId==null) return;
	        foreach (var item in Cfg.EquipSkillId)
	        {
		        equipSkillItemList.Add(new EquipSkillItemViewModel(item,cfg.Id));
	        }
        }

        private void RefreshItem()
        {
	        IsMaxLevel = EquipManager.Instance.IsMaxLevel(Cfg.Id,ItemData?.Lv ?? 0); 
	        var rewardList = EquipManager.Instance.GetEquipLvNeedGoldItem(cfg);
	        if (rewardList != null && rewardList.Count > 0)
	        {
		        ItemPriceNodeVm = new ItemPriceNodeModel(rewardList[0]);
		        IsEnoughItem = Lodash.CheckRewardIsEnough(rewardList);
	        }
        }

        [Command]
        private void OnClickBtnSkin()
        {
	        UIManager.Instance.OpenDialog<EquipStateView>(new EquipStateViewModel(Cfg.Id)).Forget();
        }

        [Command]
        private void OnClickButton()
        {
	        if (!UIHelper.CheckRewardIsEnough(itemPriceNodeVm.Reward,isJump:true))
	        {
		        return;
	        }
	        
	        if (EquipItemViewVm == null || !(EquipItemViewVm.NeedItemNum != 0 && EquipItemViewVm.OwnItemNum >= EquipItemViewVm.NeedItemNum))
	        {
		        if (!UIHelper.CheckRewardIsEnough(new Reward(RewardType.Item, cfg.ItemId, EquipItemViewVm.NeedItemNum), isJump: true))
		        {
			        return;
		        }
		        ToastManager.ShowLanguage(GlobalLanguageId.Equip_15);
		        return;
	        }
		    EquipManager.Instance.SendEquipLevelUp(Cfg.Id, () =>
		    {
			    UIHelper.PlayEffect(UpLevelSucceedEffect);
		    }).Forget();
	        
        }

        [Command]
        private void OnClickClose()
        {
	        UIManager.Instance.CloseDialog<EquipInfoView>();
        }
    }
}