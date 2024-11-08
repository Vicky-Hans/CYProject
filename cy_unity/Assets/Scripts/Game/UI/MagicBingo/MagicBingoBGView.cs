using System.Collections.Generic;
using DG.DemiEditor;
using DH.Data;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.Game.UIViews.ItemViews;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class MagicBingoBGView : BaseItemView
    {
        public override bool FullScreen => false;
                
        public CommonTopView commonTopItems;
        public UICircularScrollView scrollView;
        [AssetPath]public string scrollViewCell;

        public DhText times;
        public UICircularScrollView ProgressScrollview;
        [AssetPath]public string ProgressScrollviewCell;

        public DhText bingoPointText;
        public DhText toDayBingoNums;
        public Slider progress;

        public GameObject bingoOverGo;
        public DhButton restButton;
        public DhButton ruleBut;
        public DhButton buyBut;

        public List<Transform> effects = new List<Transform>();

        public GameObject mask;
        
        private int bingoCount;

        public int BingoCount
        {
            get => bingoCount;
            set
            {
                bingoCount = value;
                if (bingoCount==0)
                {
                    for (int j = 0; j < effects.Count; j++)
                    {
                        effects[j].gameObject.SetActive(false);
                    }
                    return;
                }
                var temp = 0;
                for (int j = 0; j < effects.Count; j++)
                {
                    if (j<5)
                    {
                        temp = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            var pos =  (i + 1)*100 +(j+1);
                            if (DataCenter.mgicBingoData.GetGradAward(pos) != null)
                            {
                                temp++;
                            }

                            if (temp>=5)
                            {
                                effects[j].gameObject.SetActive(true);
                            }
                        } 
                    }
                    
                    if (j>4 && j < 10)
                    {
                        temp = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            var pos =  (j-5 + 1)*100+(i+1);
                            if (DataCenter.mgicBingoData.GetGradAward(pos) != null)
                            {
                                temp++;
                            }

                            if (temp>=5)
                            {
                                effects[j].gameObject.SetActive(true);
                            }
                        } 
                    }

                    if (j==10)
                    {
                        temp = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            var pos =  (i+ 1)*100+(i+1);
                            if (DataCenter.mgicBingoData.GetGradAward(pos) != null)
                            {
                                temp++;
                            }

                            if (temp>=5)
                            {
                                effects[j].gameObject.SetActive(true);
                            }
                        } 
                    }
                    if (j==11)
                    {
                        temp = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            var pos =  (i+ 1)*100+(5-i);
                            if (DataCenter.mgicBingoData.GetGradAward(pos) != null)
                            {
                                temp++;
                            }

                            if (temp>=5)
                            {
                                effects[j].gameObject.SetActive(true);
                            }
                        } 
                    }
                }

            }
        }

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            scrollView.PrefabPath = scrollViewCell;
            await base.Create();
            var bindingSet = this.CreateBindingSet<MagicBingoBGView, MagicBingoBGViewModel>();
            bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
            bindingSet.Bind(scrollView).For(v => v.Collection).To(vm => vm.ScrollViewList);
            
            bindingSet.Bind(times).For(v => v.text).To(vm => vm.TimeDes);
            
            ProgressScrollview.PrefabPath = ProgressScrollviewCell;
            bindingSet.Bind(ProgressScrollview).For(v => v.Collection).To(vm => vm.ProgressGiftList);
            bindingSet.Bind(ProgressScrollview).For(v => v.DefaultJumpIndex).To(vm => vm.ProgressGiftNowIndex);
            
            
            bindingSet.Bind(bingoPointText).For(v => v.text).To(vm => vm.BingoPointText);
            bindingSet.Bind(this).For(v => v.BingoCount).To(vm => vm.BingoCount);
            bindingSet.Bind(toDayBingoNums).For(v => v.text).To(vm => vm.ToDayBingoNums);
            bindingSet.Bind(progress).For(v => v.value).To(vm => vm.ProgressValue);
            bindingSet.Bind(buyBut).For(v => v.onClick).To(vm => vm.OnClickBuyBtnCommand);
            
            bindingSet.Bind(bingoOverGo).For(v => v.activeSelf).To(vm => vm.IsShowRestBut);
            bindingSet.Bind(mask).For(v => v.activeSelf).ToExpression(vm => vm.Manager.IsShowMask);
            bindingSet.Bind(restButton).For(v => v.onClick).To(vm => vm.OnClickRestBtnCommand);
            bindingSet.Bind(ruleBut).For(v => v.onClick).To(vm => vm.OnClickRuleButCommand);
            bindingSet.Build();
        }
    }
}