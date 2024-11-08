using Cysharp.Threading.Tasks;
using DH.Game.UIViews;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using DHFramework.Localization;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class SelectLanguageViewModel : ViewModelBase
    {
    
	    #region 多语言相关
        public ObservableList<SettingLanguageItemModel> LanguageItems = new();
        public PlayerInfoManager Manager => PlayerInfoManager.Instance;
        private int clickServerCount;
        private string curLanguageCode;
        
        public string CurLanguageCode
        {
        	get => curLanguageCode;
        	set
        	{
        		if (!Set(ref curLanguageCode, value)) return;
        		//languageTitleStr = supportLanguage[curLanguageCode];
        	}
        }
        
        private void InitLanguage()
        {
        	var currentCode = Localization.GetCurrentLanguage();
        	foreach (var kv in Manager.supportLanguage)
        	{
        		var languageModel = new SettingLanguageItemModel();
        		languageModel.Code = kv.Key;
        		languageModel.LanguageTxt = kv.Value;
        		languageModel.Selected = kv.Key == currentCode;
        		languageModel.OnClickCmd = new AsyncCommand(async () =>
        		{
        			await OnSelectLanguage(languageModel);
        		});
        		LanguageItems.Add(languageModel);
        	}

        	clickServerCount = 0;
        }
        private async UniTask OnSelectLanguage(SettingLanguageItemModel model)
        {

	        if (model.Selected)return;
        	foreach (var item in LanguageItems)
        	{
        		item.Selected = false;
        	}

        	model.Selected = true;
        	CurLanguageCode = model.Code;
        	await Localization.ChangeLanguageAsync(model.Code);
        	OnClickCloseBtn();
	        if (UIManager.Instance.IsOpen<SystemSettingView>())
		        UIManager.Instance.CloseDialog<SystemSettingView>();
	        if (UIManager.Instance.IsOpen<FunctionMenuView>())
		        UIManager.Instance.CloseDialog<FunctionMenuView>();
        	ProcedureManager.Instance.ChangeAsync(nameof(MainGameProcedure), true).Forget();
        }

        #endregion
	    

        [Preserve]
        public SelectLanguageViewModel()
        {
	        InitLanguage();
        }

        [Command]
        private void OnClickCloseBtn()
        {
	        UIManager.Instance.CloseDialog<SelectLanguageView>();
        }
        
    }
}