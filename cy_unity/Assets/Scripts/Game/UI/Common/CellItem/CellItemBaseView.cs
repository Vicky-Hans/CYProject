using System;
using DH.Config;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class CellItemBaseView : BaseItemView,IViewKey
    {
        public override bool FullScreen => false;
        public RectTransform itemRf;
		public DhImage bg;
		public DhImage icon;
		public GameObject tips;
		public GameObject fragement;
		public DhText count;
		public DhText countBottom;
		public DhButton btnTips;
		public GameObject lockIcon;
		public Transform dynamicHeadParent;
		[NonSerialized] private Vector2 startSize;

		public DhImage leftTopIcon;
		public DhImage clothesPartIcon;
		public GameObject clothesQuaNode;
		public DhImage clothesQuaBg;
		public DhText clothesQuaText;
		public DhText level;
		public GameObject redPartNode;
		public GameObject redNode;

		public RectTransform[] scaleNode;

		public GameObject showEffect;
		public GameObject showEffectMask;
		public GameObject showEffectBase;
		public int index;
		public object Key => index;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<CellItemBaseView, CellItemBaseViewModel>();
            bindingSet.Bind(count).For(v => v.text).ToExpression(vm => GetCountText(vm.CntDesc,vm.ShowLimit,vm.OwnCnt,vm.BaseData,vm.IsShowOwnNum,vm.ShowNumChange));
            bindingSet.Bind(countBottom).For(v => v.text).ToExpression(vm => GetCountText(vm.CntDesc,vm.ShowLimit,vm.OwnCnt,vm.BaseData,vm.IsShowOwnNum,vm.ShowNumChange));
            bindingSet.Bind(itemRf).For(v => v.sizeDelta).ToExpression(vm => GetBgSize(vm.SizeBg));
            bindingSet.Bind(icon.GetComponent<RectTransform>()).For(v => v.sizeDelta).To(vm => vm.SizeIcon);
            bindingSet.Bind(bg).For(v => v.sprite).ToExpression(vm => GetBgPath(vm.BgPath,vm.BaseData)).WithConversion(this);
            bindingSet.Bind(icon).For(v => v.sprite).ToExpression(vm => GetIcon(vm.BaseData)).WithConversion(this);
            bindingSet.Bind(btnTips).For(v => v.onClick).To(vm => vm.OnClickBtnTipsCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(icon.transform.position, new Vector3(0, 20, 0)));
            bindingSet.Bind(lockIcon).For(v => v.activeSelf).ToExpression(vm => vm.IsShowLock && vm.IsLock);
            bindingSet.Bind(lockIcon.GetComponent<RectTransform>()).For(v => v.sizeDelta).To(vm => vm.LockSize);
            bindingSet.Bind(tips).For(v => v.activeSelf).ToExpression(vm =>!(vm.IsShowLock && vm.IsLock) && vm.IsTips);
            bindingSet.Bind(fragement).For(v => v.activeSelf).ToExpression(vm =>!(vm.IsShowLock && vm.IsLock) && !vm.IsTips && vm.IsFragment);
            bindingSet.Bind(count.gameObject).For(v => v.activeSelf).ToExpression(vm =>vm.IsShowNum && vm.FongType == ECellItemFontType.MySelf);
            bindingSet.Bind(countBottom.gameObject).For(v => v.activeSelf).ToExpression(vm =>vm.IsShowNum && vm.FongType == ECellItemFontType.Bottom);
            bindingSet.Bind(icon).For(v => v.Grag).ToExpression(vm => vm.IsShowLock && vm.IsLock);
            bindingSet.Bind(bg).For(v => v.Grag).ToExpression(vm => vm.IsShowLock && vm.IsLock);
            bindingSet.Bind(tips.transform).For(v => v.localScale).ToExpression(vm =>EffectScale(vm.SizeBg));
            bindingSet.Bind(fragement.transform).For(v => v.localScale).ToExpression(vm =>EffectScale(vm.SizeBg));
            bindingSet.Bind(lockIcon.transform).For(v => v.localScale).ToExpression(vm =>EffectScale(vm.SizeBg));
            bindingSet.Bind(this).For(v=>v.dynamicHeadParent).To(vm=>vm.DynamicHeadParent).OneWayToSource();
            bindingSet.Bind(dynamicHeadParent).For(v=>v.localScale).ToExpression(vm =>DynamicHeadScale(vm.SizeBg));
            
            bindingSet.Bind(leftTopIcon.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.ShowRare);
            bindingSet.Bind(leftTopIcon).For(v=>v.sprite).To(vm=>vm.ClothesQuaPath).WithConversion(this);
            
            bindingSet.Bind(clothesPartIcon.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.ShowPart);
            bindingSet.Bind(clothesPartIcon).For(v=>v.sprite).To(vm=>vm.ClothesPartPath).WithConversion(this);
            
            bindingSet.Bind(clothesQuaNode).For(v=>v.activeSelf).ToExpression(vm=>vm.BaseData.Type == (int)RewardType.HeroEquip && vm.ClothesQua!=0);
            bindingSet.Bind(clothesQuaText).For(v => v.text).To(vm => vm.ClothesQua);
            bindingSet.Bind(clothesQuaBg).For(v => v.sprite).To(vm => vm.ClothesPath).WithConversion(this);
            bindingSet.Bind(level.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.BaseData.Type == (int)RewardType.HeroEquip && vm.ClothesLevel!=0);
            bindingSet.Bind(level).For(v => v.text).ToExpression(vm => GetLvDesc(vm.ClothesLevel));
            bindingSet.Bind(redPartNode).For(v => v.activeSelf).ToExpression(vm => vm.IsRedDot && vm.RedType == ECellItemRedType.ClothesUe);
            bindingSet.Bind(redNode).For(v => v.activeSelf).ToExpression(vm => vm.IsRedDot &&  vm.RedType == ECellItemRedType.Base);
            bindingSet.Bind(showEffect).For(v => v.activeSelf).To(vm => vm.ShowRare);
            bindingSet.Bind(showEffect.transform).For(v => v.localScale).To(vm => vm.ItemScale);
            bindingSet.Bind(showEffectMask).For(v => v.activeSelf).To(vm => vm.IsOpenMask);
            bindingSet.Bind(showEffectBase).For(v => v.activeSelf).ToExpression(vm => !vm.IsOpenMask);
            for (int i = 0; i < scaleNode.Length; i++)
            {
	            bindingSet.Bind(scaleNode[i]).For(v => v.localScale).To(vm => vm.ItemScale);
            }
            bindingSet.Build();
        }

        private string GetIcon(ResourceData data)
        {
	        var str = UIHelper.GetRewardsIconPath(data);
	        // DHLog.Debug($"GetIcon:{this}  {data.Type}   {data.Id} IconPath:  {str}");
	        return str;
        }

        public string GetLvDesc(int lv)
        {
	        return $"Lv.{lv}";
        }

        private Vector2 GetBgSize(Vector2 vmSizeBg)
        {
	        return vmSizeBg;
        }
        private Vector3 EffectScale(Vector2 vmSizeBg)
        {
	        float temp = vmSizeBg.x / 166f;
	        return temp *Vector3.one;
        }
        private Vector3 DynamicHeadScale(Vector2 vmSizeBg)
        {
	        float temp = vmSizeBg.x / 120;
	        return temp *Vector3.one;
        }
        private string GetBgPath(string path,ResourceData data)
        {
	        if (string.IsNullOrEmpty(path))
	        {
		        return UIHelper.GetRewardBgPath(data);
	        }
	        return path;
        }

        private string GetCountText(string desc,bool vmShowLimit,long vmOwnCnt, ResourceData vmItemData,bool isShowOwnNum,int showNumChange)
        {
	        if (vmItemData.Type == (int)RewardType.Skill)
	        {
		        var cfg = ConfigCenter.EquipSkillCfgColl.GetDataById(vmItemData.Id);
		        // return  LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_08,$"Lv.{cfg.LvUnlockId}");
		        return $"Lv.{cfg.LvUnlockId}";
	        }
	        
	        desc ??= "{0}";
	        if (vmShowLimit)
	        {
		        return string.Format(desc, UIHelper.GetIsEnoughDesc(vmOwnCnt+showNumChange, vmItemData.Count));
	        }
	        else
	        {
		        if (isShowOwnNum)
		        {
			        return string.Format(desc,vmOwnCnt+showNumChange);
		        }
		        else
		        {
			        return string.Format(desc,vmItemData.Count+showNumChange);
		        }

	        }   
	        
        }

    }
}