

using DH.UIFramework.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DH.UIFramework.Parameters
{
    public class ParameterWrapDelegateInvoker : ParameterWrapBase, IInvoker
    {
        private readonly Delegate handler;

        public ParameterWrapDelegateInvoker(Delegate handler, ICommandParameter commandParameter) : base(commandParameter)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            this.handler = handler;
        }

        public object Invoke(params object[] args)
        {
            return this.handler.DynamicInvoke(GetParameterValue());
        }
    }
}
