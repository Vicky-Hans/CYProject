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
    public partial class ChallengeBoxItemViewModel : ViewModelBase
    {
        [AutoNotify] private string normalIconStr;
        [AutoNotify] private string claimedIconStr;//可领取Icon路径
        [AutoNotify] private string progressNumStr;//宝箱进度
        [AutoNotify] private EBoxState boxState;
        [AutoNotify] private RectTransform boxRectTransform;//宝箱位置
        private DailyStageProgressRewardCfg boxCfg;//宝箱cfg
        private int MaxBoxNum = 5;
        [Preserve]
        public ChallengeBoxItemViewModel(DailyStageProgressRewardCfg boxcfg)
        {
            boxCfg = boxcfg;
            var maxIdx = ConfigCenter.DailyStageProgressRewardCfgColl.DataItems.Count;
            if (ConfigCenter.DailyStageProgressRewardCfgColl.DataItems[maxIdx-1]!=null)
            {
                MaxBoxNum = ConfigCenter.DailyStageProgressRewardCfgColl.DataItems[maxIdx-1].Value2;
            }
            UpdateBoxData();
        }

        public void UpdateBoxData()
        {
            if (boxCfg.Id == ConfigCenter.DailyStageProgressRewardCfgColl.DataItems.Count)
            {
                NormalIconStr = $"daily[daily_icon_02]";
                ClaimedIconStr = $"daily[daily_icon_05]";
            }
            else
            {
                NormalIconStr = $"daily[daily_icon_03]";
                ClaimedIconStr = $"daily[daily_icon_04]";
            }
            ProgressNumStr = $"{boxCfg.Value2}/{MaxBoxNum}";
            if (DataCenter.dailyFightData.WeekProgress >= boxCfg.Value2)
            {
                BoxState = DataCenter.dailyFightData.WeekClaim.Contains(boxCfg.Id) ? EBoxState.Open : EBoxState.Wait;
            }
            else
            {
                BoxState = EBoxState.Close;
            }
        }
        [Command]
        private void OnClickBox()
        {
            if (DataCenter.dailyFightData.WeekClaim.Contains(boxCfg.Id)) return;
            if (BoxState == EBoxState.Open) return;
            if (DataCenter.dailyFightData.WeekProgress >= boxCfg.Value2)//可领取
            {
                OnWeekendBoxClaimed();
            } 
            else if (DataCenter.dailyFightData.WeekProgress < boxCfg.Value2) //预览
            {
                var preModel = new ChapterBoxPreviewViewModel(boxCfg.Reward.ToList());
                preModel.NodePos = BoxRectTransform.position;
                UIManager.Instance.OpenDialog<ChapterBoxPreviewView>(preModel).Forget();
            }
        }
        private async void OnWeekendBoxClaimed() 
        {
            var result = await GameNetworkManager.Instance.SendAsync<RspDailyFightWeekBox>(new ReqDailyFightWeekBox{Id = boxCfg.Id});
            if (result.rsp.Status != 0)
            {
                var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
                ToastManager.Show(str);
                return;
            }
            UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), () => {
                Lodash.DealRewards(result.rsp.Reward.ToList());
                DataCenter.dailyFightData.WeekClaim.Add(boxCfg.Id);
                UpdateBoxData();
            });
        }
    }
}