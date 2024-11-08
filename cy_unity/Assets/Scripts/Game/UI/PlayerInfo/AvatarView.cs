using DH.Config;
using DH.Data;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using Extend;
using UnityEngine;

namespace DH.Game.UIViews
{
    public partial class AvatarView : BaseView
    {
        public CommonHeadItemView headView;
        public DhButton closeBtn;
        public UICircularScrollView headList;
        public UICircularScrollView headFrameList;
        public DhButton headBtn;
        public DhButton headFrameBtn;
        public DhText descTxt;
        [AssetPath] public string prefabPath;
        
        public GameObject headBtnOn;
        public GameObject frameBtnOn;
        public GameObject notHeadBtnOn;
        public GameObject notFrameBtnOn;

        public DhButton useBtn;

        public GameObject headRedDot;
        public GameObject headFrameRedDot;

        public GameObject usingDseGo;
        public GameObject lockDseGo;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
            await base.Create();
            headList.PrefabPath = prefabPath;
            headFrameList.PrefabPath = prefabPath;
            var bindSet = this.CreateBindingSet<AvatarView, AvatarViewModel>();
            bindSet.Bind(headView.BindingContext).For(v=>v.DataContext).To(vm=>vm.HeadVm);
            bindSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClose);
            bindSet.Bind(headList).For(v => v.Collection).To(vm => vm.HeadModels);
            bindSet.Bind(headFrameList).For(v => v.Collection).To(vm => vm.FrameModels);
            
            bindSet.Bind(headBtn).For(v => v.onClick).To(vm => vm.OnClickHeadTab);
            bindSet.Bind(headFrameBtn).For(v => v.onClick).To(vm => vm.OnClickFrameTab);
            bindSet.Bind(this).For(v => v.headList).To(vm => vm.HeadListView).OneWayToSource();
            bindSet.Bind(this).For(v => v.headFrameList).To(vm => vm.HeadFrameListView).OneWayToSource();
            
            bindSet.Bind(headList.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 0);
            bindSet.Bind(headFrameList.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 1);
            bindSet.Bind(headBtnOn).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 0);
            bindSet.Bind(frameBtnOn).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 1);
            bindSet.Bind(notHeadBtnOn).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 1);
            bindSet.Bind(notFrameBtnOn).For(v => v.activeSelf).ToExpression(vm => vm.CurTab == 0);
            
            bindSet.Bind(usingDseGo).For(v => v.activeSelf).ToExpression(vm => vm.BtnState == EPlayerInfoBtnState.Use);
            bindSet.Bind(lockDseGo).For(v => v.activeSelf).ToExpression(vm => vm.BtnState == EPlayerInfoBtnState.Lock);
            
            bindSet.Bind(descTxt).For(v => v.text).ToExpression(vm => GetDesc(vm.CurId));
            bindSet.Bind(useBtn).For(v => v.onClick).To(vm => vm.OnClickUsetBtn);
            bindSet.Bind(useBtn.gameObject).For(v => v.activeSelf).ToExpression(vm => vm.BtnState == EPlayerInfoBtnState.Free);
            bindSet.Bind(headRedDot).For(v => v.activeSelf).To(vm => vm.ShowHeadRedDot);
            bindSet.Bind(headFrameRedDot).For(v => v.activeSelf).To(vm => vm.ShowHeadFrameRedDot);
            bindSet.Build();
        }

        private string GetDesc(int id)
        {
            var cfgId = DataCenter.charcaterData.GetHeadCfgId(id);
            var cfg = ConfigCenter.ProPictureLanguageCfgColl.GetDataById(cfgId);
            var seasonId = DataCenter.charcaterData.GetHeadSeasonById(id);
            if (cfg == null) return "";
            return string.Format(cfg.Description, $"S{seasonId}");
        }
    }
}