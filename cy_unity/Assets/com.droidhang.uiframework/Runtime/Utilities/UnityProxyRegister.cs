using System;
using System.Collections;
using System.Reflection;
using DH.UIFramework;
using DH.UIFramework.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework.Binding
{
    public class UnityProxyRegister
    {
        public static void Initialize()
        {
            
        }

        private static void Register<T, TValue>(string name, Func<T, TValue> getter, Action<T, TValue> setter)
        {
        }
    }
}