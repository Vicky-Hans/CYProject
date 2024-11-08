using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ClothesResetViewModel : ViewModelBase
    {
	    public long ClothesUid;
		[AutoNotify] private ObservableList<CellItemBaseViewModel> scrollViewList = new();
		[AutoNotify] private CellItemBaseViewModel cellItemBaseViewVm;
		[AutoNotify] private TabBtnGroupViewModel tabGroupVm;
		[AutoNotify] private string resetDescStr;
		[AutoNotify] private string resetTipsDescStr;

		[AutoNotify] private int selectPos;
		[AutoNotify] private string noneTipsStr;
		[AutoNotify] private bool isNoneState;
        [Preserve]
        public ClothesResetViewModel(long clothesUid,int initPos = 1)
        {
	        ClothesUid = clothesUid;
	        CellItemBaseViewVm = CellItemBaseViewModel.Create(ClothesUid);
	        CellItemBaseViewVm.OnClickEvent = (info) =>
	        {
				//拦截原有点击，不可删除
	        };
	        InitTab(initPos);
	        RefreshBackReward();
        }

        private void InitTab(int initPos)
        {
	        List<TabBtnInfo> btnInfos = new();
	        var btnInfo = new TabBtnInfo()
	        {
		        Pos = 0,
		        Name = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_18),
	        }; 
	        
	        var btnInfo1 = new TabBtnInfo()
	        {
		        Pos = 1,
		        Name = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_17),
	        }; 
	        btnInfos.Add(btnInfo1);
	        btnInfos.Add(btnInfo);
	        tabGroupVm = new TabBtnGroupViewModel(btnInfos, initPos, (pos) =>
	        {
		        SelectPos = pos;
		        ResetDescStr = SelectPos == 0 ? LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_18) :LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_17);
		        ResetTipsDescStr  = SelectPos == 0 ? LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_20) :LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_19);
		        NoneTipsStr = SelectPos == 0 ? LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_22) :LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_21);
		        RefreshBackReward();
	        });
        }

        private void RefreshBackReward()
        {
	        IsNoneState = SelectPos == 0 ? !ClothesManager.Instance.IsCanResetQua(ClothesUid) : !ClothesManager.Instance.IsCanResetLv(ClothesUid);
	        ScrollViewList.ClearAndDispose();
	        if(IsNoneState) return;
	        var rewardList = ClothesManager.Instance.GetResetBackRewards(ClothesUid,SelectPos==0);
	        if (SelectPos == 0)
	        {
		        var data = DataCenter.clothesData.GetHeroEquipDataByUid(ClothesUid);
		        var resoure = new Resource()
		        {
			        Type = (int)RewardType.HeroEquip,
			        Id = data.Id,
			        Count = 0,
			        HeroEquip = new HeroEquip()
			        {
				        Uid = 0,
				        Id = data.Id,
				        Lv = 1,
				        QuaId = SelectPos == 0 ? ClothesManager.Instance.GetQuaUpStateStart(data.QuaId) : data.QuaId,
			        }
		        };
		        var model = CellItemBaseViewModel.Create(resoure);
		        model.OnClickEvent = (info) =>
		        {
					//注释原有点击事件，不可删除
		        };
		        ScrollViewList.Add(model);
	        }

	        foreach (var item in rewardList)
	        {
		        ScrollViewList.Add(CellItemBaseViewModel.Create(item));
	        }
        }

        [Command]
        private void OnClickBtnReset()
        {
	        //点击降星还是降低品质
	        ClothesManager.Instance.SendResetClothes(ClothesUid,selectPos==0?2:1, () =>
	        {
		        UIManager.Instance.CloseDialog<ClothesResetView>();
	        }).Forget();
        }

        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ClothesResetView>();
        }

        
    }
}