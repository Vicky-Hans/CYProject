using System.Collections.Specialized;
using System.ComponentModel;

namespace DH.UIFramework.Observables
{
    public interface INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }
}