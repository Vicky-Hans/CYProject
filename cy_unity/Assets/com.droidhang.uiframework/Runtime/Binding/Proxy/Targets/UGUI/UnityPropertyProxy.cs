

using DH.UIFramework.Reflection;
using UnityEngine.Events;

namespace DH.UIFramework.Proxy.Targets
{
    public class UnityPropertyProxy<TValue> : PropertyTargetProxy
    {
        private UnityEvent<TValue> unityEvent;
        public UnityPropertyProxy(object target, IProxyPropertyInfo propertyInfo, UnityEvent<TValue> unityEvent) : base(target, propertyInfo)
        {
            this.unityEvent = unityEvent;
        }

        public override BindingMode DefaultMode { get { return BindingMode.TwoWay; } }

        protected override void DoSubscribeForValueChange(object target)
        {
            if (this.unityEvent == null || target == null)
                return;

            unityEvent.AddListener(OnValueChanged);
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (unityEvent != null)
                unityEvent.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(TValue value)
        {
            this.RaiseValueChanged();
        }
    }
}
