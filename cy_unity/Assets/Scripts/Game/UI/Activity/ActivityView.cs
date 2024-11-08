using DH.Game.ViewModels;
using DH.UIFramework;
using Cysharp.Threading.Tasks;

namespace DH.Game.UIViews
{
    public partial class ActivityView : BaseItemView
    {
	    public override bool FullScreen => false;
	    public UICircularScrollView activityScrollview;
	    [AssetPath] public string activeCellPrefab;
        public override async UniTask Create()
        {
	        activityScrollview.PrefabPath	= activeCellPrefab;
            await base.Create();
            var bindingSet = this.CreateBindingSet<ActivityView, ActivityViewModel>();
            bindingSet.Bind(activityScrollview).For(v => v.Collection).To(vm => vm.ActivityScrollviewList);
            bindingSet.Build();
        }
    }
}