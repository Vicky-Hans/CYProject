using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;


namespace DH.Game.ViewModels
{
    public partial class FunctionMenuViewModel : ViewModelBase
    {
        public Vector3 Pos;
        public Vector3 Offset = new Vector3(72,0, 0);

        [AutoNotify] private bool showMailRedGo;
        
        [Preserve]
        public FunctionMenuViewModel()
        {	       
            DataCenter.maildata.UpdateRedDotCount();
            ShowMailRedGo = DataCenter.maildata.RedDotCount > 0;
        }


        [Command]
        private void OnClickSettingButton()
        {
	        UIManager.Instance.OpenDialog<SystemSettingView,SystemSettingViewModel>().Forget();
        }

        [Command]
        private void OnClickMailButton()
        {
            UIManager.Instance.OpenDialog<MailView,MailViewModel>().Forget();
        }

        private float time;
        public override void Update()
        {
            base.Update();
            if (!UIHelper.CalculateTime(ref time)) return;
            ShowMailRedGo = DataCenter.maildata.RedDotCount > 0;
        }
        
    }
}