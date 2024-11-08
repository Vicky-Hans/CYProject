using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using UnityEngine;

namespace DH.Game
{
    public class MainFightingManager : FightingBaseManager
    {
        public override int FightType => 1;
        
        private float endCheckInterval = 0f;
        private WaveRewardCfg waveRewardCfg;
        
        public int WaveGoldDrop { get; set; }  //波次所掉落的金币，波次开始时重置
        
        protected override void Start()
        {
            base.Start();
            // 初始化战斗管理器
            BattleManager.Instance.fightingManagerIns = this;
        }

        public async UniTask Init()
        {
            entityPool.forceDisableChildrenManage = true;
            await BaseInit();
            gTime = 0f;
            passedTime = false;
            stage = 1;
            wave = 1;
            enemyManager ??= new EnemyManager();
            spawnManager ??= new MainSpawnManager();
            // GameTime.Instance.Pause = false;
            //StartWave(stage, wave);
            inited = true;
           
            FightingSoundHelper.Instance.PlayBgm();
            await InitBg(stage);
        }

        public override void StartWave(int stageId, int waveId)
        {
            base.StartWave(stageId, waveId);
            stage = stageId;
            wave = waveId;
            GameTime.Instance.Pause = false;
            WaveGoldDrop = 0;
            waveRewardCfg = ConfigCenter.WaveRewardCfgColl.GetDataById(waveId);
            spawnManager.Init(stageId, waveId);
            spawnManager.StartSpawn();
            playerCtrl.WeaponSkillController.CheckedWaveStart = false;
        }

        private void Update()
        {
            if(!inited) return;
            if(GameTime.Instance.Pause) return;
            var dt = Time.deltaTime;
            var unscaledDt = Time.unscaledDeltaTime;
            gTime += dt;
            GameTime.Instance.OnUpdate(dt);
            TimerManager.Instance.Update(dt);
            enemyManager.Update(dt);
            playerCtrl.OnUpdate(dt);
            gamePlayManager.Update(dt, unscaledDt);
            lifeTimeManager.Update(dt, unscaledDt);
            CheckEnd(dt);
        }
        
        public override void OnMonsterDead(MonsterType monsterType, Vector3 pos)
        {
            base.OnMonsterDead(monsterType, pos);
            
            if (CheckAllMonsterIsDead())
            {
                var dropGold = waveRewardCfg.MaxDrop - WaveGoldDrop;
                GameDataManager.Instance.GameCoin += dropGold;
            }
        }

        public int GetDropGoldCoin()
        {
            var dropGold = 0;
            if (WaveGoldDrop < waveRewardCfg.MaxDrop)
            {
                if (Lodash.RandRangeFloat(0, 1) < waveRewardCfg.Prob * GameConst.AttributeDivisor)
                {
                    if (WaveGoldDrop + waveRewardCfg.Drop <= waveRewardCfg.MaxDrop)
                    {
                        dropGold = waveRewardCfg.Drop;
                    }
                    else
                    {
                        dropGold = waveRewardCfg.MaxDrop - WaveGoldDrop;
                    }
                }
                WaveGoldDrop += dropGold;
            }
            return dropGold;
        }

        public void CheckEnd(float dt)
        {
            endCheckInterval += dt;
            if (endCheckInterval < 0.5f) return;
            endCheckInterval = 0f;
            if (CheckAllMonsterIsDead())
            {
                playerCtrl.Player.resource.Armor = 0;
                //TO-DO: 波次结束调用
                gamePlayManager.ClearAllBullets();
                GameDataManager.Instance.WaveEnd = true;
            }
        }

        protected override void OnDestroy()
        {
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.Grinding);
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.PauseTask);
            PauseTask.RemoveAllItem();
            lifeTimeManager.Shutdown();
            lifeTimeManager = null;
            // AssetsManager.Release(fightBg);
            entityPool.ReleaseAssets();
            base.OnDestroy();
        }
    }
}