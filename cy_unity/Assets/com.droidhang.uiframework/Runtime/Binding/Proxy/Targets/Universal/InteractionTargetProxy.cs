

using DH.UIFramework.Interactivity;
using System;
using UnityEngine;

namespace DH.UIFramework.Proxy.Targets
{
    public class InteractionTargetProxy : TargetProxyBase, IObtainable
    {
        protected readonly EventHandler<InteractionEventArgs> handler;

        public InteractionTargetProxy(object target, IInteractionAction interactionAction) : base(target)
        {
            this.handler = (sender, args) =>
            {
                if (target is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                    return;

                interactionAction.OnRequest(sender, args);
            };
        }

        public override Type Type { get { return typeof(EventHandler<InteractionEventArgs>); } }

        public override BindingMode DefaultMode { get { return BindingMode.OneWayToSource; } }

        public object GetValue()
        {
            return handler;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }
    }
}
