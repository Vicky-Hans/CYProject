using System;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using INotifyPropertyChanged = DH.UIFramework.Observables.INotifyPropertyChanged;

namespace DH.UIFramework.Commands
{
    public class AsyncCommand : CommandBase,INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs Args = new PropertyChangedEventArgs(nameof(Executing));
        private readonly Func<UniTask> execute;
        private readonly Func<bool> canExecute;
        private bool executing;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public AsyncCommand(Func<UniTask> execute) : this(execute, null)
        {
        }

        public AsyncCommand(Func<UniTask> execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;

            if (canExecute != null)
                this.canExecute = canExecute;
        }

        /// <summary>
        /// 检查异步行为是否正在执行中
        /// </summary>
        public override bool Executing => executing;

        public override bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public override void Execute(object parameter)
        {
            if (executing)
            {
                return;
            }
            
            Wrap().Forget();
        }

        private async UniTaskVoid Wrap()
        {
            if (execute == null)
            {
                return;
            }

            try
            {
                executing = true;
                PropertyChanged?.Invoke(this,Args);
                await execute.Invoke();
            }
            finally
            {
                executing = false;
                PropertyChanged?.Invoke(this,Args);
            }
        }
    }
    
    public class AsyncCommand<T> : CommandBase,INotifyPropertyChanged
    {
        private readonly Func<T,UniTask> execute;
        private readonly Func<bool> canExecute;
        private bool executing;
        private static readonly PropertyChangedEventArgs Args = new PropertyChangedEventArgs(string.Empty);
        public event PropertyChangedEventHandler PropertyChanged;
        
        public AsyncCommand(Func<T,UniTask> execute) : this(execute, null)
        {
        }

        public AsyncCommand(Func<T,UniTask> execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;

            if (canExecute != null)
                this.canExecute = canExecute;
        }

        /// <summary>
        /// 检查异步行为是否正在执行中
        /// </summary>
        public override bool Executing => executing;

        public override bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public override void Execute(object parameter)
        {
            if (executing)
            {
                return;
            }

            var param = (T)Convert.ChangeType(parameter, typeof(T));
            Wrap(param).Forget();
        }

        private async UniTaskVoid Wrap(T parameter)
        {
            if (execute == null)
            {
                return;
            }

            try
            {
                executing = true;
                PropertyChanged?.Invoke(this,Args);
                await execute.Invoke((T)Convert.ChangeType(parameter, typeof(T)));
            }
            finally
            {
                PropertyChanged?.Invoke(this,Args);
                executing = false;
            }
        }
    }
}