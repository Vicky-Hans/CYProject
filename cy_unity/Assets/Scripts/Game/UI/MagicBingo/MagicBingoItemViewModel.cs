using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Proto;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class MagicBingoItemViewModel : ViewModelBase
    {
        [AutoNotify] private bool isGetAward;
        [AutoNotify] private CellItemViewModel cellItemVm;
        [AutoNotify] private bool showOpenEffect;
        [AutoNotify] private bool bingoEffect;
        private int pos;
        private MagicBingoData Data => DataCenter.mgicBingoData;
        [Preserve]
        public MagicBingoItemViewModel(int pos)
        {
            this.pos = pos;
            Data.PropertyChanged += DataPropertyChanged;
            InitUI(true);
        }
        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Data.Grid.OpenRecord) or  nameof(Data.Grid.BingoCount) )
            {
                InitUI();
            }
        }
        protected override void OnDispose()
        {
            Data.PropertyChanged -= DataPropertyChanged;
            base.OnDispose();
        }
        public void InitUI(bool isFirst = false)
        {
            if (isFirst)
            {
                IsGetAward = Data.GetGradAward(pos) != null;
                InitCellItemVm();
            }
            else
            {
                var temp =  Data.GetGradAward(pos) != null;
                if (IsGetAward != temp)
                {
                    PlayAni();
                }
                else
                {
                    if (CellItemVm!=null)
                    {
                        BingoEffect = Data.IsBingoAward(pos);
                    }

                }
            }
        }

        [Command]
        private async void OnClickOpBtn()
        {
            if (Data.IsExchangeTime)
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.niudan17));
                return;
            }
            
            if (!UIHelper.CheckRewardIsEnough(new Reward(RewardType.Item,(int)GameConst.ItemIdCode.BinGoCoin,1)))
            {
                ActivityUIManager.Instance.OpenBuyCoin();
                return;
            }
            var BingoMaxNums = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Bingo_01).Content[0];
            if (Data.Grid.Count >= BingoMaxNums)
            {
                ToastManager.Show(ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.Bingo_06).Name);
                return;
            }
            var req = new ReqBingoOpen()
            {
                Pos = pos,
            };
            var result =await GameNetworkManager.Instance.SendAsync<RspBingoOpen>(req);
            if (NetHelper.CheckNetErrorMessage(result.rsp, true))
            {
                ActivityUIManager.Instance.IsShowMask = true;
                Lodash.DealRewards(result.rsp.Reward.ToList());
                Lodash.DealRewards(result.rsp.Cost.ToList(),false);
                Data.BingoCount();
                ShowOpenEffect = true;
                IsGetAward = true;
                Data.Bingo(result.rsp.OpenRecord);
                Data.BingoNums(result.rsp.BingoCount);
                InitCellItemVm();
                await UniTask.Delay(750);
                UIHelper.OpenCommonRewardView(result.rsp.Reward.ToList());
                ActivityUIManager.Instance.IsShowMask = false;

            }
        }

        private async void PlayAni()
        {
            await UniTask.Delay(350);
            ShowOpenEffect = true;
            InitCellItemVm();
        }

        private void InitCellItemVm()
        {
            IsGetAward = Data.GetGradAward(pos) != null;
            if (IsGetAward)
            {
                CellItemVm = CellItemViewModel.Create(Data.GetGradAward(pos));
                BingoEffect = Data.IsBingoAward(pos);
            }
        }

    }
}