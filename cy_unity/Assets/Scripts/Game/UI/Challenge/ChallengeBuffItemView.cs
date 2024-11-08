using Extend;
using System;
using UnityEngine;
using DH.UIFramework;
using DH.Game.ViewModels;
using Cysharp.Threading.Tasks;
namespace DH.Game.UIViews
{
    public partial class ChallengeBuffItemView : BaseItemView
    {
        public DhImage buffIcon;
        public DhButton btnBox;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<ChallengeBuffItemView, ChallengeBuffItemViewModel>();
            bindingSet.Bind(btnBox).For(v => v.onClick).To(vm => vm.OnClickBuffCommand).CommandParameter(() => new Tuple<Vector3, Vector3>(btnBox.transform.position, new Vector3(0,20,0)));
            bindingSet.Bind(buffIcon).For(v => v.sprite).To(vm => vm.BuffIconStr).WithConversion(this);
            bindingSet.Build();
        }
    }
}