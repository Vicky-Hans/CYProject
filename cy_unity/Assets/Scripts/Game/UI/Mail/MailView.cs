using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class MailView : BaseView
    {
        public override bool FullScreen => false;
        
		public UICircularScrollView scrolView;
		[AssetPath]public string scrolViewCell;
		public DhButton closeBtn;
		public DhButton delAll;
		public DhButton claim;
		public GameObject noMail;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            
			scrolView.PrefabPath = scrolViewCell;

            await base.Create();
            var bindingSet = this.CreateBindingSet<MailView, MailViewModel>();
            bindingSet.Bind(this).For(v => v.scrolView).To(vm => vm.MailScrollView).OneWayToSource();
            bindingSet.Bind(scrolView).For(v => v.Collection).To(vm => vm.MailDictionary);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(delAll).For(v => v.onClick).To(vm => vm.OnClickDelAllCommand);
			bindingSet.Bind(delAll.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MailDictionary.Count > 0);
			bindingSet.Bind(claim).For(v => v.onClick).To(vm => vm.OnClickClaimCommand);
			bindingSet.Bind(claim.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MailDictionary.Count > 0);
			bindingSet.Bind(noMail).For(v => v.activeSelf).ToExpression(vm => vm.MailDictionary.Count == 0);
            bindingSet.Build();
        }
    }
}