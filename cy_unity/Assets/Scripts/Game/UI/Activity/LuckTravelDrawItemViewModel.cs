using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;
using System;
using DH.Config;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DH.UIFramework.ViewModels;
using UnityEngine.Scripting;


namespace DH.Game.ViewModels
{
    public partial class LuckTravelDrawItemViewModel : ViewModelBase
    {
        
		[AutoNotify] private CellItemViewModel cellItemViewVm;

        [Preserve]
        public LuckTravelDrawItemViewModel(Reward reward)
        {
            cellItemViewVm = CellItemViewModel.Create(reward);
        }
        
        

        
    }
}