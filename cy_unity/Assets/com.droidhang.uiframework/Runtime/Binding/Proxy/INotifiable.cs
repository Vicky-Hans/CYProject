

using System;

namespace DH.UIFramework.Proxy
{

    public interface INotifiable
    {
        event EventHandler ValueChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }
}
