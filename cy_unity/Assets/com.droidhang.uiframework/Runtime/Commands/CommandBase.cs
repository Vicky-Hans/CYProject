using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace DH.UIFramework.Commands
{
    /// <summary>
    ///
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Occurs when can execute changed.
        /// </summary>
        event EventHandler CanExecuteChanged;

        void AddCanExecute(Action<object, EventArgs> action);

        // remove action from the list, mimic Delegate.Remove
        void RemoveCanExecute(Action<object, EventArgs> action);

        /// <summary>
        /// Determines whether this instance can execute the specified parameter.
        /// </summary>
        /// <param name="parameter">Parameter.</param>
        /// <returns><c>true</c> if this instance can execute the specified parameter; otherwise, <c>false</c>.</returns>
        bool CanExecute(object parameter);

        /// <summary>
        /// Execute the specified parameter.
        /// </summary>
        /// <param name="parameter">Parameter.</param>
        void Execute(object parameter);

        bool Executing { get; }
    }

    /// <summary>
    /// Custom event class for better performance.
    /// </summary>
    public class BindingEvent
    {
        private List<Action<object, EventArgs>> actionList;

        public BindingEvent()
        {
            actionList = new List<Action<object, EventArgs>>();
        }

        // add action to the list, mimic Delegate.Combine
        public void Add(Action<object, EventArgs> action)
        {
            if (action == null)
            {
                return;
            }

            // does not check for duplicated entry
            actionList.Add(action);
        }

        // remove action from the list, mimic Delegate.Remove
        public void Remove(Action<object, EventArgs> action)
        {
            if (action == null)
            {
                return;
            }

            actionList.Remove(action);
        }

        public void Invoke(object sender, EventArgs args)
        {
            for (int i = 0; i < actionList.Count; i++)
            {
                var action = actionList[i];
                action.Invoke(sender, args);
            }
        }

        public void Clear()
        {
            actionList.Clear();
        }
    }
    
    public abstract class CommandBase : ICommand
    {
        private BindingEvent canExecuteChanged = new BindingEvent();

        public virtual void RaiseCanExecuteChanged()
        {
            var handler = this.canExecuteChanged;
            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public void AddCanExecute(Action<object, EventArgs> action)
        {
            canExecuteChanged.Add(action);
        }

        public void RemoveCanExecute(Action<object, EventArgs> action)
        {
            canExecuteChanged.Remove(action);
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public virtual void Release()
        {
            canExecuteChanged.Clear();
        }

        public virtual bool Executing => false;
    }
}
