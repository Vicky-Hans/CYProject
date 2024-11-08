using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;

namespace DHFramework
{
    public class DHLog
    {
        [Conditional("DH_DEBUG")]
        public static void Debug(string message)
        {
            Debug(message, null);
        }

        [Conditional("DH_DEBUG")]
        public static void Debug(string text, params object[] par)
        {
            UnityEngine.Debug.Log(GetOutput(text, par));
        }

        [Conditional("DH_DEBUG")]
        public static void Debug(bool print, string text, params object[] par)
        {
            if (print)
                UnityEngine.Debug.Log(GetOutput(text, par));
        }

        public static void Warning(string message)
        {
            Warning(message, null);
        }
        
        public static void Warning(string text , params object[] par){
            UnityEngine.Debug.LogWarning(GetOutput(text,par));
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
        
        public static void Error(string text, params object[] par)
        {
            UnityEngine.Debug.LogError(GetOutput(text, par));
        }

        [Conditional("DH_DEBUG")]
        public static void LogAssert(bool val, string text, params object[] par)
        {
            if (!val)
            {
                UnityEngine.Debug.LogError(GetOutput(text, par));
            }
        }

        private static string GetOutput(string s, params object[] par)
        {
            var output = "";
            output = par == null ? s : string.Format(s, par);
            
            return string.Concat("[", GetTime(), "] : ", output);
        }

        //private const string OUTPUT_FORMAT = "[{0}] : {1}";
        private static string GetTime()
        {
            return DateTime.Now.ToLocalTime().ToString(CultureInfo.InvariantCulture);
        }
    }
}