using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DH.Config;
using DH.Proto;
using DHFramework;
using UnityEngine;
using Random = System.Random;

namespace DH.Data
{
    public static class Lodash
    {
        private static Random random = new(123456);

        public static Random Random => random;

        public static void SetSeed(int seed)
        {
            random = new Random(seed);
        }

        public static T RandomOneInList<T>(this List<T> list)
        {
            return list.Skip(random.Next(list.Count)).First();
        }

        /// <summary>
        /// 进制转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="toBase"></param>
        /// <returns></returns>
        public static int FeelTheBase(int value, int toBase = 2)
        {
            var binaryString = Convert.ToString(value, toBase);
            return int.Parse(binaryString);
        }

        /// <summary>
        ///  进制转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="soureBase"></param>
        /// <returns></returns>
        public static int FeelTheBase(string value, int soureBase = 2)
        {
            var number = Convert.ToInt32(value, soureBase);
            return number;
        }
        

        /// <summary>
        /// 检查资源是否足够
        /// </summary>
        /// <param name="reward"></param>
        /// <returns></returns>
        public static bool CheckRewardIsEnough(Reward reward,int num=1)
        {
            if (reward == null) return true;
            bool ret = false;
            switch (reward.Type)
            {
                case RewardType.Item:
                    ret = DataCenter.itemsData.CheckItemIsEnough(reward.Id, reward.Count*num);
                    break;
                case RewardType.Lives:
                    ret = DataCenter.livesData.CheckItemIsEnough(reward.Id, reward.Count*num);
                    break;
            }

            return ret;
        }


        /// <summary>
        /// 检查资源是否足够
        /// </summary>
        /// <param name="rewards"></param>
        /// <returns></returns>
        public static bool CheckRewardIsEnough(List<Reward> rewards)
        {
            if (rewards == null) return true;
            bool ret = true;
            foreach (var reward in rewards)
            {
                if (!CheckRewardIsEnough(reward))
                {
                    ret = false;
                    break;
                }
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandRange(int min, int max)
        {
            if (min == max) return min;
            return random.Next(min, max);
        }

        public static float RandRangeFloat(float min, float max)
        {
            if (min == max) return min;
            var val = (float)random.NextDouble();
            return min + val * (max - min);
        }

        public static Vector3 PosOnCircle(Vector3 pos, float radius)
        {
            if (radius == 0) return pos;

            var theta = random.NextDouble() * 2 * Math.PI;
            var x = (float)Math.Cos(theta) * radius;
            var y = (float)Math.Sin(theta) * radius;
            return pos + new Vector3(x, y, 0);
        }

        public static Vector3 PosInCircle(Vector3 pos, float radius)
        {
            if (radius == 0) return pos;
            radius = RandRangeFloat(0, radius);
            var theta = random.NextDouble() * 2 * Math.PI;
            var x = (float)Math.Cos(theta) * radius;
            var y = (float)Math.Sin(theta) * radius;
            return pos + new Vector3(x, y, 0);
        }

        public static Vector3 PosOnBoxCorner(Vector3 pos, float edge)
        {
            var x = random.NextDouble() > 0.5 ? 1 : -1;
            var y = random.NextDouble() > 0.5 ? 1 : -1;
            return pos + new Vector3(x * edge * 0.5f, y * edge * 0.5f, 0);
        }

        public static Vector3 PosInRect(Vector3 pos, Vector2 size)
        {
            var x = RandRangeFloat(-size.x/2, size.x/2);
            var y = RandRangeFloat(-size.y/2, size.y/2);
            return pos + new Vector3(x, y, 0);
        }

        public static Vector3 Angle2Direction(float angle)
        {
            var theta = Mathf.Deg2Rad * angle;
            var x = (float)Math.Cos(theta);
            var y = (float)Math.Sin(theta);
            return new Vector3(x, y, 0);
        }

        public static float Direction2Angle(Vector3 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI;
        }

        /// <summary>
        /// 垂线 到胶囊体最近点
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Vector3 NearestPosOnCapsule(Vector3 pos, Vector2 size)
        {
            var radius = size.y * 0.5f;
            var width = size.x - size.y;
            if (pos.x <= width / 2 && pos.x >= -width / 2)
            {
                return new Vector3(pos.x, Mathf.Sign(pos.y) * radius, 0);
            }

            if (Mathf.Abs(pos.x) > size.x * 0.5f)
            {
                var oPos = pos.x > 0 ? new Vector3(width / 2, 0, 0) : new Vector3(-width / 2, 0, 0);
                var direct = (pos - oPos).normalized * radius;
                return oPos + direct;
            }

            var cx = Mathf.Abs(pos.x) - width * 0.5f;
            var cy = Mathf.Sqrt(radius * radius - cx * cx);
            return new Vector3(pos.x, cy * Mathf.Sign(pos.y), 0);
        }

        /// <summary>
        /// pos 是否在 胶囊体内
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool IsInCapsule(Vector3 pos, Vector2 size)
        {
            var radius = size.y * 0.5f;
            var width = size.x - size.y;
            if (pos.x <= width / 2 && pos.x >= -width / 2)
            {
                return Mathf.Abs(pos.y) <= radius;
            }

            if (Mathf.Abs(pos.x) > size.x * 0.5f) return false;
            var cx = Mathf.Abs(pos.x) - width * 0.5f;
            var cy = Mathf.Sqrt(radius * radius - cx * cx);
            return Mathf.Abs(pos.y) <= cy;
        }
        
        public static float DistanceToCapsule(Vector2 pos, CapsuleCollider2D capsule)
        {
            var closestPoint = capsule.ClosestPoint(pos);
            var radius = capsule.size.y / 2f;
            float distance = Vector2.Distance(pos, closestPoint);
            if (Vector2.Distance(pos, capsule.bounds.center) < radius)
            {
                return (radius-distance) * (radius-distance);
            }
            return (radius+distance)*(radius+distance);
        }

        /// <summary>
        /// 触发概率 万分比
        /// </summary>
        /// <param name="probabilityPercentage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool ShouldTrigger(int probabilityPercentage)
        {
            if (probabilityPercentage < 0 || probabilityPercentage > GameConst.TenThousand)
            {
                throw new ArgumentOutOfRangeException("probabilityPercentage",
                    "Probability percentage must be between 0 and " + GameConst.TenThousand + ".");
            }

            int randomNumber = random.Next(0, GameConst.TenThousand);
            return randomNumber <= probabilityPercentage;
        }

        public static DateTime GetOriginDateTime()
        {
            return new DateTime(1970, 1, 1);
        }
        
        public static long GetUnixTime()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds();
        }
        public static long GetUnixTimeMs()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
        
        public static long GetUnixTime(string dateTime)
        {
            return DateTimeOffset.Parse(dateTime).ToUnixTimeSeconds();
        }

        public static string GetKNum(long num)
        {
            if(num < 1000)
            {
                return DHUtility.Format("{0}", num);
            }
            var tmpNum = num * 0.001f;
            var tmpNumStr = DHUtility.Format("{0:0.0}k", tmpNum);
            return tmpNumStr;
        }

        public static int RoundToInt(float value)
        {
            return (int)(value + 0.5f);
        }

        /// <summary>
        /// 解析二进制的指定位的值是否是 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pos"></param>
        public static int ParsePosValue(int value, int pos)
        {
            return 1 << (pos - 1) & value;
        }

        public static int ParseStateValue(int value, int index)
        {
            return (1 << (index - 1)) + value;
        }
        
        /// <summary>
        /// 使用二阶贝塞尔曲线的参数方程计算点的位置
        /// </summary>
        /// <param name="t">时间</param>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">结束岱年</param>
        /// <param name="controlPos">控制点</param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t, Vector3 startPos, Vector3 endPos, Vector3 controlPos)
        {
            float s = 1.0f - t;
            var point = startPos * s * s + controlPos * 2 * s * t + endPos * t * t;
            return point;
        }

        /// <summary>
        /// 使用三阶贝塞尔曲线的参数方程计算点的位置
        /// </summary>
        /// <param name="t">时间</param>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">结束岱年</param>
        /// <param name="controlPos1">控制点</param>
        /// <param name="controlPos2">控制点</param>
        /// <returns></returns>
        public static Vector3 CalculateBezierPoint(float t, Vector3 startPos, Vector3 endPos, Vector3 controlPos1, Vector3 controlPos2)
        {
            float s = 1.0f - t;
            var point = startPos * s * s * s + controlPos1 * s * s * t * 3.0f + controlPos2 * s * t * t * 3.0f + endPos * t * t * t;
            return point;
        }
        
        

        public static string ConvertNumToString(long num, int toFixed = 0)
        {
            var str = num.ToString();
            var temp = Math.Pow(10, toFixed);
            if (num >= 1e5 && num < 1e6)
            {
                str = $"{(Math.Floor(num / 1e3 * temp) / temp)}K";
            }
            else if (num >= 1e6 && num < 1e9)
            {
                str = $"{(Math.Floor(num / 1e6 * temp) / temp)}M";
            }
            else if (num >= 1e9)
            {
                str = $"{(Math.Floor(num / 1e9 * temp) / temp)}B";
            }

            return str;
        }

        public static float GetResolutionRation()
        {
            Vector2 designResolution = new Vector2(1080, 1920);
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float aspectRatio = screenWidth / screenHeight;
            float designAspectRatio = designResolution.x / designResolution.y;
            float resolutionRatio = aspectRatio / designAspectRatio;
            
            return resolutionRatio;
        }
        
        /// <summary>
        /// 洗牌算法
        /// </summary>
        /// <param name="list"></param>
        public static List<int> GetListFisherYates(List<int> list)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                int j = RandRange(i, list.Count -1); 
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        /// <summary>
        /// 列表分割
        /// </summary>
        /// <param name="originalList"></param>
        /// <param name="chunkSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<List<T>> SplitList<T>(List<T> originalList, int chunkSize)
        {
            List<List<T>> resultList = new List<List<T>>();
            for (int i = 0; i < originalList.Count; i += chunkSize)
            {
                List<T> chunk = originalList.GetRange(i, Mathf.Min(chunkSize, originalList.Count - i));
                resultList.Add(chunk);
            }
            return resultList;
            
        }
        #region 奖励增删
        public static void DealRewards(List<Resource> rewards, List<Resource> delRewrads)
        {
            DealRewards(rewards);
            DealRewards(delRewrads,false);
        }
        
        public static void DealRewards(List<Resource> rewards, bool isAdd = true)
        {
            if(rewards==null) return;
            foreach (var reward in rewards) DealRewards(reward, isAdd);
        }

        public static void DealRewards(List<Reward> rewards, bool isAdd = true)
        {
            if(rewards==null) return;
            foreach (var reward in rewards) DealRewards(reward, isAdd);
        }

        public static void DealRewards(Resource[] rewards, bool isAdd = true)
        {
            if(rewards==null) return;
            foreach (var reward in rewards) DealRewards(reward, isAdd);
        }

        public static void DealRewards(Reward[] rewards, bool isAdd = true)
        {
            if(rewards==null) return;
            foreach (var reward in rewards) DealRewards(reward, isAdd);
        }

        public static void DealRewards(ResourceData[] rewards, bool isAdd = true)
        {
            if(rewards==null) return;
            foreach (var reward in rewards) DealRewards(reward, isAdd);
        }

        public static void DealRewards(Reward reward, bool isAdd)
        {
            if(reward==null) return;
            DealReward(reward.Type, reward.Id, reward.Count, isAdd);
        }

        public static void DealRewards(ResourceData reward, bool isAdd = true)
        {
            if(reward==null) return;
            DealReward((RewardType)reward.Type, reward.Id, reward.Count, isAdd,reward.HeroEquip);
        }
        public static void DealRewards(Resource reward, bool isAdd = true)
        {
            if(reward==null) return;
            HeroEquipData heroData = null;
            if (reward.HeroEquip != null)
            {
                heroData = new HeroEquipData(reward.HeroEquip);
            }
            DealReward((RewardType)reward.Type, reward.Id, reward.Count, isAdd,heroData);
        }
        public static void DealReward(RewardType type, int id, long count, bool isAdd,HeroEquipData heroEquip=null)
        {
            switch (type)
            {
                
                case RewardType.Item:
                    DataCenter.itemsData.ChangeItemsCount(id, count, isAdd);
                    break;
                case RewardType.Exp:
                    DataCenter.charcaterData.Digest.ChangeExp((int)count);
                    break;
                case RewardType.Lives:
                    DataCenter.livesData.DealLivesChange((int)count,isAdd);
                    break;
                case RewardType.Head:
                    DataCenter.charcaterData.Digest.AddHead(id);
                    break;
                case RewardType.HeroEquip:
                    DataCenter.clothesData.AddHeroEquip(heroEquip);
                    break;
                case RewardType.MagicCredit:
                    DataCenter.collegeData.AddScore((int)count);
                    break;
                case RewardType.Secret:
                    DataCenter.secretData.ChangeSecretScore((int)count,isAdd);
                    break;
                default:
                    DHLog.Error($"没处理的类型 ，请及时处理 {type}");
                    break;
            }
        }

        #endregion
    }
}