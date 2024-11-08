

using DH.UIFramework.Contexts;
using System;

namespace DH.UIFramework
{
    public interface IBinding : IDisposable
    {
        IBindingContext BindingContext { get; set; }

        object Target { get; }

        object DataContext { get; set; }

        void Unbind();

        void ForceRefresh();

        void Pause();
    }
}
