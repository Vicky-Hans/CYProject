using System;
using System.Collections.Generic;
using System.Threading;
namespace DHFramework
{
    public class UnityThreadModule
    {
        private readonly List<Action> callbacks = new List<Action>();
        private  volatile bool callbacksPending;
        private int threadId;
        public UnityThreadModule()
        {
            threadId = Thread.CurrentThread.ManagedThreadId;
        }
        public UnityThreadModule(int threadId)
        {
            this.threadId = threadId;
        }
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (!callbacksPending) return;
            // We copy our actions to another array to avoidlocking the queue whilst we process them.
            Action[] copy;
            lock (callbacks)
            {
                if (callbacks.Count == 0) return;
                copy = new Action[callbacks.Count];
                callbacks.CopyTo(copy);
                callbacks.Clear();
                callbacksPending = false;
            }
            foreach (var action in copy)
                action();
        }

        internal void Shutdown()
        {
            
        }
        public void RunOnMainThread(Action runnable)
        {
            // 若回调本身在Unity主线程，则可以直接回调
            if (threadId == Thread.CurrentThread.ManagedThreadId)
            {
                runnable?.Invoke();
                return;
            }
            
            lock (callbacks)
            {
                callbacks.Add(runnable);
                callbacksPending = true;
            }
        }

        public void RunOnNextFrame(Action runnable)
        {
            lock (callbacks)
            {
                callbacks.Add(runnable);
                callbacksPending = true;
            }
        }
    }
}