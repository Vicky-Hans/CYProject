using System;
using System.IO;

namespace DHFramework
{
    public static partial class DHUtility
    {

        /// <summary>
        /// JSON 相关的实用函数。
        /// </summary>
        public static partial class Json
        {
            public class JsonIgnoreAttribute : Attribute
            {
                public JsonIgnoreAttribute()
                {
                    
                }
            }

            /// <summary>
            /// JSON 辅助器接口。
            /// </summary>
            public interface IJsonHelper
            {
                /// <summary>
                /// 将对象序列化为 JSON 字符串。
                /// </summary>
                /// <param name="obj">要序列化的对象。</param>
                /// <returns>序列化后的 JSON 字符串。</returns>
                string ToJson(object obj);

                /// <summary>
                /// 将 JSON 字符串反序列化为对象。
                /// </summary>
                /// <typeparam name="T">对象类型。</typeparam>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                T ToObject<T>(string json);

                /// <summary>
                /// 将 JSON 字符串反序列化为对象。
                /// 适用于无法具体得知类型的Json对象
                /// 当前API处于Beta测试阶段
                /// </summary>
                /// <typeparam name="T">对象类型。</typeparam>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                T ToObjectBeta<T>(string json);
                
                /// <summary>
                /// 将 JSON 字符串反序列化为对象。
                /// </summary>
                /// <typeparam name="T">对象类型。</typeparam>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                T ToObject<T>(Stream json);

                /// <summary>
                /// 将 JSON 字符串反序列化为对象。
                /// </summary>
                /// <param name="objectType">对象类型。</param>
                /// <param name="json">要反序列化的 JSON 字符串。</param>
                /// <returns>反序列化后的对象。</returns>
                object ToObject(Type objectType, string json);
            }

            /// <summary>
            ///  默认使用MiniJson作为Json序列化库
            /// </summary>
            private static IJsonHelper s_JsonHelper = new JsonHelper();

            /// <summary>
            /// 设置 JSON 辅助器。
            /// </summary>
            /// <param name="jsonHelper">要设置的 JSON 辅助器。</param>
            public static void SetJsonHelper(IJsonHelper jsonHelper)
            {
                s_JsonHelper = jsonHelper;
            }

            /// <summary>
            /// 将对象序列化为 JSON 字符串。
            /// </summary>
            /// <param name="obj">要序列化的对象。</param>
            /// <returns>序列化后的 JSON 字符串。</returns>
            public static string ToJson(object obj)
            {
                if (s_JsonHelper == null)
                {
                    throw new GameFrameworkException("JSON helper is invalid.");
                }

                try
                {
                    return s_JsonHelper.ToJson(obj);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(
                        DHUtility.Format("Can not convert to JSON with exception '{0}'.", exception.ToString()),
                        exception);
                }
            }

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObject<T>(string json)
            {
                if (s_JsonHelper == null)
                {
                    throw new GameFrameworkException("JSON helper is invalid.");
                }

                try
                {
                    return s_JsonHelper.ToObject<T>(json);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(
                        DHUtility.Format("Can not convert to object with exception '{0}'.", exception.ToString()),
                        exception);
                }
            }
            
            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// 适用于无法具体得知类型的Json对象
            /// 当前API处于Beta测试阶段
            /// </summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObjectBeta<T>(string json)
            {
                if (s_JsonHelper == null)
                {
                    throw new GameFrameworkException("JSON helper is invalid.");
                }

                try
                {
                    return s_JsonHelper.ToObjectBeta<T>(json);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(
                        DHUtility.Format("Can not convert to object with exception '{0}'.", exception.ToString()),
                        exception);
                }
            }
            
            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="stream">要反序列化的 JSON 二进制数据流。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObject<T>(Stream stream)
            {
                if (s_JsonHelper == null)
                {
                    throw new GameFrameworkException("JSON helper is invalid.");
                }

                try
                {
                    return s_JsonHelper.ToObject<T>(stream);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(
                        DHUtility.Format("Can not convert to object with exception '{0}'.", exception.ToString()),
                        exception);
                }
            }

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <param name="objectType">对象类型。</param>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static object ToObject(Type objectType, string json)
            {
                if (s_JsonHelper == null)
                {
                    throw new GameFrameworkException("JSON helper is invalid.");
                }

                if (objectType == null)
                {
                    throw new GameFrameworkException("Object type is invalid.");
                }

                try
                {
                    return s_JsonHelper.ToObject(objectType, json);
                }
                catch (Exception exception)
                {
                    if (exception is GameFrameworkException)
                    {
                        throw;
                    }

                    throw new GameFrameworkException(
                        DHUtility.Format("Can not convert to object with exception '{0}'.", exception.ToString()),
                        exception);
                }
            }
        }
    }
}