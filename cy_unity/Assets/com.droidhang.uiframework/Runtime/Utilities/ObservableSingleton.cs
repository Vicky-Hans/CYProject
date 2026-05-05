using System;
using System.Reflection;
using DH.UIFramework.Observables;
using DHFramework;

namespace DH.UIFramework
{
    public abstract class ObservableSingleton<T>  : ObservableObject where T : new ()
    {
        protected ObservableSingleton()
        {
            
        }

        public static T Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly T instance = new T();
        }
    }
}