

namespace DH.UIFramework.Proxy.Sources
{
    public interface ISourceProxyFactory
    {
        ISourceProxy CreateProxy(object source, SourceDescription description);
    }
}
