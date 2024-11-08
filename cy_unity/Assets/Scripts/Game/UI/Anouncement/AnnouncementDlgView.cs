using Cysharp.Threading.Tasks;
using DH.ComponentUI;
using DH.Game.UIViews;
using DH.UIFramework;
using DH.UIFramework.Builder;
using TMPro;
using UnityEngine.UI;

public partial class AnnouncementDlgView : BaseView
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI NoticeTitleText;
    public TextMeshProUGUI ContentText;
    public Button CloseBtn;
    public Button CloseBg;
    public Button serverBtn;
    public ClickTextComponent clickText;
    public ContentSizeFitter contentSizeFitter;
    public override async Cysharp.Threading.Tasks.UniTask Create()
    {
        await base.Create();
        ContentText.text = "";
        
        var bindingSet = this.CreateBindingSet<AnnouncementDlgView, AnnouncementViewModel>();
        bindingSet.Bind(NoticeTitleText).For(v => v.text)
            .To(vm => vm.CurrentAnnouncementItem.title);
        bindingSet.Bind(ContentText).For(v => v.text).To(vm => vm.CurrentAnnouncementItem.Content);
        bindingSet.Bind(CloseBg).For(v => v.onClick).To(vm => vm.CloseDlgCmd);
        bindingSet.Bind(serverBtn).For(v => v.onClick).To(vm => vm.OnClickServerBtn);
        bindingSet.Bind(this).For(v => v.clickText).To(vm => vm.ClickTextCmp).OneWayToSource();
        if (CloseBtn)
        {
            bindingSet.Bind(CloseBtn).For(v => v.onClick).To(vm => vm.CloseDlgCmd);
        }
        bindingSet.Build();
        RefreshShowContent().Forget();
    }

    private async UniTaskVoid RefreshShowContent()
    {
        await UniTask.Delay(300);
        if(contentSizeFitter!=null)
            contentSizeFitter.SetLayoutVertical();
    }
}
