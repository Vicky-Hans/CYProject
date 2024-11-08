using System.IO;

namespace DHFramework
{
    public interface IStreamEncryptor : IEncryptor
    {
        Stream Encrypt(Stream input);

    }
}