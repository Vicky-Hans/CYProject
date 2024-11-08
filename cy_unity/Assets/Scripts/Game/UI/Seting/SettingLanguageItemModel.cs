using DH.UIFramework;
using DH.UIFramework.Commands;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;

namespace DH.Game.ViewModels
{
    public partial class SettingLanguageItemModel : ViewModelBase
    {
        private bool selected;
        [AutoNotify] private string code;
        [AutoNotify] private string languageTxt;
        [Preserve]
        public SettingLanguageItemModel()
        {
        }
        public ICommand OnClickCmd { get; set; }

        public bool Selected
        {
            get => selected;
            set
            {
                if (!Set(ref selected, value)) return;
            }
        }
    }
}