using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine.UI;

namespace DH.Game.UIViews.ItemViews
{
public partial class SettingLanguageItem : BaseItemView
{
    public Text languageTxt;
    public DhButton btn;
    public override async Cysharp.Threading.Tasks.UniTask Create()
    {
        await base.Create();
        var bindSet = this.CreateBindingSet<SettingLanguageItem, SettingLanguageItemModel>();
        bindSet.Bind(languageTxt).For(v => v.text).To(vm => vm.LanguageTxt);
        bindSet.Bind(btn).For(v => v.onClick).To(vm => vm.OnClickCmd);
        bindSet.Build();
    }
}
    
}
