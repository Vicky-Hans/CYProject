using System;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using UnityEngine;
using Extend;
namespace DH.Game.UIViews
{
    public partial class AdFreeGiftEffectItemView : BaseItemView
    {
        public override bool FullScreen => false;
        
		public DhButton adInfoButton;
		public DhText effectText;
		public DhText adFreeDes;
		public UICircularScrollView awardScrollview;
		[AssetPath]public string awardScrollviewCell;

		public GameObject freeAdGo;
		public GameObject goldNameGo;
		public GameObject GetAward;
		public GameObject effectDes;

		public CommonPlayerNameView cPlayerName;
		private MonthCardEffectType showEffectType;

		public MonthCardEffectType ShowEffectType
		{
			get => showEffectType;
			set
			{
				showEffectType = value;
				ShowEffectDes(showEffectType);
			}
		}

		public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			awardScrollview.PrefabPath = awardScrollviewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<AdFreeGiftEffectItemView, AdFreeGiftEffectItemViewModel>();
            
			bindingSet.Bind(adInfoButton).For(v => v.onClick).To(vm => vm.OnClickIconBtn)
				.CommandParameter(() => new Tuple<Vector3, Vector3>(adInfoButton.transform.position, new Vector3(0,20,0)));
			
			bindingSet.Bind(effectText).For(v => v.text).To(vm => vm.EffectTextStr);
			bindingSet.Bind(awardScrollview).For(v => v.Collection).To(vm => vm.AwardScrollviewList);
			bindingSet.Bind(this).For(v => v.ShowEffectType).ToExpression(vm => vm.EffectType);
			bindingSet.Bind(adFreeDes).For(v => v.text).To(vm => vm.AdFreeDes);
			bindingSet.Bind(cPlayerName.BindingContext).For(v => v.DataContext).ToExpression(vm => vm.PlayerNameVm);
            bindingSet.Build();
        }

        void ShowEffectDes(MonthCardEffectType effectType)
        {
	        freeAdGo.SetActive(false);
	        goldNameGo.SetActive(false);
	        GetAward.SetActive(false);
	        effectDes.SetActive(false);
	        adInfoButton.gameObject.SetActive(false);
	        switch (effectType)
	        {
		        case MonthCardEffectType.AdRreeReward:
			        freeAdGo.SetActive(true);
			        adInfoButton.gameObject.SetActive(true);
			        break;
		        case MonthCardEffectType.GoldenNickname:
			        goldNameGo.SetActive(true);
			        break;
		        case MonthCardEffectType.DedicatedAvatar:
			        GetAward.SetActive(true);
			        break;
		        case MonthCardEffectType.ADFreeForever:
			        freeAdGo.SetActive(true);
			        break;
		        default:
			        effectDes.SetActive(true);
			        break;
	        }
        }
    }
}