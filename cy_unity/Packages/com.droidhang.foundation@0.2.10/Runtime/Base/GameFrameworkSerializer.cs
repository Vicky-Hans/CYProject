using System.Collections.Generic;
using System.IO;

namespace DHFramework
{
    /// <summary>
    /// 游戏框架序列化器基类。
    /// </summary>
    /// <typeparam name="T">要序列化的数据类型。</typeparam>
    public abstract class GameFrameworkSerializer<T>
    {
        /// <summary>
        /// 初始化游戏框架序列化器基类的新实例。
        /// </summary>
        public GameFrameworkSerializer()
        {
        }

        /// <summary>
        /// 序列化数据到目标流中。
        /// </summary>
        /// <param name="stream">目标流。</param>
        /// <param name="data">要序列化的数据。</param>
        /// <param name="version">序列化回调函数的版本。</param>
        /// <returns>是否序列化数据成功。</returns>
        public abstract bool Serialize(Stream stream, T data);

        /// <summary>
        /// 从指定流反序列化数据。
        /// </summary>
        /// <param name="stream">指定流。</param>
        /// <returns>反序列化的数据。</returns>
        public abstract T Deserialize(Stream stream);

        /// <summary>
        /// 获取数据头标识。
        /// </summary>
        /// <returns>数据头标识。</returns>
        protected abstract string GetHeader();
    }
}