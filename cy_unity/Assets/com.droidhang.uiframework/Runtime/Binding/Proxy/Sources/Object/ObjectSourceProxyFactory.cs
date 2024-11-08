

using DH.UIFramework.Paths;
using System.Collections.Generic;

namespace DH.UIFramework.Proxy.Sources.Object
{
    public class ObjectSourceProxyFactory : TypedSourceProxyFactory<ObjectSourceDescription>, INodeProxyFactory, INodeProxyFactoryRegister
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ObjectSourceProxyFactory));

        private List<PriorityFactoryPair> factories = new List<PriorityFactoryPair>();

        protected override bool TryCreateProxy(object source, ObjectSourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            var path = description.Path;
            if (path.Count <= 0)
                throw new ProxyException("The path nodes of the ObjectSourceDescription \"{0}\" is empty.", description.ToString());

            PathToken token = path.AsPathToken();

            if (path.Count == 1)
            {
                proxy = this.Create(source, token);
                if (proxy != null)
                    return true;
                return false;
            }

            proxy = new ChainedObjectSourceProxy(source, token, this);
            return true;
        }

        public virtual ISourceProxy Create(object source, PathToken token)
        {
            ISourceProxy proxy = null;
            foreach (PriorityFactoryPair pair in this.factories)
            {
                var factory = pair.factory;
                if (factory == null)
                    continue;

                proxy = factory.Create(source, token);
                if (proxy != null)
                    return proxy;
            }
            return proxy;
        }

        public virtual void Register(INodeProxyFactory factory, int priority = 100)
        {
            if (factory == null)
                return;

            this.factories.Add(new PriorityFactoryPair(factory, priority));
            this.factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public virtual void Unregister(INodeProxyFactory factory)
        {
            if (factory == null)
                return;

            this.factories.RemoveAll(pair => pair.factory == factory);
        }

        struct PriorityFactoryPair
        {
            public PriorityFactoryPair(INodeProxyFactory factory, int priority)
            {
                this.factory = factory;
                this.priority = priority;
            }
            public int priority;
            public INodeProxyFactory factory;
        }
    }
}
