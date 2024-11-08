using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using DG.Tweening;

namespace DH.Game.UIViews
{
    public partial class EquipBattleItemView : BaseItemView
    {
        public override bool FullScreen => false;

        public GameObject noneNode;
        public GameObject itemNode;
        public GameObject lockNode;
        public DhText lockText;
        public EquipItemView equipItemView;
        
        private Tween animationTween; 
        public bool IsReplace
        {
            get => false;
            set
            {
                PlayReplaceAnimator(value);
            }
        }

        public override async UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<EquipBattleItemView, EquipBattleItemViewModel>();
            bindSet.Bind(noneNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsOwn);
            bindSet.Bind(itemNode).For(v => v.activeSelf).To(vm => vm.IsOwn);
            bindSet.Bind(lockNode).For(v => v.activeSelf).To(vm => vm.IsLock);
            bindSet.Bind(lockText).For(v => v.text).ToExpression(vm => GetLockDesc(vm.Cfg));
            bindSet.Bind(this).For(v => v.IsReplace).To(vm => vm.IsReplace);
            bindSet.Bind(equipItemView.BindingContext).For(v => v.DataContext).To(vm => vm.ItemViewModel);
            bindSet.Bind(this).For(v => v.equipItemView).To(vm => vm.EquipItemViewObj).OneWayToSource();
            bindSet.Build();
        }

        private string GetLockDesc(EquipSlotsCfg cfg)
        {
            return LocalizeHelper.GetGlobal(GlobalLanguageId.Equip_04,cfg?.Unlock); // $"通过章节{cfg.Unlock}解锁";
        }

        private void PlayReplaceAnimator(bool isReplace)
        {
            if (animationTween != null) animationTween.Kill();
            if (isReplace)
            {
                equipItemView.transform.localScale = Vector3.one;
                animationTween = equipItemView.transform.DOScale(0.9f, 0.8f).SetLoops(-1, LoopType.Yoyo).OnComplete(() => {
                    equipItemView.transform.localScale = Vector3.one;
                });  
            }
            else
            {
                equipItemView.transform.localScale = Vector3.one;
            }
        }
    }
}