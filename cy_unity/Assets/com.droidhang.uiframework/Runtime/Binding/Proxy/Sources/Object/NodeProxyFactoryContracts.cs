using DH.UIFramework.Paths;

namespace DH.UIFramework.Proxy.Sources.Object
{
    public interface INodeProxyFactory
    {
        ISourceProxy Create(object source, PathToken token);
    }

    public interface INodeProxyFactoryRegister
    {
        void Register(INodeProxyFactory factory, int priority = 100);

        void Unregister(INodeProxyFactory factory);
    }
}
