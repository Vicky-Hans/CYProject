using System;
using System.IO;
using System.IO.Compression;

namespace DH.Game
{
    public static class CompressHelper
    {
        public static byte[] CompressBytes(byte[] bytes)
        {
            using (MemoryStream compressStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }

                return compressStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (var compressStream = new MemoryStream(bytes))
            {
                using (GZipStream zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
                {
                    byte[] nb =
                    {
                        bytes[^4], bytes[^3],
                        bytes[^2], bytes[^1]
                    };
                    var count = BitConverter.ToInt32(nb, 0);
                    byte[] resultStream = new byte[count];
                    zipStream.Read(resultStream, 0, count);
                    return resultStream;
                }
            }
        }
    }
}