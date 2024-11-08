using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Spine.Unity;
using UnityEngine;


namespace DH.Game.ViewModels
{
	public enum ECellItemSizeType
	{
		Size200X180,
		Size180X150,
		Size150X150,
		Size216X150,
		Size166X150,
		Size150X134,
		Size150X135,
		Size120X100,
		Size132X132,
		Size117X76,
		Size100X90,
		Size90X80,
	}
	public enum ECellItemFontType
	{
		MySelf,
		Bottom,
	}

	public enum ECellItemRedType
	{
		Base,
		ClothesUe,
	}

	public partial class CellItemBaseViewModel : ViewModelBase
    {
	    public Action<Tuple<Vector3, Vector3>> OnClickEvent;
	    [AutoNotify] private long ownCnt=-1;
	    [AutoNotify] private bool showLimit;
        
	    private ResourceData data;

	    [AutoNotify] private ResourceData baseData;
	    [AutoNotify] private Vector2 sizeIcon;
	    [AutoNotify] private Vector2 sizeBg;
	    //[AutoNotify] private Color cntColor;
	    [AutoNotify] private string cntDesc;
	    
		[AutoNotify] private string bgPath;
		[AutoNotify] private string iconPath;

		[AutoNotify] private bool isTips; 
		[AutoNotify] private bool isFragment;
		[AutoNotify] private bool isShowNum;
		[AutoNotify] private ECellItemFontType fongType;
		[AutoNotify] private bool isLock;
		[AutoNotify] private Vector2 lockSize;
		[AutoNotify] private Vector2 countSize;
		private Vector2 lockDefaultSize = new(66, 72);
		private bool isShowLock;
		private Transform dynamicHeadParent;


		[AutoNotify] private string clothesPartPath;
		[AutoNotify] private string clothesQuaPath;
		[AutoNotify] private int clothesQua;
		[AutoNotify] private int clothesLevel;
		[AutoNotify] private bool isShowOwnNum=false;
		[AutoNotify] private int showNumChange = 0;

		[AutoNotify] private ECellItemRedType redType = ECellItemRedType.Base;
		[AutoNotify] private bool isRedDot;
		[AutoNotify] private bool showPart;
		[AutoNotify] private bool showRare;
		[AutoNotify] private string clothesPath;

		[AutoNotify] private Vector3 itemScale;

		[AutoNotify] private bool isHighState;
		[AutoNotify] private bool isOpenMask;
		public Action DataChanged;
		public Transform DynamicHeadParent
		{
			get => null;
			set
			{
				dynamicHeadParent = value;
				GetDynamicParentActive();
			}
		}

		public bool IsShowLock
		{
			get => isShowLock;
			set
			{
				Set(ref isShowLock, value);
				RefreshLock();
			}
		}

		public ResourceData Data
		{
			get => data;
			set
			{
				var old = data;
				Set(ref data, value);
				if (old != null)
				{
					old.PropertyChanged -= ItemChange;
				}
                
				if (data != null)
				{
					data.PropertyChanged += ItemChange;
				}
			}
		}

		private HeroEquipData heroEquipData;
		public HeroEquipData HeroEquipData
		{
			get => heroEquipData;
			set
			{
				var old = heroEquipData;
				Set(ref heroEquipData, value);
				if (old != null)
				{
					old.PropertyChanged -= HeroEquipDataChange;
				}
                
				if (heroEquipData != null)
				{
					heroEquipData.PropertyChanged += HeroEquipDataChange;
				}
			}
		}
		public static CellItemBaseViewModel Create(long uid,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
		{
			var heroEquipData = DataCenter.clothesData.GetHeroEquipDataByUid(uid);
			return  heroEquipData==null || heroEquipData.IsNull() ? null : new  CellItemBaseViewModel(heroEquipData.Id,(int)RewardType.HeroEquip,1,sizeType,showLimit,isShowNum,UIHelper.HeroEquipDataToHeroEquip(heroEquipData));
		}
		public static CellItemBaseViewModel Create(HeroEquipData heroEquipData,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
		{
			if (heroEquipData==null || heroEquipData.IsNull()) return null;
			return new  CellItemBaseViewModel(heroEquipData.Id,(int)RewardType.HeroEquip,1,sizeType,showLimit,isShowNum,UIHelper.HeroEquipDataToHeroEquip(heroEquipData));
		}
		
		public static CellItemBaseViewModel Create(Resource reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
		{
			return new  CellItemBaseViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,reward.HeroEquip);
		}
		public static CellItemBaseViewModel Create(ResourceData reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
		{
			return new  CellItemBaseViewModel(reward.Id,reward.Type,reward.Count,sizeType,showLimit,isShowNum,UIHelper.HeroEquipDataToHeroEquip(reward.HeroEquip));
		}

		public static CellItemBaseViewModel Create(Reward reward,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true)
		{
			return new  CellItemBaseViewModel(reward.Id,(int)reward.Type,reward.Count,sizeType,showLimit,isShowNum);
		}


		/// <summary>
		/// 基础Item显示
		/// </summary>
		/// <param name="id">唯一id</param>
		/// <param name="type">类型</param>
		/// <param name="count">数量</param>
		/// <param name="sizeType">尺寸</param>
		/// <param name="showLimit">是否显示拥有数量</param>
		/// <param name="isShowNum">是否显示数量</param>
		/// <param name="heroEquip">英雄装备</param>
		[Preserve]
		public CellItemBaseViewModel(int id,int type,long count,ECellItemSizeType sizeType=ECellItemSizeType.Size166X150,bool showLimit=false,bool isShowNum=true,HeroEquip heroEquip=null)
		{
			RefreshInfo(id,type,count,showLimit,isShowNum,heroEquip);
			SetSize(sizeType);
		}

		private void RefreshInfo(int id,int type,long count,bool showLimit=false,bool isShowNum=true,HeroEquip heroEquip=null)
		{
			ShowLimit = showLimit;
			IsShowNum = isShowNum && type!=(int) RewardType.HeroEquip;
			var resource = new Resource()
			{
				Id = id,
				Count = count,
				Type = type,
				HeroEquip = heroEquip
			};
			
			BaseData = new ResourceData(resource);
			
			if (BaseData.Type == (int)RewardType.Item)
			{
				Data = DataCenter.itemsData.GetResourceDataById(BaseData.Id);
				OwnCnt = Data?.Count ?? 0;
			} else
			{
				ShowLimit = false;
			}

			IsShowTips();
			FongType = ECellItemFontType.MySelf;
			RefreshFragmentTips();
			RefreshLock();
			RefreshClothes();
		}



		private void RefreshClothes()
		{
			ShowPart = false;
			ShowRare = false;
			//服饰功能
			if (BaseData!=null && BaseData.Type == (int)RewardType.HeroEquip  && BaseData.HeroEquip!=null)
			{
				HeroEquipData = DataCenter.clothesData.GetHeroEquipDataByUid(BaseData?.HeroEquip?.Uid ?? 0);
				if (HeroEquipData != null)
				{
					BaseData.HeroEquip.QuaId = HeroEquipData.QuaId;
					BaseData.HeroEquip.Lv = heroEquipData.Lv;
				}
				ClothesPartPath = ClothesManager.Instance.GetPartEquipMinIcon(BaseData?.Id ?? 0,BaseData?.HeroEquip?.QuaId ?? 0);
				ClothesQuaPath = ClothesManager.Instance.GetRareIcon(BaseData.Id);
				ClothesPath = ClothesManager.Instance.GetClothesQuaBgSmallPath(ClothesManager.Instance.GetQuaSmallByQuaId(HeroEquipData?.QuaId ?? 0));
				ClothesQua = ClothesManager.Instance.GetQuaSmallPos(ClothesManager.Instance.GetQuaSmallById(BaseData.Id,HeroEquipData ?? BaseData.HeroEquip));
				ClothesLevel = HeroEquipData?.Lv ?? BaseData?.HeroEquip?.Lv ?? 0;
				RaisePropertyChanged(nameof(BgPath));
				RaisePropertyChanged(nameof(ClothesQua));
				RaisePropertyChanged(nameof(ClothesLevel));
				ShowPart = true;
				ShowRare = ClothesManager.Instance.IsRareShowById(BaseData.Id);
			}
			else
			{
				ClothesPartPath = UIHelper.NoneImagePath();
				ClothesQuaPath = UIHelper.NoneImagePath();
				ClothesQua = 0;
				ClothesLevel = 0;
				HeroEquipData = null;
				RaisePropertyChanged(nameof(BaseData));
			}

			if (BaseData.Type == (int)RewardType.Item)
			{
				var cfg = ConfigCenter.ItemCfgColl.GetDataById(BaseData.Id);
				if (cfg != null && cfg.Type is (int)GameConst.ItemType.HeroEquipMerge or (int)GameConst.ItemType.HeroEquipRandom )
				{
					ClothesPartPath = ClothesManager.Instance.GetPartEquipMinIcon(cfg.TypeValue,cfg.Quality);
					ClothesQuaPath = ClothesManager.Instance.GetRareIconByRareId(cfg.TypeValue1);
					ShowPart = true;
					ShowRare = cfg.TypeValue1!=1;
				}
			}
		}

		private void IsShowTips()
		{
			if (BaseData.Type == (int)RewardType.Item)
			{
				var cfg = ConfigCenter.ItemCfgColl.GetDataById(BaseData.Id);
				if (cfg != null)
				{
					if (cfg.Type == 3)
					{
						IsTips = true;
					}
				}
			}
			else
			{
				IsTips = false;
			}
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			Data = null;
			HeroEquipData = null;
		}

		private void RefreshLock()
		{
			IsLock = IsShowLock && UIHelper.GetItemIsLock(baseData);
		}

		private void RefreshFragmentTips()
		{
			IsFragment = false;
			if (BaseData.Type == (int)RewardType.Item)
			{
				var cfg = ConfigCenter.ItemCfgColl.GetDataById(BaseData.Id);
				if (cfg == null) return;
				if (cfg.Type is 2)
				{
					IsFragment = true;
				}
			}
		}

		private void ItemChange(object sender, PropertyChangedEventArgs e)
        {
	        OwnCnt = DataCenter.itemsData.GetItemCountById(BaseData.Id);
	        DataChanged?.Invoke();
        }
		
		 
		private void HeroEquipDataChange(object sender, PropertyChangedEventArgs e)
		{
			RefreshClothes();
			DataChanged?.Invoke();
		}

        public void SetSize(ECellItemSizeType sizeType=ECellItemSizeType.Size166X150)
        {
	        switch (sizeType)
	        {
		        case ECellItemSizeType.Size200X180:
		        {
			        SizeIcon = new Vector2(180, 180);
			        SizeBg = new Vector2(200, 200);
			        LockSize = lockDefaultSize;
			        ItemScale = Vector3.one * 200 / 166;
			        break;
		        }
		        case ECellItemSizeType.Size216X150:
		        {
			        SizeIcon = new Vector2(150, 150);
			        SizeBg = new Vector2(216, 216);
			        LockSize = lockDefaultSize * 216 / 166;
			        ItemScale = Vector3.one *  216/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size180X150:
		        {
			        SizeIcon = new Vector2(150, 150);
			        SizeBg = new Vector2(180, 180);
			        LockSize = lockDefaultSize;
			        ItemScale = Vector3.one *  180/ 166;
			        break;
		        } 
		        case ECellItemSizeType.Size166X150:
		        {
			        SizeIcon = new Vector2(150, 150);
			        SizeBg = new Vector2(166, 166);
			        LockSize = lockDefaultSize;
			        ItemScale = Vector3.one;
			        break;
		        }   
		        case ECellItemSizeType.Size150X134:
		        {
			        SizeIcon = new Vector2(134, 134);
			        SizeBg = new Vector2(150, 150);
			        LockSize = lockDefaultSize;
			        ItemScale = Vector3.one *  150/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size150X135:
		        {
			        SizeIcon = new Vector2(135, 135);
			        SizeBg = new Vector2(150, 150);
			        LockSize = lockDefaultSize;
			        break;
		        }   
		        case ECellItemSizeType.Size132X132:
		        {
			        SizeIcon = new Vector2(100, 100);
			        SizeBg = new Vector2(132, 132);
			        LockSize = lockDefaultSize * 132 / 166;
			        ItemScale = Vector3.one *  132/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size100X90:
		        {
			        SizeIcon = new Vector2(90, 90);
			        SizeBg = new Vector2(100, 100);
			        LockSize = lockDefaultSize * 100 / 166;
			        ItemScale = Vector3.one *  100/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size90X80:
		        {
			        SizeIcon = new Vector2(80, 80);
			        SizeBg = new Vector2(90, 90);
			        LockSize = lockDefaultSize * 90 / 166;
			        ItemScale = Vector3.one *  90/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size120X100:
		        {
			        SizeIcon = new Vector2(100, 100);
			        SizeBg = new Vector2(120, 120);
			        LockSize = lockDefaultSize * 120 / 166;
			        ItemScale = Vector3.one *  120/ 166;
			        break;
		        }
		        case ECellItemSizeType.Size117X76:
		        {
			        SizeIcon = new Vector2(76, 76);
			        SizeBg = new Vector2(117, 117);
			        LockSize = lockDefaultSize * 120 / 166;
			        ItemScale = Vector3.one *  117/ 166;
			        break;
		        }
		        default:
		        {
			        SizeIcon = new Vector2(150, 150);
			        SizeBg = new Vector2(166, 166);
			        LockSize = lockDefaultSize;
			        ItemScale = Vector3.one;
			        break;
		        } 
	        }
	        
        }
        
        public bool Merge(Reward reward)
        {
	        if (reward == null) return false;
	        RefreshInfo(reward.Id,(int)reward.Type,reward.Count,ShowLimit,IsShowNum);
	        return true;
        }

        [Command]
        private void OnClickBtnTips(Tuple<Vector3, Vector3> info)
        {
	        if (OnClickEvent != null)
	        {
		        OnClickEvent?.Invoke(info);
		        return;
	        }
	        UIHelper.OpenItemTips(BaseData, info);
        }
        
        private void GetDynamicParentActive()
        {
	        if ( BaseData == null) return;
	        if (dynamicHeadParent == null) return;
            
	        for (int i = 0; i < dynamicHeadParent.childCount; ++i)
	        {
		        var child = dynamicHeadParent.GetChild(i);
		        AssetsManager.ReleaseInstance(child.gameObject);
	        }
	        if(BaseData.Type !=(int) RewardType.Head) return;
	        var headCfg = ConfigCenter.ProPictureCfgColl.GetDataById(BaseData.Id);
	        if (headCfg == null) return;
	        if (headCfg.ShowType == (int) EHeadShowType.HeadShowTypeStatic)
	        {
		        return;
	        }
	        // 这里处理动态显示
	        LoadDynamicHead().Forget();
        }
        private readonly string headPrefabPath = "UI/Common/Head/Head_";
        private float defaultX = 166;
        private async UniTask LoadDynamicHead()
        {
	        var path = $"{headPrefabPath}{BaseData.Id}";
	        var tempX = sizeBg.x;
	        if (tempX != 0)
	        {
		        tempX = defaultX;
	        }

	        var scale = tempX /defaultX;
	        var tempHead =  await AssetsManager.InstantiateWithParentAsync(path,dynamicHeadParent, false);
	        if(dynamicHeadParent ==null || tempHead==null ) return;
	        tempHead.transform.localScale = new Vector3(scale, scale, 1);
	        SkeletonGraphic tempHeadSkele = tempHead.transform.Find("ActionNode").gameObject.GetComponent<SkeletonGraphic>();
	        if(tempHeadSkele == null) return;
	        tempHeadSkele.AnimationState.SetAnimation(0, "animation", true);
        }
    }
}