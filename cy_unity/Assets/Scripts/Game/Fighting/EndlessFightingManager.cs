using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using UnityEngine;
namespace DH.Game
{
    public class EndlessFightingManager : FightingBaseManager
    {
        public override int FightType => 3;
        private float endCheckInterval = 0f;
        public int WaveGoldDrop { get; set; }  //波次所掉落的金币，波次开始时重置
        protected override void Start()
        {
            base.Start();
            BattleManager.Instance.fightingManagerIns = this;
        }
        public async UniTask Init()
        {
            entityPool.forceDisableChildrenManage = true;
            BaseInit().Forget();
            gTime = 0f;
            passedTime = false;
            stage = 1;
            wave = 1;
            enemyManager ??= new EnemyManager();
            spawnManager ??= new EndlessSpawnManager();
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
            spawnManager.Init(stageId, waveId);
            spawnManager.PreGenerateMonster();//预生成怪物Id
            spawnManager.StartSpawn();
            playerCtrl.WeaponSkillController.CheckedWaveStart = false;
        }
        /// <summary>
        /// 自动进入战斗
        /// </summary>
        private void AutoEnterFighting(int stageId, int waveId)
        {
            stage = stageId;
            wave = waveId;
            WaveGoldDrop = 0;
            ((EndlessSpawnManager)spawnManager).RefreshEndlessModel(stageId, wave);
            spawnManager.PreGenerateMonster();//预生成怪物Id
            spawnManager.StartSpawn();
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
        private void CheckEnd(float dt)
        {
            endCheckInterval += dt;
            if (endCheckInterval < 0.5f) return;
            endCheckInterval = 0f;
            var timerList = ((EndlessSpawnManager)spawnManager).SpawnTimerList;
            if (timerList.Count > 0)
            {
                for (var i = timerList.Count-1; i >= 0; i--)
                {
                    var tmpTimer = TimerManager.Instance.FindTimer(timerList[i]);
                    if (tmpTimer == null) timerList.RemoveAt(i);
                }
            }
            if (timerList.Count != 0 || GameTime.Instance.Pause) return;
            var maxPassChapter = DataCenter.mainStageData.GetMaxPassChapter();
            GameDataManager.Instance.Wave++;
            GameDataManager.Instance.EndlessRewardCoinNum +=
                GameManager.Instance.GetRewardCoinNumByChapterId(maxPassChapter,GameDataManager.Instance.Wave-1);
            GameManager.Instance.ReqBattleEndlessWaveDone(AutoTriggerEndlessFighting).Forget();//这里开始刷新玩家操作
        }
        /// <summary>
        /// 自动触发战斗
        /// </summary>
        private void AutoTriggerEndlessFighting()
        {
            var tmpWave = GameDataManager.Instance.Wave;
            tmpWave %= GameManager.Instance.EndlessMaxWaveId;
            if (tmpWave == 0) tmpWave = GameManager.Instance.EndlessMaxWaveId;
            var startId = GetStartIdByWaveId(tmpWave);
            //根据波次计算当前的StartId以及实际的waveId
            AutoEnterFighting(startId, tmpWave);
        }
        /// <summary>
        /// 根据waveId获取对应的StartId
        /// </summary>
        /// <param name="waveId"></param>
        /// <returns></returns>
        private int GetStartIdByWaveId(int waveId)
        {
            var startId = 1;
            var cfgList = ConfigCenter.EndlessStageGrinDingCfgColl.DataItems;
            for (var i = 0; i < cfgList.Count; i++)
            {
                if (cfgList[i].WaveId != waveId) continue;
                startId = cfgList[i].Id;
                break;
            }
            return startId;
        }
        protected override void OnDestroy()
        {
            playerCtrl.Player.resource.Armor = 0;
            gamePlayManager.ClearAllBullets();//TO-DO: 波次结束调用
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.Grinding);
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.PauseTask);
            PauseTask.RemoveAllItem();
            lifeTimeManager.Shutdown();
            lifeTimeManager = null;
            entityPool.ReleaseAssets();
            base.OnDestroy();
        }
    }
}