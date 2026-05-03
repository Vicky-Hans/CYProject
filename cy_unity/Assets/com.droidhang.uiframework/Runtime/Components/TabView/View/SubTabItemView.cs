using DH.UIFramework.Builder;
using DH.UIFramework.Commands;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DH.UIFramework
{
    public partial class SubTabItemView : BaseItemView
    {
        public Button tabButton;

        public bool Selected
        {
            get => selected;
            set
            {
                lastState = selected;
                selected = value;

                if (!lastState && selected)
                {
                    OnSelect();
                }
                else if (lastState && !selected)
                {
                    OnUnSelect();
                }
            }
        }

        private bool selected;
        private bool lastState;
        
        public void BindTabItemView<TBehaviour, TSource>(BindingSet<TBehaviour, TSource> bindingSet)
            where TBehaviour : SubTabItemView
            where TSource: SubTabViewModelBase
        {
            OnUnSelect(true);
            
            bindingSet.Bind(tabButton).For(v => v.onClick).To(vm => vm.SelectCmd);
            bindingSet.Bind(this).For(v => v.Selected).To(vm => vm.Selected);           
        }

        protected virtual void OnSelect()
        {
            
        }

        protected virtual void OnUnSelect(bool init = false)
        {
            
        }
    }
}