using System;
using System.Collections.Concurrent;
using System.Threading;
using DHFramework;

namespace DH.UIFramework.Messaging
{
    public abstract class SubjectBase
    {
        public abstract void Publish(object message);
    }

    public class Subject<T> : SubjectBase
    {
        private readonly ConcurrentDictionary<string, WeakReference<Subscription>> subscriptions = new ConcurrentDictionary<string, WeakReference<Subscription>>();
        public bool IsEmpty() { return subscriptions.Count <= 0; }

        public override void Publish(object message)
        {
            this.Publish((T)message);
        }

        public void Publish(T message)
        {
            if (subscriptions.Count <= 0)
                return;

            foreach (var kv in subscriptions)
            {
                Subscription subscription;
                kv.Value.TryGetTarget(out subscription);
                if (subscription != null)
                    subscription.Publish(message);
                else
                    subscriptions.TryRemove(kv.Key, out _);
            }
        }

        public ISubscription<T> Subscribe(Action<T> action)
        {
            return new Subscription(this, action);
        }

        void Add(Subscription subscription)
        {
            var reference = new WeakReference<Subscription>(subscription, false);
            this.subscriptions.TryAdd(subscription.Key, reference);
        }

        void Remove(Subscription subscription)
        {
            this.subscriptions.TryRemove(subscription.Key, out _);
        }

        class Subscription : ISubscription<T>
        {
            private Subject<T> subject;
            private Action<T> action;
            private SynchronizationContext context;
            public string Key { get; private set; }

            public Subscription(Subject<T> subject, Action<T> action)
            {
                this.subject = subject;
                this.action = action;
                this.Key = Guid.NewGuid().ToString();
                this.subject.Add(this);
            }

            public void Publish(T message)
            {
                try
                {
                    if (this.context != null)
                        context.Post(state => action((T)state), message);
                    else
                        action(message);
                }
                catch (Exception e)
                {
                    DHLog.Warning(e.ToString());
                }
            }

            public ISubscription<T> ObserveOn(SynchronizationContext context)
            {
                this.context = context ?? throw new ArgumentNullException("context");
                return this;
            }

            #region IDisposable Support
            private bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (this.disposed)
                    return;

                try
                {
                    if (this.disposed)
                        return;

                    if (subject != null)
                        subject.Remove(this);

                    context = null;
                    action = null;
                    subject = null;

                }
                catch (Exception) { }
                disposed = true;
            }

            ~Subscription()
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
