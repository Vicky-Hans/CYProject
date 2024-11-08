using System.IO;

namespace DHFramework
{
    public interface IDecryptor
    {
        string AlgorithmName { get; }

        byte[] Decrypt(byte[] buffer);
    }
}