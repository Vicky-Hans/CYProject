using DH.Game.ViewModels;
using DH.UIFramework;
namespace DH.Game.UIViews
{
    public partial class LuckTravelDrawItemView : BaseItemView, IViewKey

    {
    public override bool FullScreen => false;

    public CellItemView cellItemView;

    public int index;

    public object Key => index;

    public override async Cysharp.Threading.Tasks.UniTask Create()
    {


        await base.Create();
        var bindingSet = this.CreateBindingSet<LuckTravelDrawItemView, LuckTravelDrawItemViewModel>();

        bindingSet.Bind(cellItemView.BindingContext).For(v => v.DataContext).To(vm => vm.CellItemViewVm);

        bindingSet.Build();
    }
    }
}