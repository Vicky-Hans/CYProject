using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using UnityEngine;
namespace DH.Game
{
    public class ChallengeFightingManager : FightingBaseManager
    {
        public override int FightType => 1;
        
        private float endCheckInterval = 0f;
        private WaveRewardCfg waveRewardCfg;
        
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
            spawnManager ??= new ChallengeSpawnManager();
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
        public override MonsterController CloneMonster(MonsterController obj)
        {
            if (obj == null) return null;
            var monsterId = obj.MonsterData.Id;
            var entry = obj.MonsterEntry;
            var modelId = 0;
            if (obj.MonsterData.cfg.SplitModelId > 0)
            {
                modelId = obj.MonsterData.cfg.SplitModelId;
            }
            var cloneObj = CreateChallengeMonster((int)monsterId, entry,modelId:modelId, isSplitBody:true);
            if (cloneObj != null)
            {
                var transform1 = cloneObj.transform;
                var tmpScale = transform1.localScale;
                transform1.localScale = tmpScale * 0.75f;
                cloneObj.IsSplitBody = true;
            }
            return cloneObj;
        }
        public MonsterController CreateChallengeMonster(int monsterId, int entry, int count = 1, int idx = 0,
            int modelId = 0, bool isSplitBody = false, DailyStageGrindingCfg dailyGrindingCfg = null)
        {
            var cfg = ConfigCenter.MonsterCfgColl.GetDataById(monsterId);
            var modelName = modelId>0? $"monster_{modelId}" : GetModelName(cfg.ModelId);
            var assetPath = $"Fighting/Enemy/{modelName}";
            entityPool.LoadAssetSync(assetPath);
            var pos = GetEntryPos(entry, count, idx);
            var go = entityPool.InstantiateObj(assetPath, GetFightWorldPos(pos), Quaternion.identity, transform);
            var obj = go.GetComponent<MonsterController>();
            obj.IsSplitBody = isSplitBody;
            var targetPos = GetTargetPos(pos, entry, obj);
            obj.Init(cfg, GetFightWorldPos(targetPos), null, entityPool);
            if(!isSplitBody && dailyGrindingCfg != null)
            {
                var hpCo = dailyGrindingCfg.HpCoefficient * GameConst.AttributeDivisor;
                var atkCo = dailyGrindingCfg.AtkCoefficient * GameConst.AttributeDivisor;
                obj.UpdateHpAndAtk(hpCo, atkCo, obj, obj);
            }
            var entryCfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
            if (entryCfg.MoveType > 0) obj.MoveType = (EMonsterMoveType)entryCfg.MoveType;
            obj.MonsterEntry = entry;
            enemyManager.Add(obj);
            return obj;
        }
        private void Update()
        {
            if(!inited) return;
            if (DataCenter.dailyFightData.DayRefreshStamp - ServerTime.Instance.GetNowTime() <= 60)//跨天退出战斗
            {
                if (UIManager.Instance.IsOpen<ChallengeTipsView>() || GameManager.Instance.IsOpenExitTip) return;
                GameTime.Instance.Pause = true;
                long timeDiff = 10;
                if (DataCenter.dailyFightData.DayRefreshStamp - ServerTime.Instance.GetNowTime() < 10)
                {
                    timeDiff = DataCenter.dailyFightData.DayRefreshStamp - ServerTime.Instance.GetNowTime();
                }
                var vm = new ChallengeTipsViewModel(GlobalLanguageId.DailyStage_13, (int)timeDiff, () =>
                {
                    GameManager.Instance.OnGameEnd(false);
                });
                UIManager.Instance.OpenDialog<ChallengeTipsView>(vm).Forget();
                GameManager.Instance.IsOpenExitTip = true;
                return;
            }
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
        private void CheckEnd(float dt)
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
            entityPool.ReleaseAssets();
            base.OnDestroy();
        }
    }
}