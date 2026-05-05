using System;

namespace DH.UIFramework.Proxy.Sources
{
    public interface ISourceProxy : IBindingProxy
    {
        Type Type { get; }

        TypeCode TypeCode { get; }

        object Source { get; }
    }

    public interface ISourceProxyFactory
    {
        ISourceProxy CreateProxy(object source, SourceDescription description);
    }

    public interface ISourceProxyFactoryRegistry
    {
        void Register(ISourceProxyFactory factory, int priority = 100);

        void Unregister(ISourceProxyFactory factory);
    }
}
