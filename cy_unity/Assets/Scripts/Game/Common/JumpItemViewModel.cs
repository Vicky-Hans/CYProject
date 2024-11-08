using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
namespace Game.UI.CommonView
{
    public partial class JumpItemViewModel : ViewModelBase
    {
        [AutoNotify] private string titleName;
        public Action<int> jumpItemOpBtnCallback;
        public ICommand OnClickOpBtn { get; private set; }
        private int _jumpKey = 0;//跳转ID
        [Preserve]
        public JumpItemViewModel(int jumpKey,Action<int> jumpOpBtnCallback)
        {
            jumpItemOpBtnCallback = jumpOpBtnCallback;
            OnClickOpBtn = new AsyncCommand(ClickOpBtn);
            var obtainLanguage = ConfigCenter.FunctionJumpLanguageCfgColl.GetDataById(jumpKey);
            if (obtainLanguage != null) TitleName = obtainLanguage.Name;
            _jumpKey = jumpKey;
        }
        private async UniTask ClickOpBtn()
        {
            jumpItemOpBtnCallback?.Invoke(_jumpKey);
        }
    }
}