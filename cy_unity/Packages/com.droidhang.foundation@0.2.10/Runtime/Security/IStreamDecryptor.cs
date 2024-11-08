using System.IO;

namespace DHFramework
{
    public interface IStreamDecryptor : IDecryptor
    {
        Stream Decrypt(Stream input);

    }
}