using System.IO;

namespace DHFramework
{
    public interface IEncryptor
    {
        string AlgorithmName { get; }

        byte[] Encrypt(byte[] buffer);
    }
}