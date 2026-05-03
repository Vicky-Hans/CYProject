using System;
using DH.UIFramework.Builder;
using DH.UIFramework.Observables;
using UnityEngine;
using UnityEngine.Events;

namespace DH.UIFramework
{
    public partial class TabGroupView : MonoBehaviour
    {
        [Serializable]
        public class OnChangeEvent : UnityEvent<int> { }
        
        /// <summary>
        /// Event delegates triggered when the input field changes its data.
        /// </summary>
        [SerializeField]
        private OnChangeEvent m_OnValueChanged = new OnChangeEvent();
        
        public OnChangeEvent onValueChanged { 
            get => m_OnValueChanged;
            set => m_OnValueChanged = value;
        }
        
        public delegate void SelectChangeCallback(int curIdx, int lastIdx);

        public int CurIndex
        {
            get => curIndex;
            set
            {
                curIndex = value;
                SetTabSelect(value);
            }
        }

        /// <summary>
        /// 内部使用，外部不要使用该变量，应该使用CurIndex
        /// </summary>
        internal int SelectIndex
        {
            get => selectIndex;
            set
            {
                lastIndex = selectIndex;
                selectIndex = value;
                curIndex = value;
                
                selectChangeCallback?.Invoke(selectIndex, lastIndex);
                onValueChanged?.Invoke(selectIndex);
            }
        }

        private SelectChangeCallback selectChangeCallback;
        private int selectIndex = 0;
        private int curIndex = 0;
        private int lastIndex = -1;
        private int targetIndex = -1;
        private TabGroupViewModel tabGroupVM;

        /// <summary>
        /// 有固定的tablist
        /// </summary>
        /// <param name="tabList"></param>
        /// <param name="changeCallback"></param>
        /// <param name="subTabVmList"></param>
        /// <typeparam name="TView"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        public void Init<TView, TSource>(TView[] tabList, SelectChangeCallback changeCallback, int defaultIdx = 0,
            ObservableList<TSource> subTabVmList = null) 
            where TView : SubTabItemView
            where TSource : SubTabViewModelBase, new()
        {
            if (defaultIdx < 0 || defaultIdx >= tabList.Length)
            {
                defaultIdx = 0;
            }
            
            SetAnimCallback(changeCallback);
            
            tabGroupVM = new TabGroupViewModel();

            if (subTabVmList == null)
            {
                subTabVmList = new ObservableList<TSource>();
                for (int i = 0; i < tabList.Length; ++i)
                {
                    subTabVmList.Add(new TSource());
                }
            }
            
            tabGroupVM.SubTabList.AddRange(subTabVmList);
            tabGroupVM.SubTabList[defaultIdx].Selected = true;
            
            for(int i = 0; i < tabList.Length; ++i)
            {
                var context = tabList[i].GetComponent<BaseItemView>();
                context.SetDataContext(tabGroupVM.SubTabList[i]);
            }
            
            this.SetDataContext(tabGroupVM);

            InitializeBinding();
        }

        /// <summary>
        /// 内部组件手写绑定代码必须使用CreateInternalBindingSet和CompileBuild函数
        /// </summary>
        private void InitializeBinding()
        {
            BindingSet<TabGroupView, TabGroupViewModel> bindingSet = this.CreateBindingSet<TabGroupView, TabGroupViewModel>();
            {
                var builder = bindingSet.CompileCreate(this);
                var targetDesc = new DH.UIFramework.Proxy.Targets.TargetDescription<TabGroupView,int>("SelectIndex",(TabGroupView)builder.Target,t=>t.SelectIndex,(t, v) => t.SelectIndex=v,null);
                builder.Description.Target = targetDesc;
                var sourceDesc = new DH.UIFramework.Proxy.Sources.Object.ObjectSourceDescription();
                var path = new DH.UIFramework.Paths.Path();
                builder.Description.Source = sourceDesc;
                sourceDesc.Path = path;
                path.Append(new DH.UIFramework.Paths.CompiledMemberNode<TabGroupViewModel,int>("SelectIndex", null, vm => vm.SelectIndex));
                builder.Description.Mode = DH.UIFramework.BindingMode.OneWay;
            }
            bindingSet.Build();
        }
        
        /// <summary>
        /// 选中目标tab
        /// </summary>
        /// <param name="targetIndex"></param>
        public void SetTabSelect(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= tabGroupVM.SubTabList.Count)
            {
                return;
            }

            tabGroupVM.SubTabList[targetIndex].Selected = true;
        }

        private void SetAnimCallback(SelectChangeCallback changeCallback)
        {
            this.selectChangeCallback = changeCallback;
        }

        public void UpdateRedDot(int tabIndex, bool active)
        {
            tabGroupVM.SubTabList[tabIndex].RedDotActive = active;
        }
        public void UpdateUnlockState(int tabIndex, bool active)
        {
            tabGroupVM.SubTabList[tabIndex].IsUnlock = active;
        }

        public void UnlockCallback(int tabIndex,Action<int> callback)
        {
            tabGroupVM.SubTabList[tabIndex].UnlockCallback = callback;
        }

    }
}