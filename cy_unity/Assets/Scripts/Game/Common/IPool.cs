using UnityEngine;

namespace DH.Game
{
    public interface IPool<T>
    {
        T InstantiateObj(string assetPath, Vector3 position, Quaternion rotation,Transform parent);
        T InstantiateObj(string assetPath, Transform parent, bool instantiateInWorldSpace = false);
        void ReleaseObj(T item);
    }
}