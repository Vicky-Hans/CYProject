using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;



namespace DH.Game.ViewModels
{
    public partial class GmSelectInfoItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private string descStr;

        [Preserve]
        public GmSelectInfoItemViewModel(string desc)
        {
            DescStr = desc;
        }
    }
}