using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using UnityEngine.UI;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class RoleInfoView : BaseView
    {
        public override bool FullScreen => false;
        
        #region 顶部英雄相关

        public List<Texture2D> QltTextures;
        
        public CommonTopView topView;
        
        public RawImage qualityBg;
        public DhText heroName;
        public DhText attrHPDes;
        public DhText attrDEFDes;
        public DhText skillName;
        public DhImage skillIcon;
        public DhText skillDes;
        public DhText heroLevel;
	    public GameObject roleSpineGo;
	    public GameObject levelUpEffect;
	    public GameObject levelMainEffect;
	    public GameObject starMainEffect;
        public GameObject starPr;
        
        public GameObject showUpArrow;
        public GameObject showUpArrow2;

        private int qlt;
        public int Qlt
        {
	        get => qlt;
	        set
	        {
		        qlt = value;
		        if (qlt-3<0 || qlt-3 > QltTextures.Count-1)
		        {
			        qualityBg.texture = QltTextures[0]; 
			        return;
		        }
		        qualityBg.texture = QltTextures[qlt-3];
	        }
        }

        private int star;
        public int Star
        {
	        get => star;
	        set
	        {
		        star = value;
		        var childs = starPr.transform.childCount;
		        for (int i = 0; i < childs; i++)
		        {
			        starPr.transform.GetChild(i).gameObject.SetActive(star-1>=i);
		        }

	        }
        }
        
        #endregion

        #region 页签切换

        public GameObject selectAwakeTab;
        public GameObject selectLevelTab;
        public GameObject notSelectLevelTab;
        public GameObject notSelectAwakeTab;
        
        public DhButton levelBut;
        public DhButton awakeBut;

        #endregion
        
        #region 中部 觉醒 升级 消耗相关
        
        public UICircularScrollView levelEffectDes;
        [AssetPath]public string levelEffectDesCell;
        public ScrollRectExtend awakeUpEffectDes;
        [AssetPath]public string awakeUpEffectDesCell;
        public DhImage shardBg;
        public Slider shardSlider;
        public DhText shardNums;
        
        public GameObject cosGo;
        
        public DhText starMaxLevelDes;
        public GameObject maxLvDesGo;
        public GameObject maxStarDesGo;
        
        public ScrollRectExtend cosScrollview;
        [AssetPath]public string cosScrollviewCell;

        public GameObject shardGo;
        public DhText awakeUpButText;
        public DhText gettingButText;
        #endregion
        
		#region 底部按钮相关
		public DhButton restBut;
		public DhButton awardUpBut;
		public DhButton gettingBut;
		public DhButton levelUp;
		public DhButton fastLevelUp;
		public DhButton close; 
		public DhButton diaBuyBut;
		public ItemPriceNodeView diaBuyPriceNode;
	    public DhButton moneyBuyBut;
	    public BtnPriceNode moneyBuyPriceNod;
	    
	    public GameObject levelUpButRed;
	    public GameObject fastLevelUpButRed;
	    public GameObject awakeUpButRed;
	    public GameObject gettingButRed;
		#endregion
		
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			levelEffectDes.PrefabPath = levelEffectDesCell;
			awakeUpEffectDes.PrefabPath = awakeUpEffectDesCell;
			cosScrollview.PrefabPath = cosScrollviewCell;
			
            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleInfoView, RoleInfoViewModel>();

            #region 顶部英雄相关
            bindingSet.Bind(topView.BindingContext).For(v => v.DataContext).To(vm => vm.TopVm);
            bindingSet.Bind(this).For(v => v.Qlt).ToExpression(vm => vm.Quality);
            bindingSet.Bind(heroName).For(v => v.text).To(vm => vm.HeroNameStr);
            bindingSet.Bind(attrHPDes).For(v => v.text).To(vm => vm.AttrHPDesStr);
            bindingSet.Bind(attrDEFDes).For(v => v.text).To(vm => vm.AttrDEFDesStr);
            bindingSet.Bind(skillDes).For(v => v.text).To(vm => vm.SkillDesStr);
            bindingSet.Bind(heroLevel).For(v => v.text).To(vm => vm.HeroLevelStr);
            bindingSet.Bind(skillName).For(v => v.text).To(vm => vm.SkillNameStr);
            bindingSet.Bind(skillIcon).For(v => v.sprite).To(vm => vm.SkillIconPath).WithConversion(this);
            bindingSet.Bind(this).For(v => v.Star).ToExpression(vm => vm.Star);
            bindingSet.Bind(levelUpEffect).For(v => v.activeSelf).ToExpression(vm => vm.LvUpEffectGo);
            bindingSet.Bind(levelMainEffect).For(v => v.activeSelf).ToExpression(vm => vm.LvUpMainEffectGo);
            bindingSet.Bind(starMainEffect).For(v => v.activeSelf).ToExpression(vm => vm.StarUpMainEffectGo);
            bindingSet.Bind(showUpArrow).For(v => v.activeSelf).To(vm => vm.ShowUpArrow);
            bindingSet.Bind(showUpArrow2).For(v => v.activeSelf).To(vm => vm.ShowUpArrow);
            #endregion

            #region 中部升级觉醒展示相关
            bindingSet.Bind(levelEffectDes).For(v => v.Collection).To(vm => vm.LevelEffectDesList);
            bindingSet.Bind(levelEffectDes).For(v => v.DefaultJumpIndex).To(vm => vm.JumpSkillScrollPos);
            bindingSet.Bind(awakeUpEffectDes).For(v => v.Collection).To(vm => vm.AwakeUpEffectDesList);
            bindingSet.Bind(shardBg).For(v => v.sprite).To(vm => vm.ShardBgPath).WithConversion(this);
            bindingSet.Bind(roleSpineGo).For(v => v.activeSelf).To(vm => vm.IsShowChapterEffectNode);
            bindingSet.Bind(this).For(v => v.roleSpineGo).To(vm => vm.EffectParentNode).OneWayToSource();
            bindingSet.Bind(shardSlider).For(v => v.value).To(vm => vm.ShardSliderValue);
            bindingSet.Bind(shardNums).For(v => v.text).To(vm => vm.ShardNumsStr);
            bindingSet.Bind(cosScrollview).For(v => v.Collection).To(vm => vm.CosScrollviewList);
            // bindingSet.Bind(awakeUpButText).For(v => v.color).To(vm => vm.AwakeUpButTextColor);
            // bindingSet.Bind(gettingButText).For(v => v.color).To(vm => vm.GettingButTextColor);
            
            bindingSet.Bind(levelEffectDes.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowType == RoleShowType.Level);
            bindingSet.Bind(awakeUpEffectDes.transform.parent.parent.parent.parent.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.ShowType == RoleShowType.Awakening);
            
            bindingSet.Bind(cosGo).For(v => v.activeSelf).ToExpression(vm => vm.CosGo);
            bindingSet.Bind(shardGo).For(v => v.activeSelf).ToExpression(vm => vm.ShardGo);
            
            bindingSet.Bind(starMaxLevelDes).For(v => v.text).To(vm => vm.LevelEffectDes);
            bindingSet.Bind(starMaxLevelDes.gameObject).For(v => v.activeSelf).To(vm => vm.LevelEffectDesGo);
            bindingSet.Bind(maxLvDesGo).For(v => v.activeSelf).To(vm => vm.MaxLvDesGo);
            bindingSet.Bind(maxStarDesGo).For(v => v.activeSelf).To(vm => vm.MaxStarDesGo);
            bindingSet.Bind(this).For(v=>v.ScrollPos).ToExpression(vm=>vm.JumpScrollPos);
            bindingSet.Bind(this).For(v=>v.skillScrollPos).ToExpression(vm=>vm.JumpSkillScrollPos);
            #endregion
            
			#region  底部按钮相关
			
			bindingSet.Bind(restBut).For(v => v.onClick).To(vm => vm.OnClickRestButCommand);
			bindingSet.Bind(restBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowRestBut);
			bindingSet.Bind(awardUpBut).For(v => v.onClick).To(vm => vm.OnClickAwakeUpButCommand);
			bindingSet.Bind(awardUpBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowAwakeUpBut);
			bindingSet.Bind(gettingBut).For(v => v.onClick).To(vm => vm.OnClickGettingButCommand);
			bindingSet.Bind(gettingBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowGettingBut);
			bindingSet.Bind(levelUp).For(v => v.onClick).To(vm => vm.OnClickLevelUpButCommand);
			bindingSet.Bind(levelUp.gameObject).For(v => v.activeSelf).To(vm => vm.ShowLevelUpBut);
			bindingSet.Bind(fastLevelUp).For(v => v.onClick).To(vm => vm.OnClickFastLevelUpButCommand);
			bindingSet.Bind(fastLevelUp.gameObject).For(v => v.activeSelf).To(vm => vm.ShowFastLevelUpBut);
			bindingSet.Bind(close).For(v => v.onClick).To(vm => vm.OnClickCloseCommand);

			bindingSet.Bind(diaBuyBut).For(v => v.onClick).To(vm => vm.OnClickDiaBuyButCommand);
			bindingSet.Bind(diaBuyBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowDiaBuyBut);
			bindingSet.Bind(moneyBuyBut).For(v => v.onClick).To(vm => vm.OnClickMoneyBuyButCommand);
			bindingSet.Bind(moneyBuyBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowMoneyBuyBut);
			bindingSet.Bind(diaBuyPriceNode.BindingContext).For(v => v.DataContext).To(vm => vm.DiaBuyPriceNodeModel);
			bindingSet.Bind(moneyBuyPriceNod.BindingContext).For(v => v.DataContext).To(vm => vm.MoneyBuyPriceNodeModel);
			
			//页签切换
			bindingSet.Bind(levelBut).For(v => v.onClick).To(vm => vm.OnClickTabLevelButCommand);
			bindingSet.Bind(awakeBut).For(v => v.onClick).To(vm => vm.OnClickTabAwakeButCommand);
			
			bindingSet.Bind(selectAwakeTab).For(v => v.activeSelf).ToExpression(vm =>vm.ShowType == RoleShowType.Awakening);
			bindingSet.Bind(notSelectLevelTab).For(v => v.activeSelf).ToExpression(vm =>vm.ShowType == RoleShowType.Awakening);
			bindingSet.Bind(selectLevelTab).For(v => v.activeSelf).ToExpression(vm => vm.ShowType == RoleShowType.Level);
			bindingSet.Bind(notSelectAwakeTab).For(v => v.activeSelf).ToExpression(vm => vm.ShowType == RoleShowType.Level);
			
			#endregion

			#region 红点相关
			bindingSet.Bind(levelUpButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.LevelUpRed);
	        bindingSet.Bind(fastLevelUpButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.LevelUpRed);
	        bindingSet.Bind(awakeUpButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.AwakeUpRed);
	        bindingSet.Bind(gettingButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.GettingRed);

			#endregion
			
            bindingSet.Build();
        }

        #region 等级技能跳转
        public int skillScrollPos
        {
            get => 0;
            set => DelaySkillScrollToPos(value).Forget();
        }
        private RectTransform skillContentTr;
        private  async UniTaskVoid DelaySkillScrollToPos(int pos)
        {
	        if(levelEffectDes ==null) return;
				levelEffectDes.Jump2SpecificItem(pos);
        }
        

        #endregion


        #region 觉醒技能跳转

        public int ScrollPos
        {
	        get => 0;
	        set => DelayScrollToPos(value).Forget();
        }

        private int isFirst;
        private RectTransform contentTr;
        private async UniTaskVoid DelayScrollToPos(int pos)
        {
	        //if (pos==0) return;
	        if (isFirst <2)
		        await UniTask.Delay(150);
	        isFirst++;
	        if(awakeUpEffectDes ==null) return;
	        if (contentTr == null)
		        contentTr = awakeUpEffectDes.transform.parent.GetComponent<RectTransform>();
	        var nums = 0f;
	        bool isend = false;
	        if (awakeUpEffectDes==null ||awakeUpEffectDes.Collection ==null)return;
	        

	        if (pos > awakeUpEffectDes.transform.childCount-1)
		        return;
	        
	        var rectTransform = awakeUpEffectDes.transform.parent.parent.GetComponent<RectTransform>();
	        // 获取物体在屏幕空间中的高度
	        float screenHeight = Screen.height;
	        float objectHeight = rectTransform.rect.height * screenHeight / Screen.height;
	        var temp = 0f;
	        var tempIndex = 0;
	        var temp2 = 0f;
	        for (int i =  awakeUpEffectDes.transform.childCount - 1; i >= 0; i--)
	        {
		        temp += awakeUpEffectDes.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
		        if (temp>=objectHeight)
		        {
			        tempIndex = i;
			        temp2 = temp - objectHeight;
			        break;
		        }
	        }

	        if (pos> tempIndex)
	        {
		        pos = tempIndex;
		        isend = true;
	        }
	        for (int i = 0; i < pos; i++)
	        {
		        nums += awakeUpEffectDes.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
	        }
	        var RtPos =contentTr.localPosition;
	        contentTr.localPosition = new Vector3(RtPos.x,nums+ (isend?  temp2-20:0),RtPos.z);
        }

        #endregion

    }
}