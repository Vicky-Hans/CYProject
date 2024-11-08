using System;
using DH.UIFramework.Reflection;
using UnityEngine.Events;

namespace DH.UIFramework.Proxy.Targets
{
    [Serializable]
    public abstract class TargetDescription
    {
        public abstract ITargetProxy CreateProxy();
    }

    public class TargetDescription<TTarget, TValue> : TargetDescription
    {
        private Func<TTarget, TValue> getter;
        private Action<TTarget, TValue> setter;
        private string propertyName;
        private TTarget target;
        private Func<TTarget, UnityEvent<TValue>> updateTrigger;

        public TargetDescription(string name, TTarget target, Func<TTarget, TValue> getter,
            Action<TTarget, TValue> setter,
            Func<TTarget, UnityEvent<TValue>> updateTrigger)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
            this.updateTrigger = updateTrigger;
        }

        public override ITargetProxy CreateProxy()
        {
            if (updateTrigger == null)
            {
                return new PropertyTargetProxy(target, CreatePropertyProxy());
            }
            else
            {
                return new UnityPropertyProxy<TValue>(target, CreatePropertyProxy(),updateTrigger(target));
            }
        }
        
        private IProxyPropertyInfo CreatePropertyProxy()
        {
            return new ProxyPropertyInfo<TTarget, TValue>(null,propertyName,getter,setter);
        }
    }
    
    public class SimpleEventTargetDescription<TTarget,TEvent> : TargetDescription where TEvent : UnityEvent
    {
        private Func<TTarget, TEvent> getter;
        private Action<TTarget, TEvent> setter;
        private string propertyName;
        private TTarget target;

        public SimpleEventTargetDescription(string name, TTarget target, Func<TTarget, TEvent> getter,
            Action<TTarget, TEvent> setter)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
        }
        
        public override ITargetProxy CreateProxy()
        {
            var proxy = new UnityEventProxy(target, getter(target));
            return proxy;
        }
    }
    
    public class UnityEventTargetDescription<TTarget,TParam> : TargetDescription
    {
        private Func<TTarget, UnityEvent<TParam>> getter;
        private Action<TTarget, UnityEvent<TParam>> setter;
        private string propertyName;
        private TTarget target;

        public UnityEventTargetDescription(string name, TTarget target, Func<TTarget, UnityEvent<TParam>> getter,
            Action<TTarget, UnityEvent<TParam>> setter)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
        }
        
        public override ITargetProxy CreateProxy()
        {
            var proxy = new UnityEventProxy<TParam>(target, getter(target));
            return proxy;
        }
    }
    
    public class UnityEventTargetDescription<TTarget,T0,T1> : TargetDescription
    {
        private Func<TTarget, UnityEvent<T0,T1>> getter;
        private Action<TTarget, UnityEvent<T0,T1>> setter;
        private string propertyName;
        private TTarget target;
        public UnityEventTargetDescription(string name, TTarget target, Func<TTarget, UnityEvent<T0,T1>> getter,
            Action<TTarget, UnityEvent<T0,T1>> setter)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
        }
        
        public override ITargetProxy CreateProxy()
        {
            var proxy =  new UnityEventProxy<T0,T1>(target, getter(target));
            return proxy;
        }
    }
    
    public class UnityEventTargetDescription<TTarget,T0,T1,T2> : TargetDescription
    {
        private Func<TTarget, UnityEvent<T0,T1,T2>> getter;
        private Action<TTarget, UnityEvent<T0,T1,T2>> setter;
        private string propertyName;
        private TTarget target;
        public UnityEventTargetDescription(string name, TTarget target, Func<TTarget, UnityEvent<T0,T1,T2>> getter,
            Action<TTarget, UnityEvent<T0,T1,T2>> setter)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
        }
        
        public override ITargetProxy CreateProxy()
        {
            var proxy =  new UnityEventProxy<T0,T1,T2>(target, getter(target));
            return proxy;
        }
    }
    
    public class UnityEventTargetDescription<TTarget,T0,T1,T2,T3> : TargetDescription
    {
        private Func<TTarget, UnityEvent<T0,T1,T2,T3>> getter;
        private Action<TTarget, UnityEvent<T0,T1,T2,T3>> setter;
        private string propertyName;
        private TTarget target;
        public UnityEventTargetDescription(string name, TTarget target, Func<TTarget, UnityEvent<T0,T1,T2,T3>> getter,
            Action<TTarget, UnityEvent<T0,T1,T2,T3>> setter)
        {
            this.propertyName = name;
            this.target = target;
            this.getter = getter;
            this.setter = setter;
        }
        
        public override ITargetProxy CreateProxy()
        {
            var proxy = new UnityEventProxy<T0,T1,T2,T3>(target, getter(target));
            return proxy;
        }
    }
}