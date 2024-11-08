namespace DH.UIFramework.Proxy.Sources.Expressions
{
    public class CompiledExpressionSourceProxyFactory: TypedSourceProxyFactory<CompiledExpressionSourceDescription>
    {
        private ISourceProxyFactory factory;
        public CompiledExpressionSourceProxyFactory(ISourceProxyFactory factory)
        {
            this.factory = factory;
        }

        protected override bool TryCreateProxy(object source, CompiledExpressionSourceDescription description, out ISourceProxy proxy)
        {
            proxy = description.CreateProxy(factory,source);
            if (proxy != null)
                return true;

            return false;
        }
    }
}