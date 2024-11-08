using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DHFramework;
using Game.UI.MainStage;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class MainStageBoxItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string desc;
        [AutoNotify] private string boxPath;
        [AutoNotify] private Color descColor;
        [AutoNotify] private EBoxState boxState;
        [AutoNotify] private bool boxActive;
        [AutoNotify] private bool isShowTips;
        [AutoNotify] private string boxImgPath;
        private object cfg;
        private bool isMainStage;
        private Animator starAni;
        public RectTransform BoxPosTransform;
        private Action<int, int, Action<List<Resource>>> onClickBoxCallback;
        /// <summary>
        /// 服务器那宝箱的ID走 1 开始
        /// </summary>
        private int index;
        public Animator StarAni
        {
            get => null;
            set
            {
                starAni = value;
                ParseBoxState();
            }
        }

        private ParticleSystem starEffect;
        public ParticleSystem StarEffect
        {
            get => null;
            set
            {
                starEffect = value;
                ParseBoxState();
            }
        }

        

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"> 配置文件</param>
        /// <param name="isMainStage"> 是否是主线</param>
        /// <param name="boxIndex">宝箱下标 (1 开始)</param>
        /// <param name="clickBoxCallback">点击宝箱的回调 (1 开始)</param>
        /// <param name="isShowTips">是否展示宝箱下面的提示文本</param>
        [Preserve]
        public MainStageBoxItemViewModel(object cfg,bool isMainStage, int boxIndex, Action<int,int, Action<List<Resource>>> clickBoxCallback, bool isShowTips = true)
        {
            this.cfg = cfg;
            this.isMainStage = isMainStage;
            index = boxIndex;
            onClickBoxCallback = clickBoxCallback;
            IsShowTips = isShowTips;
            ParseBoxState();
        }
        public void OnClickBoxBtn()
        {
            OnClickMainStageBox();
        }

   
        private async void OnClickMainStageBox()
        {
            DHLog.Debug($" 点击宝箱 {index}");
            var tempCfg = cfg as CopyCfg;
            if (tempCfg == null)
            {
                DHLog.Error(" OnClickMainStageBox 配置错误 请检查配置");
                return;
            }
            if (DataCenter.mainStageData.CheckChapterBoxIsCanClaim(tempCfg.Id, index) != 0)
            {
                var rewards = tempCfg.Reward;
                switch (index)
                {
                    case 1: rewards = tempCfg.CopyReward1; break;
                    case 2: rewards = tempCfg.CopyReward2; break;
                    case 3: rewards = tempCfg.CopyReward3; break;
                }
                
                var preModel = new ChapterBoxPreviewViewModel(rewards);
                preModel.NodePos = BoxPosTransform.position;
                UIManager.Instance.OpenDialog<ChapterBoxPreviewView>(preModel).Forget();
                return;
            }
            if (BoxState == EBoxState.Open)
            {
                return;
            }

            onClickBoxCallback(tempCfg.Id, index, ShowRewardsMsgBox);
            ParseBoxState();
        }
        
        
        /// <summary>
        /// 展示奖励
        /// </summary>
        /// <param name="resources"></param>
        private void ShowRewardsMsgBox(List<Resource> resources)
        {
            // CommonRewardMsgBoxViewModel rewardVm = new CommonRewardMsgBoxViewModel(resources);
            // rewardVm.SpacingY = 20;
            // rewardVm.CloseCallback = () =>
            // {
            //     PlayStarAni().Forget();
            // };
            // UIManager.Instance.OpenDialog<CommonRewardMsgBoxView>(rewardVm).Forget();
            
            UIHelper.OpenCommonRewardView(resources, () =>
            {
                PlayStarAni().Forget();
            });
        }
        
        

        private void ParseDesc()
        {
            var tempCfg = cfg as CopyCfg;
            var replayStr = tempCfg.Condition[index -1];
            string tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips04, replayStr.Count);
            // switch (index)
            // {
            //     case 1: tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.Fuben_tips04, replayStr.Count); break;
            //     case 2: tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xinfuben02); break;
            //     case 3: tempStr = LocalizeHelper.GetGlobal(GlobalLanguageId.xinfuben03); break;
            // }
            //
            Desc = tempStr;
        }

        private void UpdateMainStageBoxState()
        {
            var tempCfg = cfg as CopyCfg;
            if (tempCfg == null) return;
            int chapterId = tempCfg.Id;
            
            int boxState = (int)EBoxState.Close;
            var chapterInfo = DataCenter.mainStageData.GetChapterInfo(chapterId);
            if (chapterInfo != null)
            {
                boxState = chapterInfo.BoxClaimStatus;
            }
            var state = Lodash.ParsePosValue(boxState, index);
            
            
            BoxImgPath =  state != 0 ? "mainui[mainui_icon_9]" : "mainui[mainui_icon_8]";
            if (state == 0)
            {
                var canClaim = DataCenter.mainStageData.CheckChapterBoxIsCanClaim(chapterId, index);
                if (canClaim == 0)
                {
                    BoxState = EBoxState.Wait;
                }
                else
                {
                    BoxState = EBoxState.Close;
                }
                if (starAni != null)
                {
                    starAni.gameObject.SetActive(false);
                }

                if (starEffect != null)
                {
                    starEffect.gameObject.SetActive(false);
                }
                BoxActive = true;
            }
            else
            {
                BoxState = EBoxState.Open;
                if (starAni != null)
                {
                    starAni.gameObject.SetActive(true);
                }

                if (starEffect != null)
                {
                    starEffect.gameObject.SetActive(false);
                }
                BoxActive = false;
            }

            // int passState = DataCenter.mainStageData.CheckChapterBoxIsCanClaim(chapterId, index);
            // DescColor = passState == 0 ? new Color(0,255,252,255): UIHelper.HexColorStrToColor(DhHexColor.White);
            ParseDesc();
            
        }

        private void ParseBoxState()
        {
            UpdateMainStageBoxState();
        }
        
        private async UniTaskVoid PlayStarAni()
        {

            if (starAni == null)
            {
                UpdateMainStageBoxState();
                return;
            }

            AudioManager.Instance.PlayAudio("SFX_UI/ui_stageChest_Star"); 
            starAni.SetBool("isPlay", true);
            starAni.gameObject.SetActive(true);
            starEffect.gameObject.SetActive(true);
            BoxActive = false;
            starEffect.Play();
            starAni.Play("star_animition");
            await UniTask.Delay(2000);
            if (starEffect != null)
            {
                starEffect.gameObject.SetActive(false);
            }
            UpdateMainStageBoxState();
        }


    }
}