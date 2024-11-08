using System;
using System.Text;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Proto;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using DHFramework;
using UnityEngine;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public enum ERenameState
    {
        Free,
        Cd
    }

    public partial class RenameViewModel : ViewModelBase
    {
        private string inputStr;
        private string inputEditorStr;
        private ERenameState renameState;
        private long cdTime;
        private float tempTime;
        public ICommand OnClickOKBtn;
        private int cfgCd;

        [Preserve]
        public RenameViewModel()
        {
            tempTime = 0;
            OnClickOKBtn = new AsyncCommand(OnClickOk);
            var lastTime =  ServerTime.Instance.GetNowTime() - DataCenter.charcaterData.ChangeNameStamp;
            cfgCd = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.rename_cd).Content[0]*3600;//小时
            if (DataCenter.charcaterData.ChangeNameStamp == 0 || lastTime > cfgCd)
            {
                RenameState = ERenameState.Free;
                CdTime = 0;
            }
            else
            {
                CdTime = cfgCd - lastTime;
                RenameState = ERenameState.Cd;
            }
            UpdateCd();
        }

        public void OnClickClose()
        {
            UIManager.Instance.CloseDialog<RenameView>();
        }

        public ERenameState RenameState
        {
            get => renameState;
            set { Set(ref renameState, value); }
        }

        public string InputStr
        {
            get => inputStr;
            set { Set(ref inputStr, value); }
        }

        public string InputEditorStr
        {
            get => inputEditorStr;
            set
            {
                int byteCount = Encoding.UTF8.GetByteCount(value);
                DHLog.Debug("inputEditor====" + value + "====" + byteCount);

                Set(ref inputEditorStr, value);
            }
        }

        public long CdTime
        {
            get => cdTime;
            set { Set(ref cdTime, value); }
        }

        public async UniTask OnClickOk()
        {
            DHLog.Debug("input-------" + InputStr);
            if (string.IsNullOrEmpty(InputStr))
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.ProName07));
                return;
            }

            if (InputStr == DataCenter.charcaterData.Digest.Name)
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips29));
                return;
            }
            int length = Encoding.Default.GetByteCount(InputStr);
            if (length > 15)
            {
                ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.ProName08));
                return;
            }
            var req = new ReqChangeName
            {
                Name = InputStr
            };
            var result = await GameNetworkManager.Instance.SendAsync<RspChangeName>(req);
            if (result.rsp != null)
            {
                if (result.rsp.Status == 0)
                {
                    DataCenter.charcaterData.Digest.Name = InputStr;
                    DataCenter.charcaterData.changeNameCdPullTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    DataCenter.charcaterData.ChangeNameStamp =  ServerTime.Instance.GetNowTime() ;
                    RenameState = ERenameState.Cd;
                    UIManager.Instance.CloseDialog<RenameView>();
                    ToastManager.Show(LocalizeHelper.GetGlobal(GlobalLanguageId.ProName09));
                }
                else
                {
                    ToastManager.Show(UIHelper.GetNetErrorMessage(result.rsp.Status));
                }
            }
        }

        #region 倒计时相关
        private void UpdateCd()
        {
            if (RenameState == ERenameState.Cd)
            {
                var lastTime =  ServerTime.Instance.GetNowTime() - DataCenter.charcaterData.ChangeNameStamp;
                var cd = cfgCd - lastTime;
                if (DataCenter.charcaterData.ChangeNameStamp == 0 || lastTime > cfgCd)
                {
                    RenameState = ERenameState.Free;
                    CdTime = 0;
                }
                else
                {
                    CdTime = cd;
                }
            }
        }
        private float time;
        public override void Update()
        {
            base.Update();
            if (!UIHelper.CalculateTime(ref time)) return;
            UpdateCd();
        }
        
        #endregion

    }
}