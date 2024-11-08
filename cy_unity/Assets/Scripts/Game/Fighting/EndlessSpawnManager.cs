using System.Collections.Generic;
using DH.Config;
using DH.Data;
using UnityEngine;
namespace DH.Game
{
    public class EndlessSpawnManager : SpawnManager
    {
        public int TotalSpawnNum;//总波次
        private List<EndlessStageGrinDingCfg> cfgList;//波次列表
        public List<long> SpawnTimerList = new (50);//计时器存储列表
        public override void Init(int startId, int wave = 1)
        {
            RefreshEndlessModel(startId, wave);
        }
        /// <summary>
        /// 刷新无尽关卡数据
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="wave"></param>
        public void RefreshEndlessModel(int startId,int wave = 1)
        {
            TotalSpawnCount = 0;
            CurrenSpawnCount = 0;
            cfgList ??= new List<EndlessStageGrinDingCfg>();
            cfgList.Clear();
            var curStartId = startId;
            var startGridingCfg = ConfigCenter.EndlessStageGrinDingCfgColl.GetDataById(curStartId);
            if (startGridingCfg != null) cfgList.Add(startGridingCfg);
            while (startGridingCfg != null && startGridingCfg.NextId > curStartId && startGridingCfg.WaveId == wave)
            {
                curStartId = startGridingCfg.NextId;
                startGridingCfg = ConfigCenter.EndlessStageGrinDingCfgColl.GetDataById(curStartId);
                if(startGridingCfg.WaveId == wave) cfgList.Add(startGridingCfg);
            }
            //添加boss数据
            if (startGridingCfg != null && startGridingCfg.Incident != 1 && startGridingCfg.NextId < curStartId 
                && startGridingCfg.GrinDing == null && startGridingCfg.WaveId == wave)
            {
                cfgList.Add(startGridingCfg);
            }
        }
        /// <summary>
        /// 预随机当前轮的怪物Id
        /// </summary>
        public override void PreGenerateMonster()
        {
            if (cfgList.Count < 1) return;
            if (GameDataManager.Instance.RandomMonsterList.Count > 0) GameDataManager.Instance.RandomMonsterList.Clear();
            for (var i = 0; i < cfgList.Count; i++)
            {
                var cfg = cfgList[i];
                if (cfg.Incident == 1) continue;
                for (var j = 0; j < cfg.GrinDing.Count; j++)
                {
                    if (GameDataManager.Instance.RandomMonsterList.ContainsKey(cfg.GrinDing[j].MonsterId)) continue;
                    var newMonsterID = GetMonsterID(cfg.GrinDing[j].MonsterId);
                    GameDataManager.Instance.RandomMonsterList.Add(cfg.GrinDing[j].MonsterId,newMonsterID);//获取怪物id
                }
            }
        }
        /// <summary>
        /// 开始刷怪，刷怪起始时间都是以上次刷怪起始时间+持续时间作为下波次的起始时间
        /// </summary>
        public override void StartSpawn()
        {
            for (var i = 0; i < cfgList.Count; i++) 
            {
                var cfg = cfgList[i];
                float delay = cfg.ArrivalTime;
                float duration = cfg.DurationTime;//持续刷怪时间
                if (cfg.Incident == 1) //Incident:0=战斗开始;1=BOSS;2=战斗结束
                {
                    TimerManager.Instance.AddTimer(() =>
                    {
                        if(BattleManager.Instance.fightingManagerIns != null) BattleManager.Instance.fightingManagerIns.ShowBossComing();
                    }, 1.0f, delay, 1, GameConst.TimerTagName.Grinding);
                }
                else if (cfg.GrinDing is { Count: > 0 })
                {
                    TotalSpawnNum += 1;
                    for (var j = 0; j < cfg.GrinDing.Count; j++) //GrinDing=刷怪数据
                    {
                        Spawn(delay, duration, cfg.GrinDing[j]);
                    }
                }
            }
        }
        private void Spawn(float delay, float duration, TourGrinDing grinding)
        {
            var monsterId = 1;
            var tmpTotalSpawnNum = TotalSpawnNum;
            monsterId = GameDataManager.Instance.RandomMonsterList[grinding.MonsterId];
            var frequent = grinding.Frequency;//频率（间隔时间，单位ms）
            var entry = GetMonsterEnterType(monsterId);//刷怪方式
            var spawnCount = grinding.Num;//数量
            var repeat = 1;//重复出现
            if (grinding.Repetition > 0) repeat = Mathf.FloorToInt(duration/frequent);
            TotalSpawnCount += repeat;
            var timerId = TimerManager.Instance.AddTimer(() =>
            {
                GameDataManager.Instance.UpdateMonsterBaseAttrBySpawnNum(grinding.MonsterId,tmpTotalSpawnNum);
                for (var i = 0; i < spawnCount; i++)
                {
                    BattleManager.Instance.fightingManagerIns.CreateMonster(monsterId, entry, spawnCount, i);
                }
                CurrenSpawnCount++;
            }, frequent/1000f, delay, repeat, GameConst.TimerTagName.Grinding);
            SpawnTimerList.Add(timerId);
        }
        //怪物转换，根据传入的随机类型转换为怪物ID
        //小怪1模型=101;小怪2模型=102;小怪3模型=103 boss1=104 boss2 = 105
        private int GetMonsterID(int monsterTypeID)
        {
            var monsterList = new List<RandomReward>(10);
            var jackpotCfg = ConfigCenter.JackpotCfgColl.GetDataById(monsterTypeID);
            if (jackpotCfg != null && jackpotCfg.RandomReward.Count > 0) monsterList = jackpotCfg.RandomReward;
            for (var i = monsterList.Count-1; i >= 0; i--)
            {
                if (GameDataManager.Instance.RandomMonsterList.ContainsValue(monsterList[i].Id))
                {
                    monsterList.RemoveAt(i);
                }
                else
                {
                    var cfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterList[i].Id);
                    if (cfg == null) continue;
                    foreach (var monsterInfo in GameDataManager.Instance.RandomMonsterList)
                    {
                        var tmpCfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterInfo.Value);
                        if (tmpCfg == null || tmpCfg.ModelId != cfg.ModelId) continue;
                        monsterList.RemoveAt(i);
                        break;
                    }
                }
            }
            return MonsterIDRandom(monsterList);
        }
        //怪物ID随机算法
        private int MonsterIDRandom(List<RandomReward> monsterList)
        {
            if (monsterList == null || monsterList.Count == 0) return 1;
            var totalNum = 0;
            for (var i = 0; i < monsterList.Count; i++)
                totalNum += monsterList[i].Weight;
            var randomSeed = Lodash.RandRange(1, totalNum);
            totalNum = 0;
            for (var i = 0; i < monsterList.Count; i++)
            {
                totalNum += monsterList[i].Weight;
                if (randomSeed <= totalNum) return monsterList[i].Id;
            }
            return 1;
        }
        //获取怪物进场方式
        private int GetMonsterEnterType(int monsterID)
        {
            var enterType = 401;
            var cfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterID);
            if (cfg == null) return enterType;
            var enterCfg = ConfigCenter.MonsterModelCfgColl.GetDataById(cfg.ModelId);
            if (enterCfg != null) enterType = enterCfg.EnterId;
            return enterType;
        }
    }
}