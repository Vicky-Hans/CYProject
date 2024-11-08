using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine.UI;


namespace DH.Game.UIViews
{
    public partial class RenameView : BaseView
    {
        public override bool FullScreen => false;
        public DhButton closeBtn;
        public DhButton okBtn;
        public DhButton cdBtn;
        public DhText cdBtnTxt;
        public InputField input;

        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<RenameView, RenameViewModel>();
            bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickClose);
            bindingSet.Bind(okBtn).For(v => v.onClick).To(vm => vm.OnClickOKBtn);
            bindingSet.Bind(okBtn.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.RenameState==ERenameState.Free);
            bindingSet.Bind(cdBtn.gameObject).For(v=>v.activeSelf).ToExpression(vm=>vm.RenameState==ERenameState.Cd);
            bindingSet.Bind(input).For(v => v.text, v => v.onValueChanged).To(vm => vm.InputEditorStr).OneWayToSource();
            bindingSet.Bind(input).For(v=>v.text,v=>v.onEndEdit).To(vm=>vm.InputStr).TwoWay();
            bindingSet.Bind(cdBtnTxt).For(v => v.text).ToExpression(vm =>GetCdTimeStr( vm.CdTime));
            bindingSet.Bind(input).For(v=>v.interactable).ToExpression(vm=>vm.CdTime<=0);
            bindingSet.Build();
        }

        public string GetCdTimeStr(long time)
        {
            return $"{ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.ProName04).Name}\n{UIHelper.ConvertTimeSecondToString(time)}";
        }
    }
}
