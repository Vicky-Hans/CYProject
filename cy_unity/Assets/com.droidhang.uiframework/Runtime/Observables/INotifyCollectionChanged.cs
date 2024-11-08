using System.Collections.Specialized;

namespace DH.UIFramework.Observables
{
    public interface INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }
}