using System.ComponentModel;
using Cysharp.Threading.Tasks;
using DH.Data;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;

namespace DH.Game.ViewModels
{
    public partial class PlayerInfoViewModel : ViewModelBase
    {
		public CommonHeadItemViewModel headVm;
		public DigestData Digest => DataCenter.charcaterData.Digest;
		public MainStageData MainStageData => DataCenter.mainStageData;
		
		[AutoNotify] private CommonPlayerNameViewModel commonPlayerNameVm;
        [Preserve]
        public PlayerInfoViewModel()
        {
	        InitUI();
	        Digest.PropertyChanged += NameChange;
        }

        private void InitUI()
        {
	        CommonHeadData headData = new(DataCenter.charcaterData.Digest.HeadId, DataCenter.charcaterData.Digest.HeadFrame,() =>
	        {
		        UIManager.Instance.OpenDialog<AvatarView>(new AvatarViewModel()).Forget();
		        
	        });
	        if (headVm == null)
	        {
		        headVm = new CommonHeadItemViewModel(headData, true,true);
	        }
	        else
	        {  
		        headVm.UpdatePanel(headData);
	        }

	        InitPlayerName();
        }

        void InitPlayerName()
        {
	        if (CommonPlayerNameVm ==null)
	        {
		        CommonPlayerNameVm = new CommonPlayerNameViewModel(UIHelper.GetStringByLength(DataCenter.charcaterData.Digest.Name),UIHelper.HexColorStrToColor("#6d4f3a"), 
			        DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname));
	        }
	        else
	        {
		        CommonPlayerNameVm.InitUI(UIHelper.GetStringByLength(DataCenter.charcaterData.Digest.Name),UIHelper.HexColorStrToColor("#6d4f3a"),
			        DataCenter.monthCardData.MonthCardFuncIsOpen(MonthCardEffectType.GoldenNickname));
	        }
        }

        private void NameChange(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName == nameof(Digest.Name))
	        {
		        InitPlayerName();
	        }
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<PlayerInfoView>();
        }

        [Command]
        private void OnClickChangeNameBtn()
        {
	        UIManager.Instance.OpenDialog<RenameView,RenameViewModel>().Forget();
        }
        protected override void OnDispose()
        {
	        Digest.PropertyChanged -= NameChange;
	        base.OnDispose();
        }
    }
}