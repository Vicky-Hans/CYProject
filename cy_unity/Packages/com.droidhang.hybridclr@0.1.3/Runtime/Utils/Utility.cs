using System;
using System.IO;

namespace DHHybridCLR.Utils
{
    public class Utility
    {
        public static bool Compress(MemoryStream msInput, MemoryStream msOutput)
        {
            if (msInput == null || msInput.Length == 0)
            {
                return false;
            }

            msInput.Seek(0, SeekOrigin.Begin);

            var coder = new SevenZip.Compression.LZMA.Encoder();
            coder.WriteCoderProperties(msOutput);
            msOutput.Write(BitConverter.GetBytes(msInput.Length), 0, 8);
            coder.Code(msInput, msOutput, msInput.Length, -1, null);
            msOutput.Flush();

            return true;
        }

        public static bool Decompress(MemoryStream msInput, MemoryStream msOutput)
        {
            if (msInput == null || msInput.Length == 0)
            {
                return false;
            }

            msInput.Seek(0, SeekOrigin.Begin);

            var coder = new SevenZip.Compression.LZMA.Decoder();
            var properties = new byte[5];
            msInput.Read(properties, 0, 5);

            var fileLengthBytes = new byte[8];
            msInput.Read(fileLengthBytes, 0, 8);
            var fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            msOutput.Capacity += (int) fileLength;
            coder.SetDecoderProperties(properties);
            coder.Code(msInput, msOutput, msInput.Length, fileLength, null);
            msOutput.Flush();

            return true;
        }
    }
}
