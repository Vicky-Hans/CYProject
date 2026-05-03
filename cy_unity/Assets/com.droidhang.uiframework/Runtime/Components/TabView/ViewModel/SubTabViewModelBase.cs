using System;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;

namespace DH.UIFramework
{
    public class SubTabViewModelBase : ViewModelBase
    {
        public bool Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                {
                    return;
                }
                
                Set(ref selected, value);

                if (value && ItemIndex >= 0)
                {
                    selectCallback?.Invoke(ItemIndex);
                }
            }
        }

        
        
        public bool RedDotActive
        {
            get=>redDotActive;
            set
            {
                Set(ref redDotActive, value);
                
            }
        }

        public bool IsUnlock
        {
            get => isUnLock;
            set => Set(ref isUnLock, value);
        }

        public Action<int> UnlockCallback { get; set; }

        public ICommand SelectCmd { get; private set; }
        
        public int ItemIndex 
        {
            get => itemIndex;
            set => Set(ref itemIndex, value);
        }

        private bool selected = false;
        private bool redDotActive = false;
        private bool isUnLock = true;
        private Action<int> selectCallback;
        private int itemIndex;
        
        public SubTabViewModelBase()
        {
            SelectCmd = new SimpleCommand(OnSelect);
            ItemIndex = -1;
        }

        public void SetSelectedCallback(Action<int> selectCallback)
        {
            this.selectCallback = selectCallback;
        }

        private void OnSelect()
        {
            if (!IsUnlock)
            {
                UnlockCallback(ItemIndex);
                return;
            }

            Selected = true;
        }
    }
}