using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHFramework.Json
{
    public static class JsonExtension
    {
        public static T ReadValue<T>(this Dictionary<string, object> dic, string key)
        {
            if (dic.TryGetValue(key, out var objValue))
            {
                return (T) objValue;
            }

            return default(T);
        }
    }
}
