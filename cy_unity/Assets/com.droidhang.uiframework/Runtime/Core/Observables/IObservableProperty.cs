

using System;

namespace DH.UIFramework.Observables
{
    public interface IObservableProperty
    {
        event EventHandler ValueChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        Type Type { get; }

        object Value { get; set; }
    }

    public interface IObservableProperty<T> : IObservableProperty
    {
        new T Value { get; set; }
    }
}
