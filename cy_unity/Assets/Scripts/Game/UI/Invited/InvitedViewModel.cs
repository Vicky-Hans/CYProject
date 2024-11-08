using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using TMPro;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class InvitedViewModel : ViewModelBase
    {
	    [AutoNotify] private TMP_InputField inputField;
	    [AutoNotify] private bool isShowInputInvitedIdNode;
	    [AutoNotify] private ObservableList<InvitedRewardItemViewModel> rewardScrollviewList = new();
	    [AutoNotify] private string progressTextStr;
	    [AutoNotify] private float progressSliderValue;
	    private int maxCount;
	    private List<int> progressList = new();
	    public Func<object,object> GetRewardScrollviewListValue=> GetRewardScrollviewListValueByKey;

        [Preserve]
        public InvitedViewModel()
        {
	        IsShowInputInvitedIdNode = true;
	        var cfgs = ConfigCenter.ShareRewardProgressCfgColl.DataItems;
	        for (int i = 0; i < cfgs.Count; i++)
	        {
		        var cfg = cfgs[i];
		        var tempVm = new InvitedRewardItemViewModel(cfg, OnClaimReward);
		        rewardScrollviewList.Add(tempVm);
		        maxCount = Mathf.Max(maxCount, cfg.Value1);
		        progressList.Add(cfg.Value1);
	        }

	        progressList.Sort((a, b) => a.CompareTo(b));

	        UpdateProgress();
	        DataCenter.charcaterData.PropertyChanged += OnPropertyChanged;
        }

        protected override void OnDispose()
        {
	        DataCenter.charcaterData.PropertyChanged -= OnPropertyChanged;
	        base.OnDispose();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(DataCenter.charcaterData.InviteNumber))
	        {
				UpdateProgress();
	        }
        }
        
        [Command]
        private void OnClickInvitedBtn()
        {
	        DHLog.Debug("点击 系统 分享");
        }

		[Command]
		private void OnClickFacebookInvitedBtn()
		{
			DHLog.Debug("点击facebook 分享");
		}

		[Command]
		private void OnClickInfoBtn()
		{
			UIManager.Instance.OpenDialog<InvitedRuleView, InvitedRuleViewModel>().Forget();
		}
		
		[Command]
		private void OnCLickCloseBtn()
		{
			UIManager.Instance.CloseDialog<InvitedView>();
		}

		[Command]
		private async void OnClickSendBtn()
		{
			if (InputField == null) return;
			
			var inputStr = InputField.text;
			if (inputStr.Length != 8)
			{
				DHLog.Warning("请输入8位邀请码");
				return;	
			}
			bool isAllDigits = Regex.IsMatch(inputStr, @"^\d+$");
			if (!isAllDigits)
			{
				DHLog.Warning("请输入纯数字");
				return;
			}
			
			ReqInviteReport req = new ReqInviteReport();
			req.RoleId = long.Parse(inputStr);
			var result = await GameNetworkManager.Instance.SendAsync<RspInviteReport>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return;
			}
			// 没成功，但是有人，只是该人邀请已经满了
			if (!result.rsp.Success)
			{
				DHLog.Debug("该人邀请已经满了");
				return;
			}
			// 已经被邀请了
			DataCenter.charcaterData.IsInvited = true;
			IsShowInputInvitedIdNode = false;
		}

		private async void OnClaimReward(ShareRewardProgressCfg cfg)
		{
			ReqInviteClaim req = new ReqInviteClaim();
			req.Id = cfg.Id;
			var result = await GameNetworkManager.Instance.SendAsync<RspInviteClaim>(req);
			if (result.rsp is not { Status: 0 })
			{
				if(result.rsp == null) return;
				var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
				ToastManager.Show(str);
				return;
			}
			UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), CheckIsCloseAnRefreshView);
		}

		private void CheckIsCloseAnRefreshView()
		{
			foreach (var item in rewardScrollviewList)
			{
				item.UpdateState();
			}
		}

		private object GetRewardScrollviewListValueByKey(object key)
		{
			int index = (int) key;
			if (index < 0 || rewardScrollviewList.Count <= index) return null;
			return rewardScrollviewList[index];
		}

		private void UpdateProgress()
		{
			var curCount = DataCenter.charcaterData.InviteNumber;
			curCount = 2;
			if(progressList.Count == 0) return;
			var totalCount = progressList.Count;
			ProgressTextStr = $"{curCount}/{progressList[totalCount - 1]}";
			for (int i = totalCount - 1; i >=0; i--)
			{
				if(curCount >= progressList[i])
				{
					ProgressSliderValue = 0.25f * (i + 1);
					break;
				}
			}
		}
    }
}