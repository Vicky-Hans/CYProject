using System;
using System.Reflection;
using UnityEngine;

namespace DHFramework
{
    /// <summary>
    /// 线程安全单例类，不许再添加任何静态对象，函数和类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> : IDisposable where T : class, new()
    {
        protected Singleton()
        {
            Initialization();
            //Debug.Log("construct Singleton over here");
        }

        public static T Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
                //Debug.Log("construct Nested over here");
            }

            internal static readonly T instance = new T();
        }

        /// <summary>
        /// 初始化虚函数，需重写
        /// </summary>
        protected virtual void Initialization()
        {
            //Debug.Log("virtual Initialization over here");
        }

        /// <summary>
        /// 销毁虚函数，需重写
        /// </summary>
        protected virtual void Release()
        {
            //Debug.Log("virtual UnInitialization over here");
        }

        /// <summary>
        /// 局部函数反射
        /// </summary>
        /// <param name="funcName"></param>
        private static void CallFunction(string funcName)
        {
            Type type = Nested.instance.GetType();
            var func = type.GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            func?.Invoke(Nested.instance, null);
            type = type.BaseType;
        }

        /// <summary>
        /// 释放函数
        /// </summary>
        public void Dispose()
        {
            Release();
        }
    }
}