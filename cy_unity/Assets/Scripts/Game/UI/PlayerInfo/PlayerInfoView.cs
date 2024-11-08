using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews.ItemViews;
using DH.Game.ViewModels;
using DH.NativeCore.Platform;
using DH.UIFramework;
using UnityEngine.UI;
using Extend;

namespace DH.Game.UIViews
{
    public partial class PlayerInfoView : BaseView
    {
        public override bool FullScreen => false;
        public CommonHeadItemView headView;
		public DhButton closeBtn;
		public DhButton changeNameBtn;
		public Slider slider;
		public DhText level;
		public DhText exp;
		public DhText pidNums;
		public DhButton pidBut;
		public DhText levelNums;
		public DhText levelWaveNums;
		public CommonPlayerNameView commonPlayerNameView;
        public override async Cysharp.Threading.Tasks.UniTask Create()
        {
	        await base.Create();
            var bindingSet = this.CreateBindingSet<PlayerInfoView, PlayerInfoViewModel>();
            bindingSet.Bind(headView.BindingContext).For(v => v.DataContext).To(vm => vm.headVm);
			bindingSet.Bind(closeBtn).For(v => v.onClick).To(vm => vm.OnClickCloseBtnCommand);
			bindingSet.Bind(changeNameBtn).For(v => v.onClick).To(vm => vm.OnClickChangeNameBtnCommand);
			bindingSet.Bind(pidNums).For(v => v.text).To(vm => vm.Digest.RoleId);
			bindingSet.Bind(levelNums).For(v => v.text).To(vm => vm.MainStageData.CurrChapter);
			bindingSet.Bind(levelWaveNums).For(v => v.text).To(vm => vm.MainStageData.LevelWave());

			bindingSet.Bind(slider).For(v => v.value).ToExpression(vm => ExpValue(vm.Digest.Exp, vm.Digest.Lv));
			bindingSet.Bind(exp).For(v => v.text).ToExpression(vm => GetExpTxt(vm.Digest.Exp, vm.Digest.Lv));
			bindingSet.Bind(level).For(v => v.text).ToExpression(vm => $"Lv.{vm.Digest.Lv}");
			bindingSet.Bind(commonPlayerNameView.BindingContext).For(v => v.DataContext).To(vm => vm.CommonPlayerNameVm);
            bindingSet.Build();
            
            pidBut.onClick.AddListener(CopyTextToClipboard);
        }
        
        public string GetExpTxt(long exp, int lv)
        {
	        var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(lv);
	        if (cfg == null)//判断为满级
	        {
		        return "";
	        }
	        return $"{exp}/{cfg.Exp}";
        }
        
        
        public float ExpValue(long exp, int lv)
        {
	        var cfg = ConfigCenter.ProLevelCfgColl.GetDataById(lv);
	        if (cfg == null) return 1;
	        var value = (float)exp / cfg.Exp;
	        return value;
        }
        
        public void CopyTextToClipboard()
        {
	        DeviceUtility.CopyToClipboard($"{DataCenter.charcaterData.Digest.RoleId}");
	        var str = LocalizeHelper.GetGlobal(GlobalLanguageId.GiftCode_tips06);
	        ToastManager.Show(str);
        }
        
    }
}