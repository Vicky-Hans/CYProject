using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DHFramework
{
    public static partial class DHUtility
    {
        private const int StringBuilderCapacity = 1024;

        [ThreadStatic] private static StringBuilder s_CachedStringBuilder = null;

        /// <summary>
        /// 获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg0">字符串参数 0。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format(string format, object arg0)
        {
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, arg0);
            return s_CachedStringBuilder.ToString();
        }

        /// <summary>
        /// 获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg0">字符串参数 0。</param>
        /// <param name="arg1">字符串参数 1。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format(string format, object arg0, object arg1)
        {
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, arg0, arg1);
            return s_CachedStringBuilder.ToString();
        }

        /// <summary>
        /// 获取格式化字符串，减少gc的拼接函数
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg0">字符串参数 0。</param>
        /// <param name="arg1">字符串参数 1。</param>
        /// <param name="arg2">字符串参数 2。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, arg0, arg1, arg2);
            return s_CachedStringBuilder.ToString();
        }

        public static string JsonFormat(string json, params object[] args)
        {
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.Append("{");
            s_CachedStringBuilder.AppendFormat(json, args);
            s_CachedStringBuilder.Append("}");
            return s_CachedStringBuilder.ToString();
        }

        /// <summary>
        /// 获取格式化字符串。
        /// </summary>
        /// <param name="format">字符串格式。</param>
        /// <param name="args">字符串参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format(string format, params object[] args)
        {
            CheckCachedStringBuilder();
            s_CachedStringBuilder.Length = 0;
            s_CachedStringBuilder.AppendFormat(format, args);
            return s_CachedStringBuilder.ToString();
        }

        private static void CheckCachedStringBuilder()
        {
            if (s_CachedStringBuilder == null)
            {
                s_CachedStringBuilder = new StringBuilder(StringBuilderCapacity);
            }
        }


        public static string MD5Hash(string value)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                var md5Hash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                var md5Str = BitConverter.ToString(md5Hash).Replace("-", "");
                return md5Str;
            }
        }

        public static string MD5Hash(Stream stream)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                var md5Hash = md5.ComputeHash(stream);
                var md5Str = BitConverter.ToString(md5Hash).Replace("-", "");
                return md5Str.ToLowerInvariant();
            }
        }

        /// <summary>
        /// 日志MD5加密
        /// </summary>
        /// <param name="tick">秒级时间戳</param>
        /// <returns>MD5加密</returns>
        public static string MD5Tick(string tick)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                //混淆加密串
                string md5Salt = "fbiubi9gewaga=niu1n3091mlnoahgawng";
                string tickValue = md5Salt + tick;
                byte[] md5Hash = md5.ComputeHash(Encoding.UTF8.GetBytes(tickValue));
                string md5Str = BitConverter.ToString(md5Hash).Replace("-", "");
                //需要小写
                return md5Str.ToLowerInvariant();
            }
        }
        
        public enum TicksType
        {
            /// <summary>
            /// 秒 13位
            /// </summary>
            S,
            /// <summary>
            /// 毫秒 16位
            /// </summary>
            MS,
            /// <summary>
            /// 纳秒 19位
            /// </summary>
            NS 
        }
        
        private static DateTime StartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long GetGameTime(TicksType type, DateTime ? time = null)
        {
            //以UTC时间为准
            //算出时间差
            long tick = Int64.MinValue;
            TimeSpan timeSpan;
            //是否有传参，有则用传入的时间，否则使用UTC时间
            if (time.HasValue)
            {
                timeSpan = (time.Value - StartTime);
            }
            else
            {
                timeSpan = (DateTime.UtcNow - StartTime);
            }
            switch (type)
            {
                case TicksType.S:
                    tick = Convert.ToInt64(timeSpan.TotalSeconds);
                    break;
                case TicksType.MS:
                    tick = Convert.ToInt64(timeSpan.TotalMilliseconds);
                    break;
                case TicksType.NS:
                    //纳秒级(1tick = 100ns)
                    //此处是以Ticks为最小单位获取的时间戳，C#中并无纳秒级的API，故而此处转换的并非真正的纳秒
                    tick = timeSpan.Ticks * 100;
                    break;
            }

            return tick;
        }
        
        
        /// <summary>
        /// 检测数美ID是否解析正确
        /// </summary>
        /// <param name="smid"></param>
        /// <returns></returns>
        public static bool CheckShuMeiID(string smid)
        {
            if (string.IsNullOrEmpty(smid))
            {
                return false;
            }
            bool isNumber = Char.IsNumber(smid[0]) && smid[0].Equals('2');
            return isNumber;
        }

        /// <summary>
        /// 流转字节
        /// </summary>
        /// <param name="stream">输入输出流</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 字节转流
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }

        /// <summary>
        /// 流写入文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        public static void StreamWriteToFile(string fileName, Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                    binaryWriter.Flush();
                    binaryWriter.Close();
                }
                fileStream.Close();
            }
        }
        
        /// <summary>
        /// 字节写入文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        public static void BytesWriteToFile(string fileName, byte[] bytes)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(bytes);
                    binaryWriter.Flush();
                    binaryWriter.Close();
                }
                fileStream.Close();
            }
        }

        /// <summary>
        /// 读取文件字节和流
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Stream FileReadToStream(string fileName, out byte[] bytes)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bytes = new byte[fileStream.Length];
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    binaryReader.Read(bytes, 0, bytes.Length);
                    binaryReader.Close();
                }
                Stream stream = new MemoryStream(bytes);
                fileStream.Close();
                return stream;
            }
        }
        
    }
}