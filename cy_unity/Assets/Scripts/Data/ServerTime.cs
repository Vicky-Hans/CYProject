using System;
using DH.Config;
using DHFramework;

namespace DH.Data
{
    public class ServerTime : Singleton<ServerTime>
    {
        /// <summary>
        /// 同步数据时的本地时间
        /// </summary>
        public long PullTime { get; set; }
        /// <summary>
        /// 同步数据时的服务器时间
        /// </summary>
        public long SvrTime { get; set; }

        /// <summary>
        /// 同步数据时的服务器时间初始化
        /// </summary>
        /// <param name="svrTime"></param>
        public void Init(long svrTime)
        {
            PullTime = Lodash.GetUnixTime();
            SvrTime = svrTime;
        }
        
        public string Time2Yymmdd(long timeStamp)
        {
            var origin = Lodash.GetOriginDateTime();
            return origin.AddSeconds(timeStamp).ToString("yyy-MM-dd");
        }

        public string Time2YymmddHHmm(long timeStamp)
        {
            var origin = Lodash.GetOriginDateTime();
            return origin.AddSeconds(timeStamp).ToString("yyyy-MM-dd HH:mm");
        }

        public string Time2YymmddHHmmss(long timeStamp)
        {
            var origin = Lodash.GetOriginDateTime();
            return origin.AddSeconds(timeStamp).ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        public long String2Time(string dateTime)
        {
            return Lodash.GetUnixTime(dateTime);
        }
        
        /// <summary>
        /// 获取当前服务器时间
        /// </summary>
        /// <returns></returns>
        public long GetNowTime()
        {
            return SvrTime + Lodash.GetUnixTime() - PullTime;
        }

        /// <summary>
        /// 自上次同步数据后经过的时间
        /// </summary>
        /// <returns></returns>
        public long PassedTime()
        {
            return GetNowTime() - SvrTime;
        }

        /// <summary>
        /// 离指定时间还有多少秒
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public long RemainTime(long endTime)
        {
            return endTime - GetNowTime();
        }
        
        /// <summary>
        /// 离指定时间还有多少秒
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public long RemainTime(string endTime)
        {
            return RemainTime(String2Time(endTime));
        }
        
        /// <summary>
        /// 活动是否处于开放时间
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool IsOpenTime(long startTime, long endTime)
        {
            var nowTime = GetNowTime();
            return nowTime >= startTime && nowTime < endTime;
        }
        
        /// <summary>
        /// 活动是否处于开放时间 只需要考虑结束时间
        /// </summary>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool IsOpenTime(long endTime)
        {
            var nowTime = GetNowTime();
            return nowTime < endTime;
        }

        /// <summary>
        /// 活动是否处于开放时间
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public bool IsOpenTime(string startTime, string endTime)
        {
            var nowTime = GetNowTime();
            return nowTime >= String2Time(startTime) && nowTime < String2Time(endTime);
        }
        
        
        /// <summary>
        /// 把second转换成HHMMSS格式
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public string Seconds2ShowTime(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            if (time.Days <= 0)
            {
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
            else
            {
                var dayDesc = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime01).Name;
                var hourDesc = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime02).Name;
                var minDesc = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime03).Name;
                return $"{time.Days:D2}{dayDesc}{time.Hours:D2}{hourDesc}{time.Minutes:D2}{minDesc}";
            }
        }

        
        /// <summary>
        /// 把second转换成HHMMSS格式
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public string Seconds2Hhmmss(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
        /// <summary>
        /// 只显示xx时xx分xx秒
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public  string ConvertSecondsToTime(long totalSeconds)
        {
            long days = totalSeconds / (24 * 3600); // 计算天数
            long hours = (totalSeconds % (24 * 3600)) / 3600; // 计算小时
            long minutes = (totalSeconds % 3600) / 60; // 计算分钟
            long seconds = totalSeconds % 60; // 计算秒

            var hours1 = hours + (days * 24);
            string hours2 = hours1 < 10 ? $"0{hours1}" : hours1+"";
            string minutes2 = minutes < 10 ? $"0{minutes}" : minutes+"";
            string seconds2 = seconds < 10 ? $"0{seconds}" : seconds+"";
            return $"{hours2}:{minutes2}:{seconds2}";
        }
        
        public string Seconds2Hhmm(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            return $"{time.Hours:D2}:{time.Minutes:D2}";
        }

        public string Seconds2Mmss(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
        
        
        public string SecondsDHms(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            var d = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime01).Name;
            var h = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime02).Name;
            var m = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime03).Name;
            //var s = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime04).Name;
            if (time.Days > 0)
            {
                return $"{time.Days:D2}{d}{time.Hours:D2}{h}{time.Minutes:D2}{m}";
            }
            else
            {
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }
        public string SecondsDHAndMS(long seconds)
        {
            var time = new System.TimeSpan(seconds * 10000000);
            var d = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime01).Name;
            var h = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime02).Name;
            var m = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime03).Name;
            //var s = ConfigCenter.GlobalLanguageCfgColl.GetDataById(GlobalLanguageId.TfTime04).Name;
            if (time.Days > 0)
            {
                return $"{time.Days:D2}{d} {time.Hours:D2}{h}";
            }
            else
            {
                return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }

        #region 计算天数

        public  int GetDay(long time)
        {
            return (int)time / 86400;
        }
        
        public  int GetPassDay(long time)
        {
            var temp = GetNowTime() - time;
            return (int)(temp / 86400) + (temp%86400>0?1:0);
        }


        /// <summary>
        /// 计算当前时间是第几天 (0为计算现在到1970 0 0时间)
        /// </summary>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        public  int GetTimeDay(long lastTime = 0)
        {
            if (lastTime == 0)
            {
                return GetDay(GetNowTime());
            }
            else
            {
                return GetDay(GetNowTime()) - GetDay(lastTime);
            }
        }

        /// <summary>
        /// 距离第二天凌晨还有多少秒
        /// </summary>
        /// <returns></returns>
        public long SecondDaySeconds()
        {
            // 获取当前时间戳（Unix 时间戳）
            
            DateTime currentTime = Lodash.GetOriginDateTime().AddSeconds(GetNowTime());;
            long currentTimestamp = (long)(currentTime - Lodash.GetOriginDateTime()).TotalSeconds;
            // 获取今天凌晨十二点的时间戳
            DateTime todayMidnight = currentTime.Date;
            DateTime todayMidnightPlus12 = todayMidnight.AddHours(24);
            long midnightTimestamp = (long)(todayMidnightPlus12 - Lodash.GetOriginDateTime()).TotalSeconds;

            // 计算距离当前凌晨十二点还有多久
            return midnightTimestamp - currentTimestamp;
            
        }

        #endregion
        
    }
}