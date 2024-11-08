using System;
using System.Collections.Generic;
using DH.Config;
using DH.Data;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public class SpawnManager
    {
        public int TotalSpawnCount { get; set; }
        public int CurrenSpawnCount { get; set; }
        public virtual void Init(int stage, int wave = 1)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 预生成怪物信息
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void PreGenerateMonster()
        {
            throw new NotImplementedException();
        }
        public virtual void StartSpawn()
        {
            throw new NotImplementedException();
        }
        
        public void Spawn(float delay, float duration, Grinding grinding, MainGrindingCfg cfg)
        {
            var monsterId = grinding.Id;//怪物id
            var frequent = grinding.Frequency;//频率（间隔时间，单位ms）
            var entry = grinding.Type;//刷怪方式
            var spawnCount = grinding.Num;//数量
            var repeat = grinding.Repetition;//重复出现
            if (repeat > 0)
            {
                repeat = Mathf.RoundToInt(duration * 1f / frequent);
            }
            else
            {
                repeat = 1;
            }
            TotalSpawnCount += repeat;
            TimerManager.Instance.AddTimer(() =>
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    BattleManager.Instance.fightingManagerIns.CreateMonster(monsterId, entry, spawnCount, i, grindingCfg:cfg);
                }
                CurrenSpawnCount++;
            }, frequent/1000f, delay, repeat, GameConst.TimerTagName.Grinding);
        }
    }

    public class MainSpawnManager : SpawnManager
    {
        private List<MainGrindingCfg> cfgList;

        public override void Init(int stage, int wave = 1)
        {
            TotalSpawnCount = 0;
            CurrenSpawnCount = 0;
            cfgList ??= new List<MainGrindingCfg>();
            cfgList.Clear();
            var stageCfg = ConfigCenter.CopyCfgColl.GetDataById(stage);
            var startId = stageCfg.WaveStartId[wave - 1];
            var cfg = ConfigCenter.MainGrindingCfgColl.GetDataById(startId);
            if (cfg == null)
            {
                DHLog.Error("当前Id不存在："+startId);
                return;
            }
            cfgList.Add(cfg);
            while (cfg != null && cfg.CustomId == stage && cfg.NextId>startId && cfg.NowWave==wave)
            {
                startId = cfg.NextId;
                cfg = ConfigCenter.MainGrindingCfgColl.GetDataById(startId);
                if(cfg != null && cfg.NowWave == wave) cfgList.Add(cfg);
            }
        }

        public override void StartSpawn()
        {
            if (cfgList.Count < 1) return;
            for (int i = 0; i < cfgList.Count; i++)
            {
                var cfg = cfgList[i];
                float delay = cfg.Time; // - startTime;
                float duration = cfg.GringTime;
                if (cfg.Incident == 1)
                {
                    SpawnEvent(delay, cfg);
                    continue;
                }
                foreach (var grinding in cfg.Summon)
                {
                    Spawn(delay, duration, grinding, cfg);
                }
            }
        }
        
        public void SpawnEvent(float delay, MainGrindingCfg cfg)
        {
            if(cfg.Incident != 1)return;
            TimerManager.Instance.AddTimer(() =>
            {
                if(BattleManager.Instance.fightingManagerIns != null)
                    BattleManager.Instance.fightingManagerIns.ShowBossComing();
            }, 10000, delay, 1, GameConst.TimerTagName.Grinding);
        }
    }
    public class ChallengeSpawnManager : SpawnManager
    {
        private List<DailyStageGrindingCfg> cfgList;
        public override void Init(int stage, int wave = 1)
        {
            TotalSpawnCount = 0;
            CurrenSpawnCount = 0;
            cfgList ??= new List<DailyStageGrindingCfg>();
            cfgList.Clear();
            var startId = stage;
            var cfg = ConfigCenter.DailyStageGrindingCfgColl.GetDataById(startId);
            cfgList.Add(cfg);
            while (cfg.NextId > startId && cfg.NowWave==wave)
            {
                startId = cfg.NextId;
                cfg = ConfigCenter.DailyStageGrindingCfgColl.GetDataById(startId);
                if(cfg.NowWave == wave) cfgList.Add(cfg);
            }
        }
        public override void StartSpawn()
        {
            if (cfgList.Count < 1) return;
            for (var i = 0; i < cfgList.Count; i++)
            {
                var cfg = cfgList[i];
                float delay = cfg.Time; // - startTime;
                float duration = cfg.GringTime;
                if (cfg.Incident == 1)
                {
                    TimerManager.Instance.AddTimer(() =>
                    {
                        if(BattleManager.Instance.fightingManagerIns != null)
                            BattleManager.Instance.fightingManagerIns.ShowBossComing();
                    }, 10000, delay, 1, GameConst.TimerTagName.Grinding);
                    continue;
                }
                for (var j = 0; j < cfg.Summon.Count; j++)
                {
                    Spawn(delay, duration, cfg.Summon[j],cfg);
                }
            }
        }
        public void Spawn(float delay, float duration, Grinding grinding, DailyStageGrindingCfg cfg)
        {
            var monsterId = GameDataManager.Instance.GetMonsterIdByType(grinding.Id);//怪物id类型
            var frequent = grinding.Frequency;//频率（间隔时间，单位ms）
            var entry = GetMonsterEnterType(monsterId);//入场方式
            var spawnCount = grinding.Num;//数量
            var repeat = 1;//重复出现
            if (grinding.Repetition > 0) repeat = Mathf.RoundToInt(duration * 1f / frequent);
            TotalSpawnCount += repeat;
            TimerManager.Instance.AddTimer(() =>
            {
                if (BattleManager.Instance.fightingManagerIns is not ChallengeFightingManager) return;
                var curFightManager = (ChallengeFightingManager)BattleManager.Instance.fightingManagerIns;
                for (var i = 0; i < spawnCount; i++)
                {
                    curFightManager.CreateChallengeMonster(monsterId, entry, spawnCount, i,dailyGrindingCfg:cfg);
                }
                CurrenSpawnCount++;
            }, frequent/1000f, delay, repeat, GameConst.TimerTagName.Grinding);
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