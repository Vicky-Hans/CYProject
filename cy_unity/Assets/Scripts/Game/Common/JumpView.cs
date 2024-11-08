using DH.Game;
using DH.UIFramework;
using Extend;
namespace Game.UI.CommonView
{
    public partial class JumpView : BaseView
    {
        
        public override bool FullScreen => false;
        public DhText itemName;
        public DhText itemDes;
        public DhButton cancelBtn;
        public DhImage itemIcon;
        public DhImage itemBgIcon;
        public UICircularScrollView jumpScrollView;
        [AssetPath] public string itemPrefab;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            jumpScrollView.PrefabPath = itemPrefab;
            var bindSet = this.CreateBindingSet<JumpView, JumpViewModel>();
            bindSet.Bind(itemName).For(v => v.text).To(vm => vm.ItemNameTxt);
            bindSet.Bind(itemDes).For(v => v.text).To(vm => vm.ItemDesTxt);
            bindSet.Bind(cancelBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtn);
            bindSet.Bind(itemIcon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindSet.Bind(itemBgIcon).For(v => v.sprite).To(vm => vm.IconBgPath).WithConversion(this);
            bindSet.Bind(jumpScrollView).For(v=>v.Collection).To(vm=>vm.JumpItemDic);
            bindSet.Build();
        }
    }
}