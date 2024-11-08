using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class ForestSpawnManager : SpawnManager
    {
        private List<SecretGrinDingCfg> cfgList;
        private MapFightingManager mapFightingManager;
        public bool AllWaveEnd { get; set; }
        /// <summary>
        /// startId is no use, Use wave instead
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="wave"></param>
        public override void Init(int startId, int wave = 1)
        {
            mapFightingManager = BattleManager.Instance.fightingManagerIns as MapFightingManager;
            TotalSpawnCount = 0;
            CurrenSpawnCount = 0;
            cfgList ??= new List<SecretGrinDingCfg>();
            cfgList.Clear();
            startId = GetStartIdByWave(wave);
            var cfg = ConfigCenter.SecretGrinDingCfgColl.GetDataById(startId);
            if(cfg == null) return;
            var curWave = cfg.WaveId;
            cfgList.Add(cfg);
            while (cfg.NextId>startId)
            {
                startId = cfg.NextId;
                cfg = ConfigCenter.SecretGrinDingCfgColl.GetDataById(startId);
                if (cfg.WaveId != curWave) break;
                cfgList.Add(cfg);
            }
        }
        
        /// <summary>
        /// 预随机当前轮的怪物Id
        /// </summary>
        public override void PreGenerateMonster()
        {
            if (cfgList.Count < 1) return;
            if (GameDataManager.Instance.RandomMonsterList.Count > 0)
            {
                GameDataManager.Instance.RandomMonsterList.Clear();
                GameDataManager.Instance.RandomMonsterTypeList.Clear();
            }
            for (var i = 0; i < cfgList.Count; i++)
            {
                var cfg = cfgList[i];
                if (cfg.Incident == 1) continue;
                for (var j = 0; j < cfg.GrinDing.Count; j++)
                {
                    if (GameDataManager.Instance.RandomMonsterList.ContainsKey(cfg.GrinDing[j].MonsterId)) continue;
                    var newMonsterID = GetMonsterID(cfg.GrinDing[j].MonsterId);
                    if ((cfg.GrinDing[j].Type == 4 || cfg.GrinDing[j].Type == 5) && GameDataManager.Instance.CurFightData.Stage.Boss > 0)
                    {
                        GameDataManager.Instance.RandomMonsterTypeList.TryAdd(cfg.GrinDing[j].Type,GameDataManager.Instance.CurFightData.Stage.Boss);
                        GameDataManager.Instance.RandomMonsterList.Add(cfg.GrinDing[j].MonsterId,GameDataManager.Instance.CurFightData.Stage.Boss);//获取怪物id
                        GameDataManager.Instance.CurFightData.Stage.Boss = 0;
                    }
                    else
                    {
                        GameDataManager.Instance.RandomMonsterTypeList.TryAdd(cfg.GrinDing[j].Type,newMonsterID);
                        GameDataManager.Instance.RandomMonsterList.Add(cfg.GrinDing[j].MonsterId,newMonsterID);//获取怪物id
                    }
                }
            }
        }
        
        public int GetStartIdByWave(int wave)
        {
            foreach (var cfg in ConfigCenter.SecretGrinDingCfgColl.DataItems)
            {
                if (cfg.WaveId == wave) return cfg.Id;
            }
            return 0;
        }
        
        public override void StartSpawn()
        {
            if (cfgList.Count < 1) return;
            //PreGenerateMonster();
            for (int i = 0; i < cfgList.Count; i++)
            {
                var cfg = cfgList[i];
                float delay = cfg.ArrivalTime; // - startTime;
                float duration = cfg.DurationTime;
                if (cfg.Incident == 1)
                {
                    SpawnEvent(delay, cfg);
                    continue;
                }
                foreach (var grinding in cfg.GrinDing)
                {
                    DHLog.Debug($"spawn ----> {cfg.WaveId}");
                    Spawn(delay, duration, grinding, cfg);
                }
            }
        }
        
        public void SpawnEvent(float delay, SecretGrinDingCfg cfg)
        {
            if(cfg.Incident != 1)return;
            TimerManager.Instance.AddTimer(() =>
            {
                if(BattleManager.Instance.fightingManagerIns != null)
                    BattleManager.Instance.fightingManagerIns.ShowBossComing();
            }, 10000, delay, 1, GameConst.TimerTagName.Grinding);
        }
        
        public void Spawn(float delay, float duration, SecretGrinDing grinding, SecretGrinDingCfg cfg)
        {
            var frequent = grinding.Frequency;//频率（间隔时间，单位ms）
            var monsterId = GameDataManager.Instance.RandomMonsterList[grinding.MonsterId];//入场方式
            var entry = grinding.EnterTypeJackpot;//入场方式
            var spawnCount = grinding.Num;//数量
            var repeat = 1;//重复出现
            if (grinding.Repetition > 0) repeat = Mathf.RoundToInt(duration * 1f / frequent);
            TotalSpawnCount += repeat;
            GameDataManager.Instance.UpdateSecretMonsterBaseAttrBySpawnNum(grinding.Type-1,grinding.MonsterId, cfg.WaveId);
            TimerManager.Instance.AddTimer(() =>
            {
                for (var i = 0; i < spawnCount; i++)
                {
                    // 超过一定数量就不刷
                    if (grinding.MonsterId is 106 or 107 or 108)
                    {
                        var monsterCount = mapFightingManager.enemyManager.EnemyList.Count;
                        if (monsterCount > mapFightingManager.MaxMonsterCount) continue;
                    }
                    var monster = mapFightingManager.CreateMonster(monsterId, entry, spawnCount, i, grindingCfg:cfg);
                    // for debug
                    if(i == 0 && monster != null)
                    {
                        DHLog.Debug($"secret Monster, wave:{cfg.WaveId} id:{monsterId} atk:{monster.attackBox.damageArgs.damagePoint} hp:{monster.Data.resource.Hp}");
                    }
                }
                CurrenSpawnCount++;
                if (CurrenSpawnCount >= TotalSpawnCount && cfg.NextId <= 0)
                {
                    AllWaveEnd = true;
                }
            }, frequent/1000f, delay, repeat, GameConst.TimerTagName.Grinding);
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
            var randomSeed = Random.Range(1, totalNum);
            totalNum = 0;
            for (var i = 0; i < monsterList.Count; i++)
            {
                totalNum += monsterList[i].Weight;
                if (randomSeed <= totalNum) return monsterList[i].Id;
            }
            return 1;
        }
        
    }
}