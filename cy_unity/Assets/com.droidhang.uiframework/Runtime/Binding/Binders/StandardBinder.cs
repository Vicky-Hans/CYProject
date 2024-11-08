using System;
using System.Collections.Generic;
using System.Linq;
using DH.UIFramework.Contexts;

namespace DH.UIFramework.Binders
{
    public class StandardBinder : IBinder
    {
        protected IBindingFactory factory;

        public StandardBinder(IBindingFactory factory)
        {
            this.factory = factory;
        }

        public IBinding Bind(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription)
        {
            return factory.Create(bindingContext, source, target, bindingDescription);
        }

        public IEnumerable<IBinding> Bind(IBindingContext bindingContext, object source, object target, IEnumerable<BindingDescription> bindingDescriptions)
        {
            if (bindingDescriptions == null)
                return Array.Empty<IBinding>();

            return bindingDescriptions.Select(description => this.Bind(bindingContext, source, target, description));
        }
    }
}