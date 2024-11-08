using System;
using System.Collections.Generic;
using DHFramework;
using UnityEngine.Pool;

namespace DH.UIFramework.Proxy.Sources
{
    public abstract class SourceProxyBase : BindingProxyBase, ISourceProxy
    {
        protected TypeCode typeCode = TypeCode.Empty;
        protected readonly object source;
        public SourceProxyBase(object source)
        {
            this.source = source;
        }

        public abstract Type Type { get; }

        public virtual TypeCode TypeCode
        {
            get
            {
                if (typeCode == TypeCode.Empty)
                {
#if NETFX_CORE
                    typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(Type);
#else
                    typeCode = Type.GetTypeCode(Type);
#endif
                }
                return typeCode;
            }
        }

        public virtual object Source { get { return this.source; } }
    }

    public abstract class NotifiableSourceProxyBase : SourceProxyBase, INotifiable
    {
        protected readonly object _lock = new object();
        protected List<EventHandler> valueChanged;

        public NotifiableSourceProxyBase(object source) : base(source)
        {
        }

        public virtual event EventHandler ValueChanged
        {
            add
            {
                lock (_lock)
                {
                    valueChanged ??= ListPool<EventHandler>.Get();
                    valueChanged.Add(value);
                }
            }

            remove
            {
                lock (_lock)
                {
                    valueChanged.Remove(value);
                    if (valueChanged.Count == 0)
                    {
                        ListPool<EventHandler>.Release(valueChanged);
                        valueChanged = null;
                    }
                }
            }
        }

        protected virtual void RaiseValueChanged()
        {
            try
            {
                if (this.valueChanged != null)
                    foreach (var item in valueChanged)
                    {
                       item(this, EventArgs.Empty);
                    }
            }
            catch (Exception e)
            {
                DHLog.Warning(e.ToString());
            }
        }
    }
}
