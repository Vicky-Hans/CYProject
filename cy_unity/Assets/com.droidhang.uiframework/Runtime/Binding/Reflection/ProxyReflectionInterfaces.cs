using System;

namespace DH.UIFramework.Reflection
{
    public interface IProxyMemberInfo
    {
        Type DeclaringType { get; }

        string Name { get; }

        bool IsStatic { get; }
    }

    public interface IProxyPropertyInfo : IProxyMemberInfo
    {
        bool IsValueType { get; }

        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target);

        void SetValue(object target, object value);
    }

    public interface IProxyPropertyInfo<TValue> : IProxyPropertyInfo
    {
        new TValue GetValue(object target);

        void SetValue(object target, TValue value);
    }

    public interface IProxyPropertyInfo<T, TValue> : IProxyPropertyInfo<TValue>
    {
        TValue GetValue(T target);

        void SetValue(T target, TValue value);
    }

    public interface IProxyItemInfo : IProxyMemberInfo
    {
        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target, object key);

        void SetValue(object target, object key, object value);
    }

    public interface IProxyItemInfo<TKey, TValue> : IProxyItemInfo
    {
        TValue GetValue(object target, TKey key);

        void SetValue(object target, TKey key, TValue value);
    }

    public interface IProxyItemInfo<T, TKey, TValue> : IProxyItemInfo<TKey, TValue>
    {
        TValue GetValue(T target, TKey key);

        void SetValue(T target, TKey key, TValue value);
    }
}
