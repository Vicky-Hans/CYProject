using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace DH.NativeCore.MonoSingleton
{
    /// <summary>
    /// 继承了MonoBehaviour的单例
    /// 子类不要覆盖Awake函数
    /// 执行初始化重写Init虚函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _Instance = default(T);
        private static bool released = false;

        public static T Instance
        {
            get
            {
                if(released)
                {
                    return null;
                }

                //先寻找场景中有无预制
                if (!_Instance)
                {
                    _Instance = FindObjectOfType<T>();
                }

                //没有则生成
                if (!_Instance)
                {
                    var singletonObj = new GameObject($"@{typeof(T).Name}");
                    _Instance = singletonObj.AddComponent<T>();
                }

                return _Instance;
            }
        }

        /// <summary>
        /// 初始化虚函数，子类重写
        /// </summary>
        protected virtual void Init()
        {
            
        }

        protected virtual void Release()
        {
            
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            //Debug.Log("Awake" + gameObject.name);
            Init();
        }

        private void OnDestroy()
        {
            Release();
            released = true;
            //Debug.Log("OnDestroy1"+ gameObject.name);
            _Instance = null;
            //Debug.Log("OnDestroy2"+ gameObject.name);
        }
    }
    
}