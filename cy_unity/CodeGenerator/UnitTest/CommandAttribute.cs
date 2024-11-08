using System;

namespace UnityEngine.Pool { 
}


namespace DH.UIFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
    }


    namespace Commands
    {
        public interface ICommand
        {
            void Execute();
        }

        public class RelayCommand : ICommand
        {
            public RelayCommand()
            {

            }

            public void Execute()
            {

            }
        }

        public class AsyncCommand : ICommand 
        {
            public void Execute()
            {

            }
        }
    }
}