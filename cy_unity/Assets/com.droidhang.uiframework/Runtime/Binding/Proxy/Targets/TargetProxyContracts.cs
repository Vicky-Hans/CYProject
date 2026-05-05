using System;

namespace DH.UIFramework.Proxy.Targets
{
    public interface ITargetProxy : IBindingProxy
    {
        Type Type { get; }

        TypeCode TypeCode { get; }

        object Target { get; }

        BindingMode DefaultMode { get; }
    }

    public interface ITargetProxyFactory
    {
        ITargetProxy CreateProxy(object target, BindingDescription description);
    }

    public interface ITargetProxyFactoryRegister
    {
        void Register(ITargetProxyFactory factory, int priority = 100);

        void Unregister(ITargetProxyFactory factory);
    }
}
