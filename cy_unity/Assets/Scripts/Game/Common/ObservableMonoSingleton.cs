using UnityEngine;

namespace DH.Game
{
    public class ObservableMonoSingleton <T> : ObservableMonoBehavior where T : MonoBehaviour
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                //先寻找场景中有无预制
                if (!instance)
                {
                    instance = FindObjectOfType<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            instance = GetComponent<T>();
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }
}