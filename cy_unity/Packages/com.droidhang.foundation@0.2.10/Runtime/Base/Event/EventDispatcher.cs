using System;

namespace DH.Foundations.Event
{
    public static class EventDispatcher
    {
        private static EventInternal EventInternal = new EventInternal();
        
        public static void RegEventListener(string eventType, Delegate handler)
        {
            EventInternal.RegEventListener(eventType, handler);
        }

        public static void RegEventListener(string eventType, Action handler)
        {
            EventInternal.RegEventListener(eventType, handler);
        }

        public static void RegEventListener<T>(string eventType, Action<T> handler)
        {
            EventInternal.RegEventListener<T>(eventType, handler);
        }

        internal static void RegEventListener(object updateView)
        {
            throw new NotImplementedException();
        }

        public static void RegEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            EventInternal.RegEventListener<T, U>(eventType, handler);
        }

        public static void RegEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            EventInternal.RegEventListener<T, U, V>(eventType, handler);
        }

        public static void RegEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            EventInternal.RegEventListener<T, U, V, W>(eventType, handler);
        }

        public static void UnRegEventListener(string eventType, Action handler)
        {
            EventInternal.UnRegEventListener(eventType, handler);
        }

        public static void UnRegEventListener<T>(string eventType, Action<T> handler)
        {
            EventInternal.UnRegEventListener<T>(eventType, handler);
        }

        public static void UnRegEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            EventInternal.UnRegEventListener<T, U>(eventType, handler);
        }

        public static void UnRegEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            EventInternal.UnRegEventListener<T, U, V>(eventType, handler);
        }

        public static void UnRegEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            EventInternal.UnRegEventListener<T, U, V, W>(eventType, handler);
        }

        public static void TriggerEvent(string eventType)
        {
            EventInternal.TriggerEvent(eventType);
        }

        public static void TriggerEvent<T>(string eventType, T arg1)
        {
            EventInternal.TriggerEvent<T>(eventType, arg1);
        }

        public static void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
        {
            EventInternal.TriggerEvent<T, U>(eventType, arg1, arg2);
        }

        public static void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
            EventInternal.TriggerEvent<T, U, V>(eventType, arg1, arg2, arg3);
        }

        public static void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            EventInternal.TriggerEvent<T, U, V, W>(eventType, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 检测有无注册过该事件
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public static bool ContainsEvent(string eventType)
        {
            return EventInternal.ContainsEvent(eventType);
        }
    }
}
