

using DH.UIFramework.Converters;
using DH.UIFramework.Proxy;
using DH.UIFramework.Reflection;
using DH.UIFramework.Commands;
using System;

namespace DH.UIFramework.Parameters
{
    public class ParameterWrapConverter : AbstractConverter
    {
        private readonly ICommandParameter commandParameter;
        public ParameterWrapConverter(ICommandParameter commandParameter)
        {
            if (commandParameter == null)
                throw new ArgumentNullException("commandParameter");

            this.commandParameter = commandParameter;
        }

        public override void Convert(object value,IModifiable modifiable)
        {
            if (value == null)
            {
                modifiable?.SetValue(null);
                return;
            }

            if (value is Delegate)
            {
                modifiable?.SetValue(new ParameterWrapDelegateInvoker(value as Delegate, commandParameter));
                return;
            }

            if (value is ICommand)
            {
                modifiable?.SetValue(new ParameterWrapCommand(value as ICommand, commandParameter));
                return;
            }

            if (value is IScriptInvoker)
            {
                modifiable?.SetValue(new ParameterWrapScriptInvoker(value as IScriptInvoker, commandParameter));
                return;
            }

            throw new NotSupportedException(string.Format("Unsupported type \"{0}\".", value.GetType()));
        }

        public override object ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }
}
