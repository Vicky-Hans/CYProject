using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace DH.UIFramework.Commands
{
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
