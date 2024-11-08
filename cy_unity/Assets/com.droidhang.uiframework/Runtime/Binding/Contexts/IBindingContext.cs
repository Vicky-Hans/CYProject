using System;
using System.Collections.Generic;

namespace DH.UIFramework.Contexts
{
    public interface IBindingContext : IDisposable
    {
        event EventHandler DataContextChanged;

        object Owner { get; }

        object DataContext { get; set; }

        void Add(IBinding binding,object key=null);

        void Add(IEnumerable<IBinding> bindings,object key = null);

        void Add(object target, BindingDescription description,object key = null);

        void Add(object target, IEnumerable<BindingDescription> descriptions, object key = null);
        // 临时暂停绑定，恢复后自动刷新
        void PauseBind(bool flag);

        void Unbind();

        void Clear(object key);

        void Clear();
    }
}