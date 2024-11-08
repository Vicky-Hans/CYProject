using UnityEngine;

namespace DH.UIFramework
{
    public static class MonoBehaviourExtension
    {
        public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
        {
            return GetOrAddComponent<T>(behaviour.gameObject);
        }
        
        public static T GetOrAddComponent<T>(this GameObject behaviour) where T : Component
        {
            var com = behaviour.GetComponent<T>();
            if (!com)
            {
                com = behaviour.gameObject.AddComponent<T>();
            }

            return com;
        }
    }
}