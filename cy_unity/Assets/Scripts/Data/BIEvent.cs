using System.Collections.Generic;
using DH.Log;
using DHFramework;

namespace DH.Data
{
    public class BiEvent
    {
        private static readonly string EmptyEventInfo = DHUtility.Json.ToJson(new Dictionary<string, string>());
        
        /// <summary>
        /// 上报新手引导
        /// </summary>
        /// <param name="guideId"></param>
        /// <param name="subGuideId"></param>
        public static void ReportGuide(int guideId, int subGuideId = 0)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("guide_id", $"{guideId}");
            dic.Add("sub_guide_id", $"{subGuideId}"); //可选
            string eventInfo = DHUtility.Json.ToJson(dic);
            ULogClient.ReportEvent("event", "guide", "2100210018", string.IsNullOrEmpty(eventInfo) ? EmptyEventInfo : eventInfo);
        }
        
        /// <summary>
        /// 上报Info级别信息
        /// </summary>
        /// <param name="type">日志信息MainTag用于日志分类</param>
        /// <param name="name">日志信息SubTag</param>
        /// <param name="extra">额外信息，可以使用常规字符串或者json字符串(建议传入json)</param>
        public static void ReportInfo(string type, string name, string extra)
        {
            ULogClient.ReportInfo(type, name, extra);
        }

        /// <summary>
        /// 上报Error级别信息
        /// </summary>
        /// <param name="type">日志信息MainTag用于日志分类</param>
        /// <param name="name">日志信息SubTag</param>
        /// <param name="extra">额外信息，可以使用常规字符串或者json字符串(建议传入json)</param>
        public static void ReportError(string type, string name, string extra)
        {
            ULogClient.ReportError(type, name, extra);
        }
    }
}