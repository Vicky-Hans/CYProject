using System.Collections.Generic;
using DH.Log;
using DHFramework;

namespace DH.Launch
{
    public partial class StartupEntry
    {
        private readonly BIDataEntity dataEntity = new BIDataEntity();
        private readonly string emptyEventInfo = DHUtility.Json.ToJson(new Dictionary<string, string>());

        internal void SendULogEvent(string eventCode, string eventName, string eventInfo = "")
        {
            ULogClient.ReportEvent("event", eventName, eventCode, string.IsNullOrEmpty(eventInfo) ? emptyEventInfo : eventInfo);
        }
    }
}