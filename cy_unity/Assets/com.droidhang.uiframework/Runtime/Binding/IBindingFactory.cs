
using DH.UIFramework.Contexts;

namespace DH.UIFramework
{
    public interface IBindingFactory
    {
        IBinding Create(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription);
    }
}
