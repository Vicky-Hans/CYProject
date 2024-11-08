using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework;
using DH.UIFramework.Observables;


namespace DH.Game.ViewModels
{
    public partial class LuckEggFundItemViewModel : ViewModelBase
    {
        public LuckyEggData Data => DataCenter.luckyEggData;
        public ActivityFundCfg Cfg;
        public CellItemViewModel ItemFreeData;
        public ObservableDictionary<int, CellItemViewModel> Items = new();
        public Func<System.Object, object> GetOutSideRewardByIndex => GetOutSideRewardVm;

        [AutoNotify] private bool showLine;

        [Preserve]
        public LuckEggFundItemViewModel(ActivityFundCfg cfg)
        {
            Cfg = cfg;
            ItemFreeData = CellItemViewModel.Create(Cfg.FreeReward[0]);
            ItemFreeData.SetClickAction((info) => { OnClickFree(); });
            for (int i = 0; i < Cfg.PassReward2.Count; i++)
            {
                var cellItem = CellItemViewModel.Create(Cfg.PassReward2[i]);
                cellItem.SetClickAction((info) => { OnClickLevel1(); });
                Items.Add(i, cellItem);
            }

            RefreshStateNew();
            Data.PropertyChanged += BuyChanged;
            DataCenter.luckyEggData.FundClaimed.CollectionChanged += ClaimChanged;
            DataCenter.luckyEggData.FundPlusClaimed.CollectionChanged += ClaimChanged;
        }

        private void ClaimChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshStateNew();
        }


        protected override void OnDispose()
        {
            base.OnDispose();
            Data.PropertyChanged -= BuyChanged;
            DataCenter.luckyEggData.FundClaimed.CollectionChanged -= ClaimChanged;
            DataCenter.luckyEggData.FundPlusClaimed.CollectionChanged -= ClaimChanged;
        }

        private void RefreshStateNew()
        {
            var finish = DataCenter.luckyEggData.IsFinish(Cfg.Factor);
            if (DataCenter.luckyEggData.IsClaimed(Cfg.Id))
            {
                ItemFreeData.State = ECellItemState.Finish;
                ItemFreeData.IsShowLock = false;
            }
            else
            {
                if (finish)
                {
                    ItemFreeData.State = ECellItemState.GetIng;
                    ItemFreeData.IsShowLock = false;
                }
                else
                {
                    ItemFreeData.State = ECellItemState.None;
                    ItemFreeData.IsShowLock = true;
                }
            }

            var isPlus = DataCenter.luckyEggData.IsPlusClaimed(Cfg.Id);
            var isBuy = DataCenter.luckyEggData.GetPlusIsBuy();
            for (int i = 0; i < Items.Count; i++)
            {
                if (isBuy)
                {
                    if (isPlus)
                    {
                        Items[i].State = ECellItemState.Finish;
                        Items[i].IsShowLock = false;
                    }
                    else
                    {
                        if (finish)
                        {
                            Items[i].State = ECellItemState.GetIng;
                            Items[i].IsShowLock = false;
                        }
                        else
                        {
                            Items[i].State = ECellItemState.None;
                            Items[i].IsShowLock = true;
                        }
                    }
                }
                else
                {
                    Items[i].State = ECellItemState.None;
                    Items[i].IsShowLock = true;
                }
            }
            
            var showLine = false;
            var next = ConfigCenter.FundRewardsCfgColl.GetDataById(Cfg.Id + 1);
            if (next != null)
            {
                showLine = DataCenter.luckyEggData.IsFinish(Cfg.Factor) && !DataCenter.luckyEggData.IsFinish(next.Factor);
            }
            ShowLine = showLine;
        }

        private object GetOutSideRewardVm(object index)
        {
            if (Items.TryGetValue((int)index, out CellItemViewModel ret))
            {
                return ret;
            }

            return null;
        }

        private void BuyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataCenter.luckyEggData.FundPlus))
            {
                RefreshStateNew();
            }
        }

        [Command]
        public void OnClickFree()
        {
            ClaimCrystal(1);
        }

        [Command]
        public void OnClickLevel1()
        {
            ClaimCrystal(2);
        }

        private async void ClaimCrystal(int op)
        {
            if (Data.IsTimeOver()) return;
            if (!DataCenter.luckyEggData.IsFinish(Cfg.Factor))
            {
                // ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.chengzhjijin23).Name);
                AudioManager.Instance.PlayWrongTips();
                return;
            }

            if (op == 1)
            {
                if (DataCenter.luckyEggData.IsClaimed(Cfg.Id))
                {
                    //    ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.challengeStage_tips_1).Name);
                    AudioManager.Instance.PlayWrongTips();
                    return;
                }
            }
            else if (op == 2)
            {
                if (!DataCenter.luckyEggData.GetPlusIsBuy())
                {
                    // ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.chengzhjijin11).Name);
                    AudioManager.Instance.PlayWrongTips();
                    return;
                }

                if (DataCenter.luckyEggData.IsPlusClaimed(Cfg.Id))
                {
                    //ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.challengeStage_tips_1).Name);
                    AudioManager.Instance.PlayWrongTips();
                    return;
                }
            }
            
            var result = await GameNetworkManager.Instance.SendAsync<RspLuckyEggFundClaim>(new ReqLuckyEggFundClaim());
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                Lodash.DealRewards(result.rsp.Rewards.ToList());
                Data.AddFreeIds(result.rsp.FreeIds.ToList());
                Data.AddPlusOneIds(result.rsp.PlusIds.ToList());
                UIHelper.OpenCommonRewardView(result.rsp.Rewards.ToList());
                RefreshStateNew();
            }
        }

    }
}