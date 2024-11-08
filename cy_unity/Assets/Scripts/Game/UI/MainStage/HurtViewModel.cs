using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class HurtViewModel : ViewModelBase
    {
        
		[AutoNotify] private Vector2 bgSize = new(1024, 1200);
	    [AutoNotify] private ObservableList<HurtCellItemViewModel> hurtScrollviewList = new();

        [Preserve]
        public HurtViewModel(Dictionary<long,long> hurtDic)
        {
	        
	        var toalHurt = 0f;
	        var equipHurtDic =new Dictionary<int, long>();
	        foreach (var item in hurtDic)
	        {
		        toalHurt+= item.Value;
		        var cfg = ConfigCenter.EquipModelCfgColl.GetDataById((int)item.Key);
		        if (!equipHurtDic.TryGetValue(cfg.Equip, out long value))
		        {
			        value = 0;
			        equipHurtDic.Add(cfg.Equip, value);
		        }
		        value += (long)item.Value;
		        equipHurtDic[cfg.Equip] = value;
	        }

	        var tempList = equipHurtDic.ToList();
	        tempList.Sort((a, b) =>
	        {
		        return b.Value > a.Value ? 1 : -1;
	        });
	        foreach (var item in tempList)
	        {
		        HurtCellItemViewModel tempVm = new(item.Key, item.Value/toalHurt);
		        HurtScrollviewList.Add(tempVm);
	        }
        }
        public HurtViewModel(Dictionary<int,long> hurtDic)
        {
	        var toalHurt = 0f;
	        Dictionary<int, long> equipHurtDic = new();
	        foreach (var item in hurtDic)
	        {
		        toalHurt += item.Value;
		        var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(item.Key);
		        if (!equipHurtDic.TryGetValue(cfg.Equip, out long value))
		        {
			        value = 0;
			        equipHurtDic.Add(cfg.Equip, value);
		        }
		        value += (long)item.Value;
		        equipHurtDic[cfg.Equip] = value;
	        }

	        foreach (var item in equipHurtDic)
	        {
		        HurtCellItemViewModel tempVm = new(item.Key, item.Value/toalHurt);
		        HurtScrollviewList.Add(tempVm);
	        }
        }


        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<HurtView>();
        }
    }
}