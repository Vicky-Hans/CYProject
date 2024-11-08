using System.Collections.Generic;
using DH.Log;
using Newtonsoft.Json;

namespace DH.Game
{
    public static class LogHelper
    {
        private class LogType
        {
            public const string UI = "UI";
            public const string Event = "Event";
            public const string Error = "Error";
        }
        
        private static void ReportEvent(string logType, string name, string extra)
        {
            ULogClient.ReportInfo(logType, name, extra);
        }

        public static void ReportUI(string name, string extra = "")
        {
            ReportEvent(LogType.UI, name, extra);
        }

        public static void ReportUI<T, K>(string name, Dictionary<T, K> dic)
        {
            var extra = JsonConvert.SerializeObject(dic);
            ReportUI(name, extra);
        }

        public static void ReportEvent(string name, string extra = "")
        {
            ReportEvent(LogType.Event, name, extra);
        }
        
        public static void ReportEvent<T, K>(string name, Dictionary<T, K> dic)
        {
            var extra = JsonConvert.SerializeObject(dic);
            ReportEvent(LogType.Event, name, extra);
        }

        public static void ReportError(string name, string extra = "")
        {
            ULogClient.ReportError(LogType.Error, name, extra);
        }
        
        public static void ReportError<T, K>(string name, Dictionary<T, K> dic)
        {
            var extra = JsonConvert.SerializeObject(dic);
            ULogClient.ReportError(LogType.Error, name, extra);
        }
    }
}