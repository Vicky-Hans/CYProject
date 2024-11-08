using System;
using DH.UIFramework.Observables;

namespace DH.Game.UIViews
{
    public partial class DebugCellViewModel: ObservableObject
    {

        private string desc;
        private Action callback;
        public DebugCellViewModel(string title, Action callfunc)
        {
            desc = title;
            callback = callfunc;
        }

        public string Desc => desc;

        public void OnClickBtn()
        {
            callback();
        }
    }
}