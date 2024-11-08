

using System;

namespace DH.UIFramework.Reflection
{
    public class ProxyInvoker<T,TResult,TParam> : IInvoker
    {
        private T target;
        private Func<T,TParam,TResult> proxyMethodInfo;
        public ProxyInvoker(T target, Func<T,TParam,TResult> proxyMethodInfo)
        {
            this.target = target;
            this.proxyMethodInfo = proxyMethodInfo;
        }

        public object Invoke(params object[] args)
        {
            return this.proxyMethodInfo.Invoke(this.target, (TParam)args[0]);
        }
    }
}
