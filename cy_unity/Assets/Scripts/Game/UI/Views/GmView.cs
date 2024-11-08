using DH.Game.ViewModels;
using DH.UIFramework;
using TMPro;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class GmView : BaseView
    {
        public TMP_InputField commandInput;
        public Button selectBtn;
        public Button cancelBtn;
        [AssetPath] public string commandItemPrefabPath;
        public TextMeshProUGUI descTxt;
        public UICircularScrollView recordScrollView;
        [AssetPath] public string recordItemPrefabPath;
        public Button OneKeyGFS;
        
        public UICircularScrollView itemInfoScrollView;
        [AssetPath] public string itemInfoPrefabPath;
        
        public ScrollRectExtend itemParamScrollView;
        [AssetPath] public string itemParamPrefabPath;
        public Button btnSend;
        public Button btnOpenList;

        public Button btnCloseItemBg;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            recordScrollView.PrefabPath = recordItemPrefabPath;
            itemInfoScrollView.PrefabPath = itemInfoPrefabPath;
            itemParamScrollView.PrefabPath = itemParamPrefabPath;
            await base.Create();
            var bindingSet = this.CreateBindingSet<GmView, GmViewModel>();
            bindingSet.Bind(commandInput).For(v => v.text, v => v.onEndEdit).To(vm => vm.GmCommandInput).TwoWay();
            
            bindingSet.Bind(this).For(v => v.commandInput).To(vm => vm.CommandInput).OneWayToSource();
            bindingSet.Bind(selectBtn).For(v => v.onClick).To(vm => vm.SendGmCommand);
            bindingSet.Bind(btnSend).For(v => v.onClick).To(vm => vm.SendGmCommand);
            bindingSet.Bind(cancelBtn).For(v => v.onClick).To(vm => vm.Exit);
            bindingSet.Bind(descTxt).For(v => v.text).To(vm => vm.Desc);
            bindingSet.Bind(recordScrollView).For(v => v.Collection).To(vm => vm.RecordList);
            bindingSet.Bind(OneKeyGFS).For(v => v.onClick).To(vm => vm.OnClickOneKeyGFSCommand);
            bindingSet.Bind(itemInfoScrollView).For(v => v.Collection).To(vm => vm.GmItemInfoList);
            bindingSet.Bind(itemParamScrollView).For(v => v.Collection).To(vm => vm.GmItemInfoparamList);
            bindingSet.Bind(btnCloseItemBg.gameObject).For(v => v.activeSelf).To(vm => vm.IsShowItemList);
            bindingSet.Bind(btnCloseItemBg).For(v => v.onClick).To(vm => vm.OnCloseItemBgCommand);
            bindingSet.Bind(btnOpenList).For(v => v.onClick).To(vm => vm.OnClickOpenGmList);
            bindingSet.Build();
        }
    }
}