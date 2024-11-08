using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using Game.UI.MainStage;
using UnityEngine;
namespace DH.Game.ViewModels
{
    public partial class ChallengeDailyBoxViewModel : ViewModelBase
    {
        [AutoNotify] private string desc;
        [AutoNotify] private string boxPath;
        [AutoNotify] private Color descColor;
        [AutoNotify] private EBoxState boxState;
        [AutoNotify] private bool boxActive;
        [AutoNotify] private bool isShowTips;
        [AutoNotify] private string boxImgPath;
        private DailyStageProgressRewardCfg boxCfg;//宝箱cfg
        private Animator starAni;
        public RectTransform BoxPosTransform;
        private int boxIdx; //服务器那宝箱的ID走 1 开始
        private ParticleSystem starEffect;
        [Preserve]
        public ChallengeDailyBoxViewModel(int boxIndex,DailyStageProgressRewardCfg boxcfg)
        {
            boxIdx = boxIndex;
            boxCfg = boxcfg;
            UpdateBoxState();
        }
        public void OnClickBoxBtn()
        {
            if (DataCenter.dailyFightData.DailyClaim.Contains(boxCfg.Id)) return;
            if (BoxState == EBoxState.Open) return;
            if (DataCenter.dailyFightData.DailyKills >= boxCfg.Value1)//可领取
            {
                OnDailyBoxClaimed();
            } 
            else if (DataCenter.dailyFightData.DailyKills < boxCfg.Value1) //预览
            {
                var preModel = new ChapterBoxPreviewViewModel(boxCfg.Reward.ToList());
                preModel.NodePos = BoxPosTransform.position;
                UIManager.Instance.OpenDialog<ChapterBoxPreviewView>(preModel).Forget();
            }
        }
        private async void OnDailyBoxClaimed() 
        {
            var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightBox>(new ReqDailyFightBox{Id = boxCfg.Id});
            if (result.rsp.Status != 0)
            {
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), () => {
                Lodash.DealRewards(result.rsp.Reward.ToList());
            });
            DataCenter.dailyFightData.DailyClaim.Add(boxCfg.Id);
            var cfgList = ConfigCenter.DailyStageProgressRewardCfgColl.GetDataByType(1);
            var isAllClaimed = true;
            for (var i = 0; i < cfgList.Count; i++)
            {
                if (DataCenter.dailyFightData.DailyClaim.Contains(cfgList[i].Id)) continue;
                isAllClaimed = false;
                break;
            }
            if (isAllClaimed) DataCenter.dailyFightData.WeekProgress += 1;
            UpdateBoxState();
            
        }
        public void UpdateBoxState()
        {
            if (DataCenter.dailyFightData.DailyClaim.Contains(boxCfg.Id))//已领取
            {
                BoxState = EBoxState.Open;
                if (starAni != null) starAni.gameObject.SetActive(true);
                if (starEffect != null) starEffect.gameObject.SetActive(false);
                BoxActive = false;
                BoxImgPath = "mainui[mainui_icon_9]";
            }
            else
            {
                BoxImgPath = "mainui[mainui_icon_8]";
                if (DataCenter.dailyFightData.DailyKills >= boxCfg.Value1)//可领取
                {
                    BoxState = EBoxState.Wait;
                    if (starAni != null) starAni.gameObject.SetActive(false);
                    if (starEffect != null) starEffect.gameObject.SetActive(false);
                    BoxActive = true;
                }
                else
                {
                    BoxState = EBoxState.Close;
                    if (starAni != null) starAni.gameObject.SetActive(false);
                    if (starEffect != null) starEffect.gameObject.SetActive(false);
                    BoxActive = true;
                }
            }
            Desc = "";
            var languageCfg = ConfigCenter.DailyStageProgressRewardLanguageCfgColl.GetDataById(boxCfg.Id);
            if (languageCfg!=null) Desc = string.Format(languageCfg.Dec,boxCfg.Value1);
        }
    }
}