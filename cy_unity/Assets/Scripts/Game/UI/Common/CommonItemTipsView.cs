using System;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class CommonItemTipsView : BaseView
    {
        // public Button closeBtn;
        public DhText title;
        public DhText desc;
        public Transform rectTransform;
        public Transform content;
        public RectTransform bgRectTransform;
        public GameObject tipIcon;
        public ClickToClose clickToClose;

        [NonSerialized] public Vector3 position;
        public  override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<CommonItemTipsView, CommonItemTipsViewModel>();
            bindSet.Bind(title).For(v => v.text).To(vm => vm.Title);
            bindSet.Bind(desc).For(v => v.text).To(vm => vm.Desc);
            bindSet.Bind(this).For(v => v.position).ToExpression(vm => CalcPos(vm.Position,vm.OffSet, vm.IsOpenOffset));
            bindSet.Bind(bgRectTransform).For(v=>v.sizeDelta).To(vm => vm.BgSize);
            bindSet.Bind(clickToClose).For(v => v.CloseCallback).To(vm => vm.OnClickCloseCallback);
            bindSet.Bind(this).For(v => v.desc).To(vm => vm.DescText).OneWayToSource();
            bindSet.Build();
        }
        public Vector3 CalcPos(Vector3 position, Vector3 offset, bool IsOpenOffset)
        {
            float offsetx = UIHelper.GetOffsetX(position);

            var tipsPos = tipIcon.transform.localPosition;
            tipsPos.x = -offsetx;
            tipIcon.transform.localPosition = tipsPos;
       
            Vector3 localPos = content.InverseTransformPoint(position);
            rectTransform.localPosition = new Vector3(localPos.x + offsetx, localPos.y, 0);
            return  position;
        }
    }
}