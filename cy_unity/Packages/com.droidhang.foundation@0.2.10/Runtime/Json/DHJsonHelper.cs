using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

namespace DHFramework
{
    internal sealed class JsonHelper : DHUtility.Json.IJsonHelper
    {
        public string ToJson(object obj)
        {
            return JsonMapper.ToJson(obj);
        }

        public T ToObject<T>(string json)
        {
            var dictionaryType = typeof(Dictionary<string, object>);
            if (typeof(T) == dictionaryType)
            {
                return (T)Facebook.MiniJSON.Json.Deserialize(json);
            }
            return JsonMapper.ToObject<T>(json);
        }

        public T ToObjectBeta<T>(string json)
        {
            return JsonMapper.ToObjectBeta<T>(json);
        }

        public T ToObject<T>(Stream json)
        {
            using (var reader = new StreamReader(json))
            {
                var jsonText = reader.ReadToEnd();
                return ToObject<T>(jsonText);
            }
        }

        public object ToObject(Type objectType, string json)
        {
            return JsonMapper.ToObject(json, objectType);
        }
    }
}

