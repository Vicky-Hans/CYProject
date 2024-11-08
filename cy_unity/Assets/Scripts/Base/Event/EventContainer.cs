using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DHFramework;

namespace SP.Base
{
    public sealed class EventContainer<TEventType> where TEventType : Enum, IFormattable, IConvertible, IComparable
    {
        private readonly Dictionary<int, Delegate> handlers;

        static EventContainer()
        {
#if UNITY_EDITOR
            if (!typeof(TEventType).IsEnum)
            {
                throw new InvalidTypeExpection("event container : event type must be enum");
            }
#endif
        }

        public EventContainer()
        {
            handlers = new Dictionary<int, Delegate>();
            var enumsValues = Enum.GetValues(typeof(TEventType));
            foreach (var index in enumsValues)
            {
                Delegate del = null;
                handlers.Add((int)index, del);
            }
        }

        public void AddListener(TEventType type, Action<EventArgs> action)
        {
            AddListener<EventArgs>(type, action);
        }

        public void AddListener(TEventType type, Action action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
#if UNITY_EDITOR
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action)item == action)
                    {
                        throw new AddListenerExpection(string.Format("add listener for event {0} ,but action {1} has registered",
                            type, action));
                    }
                }
            }
#endif
            handlers[eventKey] = (Action)handlers[eventKey] + action;
        }

        public void AddListener<T>(TEventType type, Action<T> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
#if UNITY_EDITOR
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T>)item == action)
                    {
                        throw new AddListenerExpection(string.Format("add listener for event {0} ,but action {1} has registered",
                            type, action));
                    }
                }
            }
#endif
            handlers[eventKey] = (Action<T>)handlers[eventKey] + action;
        }

        public void AddListener<T1, T2>(TEventType type, Action<T1, T2> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
#if UNITY_EDITOR
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T1, T2>)item == action)
                    {
                        throw new AddListenerExpection(string.Format("add listener for event {0} ,but action {1} has registered",
                            type, action));
                    }
                }
            }
#endif
            handlers[eventKey] = (Action<T1, T2>)handlers[eventKey] + action;
        }

        public void AddListener<T1, T2, T3>(TEventType type, Action<T1, T2, T3> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
#if UNITY_EDITOR
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T1, T2, T3>)item == action)
                    {
                        throw new AddListenerExpection(string.Format("add listener for event {0} ,but action {1} has registered",
                            type, action));
                    }
                }
            }
#endif
            handlers[eventKey] = (Action<T1, T2, T3>)handlers[eventKey] + action;
        }

        public void RemoveListener(TEventType type, Action<EventArgs> action)
        {
            RemoveListener<EventArgs>(type, action);
        }

        public void RemoveListener(TEventType type, Action action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
#if UNITY_EDITOR
                bool exist = false;
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action)item == action)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    DHLog.Warning(string.Format("remove listener for event {0} ,but action {1} not registered",
                            type, action));
                }
#endif
                handlers[eventKey] = (Action)handlers[eventKey] - action;
            }
        }

        public void RemoveListener<T>(TEventType type, Action<T> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
#if UNITY_EDITOR
                bool exist = false;
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T>)item == action)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    DHLog.Warning(string.Format("remove listener for event {0} ,but action {1} not registered",
                            type, action));
                }
#endif
                handlers[eventKey] = (Action<T>)handlers[eventKey] - action;
            }
        }

        public void RemoveListener<T1, T2>(TEventType type, Action<T1, T2> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
#if UNITY_EDITOR
                bool exist = false;
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T1, T2>)item == action)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    DHLog.Warning(string.Format("remove listener for event {0} ,but action {1} not registered",
                            type, action));
                }
#endif
                handlers[eventKey] = (Action<T1, T2>)handlers[eventKey] - action;
            }
        }

        public void RemoveListener<T1, T2, T3>(TEventType type, Action<T1, T2, T3> action)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
#if UNITY_EDITOR
                bool exist = false;
                foreach (var item in del.GetInvocationList())
                {
                    if ((Action<T1, T2, T3>)item == action)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    DHLog.Warning(string.Format("remove listener for event {0} ,but action {1} not registered",
                            type, action));
                }
#endif
                handlers[eventKey] = (Action<T1, T2, T3>)handlers[eventKey] - action;
            }
        }

        public void TriggerEvent(TEventType type, EventArgs args)
        {
            TriggerEvent<EventArgs>(type, args);
        }

        public void TriggerEvent(TEventType type)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    var action = (Action)item;
#if !UNITY_EDITOR
                    if (!ReferenceEquals(null, action))
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            DHLog.Error(ex.StackTrace);
                        }
                    }
#else
                    action?.Invoke();
#endif
                }
            }
        }

        public void TriggerEvent<T>(TEventType type, T t)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    var action = (Action<T>)item;
#if !UNITY_EDITOR
                    if (!ReferenceEquals(null, action))
                    {
                        try
                        {
                            action(t);
                        }
                        catch (Exception ex)
                        {
                            DHLog.Error(ex.StackTrace);
                        }
                    }
#else
                    action?.Invoke(t);
#endif
                }
            }
        }

        public void TriggerEvent<T1, T2>(TEventType type, T1 t1, T2 t2)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    var action = (Action<T1, T2>)item;
#if !UNITY_EDITOR
                    if (!ReferenceEquals(null, action))
                    {
                        try
                        {
                            action(t1, t2);
                        }
                        catch (Exception ex)
                        {
                            DHLog.Error(ex.StackTrace);
                        }
                    }
#else
                    action?.Invoke(t1, t2);
#endif
                }
            }
        }

        public void TriggerEvent<T1, T2, T3>(TEventType type, T1 t1, T2 t2, T3 t3)
        {
            int eventKey = Unsafe.As<TEventType, Int32>(ref type);
            if (handlers.TryGetValue(eventKey, out Delegate del) && !ReferenceEquals(null, del))
            {
                foreach (var item in del.GetInvocationList())
                {
                    var action = (Action<T1, T2, T3>)item;
#if !UNITY_EDITOR
                    if (!ReferenceEquals(null, action))
                    {
                        try
                        {
                            action(t1, t2, t3);
                        }
                        catch (Exception ex)
                        {
                            DHLog.Error(ex.StackTrace);
                        }
                    }
#else
                    action?.Invoke(t1, t2, t3);
#endif
                }
            }
        }

        public void RemoveAllListeners()
        {
            handlers.Clear();
            var enumsValues = Enum.GetValues(typeof(TEventType));
            foreach (var index in enumsValues)
            {
                Delegate del = null;
                handlers.Add((int)index, del);
            }
        }
    }
}