using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using Spine.Unity;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class RankPlayerInfoViewModel : ViewModelBase
    {
	    [AutoNotify] private ObservableList<EquipCellViewModel> weaponScrollviewList = new();
	    [AutoNotify] private ObservableList<CellItemBaseViewModel> clothesScrollviewList = new();
		[AutoNotify] private string stageNameStr;
		[AutoNotify] private CommonHeadItemViewModel headVm;
		[AutoNotify] private int playerId;
		[AutoNotify] private string groupDescStr;
		[AutoNotify] private string weaponCountStr;
		[AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
		[AutoNotify] private bool isShowWeapon;
		[AutoNotify] private bool isShowClothes;
		[AutoNotify] private bool isShowEquipNode;
		
		private List<RankPlayerInfoWeaponItemViewModel> rankWeaponList = new();
		private GameObject weaponBgParentNode;
		public GameObject WeaponBgParentNode
		{
			get => null;
			set
			{
				weaponBgParentNode = value;
				if(value == null) return;
				CreateAllBg();
			}
		}
		private GameObject weaponParentNode;
		public GameObject WeaponParentNode
		{
			get => null;
			set
			{
				weaponParentNode = value;
				if (value == null) return;
				CreateAllWeapon();
			}
		}

		private RankMember curInfo;
		private RankDigestInfo curDigestInfo;
		private string cellPrefabPath = "UI/MainStage/RankPlayerInfoWeaponItem";
        [Preserve]
        public RankPlayerInfoViewModel(RankMember info, RankDigestInfo digestInfo)
        {
	        curInfo = info;
	        curDigestInfo = digestInfo;
	        var tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips38);
	        var languageCfg = ConfigCenter.CopyLanguageCfgColl.GetDataById(info.Stage);
	        StageNameStr = $"{tempStr}{curInfo.Stage}.{languageCfg.Name}";
	        CommonHeadData tempData = new(info.Logo, info.HeadFrame);
	        HeadVm = new CommonHeadItemViewModel(tempData, true);
	        GroupDescStr = LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips35);
	        playerId = digestInfo.CurrHero;
	        IsShowWeapon = curDigestInfo.EquipSlots.Count > 0;
	        IsShowEquipNode = curDigestInfo.Equips.Count > 0;
	        IsShowClothes = curDigestInfo.HeroEquips.Count > 0;
	        if (CommonPlayerNameVm ==null)
	        {
		        CommonPlayerNameVm = new CommonPlayerNameViewModel(curInfo.Name,UIHelper.HexColorStrToColor("#6d4f3a"), UIHelper.IsGoldName(curInfo.VipStatus));
	        }
	        else
	        {
		        CommonPlayerNameVm.InitUI(curInfo.Name,UIHelper.HexColorStrToColor("#6d4f3a"),UIHelper.IsGoldName(curInfo.VipStatus));
	        }
	        rankWeaponList.Clear();
	        var tempEquipList = curDigestInfo.Equips.ToList();
	        tempEquipList.Sort((a, b) => a.Key - b.Key);
	        // 武器
	        foreach (var item in tempEquipList)
	        {
		        EquipCellViewModel tempVm = new (item.Key, item.Value);
		        weaponScrollviewList.Add(tempVm);
	        }
	        WeaponCountStr = $"{curDigestInfo.Equips.Count}/{curDigestInfo.Equips.Count}";
	        
	        var tempClothesList = curDigestInfo.HeroEquips.ToList();
	        tempClothesList.Sort((a, b) => a.Key - b.Key);
	        // 服饰
	        foreach (var item in tempClothesList)
	        {
		        Resource tempRes = new()
		        {
			        Id = item.Value.Id,
			        Type = (int)RewardType.HeroEquip,
			        Count = 1,
			        HeroEquip = UIHelper.HeroEquipDigestToHeroEquip(item.Value)
		        };
		        CellItemBaseViewModel tempVm = CellItemBaseViewModel.Create(tempRes,ECellItemSizeType.Size117X76,false,false);
		        tempVm.IsOpenMask = true;
		        ClothesScrollviewList.Add(tempVm);
	        }
        }
        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<RankPlayerInfoView>();
        }

        private void CreateAllBg()
        {
	        if(!IsShowWeapon) return;
	        int w = 7;
	        int h = 5;
	        for (int i = 0; i < w * h; i++)
	        {
		        int x= i % w;
		        int y = i / w;
		        RankPlayerInfoWeaponItemViewModel tempVm = new(-1, new Vector2(x, y));
		        rankWeaponList.Add(tempVm);
		        CreateRankPlayerInfoCellView(weaponBgParentNode, tempVm).Forget();
	        }
        }

        private void CreateAllWeapon()
        {
	        if(!IsShowWeapon) return;
	        foreach (var item in curDigestInfo.EquipSlots)
	        {
		        var itemKey = item.Key;
		        var pos = GameDataManager.Instance.GetGridPosInfoByKey(itemKey);
		        if(pos ==null) continue;
		        RankPlayerInfoWeaponItemViewModel tempVm = new(item.Value, new Vector2(pos.Value.x, pos.Value.y));
		        rankWeaponList.Add(tempVm);
		        CreateRankPlayerInfoCellView(weaponParentNode, tempVm).Forget();
	        }
        }

        private async UniTaskVoid CreateRankPlayerInfoCellView(GameObject parentNode, RankPlayerInfoWeaponItemViewModel contentText)
        {
	        GameObject tempNode = await AssetsManager.InstantiateWithParentAsync(cellPrefabPath, parentNode.transform, false);
	        RankPlayerInfoWeaponItemView view = tempNode.GetComponent<RankPlayerInfoWeaponItemView>();
	        if(parentNode == null) return;
	        view.SetDataContext(contentText);
        }


        #region 初始化英雄立绘
        		[AutoNotify] private bool isShowChapterEffectNode;
        		private readonly string effectPath = "UI/Role/MapEffect/";
        		private SkeletonGraphic curSpine;
        		private GameObject effectParentNode;
        		public GameObject EffectParentNode
        		{
        			get=> null;
        			set
        			{
        				effectParentNode = value;
        				if (effectParentNode != null)
        				{
        					UpdateChapterMapEffect();
        				}
        			}
        		}
        		private async UniTaskVoid UpdateChapterMapEffect()
        		{
			        if (effectParentNode == null)return;
        			for (int i = 0; i < effectParentNode.transform.childCount; i++)
        			{
        				var child = effectParentNode.transform.GetChild(i);
        				AssetsManager.ReleaseInstance(child.gameObject);
        			}
        			curSpine = null;
			        var cfg = ConfigCenter.HeroMainCfgColl.GetDataById(curDigestInfo.CurrHero);
        			var path = $"{effectPath}{cfg.Model}";
        			var effectNode= await AssetsManager.InstantiateWithParentAsync(path, effectParentNode.transform, false);
        			curSpine = effectNode.GetComponent<SkeletonGraphic>();
        			if (curSpine == null) return;
        			IsShowChapterEffectNode = true;
			        UIHelper.InitHeroEquipBox(effectNode.transform,curDigestInfo.WearBox,0.7f);
        			//var boxTr = effectNode.transform.Find("Box");
        			// if (boxTr != null)
        			// {
        			// 	boxTr.transform.localScale = Vector3.one/1.37f;
        			// }
        			//皮皮鬼特殊处理
        			// var isRole3000 = DataCenter.roleData.GetNowHero().Id == 3000;
        			// effectParentNode.transform.localPosition = new Vector3(isRole3000 ? 0 : -70, -309, 0);
        		}
        		#endregion
        
    }
}