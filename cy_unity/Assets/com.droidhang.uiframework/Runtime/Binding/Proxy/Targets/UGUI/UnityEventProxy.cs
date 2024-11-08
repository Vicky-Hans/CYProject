

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
using DH.UIFramework.Commands;
using DH.UIFramework.Reflection;
using System.Threading;
using DHFramework;
using UnityEngine.UI;

namespace DH.UIFramework.Proxy.Targets
{
    public abstract class UnityEventProxyBase<T> : EventTargetProxyBase where T : UnityEventBase
    {
        private bool disposed = false;
        protected ICommand command;/* Command Binding */
        protected IInvoker invoker;/* Method Binding or Lua Function Binding */
        protected Delegate handler;/* Delegate Binding */

        protected IProxyPropertyInfo<bool> interactable;
        protected SendOrPostCallback updateInteractableAction;
        protected T unityEvent;

        public UnityEventProxyBase(object target, T unityEvent) : base(target)
        {
            if (unityEvent == null)
                throw new ArgumentNullException("unityEvent");

            this.unityEvent = unityEvent;
            this.BindEvent();
        }

        public override BindingMode DefaultMode { get { return BindingMode.OneWay; } }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UnbindCommand(this.command);
                this.UnbindEvent();
                disposed = true;
                base.Dispose(disposing);
            }
        }

        protected abstract void BindEvent();

        protected abstract void UnbindEvent();

        public override void SetValue(object value)
        {
            var target = this.Target;
            if (target == null)
                return;

            if (this.command != null)
            {
                UnbindCommand(this.command);
                this.command = null;
            }

            if (this.invoker != null)
                this.invoker = null;

            if (this.handler != null)
                this.handler = null;

            if (value == null)
                return;

            //Bind Command
            if (value is ICommand command)
            {
                if (this.interactable == null && target is Selectable)
                {
                    this.interactable = new ProxyPropertyInfo<Selectable, bool>(null, "interactable",
                        v => v.interactable, (t, v) => t.interactable = v);
                }

                this.command = command;
                BindCommand(this.command);
                UpdateTargetInteractable();
                return;
            }

            //Bind Method
            IInvoker invoker = value as IInvoker;
            if (invoker != null)
            {
                this.invoker = invoker;
            }

            //Bind Delegate
            Delegate handler = value as Delegate;
            if (handler != null)
            {
                this.handler = handler;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            this.SetValue((object)value);
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (updateInteractableAction == null)
                updateInteractableAction = UpdateTargetInteractable;

            UISynchronizationContext.Post(updateInteractableAction, null);
        }

        protected virtual void UpdateTargetInteractable(object state = null)
        {
            var target = this.Target;
            if (this.interactable == null || target == null)
                return;

            bool value = this.command == null ? false : this.command.CanExecute(null);
            interactable.SetValue(target, value);
        }

        protected virtual void BindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        protected virtual void UnbindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged -= OnCanExecuteChanged;
        }
    }

    public class UnityEventProxy : UnityEventProxyBase<UnityEvent>
    {
        public UnityEventProxy(object target, UnityEvent unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type { get { return typeof(UnityEvent); } }

        protected override void BindEvent()
        {
            this.unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            this.unityEvent.RemoveListener(OnEvent);
        }

        protected virtual void OnEvent()
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(null);
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke();
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is UnityAction)
                        (this.handler as UnityAction)();
                    else
                    {
                        this.handler.DynamicInvoke();
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DHLog.Error("{0}", e);
            }
        }
    }

    public class UnityEventProxy<T> : UnityEventProxyBase<UnityEvent<T>>
    {
        public UnityEventProxy(object target, UnityEvent<T> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type { get { return typeof(UnityEvent<T>); } }

        protected override void BindEvent()
        {
            this.unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            this.unityEvent.RemoveListener(OnEvent);
        }

        protected virtual void OnEvent(T parameter)
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(parameter);
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke(parameter);
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is UnityAction<T>)
                        (this.handler as UnityAction<T>)(parameter);
                    else
                    {
                        this.handler.DynamicInvoke(parameter);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DHLog.Error("{0}", e);
            }
        }
    }

    public class UnityEventProxy<T0, T1> : UnityEventProxyBase<UnityEvent<T0, T1>>
    {
        public UnityEventProxy(object target, UnityEvent<T0, T1> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type { get { return typeof(UnityEvent<T0, T1>); } }

        protected override void BindEvent()
        {
            this.unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            this.unityEvent.RemoveListener(OnEvent);
        }

        protected virtual void OnEvent(T0 t0, T1 t1)
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(new object[] { t0, t1 });
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke(t0, t1);
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is UnityAction<T0, T1>)
                        (this.handler as UnityAction<T0, T1>)(t0, t1);
                    else
                    {
                        this.handler.DynamicInvoke(t0, t1);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DHLog.Error("{0}", e);
            }
        }
    }

    public class UnityEventProxy<T0, T1, T2> : UnityEventProxyBase<UnityEvent<T0, T1, T2>>
    {
        public UnityEventProxy(object target, UnityEvent<T0, T1, T2> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type { get { return typeof(UnityEvent<T0, T1, T2>); } }

        protected override void BindEvent()
        {
            this.unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            this.unityEvent.RemoveListener(OnEvent);
        }

        protected virtual void OnEvent(T0 t0, T1 t1, T2 t2)
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(new object[] { t0, t1, t2 });
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke(t0, t1, t2);
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is UnityAction<T0, T1, T2>)
                        (this.handler as UnityAction<T0, T1, T2>)(t0, t1, t2);
                    else
                    {
                        this.handler.DynamicInvoke(t0, t1, t2);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DHLog.Error("{0}", e);
            }
        }
    }

    public class UnityEventProxy<T0, T1, T2, T3> : UnityEventProxyBase<UnityEvent<T0, T1, T2, T3>>
    {
        public UnityEventProxy(object target, UnityEvent<T0, T1, T2, T3> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type { get { return typeof(UnityEvent<T0, T1, T2, T3>); } }

        protected override void BindEvent()
        {
            this.unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            this.unityEvent.RemoveListener(OnEvent);
        }
        
        protected virtual void OnEvent(T0 t0, T1 t1, T2 t2, T3 t3)
        {
            try
            {
                if (this.command != null)
                {
                    this.command.Execute(new object[] { t0, t1, t2, t3 });
                    return;
                }

                if (this.invoker != null)
                {
                    this.invoker.Invoke(t0, t1, t2, t3);
                    return;
                }

                if (this.handler != null)
                {
                    if (this.handler is UnityAction<T0, T1, T2, T3>)
                        (this.handler as UnityAction<T0, T1, T2, T3>)(t0, t1, t2, t3);
                    else
                    {
                        this.handler.DynamicInvoke(t0, t1, t2, t3);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                DHLog.Error("{0}", e);

            }
        }
    }
}