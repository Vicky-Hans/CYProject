using Cysharp.Threading.Tasks;
using DG.Tweening;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class EquipItemView : BaseItemView
    {
        public override bool FullScreen => true;
        public RectTransform bgRectTf;
        public DhImage bgImage;
        public DhImage icon;
        public DhText name;
        public DhText level;
        public DhImage typeIcon;
        public DhImage attrIcon;
        public GameObject baseNode;
        public GameObject infoNode;
        public Slider slider;
        public DhText progress;
        public GameObject upState;
        public GameObject upUpState;
        public GameObject upUpStateImage;

        
        public DhButton btnInfo;
        public DhButton btnUse;
        public DhButton btnDischarge;
        public DhButton btnOpenInfo;
        private bool isLock = false;
        public bool IsLock
        {
            get => isLock;
            set
            {
                if (isLock != value)
                {
                    isLock = value;
                    SetLock(isLock);
                }
            }
        }
        
        private bool selectAnimatorIng = false;
        public bool SelectAnimatorIng
        {
            get => selectAnimatorIng;
            set
            {
                if (selectAnimatorIng != value)
                {
                    selectAnimatorIng = value;
                    selectIng = value;
                    PlayBgDoTween(SelectAnimatorIng);
                }
            }
        }
        private bool selectIng = false;
        public bool SelectIng
        {
            get => selectIng;
            set
            {
                if (selectIng != value)
                {
                    selectIng = value;
                    selectAnimatorIng = value;
                    SetBgSize(selectIng);
                }
            }
        }
        
        private bool isEnoughAttr = false;
        public bool IsEnoughAttr
        {
            get => isEnoughAttr;
            set
            {
                if (isEnoughAttr != value)
                {
                    isEnoughAttr = value;
                    SetAttrGray(IsEnoughAttr);
                }
            }
        }

        public override async UniTask Create()
        {
            await base.Create();
            var bindSet = this.CreateBindingSet<EquipItemView, EquipItemViewModel>();
            bindSet.Bind(this).For(v => v.IsLock).To(vm => vm.IsLock);
            bindSet.Bind(bgRectTf.gameObject).For(v => v.activeSelf).To(vm => vm.IsShow);
            bindSet.Bind(this).For(v => v.SelectIng).To(vm => vm.IsSelectIng);
            bindSet.Bind(this).For(v => v.SelectAnimatorIng).To(vm => vm.IsSelectAnimatorIng);
            bindSet.Bind(bgImage).For(v => v.sprite).To(vm => vm.BgPath).WithConversion(this);
            bindSet.Bind(icon).For(v => v.sprite).To(vm => vm.IconPath).WithConversion(this);
            bindSet.Bind(name).For(v => v.text).To(vm => vm.Name);
            bindSet.Bind(level).For(v => v.text).ToExpression(vm => GetLevel(vm.Level));
            bindSet.Bind(typeIcon).For(v => v.sprite).To(vm => vm.TypeIconPath).WithConversion(this);
            //bindSet.Bind(attrIcon).For(v => v.sprite).To(vm => vm.AttrIconPath).WithConversion(this);
            bindSet.Bind(attrIcon.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.AttrNum>1);
            bindSet.Bind(this).For(v => v.IsEnoughAttr).To(vm => vm.IsEnoughAttr);
            bindSet.Bind(baseNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelectIng);
            bindSet.Bind(infoNode).For(v => v.activeSelf).To(vm => vm.IsSelectIng);
            
            bindSet.Bind(baseNode).For(v => v.activeSelf).ToExpression(vm => !vm.IsSelectAnimatorIng);
            bindSet.Bind(infoNode).For(v => v.activeSelf).To(vm => vm.IsSelectAnimatorIng);
            bindSet.Bind(slider).For(v => v.value).ToExpression(vm => GetProgressValue(vm.OwnItemNum,vm.NeedItemNum,vm.IsMaxLevel));
            bindSet.Bind(progress).For(v => v.text).ToExpression(vm => GetProgrss(vm.OwnItemNum,vm.NeedItemNum,vm.IsMaxLevel));
            bindSet.Bind(upState).For(v => v.activeSelf).ToExpression(vm =>!vm.IsMaxLevel && vm.NeedItemNum!=0 && vm.OwnItemNum>=vm.NeedItemNum);
            bindSet.Bind(upUpState).For(v => v.activeSelf).ToExpression(vm =>!(vm.NeedItemNum!=0 && vm.OwnItemNum>=vm.NeedItemNum && vm.IsEnouthUpItem));
            bindSet.Bind(upUpStateImage).For(v => v.activeSelf).ToExpression(vm =>!vm.IsMaxLevel && vm.IsEnouthUpItem);
            bindSet.Bind(btnUse.gameObject).For(v => v.activeSelf).ToExpression(vm => !vm.IsUse);
            bindSet.Bind(btnDischarge.gameObject).For(v => v.activeSelf).To(vm => vm.IsUse);
            bindSet.Bind(btnDischarge).For(v => v.onClick).To(vm => vm.OnClickDischargeCommand);
            bindSet.Bind(btnInfo).For(v => v.onClick).To(vm => vm.OnClickInfoCommand);
            bindSet.Bind(btnUse).For(v => v.onClick).To(vm => vm.OnClickUseCommand);
            bindSet.Bind(btnOpenInfo).For(v => v.onClick).To(vm => vm.OnClickOpenInfoCommand);
            
            bindSet.Build();
        }

        private float GetProgressValue(long vmOwnItemNum,int vmNeedItemNum, bool vmIsMaxLevel)
        {
            return vmIsMaxLevel ? 1f : vmOwnItemNum==0?0:(float)vmOwnItemNum/vmNeedItemNum;
        }

        private string GetLevel(int vmLevel)
        {
            return $"Lv.{vmLevel}";
        }

        private string GetProgrss(long vmOwnItemNum, int vmNeedItemNum,bool isMax)
        {
            return isMax ? "Max" : $"{vmOwnItemNum}/{vmNeedItemNum}";
        }

        private void PlayBgDoTween(bool select)
        {
            float startValue;
            float endValue;
            if (select)
            {
                startValue = 304;
                endValue = 394;
            }
            else
            {
                startValue = 393;
                endValue = 304;
            }

            // 使用 DOTween 创建从 startValue 到 endValue 的变化事件
            DOTween.To(() => startValue, value => {
                // 在每次更新时执行的操作
                Debug.Log("Current value: " + value);
                bgRectTf.sizeDelta = new Vector2(216, value);
            }, endValue, 0.1f).OnComplete(() => {
                // 动画完成时执行的操作
                Debug.Log("Animation complete!");
                bgRectTf.sizeDelta = new Vector2(216, endValue);
            });
        }
        
        private void SetBgSize(bool select)
        {
            bgRectTf.sizeDelta = new Vector2(216, select?393:304);
        }

        private void SetLock(bool lockState)
        {
            //UIHelper.SetGray(bgRectTf.gameObject,lockState,true);
        }

        private void SetAttrGray(bool b)
        {
            UIHelper.SetGray(attrIcon.gameObject,b,false);
        }
    }
}