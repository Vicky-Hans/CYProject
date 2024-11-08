

using DH.UIFramework.Paths;
using DH.UIFramework.Reflection;
using DH.UIFramework.Interactivity;
using DH.UIFramework.Observables;
using System;
using System.Collections;
using System.Reflection;
using UIFramework.Binding;

namespace DH.UIFramework.Proxy.Sources.Object
{
    public class UniversalNodeProxyFactory : INodeProxyFactory
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(UniversalNodeProxyFactory));

        public ISourceProxy Create(object source, PathToken token)
        {
            IPathNode node = token.Current;
            if (source == null && !node.IsStatic)
                return null;

            if (node.IsStatic)
                throw new Exception("Not support now");

            return CreateProxy(source, node);
        }

        protected virtual ISourceProxy CreateProxy(object source, IPathNode node)
        {
            if (node is ICompiledNode compiledNode)
            {
                return new PropertyNodeProxy(source,compiledNode.CreatePropertyProxy());
            }
            
            if (node is IndexedNode indexedNode)
            {
                return indexedNode.CreateSourceProxy(source);
            }
            
            return null;
        }
    }
}
