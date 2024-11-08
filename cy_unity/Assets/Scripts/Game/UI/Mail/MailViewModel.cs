using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.Linq;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class MailViewModel : ViewModelBase
    {
        
	    [AutoNotify] private ObservableDictionary<long,MailCellItemViewModel> mailDictionary =new ();
		 private ICollectionView mailScrollView;
		 public ICollectionView MailScrollView
		 {
			 get => null;
			 set
			 {
				 mailScrollView = value;
				 if (mailScrollView == null) return;
				 mailScrollView.Refresh();
			 }
		 }
        [Preserve]
        public MailViewModel()
        {
	        UpdateMailInfo();
	        DataCenter.maildata.Mails.CollectionChanged += OnMailCollectionChanged;
        }

        private void UpdateMailInfo()
        {
	        UpdateMailInfo(true);
        }
        private void UpdateMailInfo(bool isClear)
        {
	        if (isClear)
	        {
		        mailDictionary.Clear();
		        var mails = DataCenter.maildata.Mails;
		        foreach (var item in mails)
		        {
			        MailCellItemViewModel tempVm = new MailCellItemViewModel(item);
			        tempVm.CloseCallBack = () =>
			        {
				        mailScrollView?.Refresh();
			        };
			        mailDictionary.Add(item.Id,tempVm);
		        }
	        }
	        else
	        {
		        foreach (var item in mailDictionary)
		        {
			        item.Value.UpdataMailInfo();
		        }
                
	        }


	        mailScrollView?.Refresh();
	        DataCenter.maildata.UpdateRedDotCount();
        }
        private void OnMailCollectionChanged(object sender,NotifyCollectionChangedEventArgs e)
        {
	        switch (e.Action)
	        {
		        case NotifyCollectionChangedAction.Add:
			        foreach (MailInfo item in e.NewItems)
			        {
				        MailCellItemViewModel tempVm = new MailCellItemViewModel(item);
				         tempVm.CloseCallBack = () =>
				         {
					       mailScrollView?.Refresh();
				         };
				         mailDictionary.Add(item.Id,tempVm);
			        }
			        mailScrollView?.Refresh();
		        break;
		        case NotifyCollectionChangedAction.Remove:
			        foreach (MailInfo item in e.OldItems)
			        {
				        mailDictionary.Remove(item.Id);
			        }
			        mailScrollView?.Refresh();
			        break;
	        }
        }
        
        
        

        #region 按钮事件

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<MailView>();
        }

        [Command]
        private async void OnClickDelAll()
        {
	        // ActivityManager.Instance.Show(WaitType.Net);
	        var idList = DataCenter.maildata.GetCanDelMailList();
	        if(idList.Count == 0) return;
	        var req = new ReqMailDelete();
	        foreach (var id in idList)
	        {
		        req.MailIds.Add((int)id);
	        }
	        var result = await GameNetworkManager.Instance.SendAsync<RspMailDelete>(req);
	        // ActivityManager.Instance.Hide(WaitType.Net);
	        if (result.rsp is not { Status: 0 })
	        {
		        if(result.rsp == null) return;
		        var str = UIHelper.GetNetErrorMessage(result.rsp.Status);
		        ToastManager.Show(str);
		        return;
	        }
	        foreach (var id in idList)
	        {
		        MailDictionary.Remove(id);
	        }
	        // 删除本地数据
	        DataCenter.maildata.DeleteMails(idList);

        }

        [Command]
        private  async void OnClickClaim()
        {
	        // ActivityManager.Instance.Show(WaitType.Net);
	        var idList = DataCenter.maildata.GetAllCanClaimRewardMailList();
	        if(idList.Count == 0) return;
	        var req = new ReqMailReward();
	        foreach (var id in idList)
	        {
		        req.MailIds.Add((int)id);
	        }
	        var result = await GameNetworkManager.Instance.SendAsync<RspMailReward>(req);
	        // ActivityManager.Instance.Hide(WaitType.Net, true);

	        if (result.rsp ==null || result.rsp.Status != 0)
	        {
		        DHLog.Debug($"邮件领奖 {result.rsp.Status}");
		        return;
	        }
	        var changeList = result.rsp.MailIds.ToList();
            
	        if(changeList.Count == 0) return;
	        // 更新本地数据
	        foreach (var id in changeList)
	        {
		        DataCenter.maildata.SetMailIsRead(id);
		        DataCenter.maildata.SetMailIsClaimRewardsByMailId(id);
	        }
	        // 给奖励
	        Lodash.DealRewards(result.rsp.Reward.ToList());
	        UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList(), () =>
	        {
		        UpdateMailInfo(false);
	        });
        }

        #endregion


        protected override void OnDispose()
        {
	        DataCenter.maildata.Mails.CollectionChanged -= OnMailCollectionChanged;
	        
	        base.OnDispose();
        }
    }
}