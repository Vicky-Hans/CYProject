

namespace DH.UIFramework.Proxy.Targets
{
    public interface ITargetProxyFactory
    {
        ITargetProxy CreateProxy(object target, BindingDescription description);
    }
}
