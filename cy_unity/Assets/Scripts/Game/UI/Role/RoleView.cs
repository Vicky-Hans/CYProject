using System.Collections.Generic;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class RoleView : BaseItemView
    {
        public UICircularScrollView scrollView;
        [AssetPath]public string scrollViewCell;
        
        
        #region 顶部英雄相关

        public List<Texture2D> QltTextures;
        
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

        public GameObject levelUpButRed;
        public GameObject awakeUpButRed;
        public GameObject gettingButRed;
        
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
        
        public DhButton awakeBut;
        public DhButton levelUpBut;
        public DhButton gettingBut;
        
        #endregion
        
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<RoleView, RoleViewModel>();
            
            #region 顶部英雄相关
            
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
            bindingSet.Bind(roleSpineGo).For(v => v.activeSelf).To(vm => vm.IsShowChapterEffectNode);
            
            bindingSet.Bind(awakeBut).For(v => v.onClick).To(vm => vm.OnClickAwakeButCommand);
            bindingSet.Bind(levelUpBut).For(v => v.onClick).To(vm => vm.OnClickLevelUpButCommand);
            bindingSet.Bind(gettingBut).For(v => v.onClick).To(vm => vm.OnClickGettingButCommand);
            bindingSet.Bind(awakeBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowAwakeBut);
            bindingSet.Bind(levelUpBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowLevelUpBut);
            bindingSet.Bind(gettingBut.gameObject).For(v => v.activeSelf).To(vm => vm.ShowGettingBut);
            
            bindingSet.Bind(this).For(v => v.roleSpineGo).To(vm => vm.EffectParentNode).OneWayToSource();
            
            #endregion

            #region 红点

            bindingSet.Bind(levelUpButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.LevelUpRed);
            bindingSet.Bind(awakeUpButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.AwakeUpRed);
            bindingSet.Bind(gettingButRed).For(v=>v.activeSelf).ToExpression(vm=>vm.GettingRed);

            #endregion
            
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
            bindingSet.Build();
        }

    }
}