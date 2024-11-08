using System;
using System.Collections.Generic;
using System.IO;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Foundations.Event;
using DH.Game;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using DH.UIFramework;
using DH.UIFramework.Observables;
using DHFramework;
using UnityEngine;

namespace Game.UI.Guide
{
    public enum EGuideType
    {
        /// <summary>
        /// 弱引导
        /// </summary>
        GuideTypeTips = 0,
        /// <summary>
        /// 强制引导
        /// </summary>
        GuideTypeForce,
        /// <summary>
        /// 关卡引导
        /// </summary>
        GuideTypeLevel=3
        
    }
    public enum EGuideState
    {
        /// <summary>
        /// 锁住
        /// </summary>
        GuideStateLock = 0,
        /// <summary>
        /// 未开始
        /// </summary>
        GuideStateReady,
        /// <summary>
        /// 进行中
        /// </summary>
        GuideStateRunning,
        /// <summary>
        /// 完成
        /// </summary>
        GuideStateComplete,
    }

    public partial class GuideData:ObservableObject
    {
        [AutoNotify] private GuideCfg cfg;
        [AutoNotify] EGuideState state;
        
        public GuideData(GuideCfg cfg, EGuideState guideState)
        {
            Cfg = cfg;
            State = guideState;
        }
    }

    /// <summary>
    /// 游戏中的引导
    /// </summary>
    public partial class GuideManager: DH.Game.ObservableSingleton<GuideManager>
    {
        private Transform guideRoot;   
        [AutoNotify] private ObservableDictionary<int, GuideData> guideInfoDictionary = new ();
        [AutoNotify] private ObservableDictionary<int, GuideCfg> guideConfigs = new ();
        private GuideView guideView = null;
        private GuideViewModel guideVm = null;
        [AutoNotify] private MainMergeViewModel mainMeirgeVm;
        [AutoNotify] private bool isTriggerLevelGuide = true;
        [AutoNotify] private int curGuideId;
        

        public int EnterCount
        {
            get
            {
                var key = GetEnterGameCount();
                return DHUnityUtil.PlayerPrefs.GetInt(key, 0);
            }
            set
            {
                var key = GetEnterGameCount();
                DHUnityUtil.PlayerPrefs.SetInt(key, value);
            }
        }

        private string GetEnterGameCount()
        {
            return  $"{DataCenter.charcaterData.Digest.RoleId}_{GameConst.EnterGameCount}";
        }

        /// <summary>
        ///  玩家ID
        /// </summary>
        public void Init()
        {
            Clear();
            guideRoot = UIManager.Instance.GetUILayerRoot(UILayersConfig.Guide);
            GuideInfoDictionary.Clear();
            GuideConfigs.Clear();
            InitGuideInfoDictionary();
            LoadAndResetGuideInfoDictionary();
            CreateGuideView();
        }

        public void Clear()
        {
            if (guideVm != null)
            {
                guideVm.Dispose();
                guideVm = null;
            }

            if (guideView != null)
            {
                AssetsManager.ReleaseInstance(guideView.gameObject);
                guideView = null;
            }
        }



        /// <summary>
        /// 玩家引导文件的名字
        /// </summary>
        /// <returns></returns>
        private string GetGuideFileName()
        {
            int roleId = DataCenter.charcaterData.Digest.RoleId;
            return $"{roleId}GuideInfo";
        }
        /// <summary>
        /// 初始化 读配置表 引导信息
        /// </summary>
        private void InitGuideInfoDictionary()
        {
            var cfgs = ConfigCenter.GuideCfgColl.DataItems;
            foreach (var cfg in cfgs)
            {
                GuideConfigs.Add(cfg.Id, cfg);
                GuideData guideInfo = new GuideData(cfg,EGuideState.GuideStateLock);
                GuideInfoDictionary.Add(cfg.Id, guideInfo);
            }
            foreach (var item in GuideInfoDictionary)
            {
                // DHLog.Debug($"init guide info {item.Key}");
                var cfg = GuideConfigs[item.Key];
                // 设置引导的第一步为准备状态
                if (cfg.FirstId == 0)
                {
                    item.Value.State = EGuideState.GuideStateReady;
                }
            }
        }

        public bool CheckIsTrigger(int guideId)
        {
            if(guideConfigs.TryGetValue(guideId, out var cfg))
            {
                if (cfg.Type == 3)
                {
                    return isTriggerLevelGuide;
                }
            }

            return true;
        }

        private void LoadAndResetGuideInfoDictionary()
        {
            var localInfo = LoadPlayerGuideData();
            foreach (var item in localInfo)
            {
                if(guideConfigs.TryGetValue(item.Key, out var cfg))
                {
                    if (cfg.Type == 3)
                    {
                        isTriggerLevelGuide = false;
                    }
                }
            }
            // 要是有存档 并且不是第一关 就跳过引导
            if (DataCenter.mainStageData.HasArchive || DataCenter.mainStageData.CurrChapter != 1)
            {
                isTriggerLevelGuide = false;
            }
            if (GameConst.IsIosAuditState) isTriggerLevelGuide = false;
            
            foreach (var item in GuideInfoDictionary)
            {
                if(item.Value.State == EGuideState.GuideStateComplete) continue;
                UpdateTaskListState(item.Key, localInfo);
            }
            
            // List<int> testCompleteList = new List<int>() {101,102,103,104};
            // foreach (var item in testCompleteList)
            // {
            //     var cfg = GuideConfigs[item];
            //     GuideInfoDictionary[item].State = EGuideState.GuideStateComplete;;
            //     if(cfg.NextId != 0)
            //     {
            //         GuideInfoDictionary[cfg.NextId].State = EGuideState.GuideStateReady;
            //     }
            // }
        }

        /// <summary>
        /// 跟新任务状态
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="saveInfo"></param>
        private void UpdateTaskListState(int taskId, Dictionary<int, EGuideState> saveInfo)
        {
            if (saveInfo.TryGetValue(taskId, out EGuideState state))
            {
                var cfg = GuideConfigs[taskId];
                GuideInfoDictionary[taskId].State = state;
                if(cfg.NextId != 0)
                {
                    GuideInfoDictionary[cfg.NextId].State = EGuideState.GuideStateReady;
                    UpdateTaskListState(cfg.NextId, saveInfo);
                }
            }
        }


        /// <summary>
        /// 加载玩家的引导文件
        /// </summary>
        /// <returns></returns>
        public Dictionary<int,EGuideState> LoadPlayerGuideData()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, GetGuideFileName());
                string loadedJson = File.ReadAllText(filePath);
                // DHLog.Debug($"muzili log guide data is load {loadedJson}");
                
                Dictionary<int,EGuideState> loadedData = DHUtility.Json.ToObject<Dictionary<int,EGuideState>>(loadedJson);
                if (loadedData == null)
                {
                    Dictionary<int,EGuideState> ret = new ();
                    SavePlayerGuideData();
                    return ret;
                }
                return loadedData;
            }
            catch (Exception e)
            {
                // DHLog.Debug($"muzili log guide data is load error {e}");
                Dictionary<int,EGuideState> ret = new ();
                SavePlayerGuideData();
                return ret;
            }
        }
        /// <summary>
        /// 存玩家的 引导数据 只保存完成的
        /// </summary>
        private void SavePlayerGuideData()
        {
            // 存储数据
            string filePath = Path.Combine(Application.persistentDataPath, GetGuideFileName());
            var saveDic = new Dictionary<int,EGuideState>();
            foreach (var item in GuideInfoDictionary)
            {
                if(item.Value.State == EGuideState.GuideStateComplete)
                {
                    saveDic.Add(item.Key, item.Value.State);
                }
            }
            string json = DHUtility.Json.ToJson(saveDic);
            // 存储 JSON 数据到文件
            File.WriteAllText(filePath, json);
        }
        
        private void UpdateGuideDataState(int id, EGuideState state)
        {
            if (GuideInfoDictionary.ContainsKey(id))
            {
                GuideInfoDictionary[id].State = state;
            }
        }
        
        private Dictionary<string, string> GetGuideReportDic(string reportStr)
        {
            Dictionary<string, string> reportMap = new();
            string[] reportStrArray = reportStr.Split("&");
            for (int i = 0; i < reportStrArray.Length; i++)
            {
                string[] param = reportStrArray[i].Split("=");
                if (param.Length > 1)
                {
                    reportMap.Add(param[0],param[1]);
                }
                else
                {
                    reportMap.Add(param[0], "click");
                }
            }
            return reportMap;
        }

        public void ReportGuideInfo(string reportStr, GameObject infoNode, GameObject target = null)
        {
           
            var reportMap = GetGuideReportDic(reportStr);
            ReportGuideInfo(reportMap, infoNode, target);
        }

        /// <summary>
        /// 引导触发上报
        /// </summary>
        /// <param name="repMap"></param>
        /// <param name="infoNode"></param>
        /// <param name="target"></param>
        public void ReportGuideInfo(Dictionary<string, string> repMap, GameObject infoNode, GameObject target = null)
        {
            Dictionary<string, string> reportMap =repMap;
            string logStr = "";
            foreach (var item in repMap)
            {
                logStr = $"{logStr} {item.Key} = {item.Value} \t";
            }
            // DHLog.Debug($"muzili log guide reportCheckGuide  string is  {logStr}");

            foreach (var item in GuideInfoDictionary)
            { 
                // 锁住的 或者 完成的不管
                if(item.Value.State is EGuideState.GuideStateLock or EGuideState.GuideStateComplete) continue;

                var cfg = GuideConfigs[item.Key];
                // 检查触发
                if (item.Value.State == EGuideState.GuideStateReady)
                {
                    bool isTrigger = CheckIsTrigger(cfg, reportMap);
                    if (isTrigger)
                    {
                        GuideInfoDictionary[item.Key].State = EGuideState.GuideStateRunning;
                        // 通知触发
                        EventDispatcher.TriggerEvent(GameConst.EventCode.GuideTrigger, item.Key,infoNode,target);
                        
                        // DHLog.Debug($"muzili log guide trigger {item.Key}");
                        break;
                    }

                } else if (item.Value.State == EGuideState.GuideStateRunning)
                {
                    bool isComplete = CheckIsComplete(cfg, reportMap);
                    if (isComplete)
                    {
                        // 完成了
                        GuideInfoDictionary[item.Key].State = EGuideState.GuideStateComplete;
                        // 通知触发
                        EventDispatcher.TriggerEvent(GameConst.EventCode.GuideComplete, item.Key);
                        // 保存进度
                        SavePlayerGuideData();
                        // DHLog.Debug($"muzili log guide complete {item.Key}");
                        ReportBiLog(item.Key);
                        if (cfg.NextId != 0)
                        {
                            UpdateGuideDataState(cfg.NextId, EGuideState.GuideStateReady);
                        }
                        else
                        {
                            IsTriggerLevelGuide = false;
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有触发的引导
        /// </summary>
        /// <param name="reportStr"></param>
        /// <returns></returns>
        public bool CheckGuideIsTrigger(string reportStr)
        {
            var reportDic = GetGuideReportDic(reportStr);
            return CheckGuideIsTrigger(reportDic);
        }
        /// <summary>
        /// 检查是否有触发的引导
        /// </summary>
        /// <param name="reportMap"></param>
        /// <returns></returns>
        public bool CheckGuideIsTrigger(Dictionary<string, string> reportMap)
        {
            bool ret = false;
            foreach (var item in GuideInfoDictionary)
            {
                var cfg = GuideConfigs[item.Key];
                if (item.Value.State is EGuideState.GuideStateReady or EGuideState.GuideStateRunning)
                {
                    ret = CheckIsTrigger(cfg, reportMap);
                    if(ret) break;
                }
            }
            return ret;
        }

        

        /// <summary>
        /// 检查是否触发
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="reportMap"></param>
        private bool CheckIsTrigger(GuideCfg cfg, Dictionary<string,string> reportMap)
        {
            bool isTrigger = true;
            if (cfg.TriggerStr != null)
            {
                foreach (var item in cfg.TriggerStr)
                {
                    if (!reportMap.TryGetValue(item.Key, out string  tempStr))
                    {
                        isTrigger = false;
                        break;
                    }
                    if (String.Compare(tempStr, item.Value, StringComparison.Ordinal) != 0)
                    {
                        isTrigger = false;
                        break;
                    }
                }
            }
            return isTrigger;
        }

        /// <summary>
        /// 检查引导是否完成
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="reportMap"></param>
        /// <returns></returns>
        private bool CheckIsComplete(GuideCfg cfg, Dictionary<string,string> reportMap)
        {
            bool ret = true;
            if (cfg.CompleteStr != null)
            {
                foreach (var item in cfg.CompleteStr)
                {
                    if (!reportMap.TryGetValue(item.Key, out string  tempStr))
                    {
                        ret = false;
                        break;
                    }
                    if (String.Compare(tempStr, item.Value, StringComparison.Ordinal) != 0)
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }
        
        /// <summary>
        /// 上报 中台 引导
        /// </summary>
        /// <param name="guideId"></param>
        private void ReportBiLog(int guideId)
        {
            BiEvent.ReportGuide(guideId);
        }

        private async void CreateGuideView()
        {
            if(guideView != null) return;
            var path = "UI/Guide/GuideView";
            var guidePanel= await AssetsManager.InstantiateWithParentAsync(path, guideRoot, false);
            guideView = guidePanel.GetComponent<GuideView>();
            guideVm = new GuideViewModel();
            guideView.SetDataContext(guideVm);
        }

        public GuideCfg GetGuideCfg(int guideId)
        {
            if (GuideConfigs.TryGetValue(guideId, out GuideCfg cfg))
                return cfg;
            return null;
        }

        public void StopGuideAction()
        {
            if (guideVm != null)
            {
                guideVm.StopHandAction();
            }
        }
        /// <summary>
        /// 获取引导状态
        /// </summary>
        /// <param name="guideId"></param>
        /// <returns></returns>
        public EGuideState GetGuideState(int guideId)
        {
            if (GuideInfoDictionary.TryGetValue(guideId, out GuideData info))
                return info.State;
            return EGuideState.GuideStateLock;
        }

        /// <summary>
        /// 检查下一步引导
        /// </summary>
        public void CheckNextGuide()
        {
            if (guideView != null)
            {
                guideView.CheckNextGuide();
            }
        }

        /// <summary>
        /// 处理事件是否透传
        /// </summary>
        /// <param name="isPass"></param>
        public void UpdateEventIsPass(bool isPass)
        {
            if (guideVm != null)
            {
                guideVm.EventTarget.IsPass = isPass;
            }
        }
    }
}