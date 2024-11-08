using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class ShopDrawClothesInfoViewModel : ViewModelBase
    {
        
		[AutoNotify] private ObservableList<ShopDrawClothesInfoItemViewModel> scrollViewList = new();

        [Preserve]
        public ShopDrawClothesInfoViewModel()
        {
	        UpdateRankPanel();
        }
        
        private void UpdateRankPanel()   
        {
	        scrollViewList.Clear();
	        var cfg =  ConfigCenter.EquipChestCfgColl.GetDataById((int)ShopChestId.Cloth);

	        var defines = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Probability);
	        var elseValue = 0f;
	        for (int i = cfg.JackpotId.Count-1; i >= 0; i--)
	        {
		        var  jackPotCfg =  ConfigCenter.JackpotCfgColl.GetDataById(cfg.JackpotId[i]);
		        if (jackPotCfg != null)
		        {
			        bool isFirst = true;
			        List<Reward> showList = new List<Reward>();
			        var value = LocalizeHelper.GetGlobal(GlobalLanguageId.heroEquip_Tips_32)+ (defines.Content[defines.Content.Count-1 - i] / 100f).ToString("0.##")+"%";
			        for (int j = 0; j < jackPotCfg.RandomReward.Count; j++)
			        {
				        showList.Add(UIHelper.RandomRewardToReward(jackPotCfg.RandomReward[j]));
				        if (showList.Count >= 5)
				        {

					        var tempVm = new ShopDrawClothesInfoItemViewModel(showList,isFirst? ShopManager.Instance
						        .GetJackpotName(jackPotCfg.Id):null,value,isFirst?jackPotCfg.RandomReward.Count:5,jackPotCfg.JackpotQua);
					        scrollViewList.Add(tempVm);
					        isFirst = false;
					        showList.Clear();
				        }
			        }

			        if (showList.Count > 0)
			        {
				        var tempVm = new ShopDrawClothesInfoItemViewModel(showList,isFirst?ShopManager.Instance
					        .GetJackpotName(jackPotCfg.Id):null,value,isFirst?jackPotCfg.RandomReward.Count:5,jackPotCfg.JackpotQua);
				        scrollViewList.Add(tempVm);
			        }
		        }
	        }
        }
        
        public string GetNumDesc(List<int> jackpotList,int curWidth,float coefficient = 1)
        {
	        int width = 0;
	        for (int i = 0; i < jackpotList.Count; i++)
	        {
		        var  jackPotCfg =  ConfigCenter.JackpotCfgColl.GetDataById(jackpotList[i]);
		        width += jackPotCfg.Weight;
	        }
	        return ((float)curWidth / width / coefficient * 100).ToString("0.##")+"%";
        }
        
        public float GetNum(List<int> jackpotList,int curWidth,float coefficient = 1)
        {
	        int width = 0;
	        for (int i = 0; i < jackpotList.Count; i++)
	        {
		        var  jackPotCfg =  ConfigCenter.JackpotCfgColl.GetDataById(jackpotList[i]);
		        width += jackPotCfg.Weight;
	        }

	        return (float)curWidth / width / coefficient;
        }



        [Command]
        private void OnClickBtnClose()
        {
	        UIManager.Instance.CloseDialog<ShopDrawClothesInfoView>();
        }

        
    }
}