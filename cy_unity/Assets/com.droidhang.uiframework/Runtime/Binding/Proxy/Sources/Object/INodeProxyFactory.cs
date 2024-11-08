

using DH.UIFramework.Paths;

namespace DH.UIFramework.Proxy.Sources.Object
{
    public interface INodeProxyFactory
    {
        ISourceProxy Create(object source, PathToken token);
    }
}
