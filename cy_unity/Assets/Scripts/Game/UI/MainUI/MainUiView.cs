using System;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DHFramework;
using Extend;
using Game.UI.MainUi;
using UnityEngine;
using UnityEngine.UI;

namespace DH.Game.UIViews
{
    public partial class MainUiView : BaseView
    {
        public override bool FullScreen => true;
        
		public CommonTopView commonTopItems;
        public CommonHeadItemView headView;
        public DhText playerLevelText;
        public Slider expSlider;
        public GameObject centerNode;
		public BottomItemView bottomNode;
        private ETabType tabType = ETabType.TabTypeMainStage;
        public MainStageInfoView mainStageInfo;
        public EquipView equipView;
        public ShopView shopView;
        public RoleView roleView;
        public ActivityView activityView;
        public CommonPlayerNameView commonPlayerNameView;
        public ClothesView clothesView;
        public override async UniTask Create()
        {
            await base.Create();
            var bindingSet = this.CreateBindingSet<MainUiView, MainUiViewModel>();
			bindingSet.Bind(commonTopItems.BindingContext).For(v => v.DataContext).To(vm => vm.CommonTopItemsVm);
			bindingSet.Bind(bottomNode.BindingContext).For(v => v.DataContext).To(vm => vm.BottomNodeVm);
            bindingSet.Bind(headView.BindingContext).For(v => v.DataContext).To(vm => vm.HeadVm);
            bindingSet.Bind(playerLevelText).For(v => v.text).To(vm => vm.PlayerLevelStr);
            bindingSet.Bind(expSlider).For(v => v.value).To(vm => vm.PlayerExpValue);
            bindingSet.Bind(this).For(v => v.TabType).To(vm => vm.MainUI.CurTabType);
            bindingSet.Bind(mainStageInfo.BindingContext).For(v => v.DataContext).To(vm => vm.MainStageInfoVm);
            bindingSet.Bind(mainStageInfo.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.TabTypeMainStage);
            bindingSet.Bind(equipView.BindingContext).For(v => v.DataContext).To(vm => vm.EquipModelVm);
            bindingSet.Bind(equipView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.TabTypeEquip);
            bindingSet.Bind(shopView.BindingContext).For(v => v.DataContext).To(vm => vm.ShopModelVm);
            bindingSet.Bind(shopView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.TabTypeShop);
            // bindingSet.Bind(roleView.BindingContext).For(v => v.DataContext).To(vm => vm.RoleViewVm);
            // bindingSet.Bind(roleView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.Role);
            bindingSet.Bind(activityView.BindingContext).For(v => v.DataContext).To(vm => vm.ActivityViewVm);
            bindingSet.Bind(activityView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.TabTypeActivity);
            bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
            bindingSet.Bind(clothesView.BindingContext).For(v => v.DataContext).To(vm => vm.ClothesViewVm);
            bindingSet.Bind(clothesView.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.MainUI.CurTabType == ETabType.Role);
            bindingSet.Build();
            
            MainUiManager.Instance.CheckIsPopUpDlg();
            AudioManager.Instance.PlayMusic("BGM/cy_bgm_ui",1f,1f);
          
        }
        

        public ETabType TabType
        {
            get => tabType;
            set
            {
                if (tabType == value)
                {
                    return;
                }

                tabType = value;
                UpDateTabInfo(tabType);
            }
        }

        protected override void OnShow()
        {
            MainUiManager.Instance.CheckIsPopUpDlg();
            base.OnShow();
        }

        private void UpDateTabInfo(ETabType type)
        {
            // 这里切换tab 根据需求去 判断是否销毁
        }

        public override bool OnPhysicExit()
        {
            Action cancleFunc = () =>
            {
            };
            Action conformFunc = () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            };
            var content = $"{LocalizeHelper.GetGlobal(GlobalLanguageId.General_tips07) }?";
            CommonMessageBoxViewModel tempVm = CommonMessageBoxViewModel.CreateCommonMsgBox(content,conformFunc,cancleFunc);
            UIManager.Instance.OpenDialog<CommonMessageBox>(tempVm).Forget();
            return true;
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                MainUiManager.Instance.UpDataMailAndDiscord();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                MainUiManager.Instance.UpDataMailAndDiscord();
            }
            else
            {
                // 请求一次体力更新

                MainUiManager.Instance.RequestSyncLives();
            }
        }
    }
}