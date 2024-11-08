using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game.ViewModels
{
    public partial class MonthCardAwardShowItemPropertyViewModel : ViewModelBase
    {
        
		[AutoNotify] private string textStr;
        [AutoNotify] private int mIndex;
        public Transform Trans;
        [Preserve]
        public MonthCardAwardShowItemPropertyViewModel(string des ,int index)
        {
            TextStr = des;
            mIndex = index;
        }
        
        

        
    }
}