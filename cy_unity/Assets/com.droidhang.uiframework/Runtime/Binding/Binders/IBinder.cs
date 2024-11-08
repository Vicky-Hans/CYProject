using System.Collections.Generic;
using DH.UIFramework.Contexts;

namespace DH.UIFramework.Binders
{
    public interface IBinder
    {
        IBinding Bind(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription);

        IEnumerable<IBinding> Bind(IBindingContext bindingContext, object source, object target, IEnumerable<BindingDescription> bindingDescriptions);

    }
}