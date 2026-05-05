using System;
using UnityEngine;

namespace DH.UIFramework.Commands
{
    public class SimpleCommand : CommandBase
    {
        public static int count;
        private bool enabled = true;
        private Action execute;

        public SimpleCommand(Action execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            count++;
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled == value)
                    return;

                this.enabled = value;
                this.RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.Enabled;
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute();
        }

        public override void Release()
        {
            execute = null;
            base.Release();
        }
    }
    
    public class SimpleCommand<T> : CommandBase
    {
        private bool enabled = true;
        private readonly Action<T> execute;

        public SimpleCommand(Action<T> execute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                if (this.enabled == value)
                    return;

                this.enabled = value;
                this.RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return this.Enabled;
        }

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter) && this.execute != null)
                this.execute((T)Convert.ChangeType(parameter, typeof(T)));
        }
    }
}

