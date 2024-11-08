using Dh.Game.ViewModels;
using DH.UIFramework;
using Extend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dh.Game.UIViews.ItemViews
{
    public partial class CollegeTaskItemView : BaseItemView
    {
        public TextMeshProUGUI desc;
        public CommonItemListView itemListView;
        public Slider slider;
        public TextMeshProUGUI progressValue;
        public DhButton btnGoWay;
        public DhButton btnGet;
        public GameObject finish;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<CollegeTaskItemView, CollegeTaskItemModel>();
            bindSet.Bind(desc).For(v => v.text).To(vm => vm.TaskDesc);
            bindSet.Bind(itemListView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemListModel);
            bindSet.Bind(slider).For(v => v.value).To(vm => vm.ProgressValue);
            bindSet.Bind(progressValue).For(v => v.text).To(vm => vm.ProgressDesc);
            bindSet.Bind(btnGoWay.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == TaskState.GoWay);
            bindSet.Bind(btnGet.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.State == TaskState.Get);
            bindSet.Bind(finish).For(v => v.activeSelf).ToExpression(vm => vm.State == TaskState.Finish);
            bindSet.Bind(btnGoWay).For(v => v.onClick).To(vm => vm.OnGoWayCommand);
            bindSet.Bind(btnGet).For(v => v.onClick).To(vm => vm.OnClickGetCommand);
            bindSet.Build();
        }
    }
}