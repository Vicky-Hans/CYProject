

using DH.UIFramework.Commands;
using System;

namespace DH.UIFramework.Parameters
{
    public class ParameterWrapCommand : ParameterWrapBase, ICommand
    {
        private readonly object _lock = new object();
        private readonly ICommand wrappedCommand;
        public ParameterWrapCommand(ICommand wrappedCommand, ICommandParameter commandParameter) : base(commandParameter)
        {
            if (wrappedCommand == null)
                throw new ArgumentNullException("wrappedCommand");

            this.wrappedCommand = wrappedCommand;
        }

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { this.wrappedCommand.CanExecuteChanged += value; } }
            remove { lock (_lock) { this.wrappedCommand.CanExecuteChanged -= value; } }
        }

        public bool Executing => wrappedCommand.Executing;

        public void AddCanExecute(Action<object, EventArgs> action)
        {
            wrappedCommand.AddCanExecute(action);
        }

        public void RemoveCanExecute(Action<object, EventArgs> action)
        {
            wrappedCommand.RemoveCanExecute(action);
        }

        public bool CanExecute(object parameter)
        {
            return wrappedCommand.CanExecute(GetParameterValue());
        }

        public void Execute(object parameter)
        {
            var param = GetParameterValue();
            if (wrappedCommand.CanExecute(param))
                wrappedCommand.Execute(param);
        }
    }
}
