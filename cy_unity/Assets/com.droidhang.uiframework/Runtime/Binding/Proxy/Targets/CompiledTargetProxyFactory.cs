namespace DH.UIFramework.Proxy.Targets
{
    public class CompiledTargetProxyFactory : ITargetProxyFactory
    {
        public ITargetProxy CreateProxy(object target, BindingDescription description)
        {
            if (description.Target == null)
            {
                return null;
            }

            return description.Target.CreateProxy();
        }
    }
}