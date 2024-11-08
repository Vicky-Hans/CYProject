using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine;

namespace DH.Foundations.Event
{
    [Serializable]
    public class EventException : Exception
    {
        public EventException(string message)
            : base(message)
        {
        }

        public EventException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class EventInternal
    {

        private Dictionary<string, Delegate> _eventRouter = new Dictionary<string, Delegate>();

        private void OnListenerAdding(string eventType, Delegate listenerAdded)
        {
            if (!_eventRouter.ContainsKey(eventType))
            {
                _eventRouter.Add(eventType, null);
            }

            Delegate eventDelegate = _eventRouter[eventType];
            if (eventDelegate != null && eventDelegate.GetType() != listenerAdded.GetType())
            {
                throw new EventException(string.Format(
                    "Try to add not correct event {0}. Current type is {1}, adding type is {2}", eventType,
                    eventDelegate.GetType().Name, listenerAdded.GetType().Name));
            }
        }

        private bool OnListenerRemoving(string eventType, Delegate listenerRemoved)
        {
            if (_eventRouter.ContainsKey(eventType))
            {
                Delegate eventDelegate = _eventRouter[eventType];
                if (eventDelegate != null && eventDelegate.GetType() != listenerRemoved.GetType())
                {
                    throw new EventException(string.Format(
                        "Remove listener {0}\" failed, Current type is {1}, adding type is {2}.", eventType,
                        eventDelegate.GetType(), listenerRemoved.GetType()));
                }

                return true;
            }

            return false;
        }

        private void OnListenerRemoved(string eventType)
        {
            DHLog.Debug($"OnListenerRemoved = {eventType}");
            if (_eventRouter.ContainsKey(eventType) && _eventRouter[eventType] == null)
            {
                _eventRouter.Remove(eventType);
            }
        }

        /// <summary>
        /// 检测容器中的事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool ContainsEvent(string eventType)
        {
            return _eventRouter.ContainsKey(eventType);
        }
        
        public void RegEventListener(string eventType, Delegate handler)
        {
            OnListenerAdding(eventType, handler);
            _eventRouter[eventType] = Delegate.Combine(this._eventRouter[eventType], handler);
        }

        public void RegEventListener(string eventType, Action handler)
        {
            OnListenerAdding(eventType, handler);
            _eventRouter[eventType] = (Action) Delegate.Combine((Action) this._eventRouter[eventType], handler);
        }

        public void RegEventListener<T>(string eventType, Action<T> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this._eventRouter[eventType] =
                (Action<T>) Delegate.Combine((Action<T>) this._eventRouter[eventType], handler);
        }

        public void RegEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this._eventRouter[eventType] =
                (Action<T, U>) Delegate.Combine((Action<T, U>) this._eventRouter[eventType], handler);
        }

        public void RegEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this._eventRouter[eventType] =
                (Action<T, U, V>) Delegate.Combine((Action<T, U, V>) this._eventRouter[eventType], handler);
        }

        public void RegEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            this.OnListenerAdding(eventType, handler);
            this._eventRouter[eventType] =
                (Action<T, U, V, W>) Delegate.Combine((Action<T, U, V, W>) this._eventRouter[eventType], handler);
        }

        public void UnRegEventListener(string eventType, Action handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this._eventRouter[eventType] = (Action) Delegate.Remove((Action) this._eventRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void UnRegEventListener<T>(string eventType, Action<T> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this._eventRouter[eventType] =
                    (Action<T>) Delegate.Remove((Action<T>) this._eventRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void UnRegEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this._eventRouter[eventType] =
                    (Action<T, U>) Delegate.Remove((Action<T, U>) this._eventRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void UnRegEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this._eventRouter[eventType] =
                    (Action<T, U, V>) Delegate.Remove((Action<T, U, V>) this._eventRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void UnRegEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            if (this.OnListenerRemoving(eventType, handler))
            {
                this._eventRouter[eventType] =
                    (Action<T, U, V, W>) Delegate.Remove((Action<T, U, V, W>) this._eventRouter[eventType], handler);
                this.OnListenerRemoved(eventType);
            }
        }

        public void TriggerEvent(string eventType)
        {
            Delegate delgate;
            if (this._eventRouter.TryGetValue(eventType, out delgate))
            {
                Delegate[] invocationList = delgate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action action = invocationList[i] as Action;
                    if (null == action)
                    {
                        throw new EventException(
                            string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        //throw new EventException($"TriggerEvent ex->{ex}");
                    }
                }
            }
        }

        public void TriggerEvent<T>(string eventType, T arg1)
        {
            Delegate delgate;
            DHLog.Debug($"Hash code is->{this.GetHashCode()}");
            if (this._eventRouter.TryGetValue(eventType, out delgate))
            {
                Delegate[] invocationList = delgate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T> action = invocationList[i] as Action<T>;
                    if (null == action)
                    {
                        throw new EventException(
                            string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                    }

                    try
                    {
                        action(arg1);
                    }
                    catch (Exception ex)
                    {
                        throw new EventException($"TriggerEvent<T> ex->{ex},T->{arg1}");
                    }
                }
            }
            else
            {
                DHLog.Debug($"eventType has not found->{eventType}");
                //throw new EventException($"eventType has not found->{eventType}");
            }
        }

        public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
        {
            Delegate delgate;
            if (this._eventRouter.TryGetValue(eventType, out delgate))
            {
                Delegate[] invocationList = delgate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U> action = invocationList[i] as Action<T, U>;
                    if (null == action)
                    {
                        throw new EventException(
                            string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                    }

                    try
                    {
                        action(arg1, arg2);
                    }
                    catch (Exception ex)
                    {
                        throw new EventException($"TriggerEvent<T> ex->{ex},T->{arg1},U->{arg2}");
                    }
                }
            }
        }

        public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
            Delegate delgate;
            if (this._eventRouter.TryGetValue(eventType, out delgate))
            {
                Delegate[] invocationList = delgate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U, V> action = invocationList[i] as Action<T, U, V>;
                    if (null == action)
                    {
                        throw new EventException(
                            string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                    }

                    try
                    {
                        action(arg1, arg2, arg3);
                    }
                    catch (Exception ex)
                    {
                        throw new EventException($"TriggerEvent<T> ex->{ex},T->{arg1},U->{arg2},V->{arg3}");
                    }
                }
            }
        }

        public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            Delegate delgate;
            if (this._eventRouter.TryGetValue(eventType, out delgate))
            {
                Delegate[] invocationList = delgate.GetInvocationList();
                for (int i = 0; i < invocationList.Length; i++)
                {
                    Action<T, U, V, W> action = invocationList[i] as Action<T, U, V, W>;
                    if (null == action)
                    {
                        throw new EventException(
                            string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                    }

                    try
                    {
                        action(arg1, arg2, arg3, arg4);
                    }
                    catch (Exception ex)
                    {
                        throw new EventException($"TriggerEvent<T> ex->{ex},T->{arg1},U->{arg2},V->{arg3},W->{arg4}");
                    }
                }
            }
        }
    }
}

