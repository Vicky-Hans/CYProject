
using System;
using System.Collections.Generic;

namespace DH.UIFramework.Services
{
    public class ServiceContainer : IServiceContainer, IDisposable
    {
        private Dictionary<string, IFactory> services = new Dictionary<string, IFactory>();

        public virtual object Resolve(Type type)
        {
            return this.Resolve<object>(GetServiceName(type));
        }

        public virtual T Resolve<T>()
        {
            return this.Resolve<T>(GetServiceName(typeof(T)));
        }

        public virtual object Resolve(string name)
        {
            return this.Resolve<object>(name);
        }

        public virtual T Resolve<T>(string name)
        {
            IFactory factory;
            if (this.services.TryGetValue(name, out factory))
                return (T)factory.Create();
            return default(T);
        }

        public virtual void Register<T>(Func<T> factory)
        {
            this.Register<T>(GetServiceName(typeof(T)), factory);
        }

        public virtual void Register(Type type, object target)
        {
            this.Register<object>(GetServiceName(type), target);
        }

        public virtual void Register(string name, object target)
        {
            this.Register<object>(name, target);
        }

        public virtual void Register<T>(T target)
        {
            this.Register<T>(GetServiceName(typeof(T)), target);
        }

        public virtual void Register<T>(string name, Func<T> factory)
        {
            if (this.services.ContainsKey(name))
                throw new DuplicateRegisterServiceException(string.Format("Duplicate key {0}", name));

            this.services.Add(name, new GenericFactory<T>(factory));
        }

        public virtual void Register<T>(string name, T target)
        {
            if (this.services.ContainsKey(name))
                throw new DuplicateRegisterServiceException(string.Format("Duplicate key {0}", name));

            this.services.Add(name, new SingleInstanceFactory(target));
        }

        public virtual void Unregister(Type type)
        {
            this.Unregister(GetServiceName(type));
        }

        public virtual void Unregister<T>()
        {
            this.Unregister(GetServiceName(typeof(T)));
        }

        public virtual void Unregister(string name)
        {
            IFactory factory;
            if (this.services.TryGetValue(name, out factory))
                factory.Dispose();

            this.services.Remove(name);
        }

        protected virtual string GetServiceName(Type type)
        {
            if (type.IsGenericType)
                return type.ToString();
            return type.Name;
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var kv in services)
                        kv.Value.Dispose();

                    this.services.Clear();
                }
                disposed = true;
            }
        }

        ~ServiceContainer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        internal interface IFactory : IDisposable
        {
            object Create();
        }

        internal class GenericFactory<T> : IFactory
        {
            private Func<T> func;

            public GenericFactory(Func<T> func)
            {
                this.func = func;
            }

            public virtual object Create()
            {
                return func();
            }

            public void Dispose()
            {
            }
        }

        internal class SingleInstanceFactory : IFactory
        {
            private object target;

            public SingleInstanceFactory(object target)
            {
                this.target = target;
            }

            public virtual object Create()
            {
                return target;
            }

            #region IDisposable Support
            private bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        var disposable = target as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                        this.target = null;
                    }

                    disposed = true;
                }
            }

            ~SingleInstanceFactory()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
