using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class ClothesInfoViewModel : ViewModelBase
    {
	    [AutoNotify] private Reward reward;
		[AutoNotify] private string nameStr;
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private int maxLevel;
		[AutoNotify] private string levelStr;
		
		[AutoNotify] private string qualityBgPath;
		[AutoNotify] private string qualityNameStr;
		[AutoNotify] private ObservableList<AttrItemViewModel> attrScrollViewList = new();
	    [AutoNotify] private ObservableList<ClothesSkillItemViewModel> scrollViewList = new();
		[AutoNotify] private string maxTipsStr;
		[AutoNotify] private bool isMaxLevel;
		[AutoNotify] private bool isAllMaxLevel;
		[AutoNotify] private bool isOwn;
		[AutoNotify] private bool isUseIng;
		[AutoNotify] private ParticleSystem upEffect;
		[AutoNotify] private ParticleSystem upEffectAttr;

		[AutoNotify] private bool isShowInfo;
		[AutoNotify] private bool isShowResetBtn;
		[AutoNotify] private bool isHightRare;
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
		
		public Func<object, object> GetGridCellCallback => GetGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,ItemPriceNodeModel> gridDictionary = new();
		
        [Preserve]
        public ClothesInfoViewModel(long uid,bool isShowBtn = false)
        {
	        IsShowInfo = isShowBtn;
	        Data = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
	        Init();
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(Data,ECellItemSizeType.Size216X150);
	        Refresh();
	        RefreshSkill();
        }
        
        [Preserve]
        public ClothesInfoViewModel(HeroEquipData data)
        {
	        Data = data;
	        Init();
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(Data,ECellItemSizeType.Size216X150);
	        Refresh();
	        RefreshSkill();
        }
        
        [Preserve]
        public ClothesInfoViewModel(Reward reward)
        {
	        Reward = reward;
	        if (reward.Type == RewardType.HeroEquip)
	        {
		        Data = new HeroEquipData()
		        {
			        Id = reward.Id,
			        QuaId = ClothesManager.Instance.GetBaseQua(reward.Id),
		        };
	        }
	        Init();
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(Reward,ECellItemSizeType.Size216X150);
	        Refresh();
	        RefreshSkill();
        }

        private void Init()
        {
	        NameStr = ClothesManager.Instance.GetClothesItemName(data?.Id ?? 0);
	        MaxLevel = ClothesManager.Instance.GetClothesMaxLevel(data?.QuaId ?? 0);
	    
        }
        
        private object GetGridCellCallbackByIndex(object index)
        {
	        if (gridDictionary.TryGetValue((int)index, out ItemPriceNodeModel ret))
	        {
		        return ret;
	        }
	        return null;
        }
        
        private void HeroEquipDataChange(object sender, PropertyChangedEventArgs e)
        {
	        Refresh();
        }

        private void Refresh()
        {
	        IsOwn =IsShowInfo && Data != null && DataCenter.clothesData.IsOwn(Data.Uid);
	        IsUseIng = Data != null && DataCenter.clothesData.IsUseIng(Data.Uid);
	        IsMaxLevel = Data!=null && Data.Lv >= MaxLevel;
	        IsAllMaxLevel = Data != null && Data.Lv >= ClothesManager.Instance.GetClothesMaxLevel();
	        LevelStr = $"{Data?.Lv ?? 0}/{MaxLevel}";
	        QualityBgPath = ClothesManager.Instance.GetQuaBgSmall(Data?.Id ?? Reward?.Id ?? 0, Data);
	        QualityNameStr = ClothesManager.Instance.GetQuaName(Data?.Id ?? Reward?.Id ?? 0, Data);
	        IsHightRare = ClothesManager.Instance.IsRareShowById(Data?.Id ?? Reward?.Id ?? 0);
	        RefreshAttr();
	        // RefreshSkill();
	        RefreshReward();
	        RefreshResetBtn();
        }

        private void RefreshAttr()
        {
	        AttrScrollViewList.ClearAndDispose();
	        var attrList = ClothesManager.Instance.GetClothesAttrList(Data);
	        // var addAttrList = ClothesManager.Instance.GetClothesNextAddAttrList(Data?.Uid ?? 0);
	        foreach (var item in attrList)
	        {
		        var addAttr = 0f;
		        // if (IsShowInfo)
		        // {
			       //  if (addAttrList.TryGetValue(item.Key, out var value))
			       //  {
				      //   addAttr = value;
			       //  }
		        // }
		        AttrScrollViewList.Add(new AttrItemViewModel(item.Key,item.Value,addAttr));
	        }
        }

        private void RefreshSkill()
        {
	        ScrollViewList.ClearAndDispose();
	        var skillList = ClothesManager.Instance.GetClothesSkillList(Data?.Id ?? Reward?.Id ?? 0);
	        if(skillList == null) return;
	        foreach (var item in skillList)
	        {
		        ScrollViewList.Add(new ClothesSkillItemViewModel(item.Id,Data?.Uid ?? 0));
	        }
        }

        private void RefreshReward()
        {
	        GridDictionary.Clear();
	        if (!IsMaxLevel && IsShowInfo)
	        {
		        var rewardList = ClothesManager.Instance.GetUpLevelNeedRewardList(Data?.Uid ?? 0);
		        for (int i = 0; i < rewardList.Count; i++)
		        {
			        var model = new ItemPriceNodeModel(rewardList[i], true, null, true);
			        model.IsShowBg = true;
			        GridDictionary.Add(i,model);
		        }
		      
	        }
        }

        private void RefreshResetBtn()
        {
	        IsShowResetBtn = ClothesManager.Instance.IsCanResetQua(Data.Uid) || ClothesManager.Instance.IsCanResetLv(Data.Uid);
        }

        private void PlayUpEffect()
        {
	        UIHelper.PlayEffect(UpEffect);
	        UIHelper.PlayEffect(UpEffectAttr);
	        AudioManager.Instance.Play(AudioType.ClothesLevelUp);
        }


        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ClothesInfoView>();
        }

		[Command]
		private void OnClickBtnRefresh()
		{
			if(Data is { Uid: 0 }) return;
			UIManager.Instance.OpenDialog<ClothesResetView>(new ClothesResetViewModel(Data.Uid)).Forget();
		}

		[Command]
		private void OnClickBtnUnUse(bool use)
		{
			if(Data.IsNull() || Data.Uid==0) return;
			if (use)
			{
				ClothesManager.Instance.SendUseClothes(Data.Uid, () =>
				{
					UIManager.Instance.CloseDialog<ClothesInfoView>();
				});
			}
			else
			{
				ClothesManager.Instance.SendUnLoadClothes(Data.Uid, () =>
				{
					UIManager.Instance.CloseDialog<ClothesInfoView>();
				});
			}
		}

		private bool CheckUpEnough()
		{
			if (GridDictionary != null)
			{
				foreach (var item in GridDictionary)
				{
					if (!UIHelper.CheckRewardIsEnough(item.Value.Reward,true))
					{
						return false;
					}
				}
			}

			return true;
		}

		[Command]
		private void OnClickBtnUpLevel()
		{
			if(IsAllMaxLevel) return;
			if (isMaxLevel)
			{
				var nextId = ClothesManager.Instance.GetNextQuaId(Data.QuaId);
				ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_25,ClothesManager.Instance.GetQuaName(nextId,Data,false),ClothesManager.Instance.GetClothesMaxLevel(nextId));
				return;
			}

			if (Data.IsNull() || Data.Uid==0) return;
			if(!CheckUpEnough()) return;
			ClothesManager.Instance.SendClothesUpLevel(Data.Uid, PlayUpEffect);
		}

		[Command]
		private void OnClickBtnOneKeyUpLevel()
		{
			if(IsAllMaxLevel) return;
			if (isMaxLevel)
			{
				var nextId = ClothesManager.Instance.GetNextQuaId(Data.QuaId);
				var nextData = new HeroEquipData()
				{
					Id = data.Id,
					QuaId = nextId,
					Lv = data.Lv,
					Uid = data.Uid,
				};
				ToastManager.ShowLanguage(GlobalLanguageId.heroEquip_Tips_25,ClothesManager.Instance.GetQuaName(nextId,nextData,false),ClothesManager.Instance.GetClothesMaxLevel(nextId));
				return;
			}
			if(Data.IsNull() || Data.Uid==0) return;
			if(!CheckUpEnough()) return;
			ClothesManager.Instance.SendClothesOneKeyUpLevel(Data.Uid,PlayUpEffect);
		}
    }
}