using System;

namespace DH.UIFramework
{
    public interface IInvoker
    {
        object Invoke(params object[] args);
    }
}

namespace DH.UIFramework.Proxy
{
    public interface IBindingProxy : IDisposable
    {
    }

    public interface IObtainable
    {
        object GetValue();

        TValue GetValue<TValue>();
    }

    public interface IModifiable
    {
        void SetValue(object value);

        void SetValue<TValue>(TValue value);
    }

    public interface INotifiable
    {
        event EventHandler ValueChanged
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }
    }

    /// <summary>
    /// Supports Lua Function.
    /// </summary>
    public interface IScriptInvoker : DH.UIFramework.IInvoker
    {
    }
}
