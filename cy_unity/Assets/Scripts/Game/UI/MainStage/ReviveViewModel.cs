using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class ReviveViewModel : ViewModelBase
    {
        
		[AutoNotify] private string timeTextStr;
		[AutoNotify] private ItemPriceNodeModel costVm;
		List<Reward> costList = new();
		private long endTime;
		private bool isEnd;
        [Preserve]
        public ReviveViewModel()
        {
	        var timeCfg = ConfigCenter.DefinesCfgColl.GetDataById(309);
	        endTime = ServerTime.Instance.GetNowTime() + timeCfg.Content[0];
	        costList = GameManager.Instance.GetReviveCost();
	        if (costList.Count > 0)
	        {
		        costVm = new ItemPriceNodeModel(costList[0],true);
		        costVm.IsShowBg = false;
	        }
	        AudioManager.Instance.Play(AudioType.CoutDown);
	        UpdateTimeStr();
        }

        protected override void OnDispose()
        {
	        base.OnDispose();
	        costVm?.Dispose();
        }

        private float time;
        public override void Update()
        {
	        base.Update();
	        if(!UIHelper.CalculateTime( ref time)) return;
	        UpdateTimeStr();
        }
        
        private void UpdateTimeStr()
        {
	        var leftTime = ServerTime.Instance.RemainTime(endTime);
	        TimeTextStr = ((int)leftTime).ToString();
	        if (leftTime <= 0 && !isEnd)
	        {
		        isEnd = true;
		        OnClickCloseBtn();
	        }
	        
            AudioManager.Instance.Play(AudioType.CoutDown);
        }
        [Command]
        private async void OnClickOpBtn()
        {
	        if(isEnd) return;
	        isEnd = true;
	        if (!UIHelper.CheckRewardIsEnough(costList[0],true))
	        {
		        return;
	        }
	        var tmpOp = 1;
	        if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge) tmpOp = 3;
	        var ret = new ReqFightRevive
	        {
		        Op = tmpOp,//1-主线钻石复活，2-服饰技能复活 3-每日挑战钻石复活
	        };
	        var result = await GameNetworkManager.Instance.SendAsync<RspFightRevive>(ret);
	        isEnd = false;
	        if (result.rsp is not { Status: 0 })
	        {
		        if (result.rsp == null)
		        {
			        return;
		        }
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        Lodash.DealRewards(result.rsp.Cost.ToList(),false);
	        UIManager.Instance.CloseDialog<ReviveView>();
	        // TO-DO 复活后，需要重新进入战斗 
	        GameManager.Instance.UpdateReviveData(result.rsp.Data);
        }
        public void OnClickCloseBtn()
        {
	        GameManager.Instance.OnGameEnd(false);
	        UIManager.Instance.CloseDialog<ReviveView>();
        }
    }
}