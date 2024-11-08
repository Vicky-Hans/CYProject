using DH.UIFramework.Observables;

namespace DH.Game
{
    public class ObservableSingleton<T> : ObservableObject where T : new()
    {
        public static T Instance
        {
            get
            {
                return Nested.Instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
                //Debug.Log("construct Nested over here");
            }

            internal static readonly T Instance = new T();
        }
    }
}