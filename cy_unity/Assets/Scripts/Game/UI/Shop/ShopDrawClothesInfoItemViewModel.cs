using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class ShopDrawClothesInfoItemViewModel : ViewModelBase
    {
	    [AutoNotify] private bool isShowTitle;
	    [AutoNotify] private Vector2 itemSize;
	    [AutoNotify] private Vector2 bgSize;
	    [AutoNotify] private Vector2 showBgSize;
		[AutoNotify] private string titleNameStr;
		[AutoNotify] private string titleValueStr;
		public Func<object, object> GetGridCellCallback => GetGridCellCallbackByIndex;
		[AutoNotify] private ObservableDictionary<int,CellItemBaseViewModel> gridDictionary = new();
		[AutoNotify] private int showItemNum = 0;
        [Preserve]
        public ShopDrawClothesInfoItemViewModel(List<Reward> clothesList,string titleName=null,string valueDes =null,int showNum = 0,int quaId = 0)
        {
	        TitleNameStr = titleName ?? string.Empty;
	        TitleValueStr = valueDes ?? string.Empty;
	        
	        showBgSize  = TitleNameStr== string.Empty ? new Vector2(910, 192) : new Vector2(910, 320);
	        itemSize  = TitleNameStr== string.Empty ? new Vector2(910, 192) : new Vector2(910, 281);
	        BgSize = TitleNameStr == string.Empty
		        ? new Vector2(910, 281)
		        : new Vector2(910, 129 + 190 * (showNum / 5 + (showNum % 5 > 0 ? 1 : 0)));

	        DHLog.Debug($"ShowNum:{showNum}  line:{(showNum / 5 + (showNum % 5 > 0 ? 1 : 0))}");
	        for (int i = 0; i < clothesList.Count; i++)
	        {
		        var data = UIHelper.RewardToResource(clothesList[i]);
		        data.HeroEquip = new HeroEquip()
		        {
					Uid = 0,
					Id = clothesList[i].Id,
					Lv = 1,
					QuaId = quaId,
		        };
		        var model = CellItemBaseViewModel.Create(data, ECellItemSizeType.Size150X134);
		        model.IsOpenMask = true;
		        gridDictionary.Add(i,model);
	        }

	        // ShowItemNum = clothesList.Count;
	        // for (int i = ShowItemNum; i < 5; i++)
	        // {
		       //  gridDictionary.Add(CellItemBaseViewModel.Create(UIHelper.GetDiamond(1)));
	        // }
        }
        
        private object GetGridCellCallbackByIndex(object index)
        {
	        if (gridDictionary.TryGetValue((int)index, out CellItemBaseViewModel ret))
	        {
		        return ret;
	        }
	        return null;
        }

        
    }
}