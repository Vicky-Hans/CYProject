using System;

namespace DH.UIFramework.Services
{
    public interface IServiceRegistry
    {
        void Register(Type type, object target);

        void Register<T>(T target);

        void Register(string name, object target);

        void Register<T>(string name, T target);

        void Register<T>(Func<T> factory);

        void Register<T>(string name, Func<T> factory);

        void Unregister<T>();

        void Unregister(Type type);

        void Unregister(string name);
    }

    public interface IServiceLocator
    {
        object Resolve(Type type);

        T Resolve<T>();

        object Resolve(string name);

        T Resolve<T>(string name);
    }

    public interface IServiceContainer : IServiceLocator, IServiceRegistry
    {
    }

    public interface IServiceBundle
    {
        void Start();

        void Stop();
    }
}
