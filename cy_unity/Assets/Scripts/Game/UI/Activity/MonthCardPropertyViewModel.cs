using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class MonthCardPropertyViewModel : ViewModelBase
    {
        
		[AutoNotify] private string textStr;

        [Preserve]
        public MonthCardPropertyViewModel(string des)
        {
            TextStr = des;
        }
    }
}