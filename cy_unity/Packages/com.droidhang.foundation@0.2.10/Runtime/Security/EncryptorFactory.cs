using UnityEngine;

namespace DHFramework
{
    public abstract class EncryptorFactory : ScriptableObject
    {
        public abstract IEncryptor Create();
    }
}