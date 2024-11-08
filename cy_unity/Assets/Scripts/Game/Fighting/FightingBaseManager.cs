using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using DH.Game.UIViews;
using DH.Game.ViewModels;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public class FightingBaseManager : ObservableMonoBehavior
    {
        private static FightingBaseManager instance = null;
        public static FightingBaseManager Instance => instance ? instance : (instance = FindObjectOfType<FightingBaseManager>());
        public virtual int FightType => 0;

        public AssetPoolEntity entityPool;
        public CharacterController playerCtrl;
        public GamePlayManager gamePlayManager = new ();
        public LifeTimeManager lifeTimeManager = new ();
        public readonly Vector2 FightPanelSize = new Vector2(10.8f, 6f);
        public Vector2 ScreenSize { get; set; }
        public float MonsterTargetDis { get; set; }

        [HideInInspector]
        public int stage;
        [HideInInspector]
        public int wave;
        protected bool inited;
        protected float gTime = 0f;
        public EnemyManager enemyManager;
        public DropManager dropManager;
        public SpawnManager spawnManager;
        public CameraController camCtrl;
        public SpriteRenderer fightBg;
        public Transform fightPanelTrans;
        [HideInInspector]
        public Vector3 playerPos;
        public readonly Vector3 PlayerLocalPos = new Vector3(-4.46f, -0.64f, 0f);
        [HideInInspector]
        public Vector3 playerTargetPos;
        protected bool passedTime;
        public GameObject MonsterHpShieldPrefab;
        public GameObject MonsterAtkBonusRangePrefab;
        public GameObject MonsterDownCdRangePrefab;
        public GameObject MonsterSelfDestructPrefab;
        public GameObject MonsterBreakArmorPrefab;
        public int BossCount { get; set; }  // 存活 boss 数量
        public GamePlayManager GamePlayManager=> gamePlayManager;
        private readonly string hurtNumAssetPath = "Effects/HurtNum";
        private readonly string superBullet4000PathStr = $"Player/PlayerSandCircle";//英雄4000大招子弹

        protected virtual void Start()
        {
            instance = GetComponent<FightingBaseManager>();
            var transCache = playerCtrl.transform;
            transCache.localPosition = PlayerLocalPos;
            playerPos = transCache.position;
            playerTargetPos = playerPos + new Vector3(1f, 0f);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Enemy"), true);
            if(FightType != (int)EStateType.StageTypeChallenge)
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyAtkRange"),
                    LayerMask.NameToLayer("Enemy"), false);
            }
            else
            {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("EnemyAtkRange"),
                    LayerMask.NameToLayer("Enemy"), true);
            }
        }
        protected virtual void OnDestroy()
        {
            instance = null;
        }
        public Vector3 GetFightWorldPos(Vector3 localPos)
        {
            return fightPanelTrans.TransformPoint(localPos);
        }
        public Vector3 GetFightLocalPos(Vector3 worldPos)
        {
            return fightPanelTrans.InverseTransformPoint(worldPos);
        }
        public async UniTask BaseInit()
        {
            Lodash.SetSeed((int)GameDataManager.Instance.Seed);
            var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.monsterDis);
            MonsterTargetDis = defineCfg.Content[0] * GameConst.AttributeDivisor;
            camCtrl = GetComponent<CameraController>();
            SetPlayerPos();
            CalcScreenSize();
            await playerCtrl.InitStart();
        }
        
        private void SetPlayerPos()
        {
            var transCache = playerCtrl.transform;
            transCache.localPosition = PlayerLocalPos;
            playerPos = transCache.position;
            playerTargetPos = playerPos + new Vector3(1f, 0f);
        }

        public Vector3 PlayerWorldPos
        {
            get
            {
                if (BattleManager.Instance.IsStageForest())
                {
                    return playerCtrl.transform.position + Vector3.up;
                }
                return playerCtrl.transform.position;
            }
        }

        public virtual void StartWave(int stageId, int waveId)
        {
            playerCtrl.playerHpShield.TriggerWaveStart();
        }
        
        private async UniTask ChangeBg(int stage)
        {
            if (FightType != (int)EStateType.StageTypeMainStage)return;
            var stageCfg = ConfigCenter.CopyCfgColl.GetDataById(stage);
            if (stageCfg == null)return;
            var bgName = stageCfg.Battle_mapy;
            if(string.IsNullOrEmpty(bgName))return;
            var bgPath = $"Fighting/Bgs/{bgName}";
            var spr = await AssetsManager.LoadAssetAsync<Sprite>(bgPath);
            if (spr != null)
            {
                fightBg.sprite = spr;
            }
        }

        protected virtual async UniTask InitBg(int stage)
        {
            // await ChangeBg(this.stage);
            var size = fightBg.size;
            var fightBgTrans = fightBg.transform;
            var parentTrans = fightBgTrans.parent;
            fightBgTrans.localPosition = new Vector3(0, size.y / 2f - FightPanelSize.y/2f, 0);
            var bgPos = fightBgTrans.localPosition;
            var bgLeft = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
            var bgRight = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
            var leftTrans = bgLeft.transform;
            var rightTrans = bgRight.transform;
            var bgLeftPos = new Vector3(bgPos.x - size.x, bgPos.y);
            var bgRightPos = new Vector3(bgPos.x + size.x, bgPos.y);
            leftTrans.localPosition = bgLeftPos;
            rightTrans.localPosition = bgRightPos;
            leftTrans.localScale = new Vector3(-1, 1, 1);
            rightTrans.localScale = new Vector3(-1, 1, 1);
            
            var bgTop = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
            var bgBottom = Instantiate(fightBg, Vector3.zero, Quaternion.identity, parentTrans);
            var topTrans = bgTop.transform;
            var bottomTrans = bgBottom.transform;
            var bgTopPos = new Vector3(bgPos.x, bgPos.y + size.y);
            var bgBottomPos = new Vector3(bgPos.x, bgPos.y - size.y);
            topTrans.localPosition = bgTopPos;
            bottomTrans.localPosition = bgBottomPos;
            topTrans.localScale = new Vector3(1, -1, 1);
            bottomTrans.localScale = new Vector3(1, -1, 1);

            fightBg.gameObject.SetActive(true);
            bgTop.gameObject.SetActive(true);
            bgBottom.gameObject.SetActive(true);
            bgLeft.gameObject.SetActive(true);
            bgRight.gameObject.SetActive(true);
        }
        public void CalcScreenSize()
        {
            Vector3 ldPoint = camCtrl.MainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
            Vector3 ruPoint = camCtrl.MainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));
            ScreenSize = new Vector2(ruPoint.x - ldPoint.x, ruPoint.y - ldPoint.y);
        }
        
        public void AddAutoReleaseUnit(GameObject target, float lifeTime, IPool<GameObject> pool, Action completed = null)
        {
            lifeTimeManager.AddAutoReleaseUnit(target, lifeTime, pool, completed);
        }
        
        public void AddAutoReleaseGObj(GameObject target, float lifeTime)
        {
            TimerManager.Instance.AddTimer(() =>
            {
                if(target == null) return;
                if (target.activeSelf)
                {
                    Destroy(target);
                }
            }, 0f, lifeTime, 1, GameConst.TimerTagName.AutoReleaseObj);
        }
        
        public string GetModelName(int modelId)
        {
            var cfg = ConfigCenter.MonsterModelCfgColl.GetDataById(modelId);
            if (cfg == null) return "";
            return cfg.Model;
        }
        public virtual MonsterController CloneMonster(MonsterController obj)
        {
            if (obj == null) return null;
            var monsterId = obj.MonsterData.Id;
            var entry = obj.MonsterEntry;
            var modelId = 0;
            if (obj.MonsterData.cfg.SplitModelId > 0)
            {
                modelId = obj.MonsterData.cfg.SplitModelId;
            }
            var cloneObj = CreateMonster((int)monsterId, entry,modelId:modelId, isSplitBody:true);
            if (cloneObj != null)
            {
                var transform1 = cloneObj.transform;
                var tmpScale = transform1.localScale;
                transform1.localScale = tmpScale * 0.75f;
                cloneObj.IsSplitBody = true;
            }
            return cloneObj;
        }

        public MonsterController CreateMonster(int monsterId, int entry, int count = 1, int idx = 0,
            int modelId = 0, bool isSplitBody = false, MainGrindingCfg grindingCfg = null)
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
            if(!isSplitBody && grindingCfg != null)
            {
                var hpCo = grindingCfg.HpCoefficient * GameConst.AttributeDivisor;
                var atkCo = grindingCfg.AtkCoefficient * GameConst.AttributeDivisor;
                obj.UpdateHpAndAtk(hpCo, atkCo, obj, obj);
            }
            
            var entryCfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
            if (entryCfg.MoveType > 0)
            {
                obj.MoveType = (EMonsterMoveType)entryCfg.MoveType;
            }

            obj.MonsterEntry = entry;
            enemyManager.Add(obj);
            return obj;
        }
        
        /// <summary>
        ///  创建掉落物品 不传id 默认掉落经验
        /// </summary>
        /// <param name="expValue"></param>
        /// <param name="pos"></param>
        /// <param name="dropId"></param>
        /// <returns></returns>
        public DropController CreateDrop(float expValue, Vector3 pos,int dropId = 0)
        {
            var modelName = "drop_exp";
            if (0 != dropId)
            {
                modelName= $"drop_{dropId}";
            }
            var assetPath = $"Fighting/Drop/{modelName}";
            entityPool.LoadAssetSync(assetPath);
            var go = entityPool.InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, fightPanelTrans);
            go.transform.localPosition = pos;
            var obj = go.GetComponent<DropController>();
            obj.Init(dropId,expValue);
            dropManager.Add(obj);
            return obj;
        }
        public async void CreatePlayerReviveEffect()
        {
            var assetPath = $"Fighting/Buff/buff_revive";
            var go = await AssetsManager.InstantiateWithParentAsync(assetPath,playerCtrl.transform,false);
            if(go == null) return;
            if (playerCtrl == null)
            {
                Destroy(go);
                return;
            }
            var effect = go.transform.GetChild(0)?.GetComponent<ParticleSystem>();
            if (effect == null)
            {
                Destroy(go);
                return;
            }

            effect.Play();
            await UniTask.Delay(1000);
            if (go != null)
            {
                Destroy(go);
            }
        }
        public Vector3 GetEntryPos(int entry, int count = 1, int idx = 0)
        {
            Vector3 pos;
            var cfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
            switch (cfg.EnterType)
            {
                case 1:
                    pos = Lodash.PosOnCircle(Vector3.zero, cfg.Distance*1.0f / GameConst.PixelPerUnit);
                    break;
                case 2:
                    pos = Lodash.PosOnBoxCorner(Vector3.zero, cfg.Distance*1.0f / GameConst.PixelPerUnit);
                    break;
                case 3:
                    pos = new Vector3(cfg.Coord[0]*1.0f/GameConst.PixelPerUnit, cfg.Coord[1]*1.0f / GameConst.PixelPerUnit, 0);
                    break;
                case 4:
                case 6:
                    pos = new Vector3(cfg.Coord[0]*1.0f/GameConst.PixelPerUnit, cfg.Coord[1]*1.0f / GameConst.PixelPerUnit, 0);
                    break;
                case 5:
                    pos = Lodash.PosInCircle(new Vector3(cfg.Coord[0], cfg.Coord[1], 0)/GameConst.PixelPerUnit, cfg.Distance/GameConst.PixelPerUnit);
                    break;
                case 10:
                case 11:
                    pos = Lodash.PosInRect(new Vector3(cfg.Coord[0], cfg.Coord[1], 0)/GameConst.PixelPerUnit, new Vector2(cfg.EndCoord[0], cfg.EndCoord[1])/GameConst.PixelPerUnit);
                    break;
                default:
                    pos = Lodash.PosOnCircle(Vector3.zero, 700f / GameConst.PixelPerUnit);
                    break;
            }

            return pos;
        }
        
        public Vector3 GetAgentTargetPos(Vector3 selfPos, MonsterController monster)
        {
            var monsterRadius = 0f;
            Vector3 atkPos;
            var targetPosYOffset = 0f;
            var circle2D = monster.attackBox.Trigger as CircleCollider2D;
            if(circle2D!= null)
            {
                monsterRadius = circle2D.radius;
                atkPos = circle2D.transform.position;
                targetPosYOffset = selfPos.y - atkPos.y;
            }

            var targetPosXOffset = (2f + monsterRadius) * 0.5f;
            return new Vector3(PlayerLocalPos.x + targetPosXOffset, PlayerLocalPos.y + targetPosYOffset, 0);
        }
        
        public Vector3 GetTargetPos(Vector3 selfPos, int entry, MonsterController monster)
        {
            var entryCfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
            switch (entryCfg.EnterType)
            {
                case 11:
                {
                    var cfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
                    var targetMinX = cfg.CoordOffset[0]/GameConst.PixelPerUnit;
                    var targetMaxX = cfg.CoordOffset[1]/GameConst.PixelPerUnit;
                    var targetX = Lodash.RandRangeFloat(targetMinX, targetMaxX);
                    return new Vector3(targetX, selfPos.y, selfPos.z);
                }
                case 6:
                {
                    var cfg = ConfigCenter.MonsterEnterCfgColl.GetDataById(entry);
                    return new Vector3(cfg.EndCoord[0], cfg.EndCoord[1])/GameConst.PixelPerUnit;
                }
                default:
                    return GetAgentTargetPos(selfPos, monster);
            }
        }

        public bool TargetInScreenForest(BaseMonoUnit unit)
        {
            if(unit == null) return false;
            var pos = unit.transform.position;
            var vPos = camCtrl.MainCamera.WorldToViewportPoint(pos);
            return vPos is { x: > 0 and < 1, y: > 0 and < 1 };
        }

        public bool TargetInScreen(BaseMonoUnit unit)
        {
            if (BattleManager.Instance.IsStageForest())
            {
                return TargetInScreenForest(unit);
            }
            if(unit == null) return false;
            var pos = unit.transform.position;
            return TargetInScreen(pos);
        }

        public bool TargetInScreen(Vector3 pos)
        {
            var px = Mathf.Abs(pos.x) - 0.2f;
            var py = Mathf.Abs(pos.y) - 0.2f;
            return (px < ScreenSize.x/2 && py < ScreenSize.y / 2);
        }
        
        public Vector3 ClampPos(Vector3 pos)
        {
            var x = Mathf.Clamp(pos.x, -ScreenSize.x / 2 + 0.2f, ScreenSize.x / 2 - 0.2f);
            var y = Mathf.Clamp(pos.y, -ScreenSize.y / 2 + 0.2f, ScreenSize.y / 2 - 0.2f);
            pos.x = x;
            pos.y = y;
            return pos;
        }

        /// <summary>
        /// 操作格子后，调用此方法
        /// </summary>
        public void OnWeaponChanged()
        {
            playerCtrl.OnWeaponChanged();
        }

        /// <summary>
        /// 获取武器的cd进度
        /// </summary>
        /// <param name="weaponUid"></param>
        /// <returns></returns>
        public float GetWeaponProgress(int weaponUid)
        {
            return playerCtrl.WeaponSkillController.GetWeaponProgress(weaponUid);
        }

        public void ShowPlayerHurtNum(DamageArgs args, Vector3 pos)
        {
            ShowPlayerHurtNumOnly(args.damagePoint, pos);
        }
        public void ShowPlayerHurtNumOnly(long num, Vector3 pos)
        {
            pos += new Vector3((float)Lodash.Random.NextDouble() - 0.5f,
                (float)Lodash.Random.NextDouble() / 2, 0f);
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go)
            {
                return;
            }
            var obj = go.GetComponent<HurtNum>();
            obj.SetDeltaPosY(1.1f);
            var dmgValue = -num;
            obj.Init(dmgValue, HurtNumType.Crit, entityPool);
        }

        public virtual void ShowHurtNum(DamageArgs args, Vector3 pos)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go) return;
            var obj = go.GetComponent<HurtNum>();
            var hurtType = args.isCrit ? HurtNumType.Extra : HurtNumType.Plain;
            obj.Init(args.damagePoint, hurtType, entityPool);
        }

        public void ShowMonsterRecoverNum(Vector3 pos, long num)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go)
            {
                return;
            }
            var obj = go.GetComponent<HurtNum>();
            obj.SetDeltaPosY(1.3f);
            obj.Init(num, HurtNumType.Recovery, entityPool);
        }
        public void ShowPlayerRecoverNum(long num)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var pos = playerCtrl.transform.position;
            pos += new Vector3((float)Lodash.Random.NextDouble() - 0.5f,
                (float)Lodash.Random.NextDouble() / 2, 0f);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go) return;
            var obj = go.GetComponent<HurtNum>();
            obj.SetDeltaPosY(1.1f);
            obj.Init(num, HurtNumType.Recovery, entityPool);
        }
        public void ShowHurtNumOnly(int num, Vector3 pos)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go)  return;
            var obj = go.GetComponent<HurtNum>();
            var hurtType = HurtNumType.Plain;
            obj.Init(num, hurtType, entityPool);
        }
        public void ShowHurtMiss(Vector3 pos)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go) return;
            var obj = go.GetComponent<HurtNum>();
            var hurtType = HurtNumType.Miss;
            obj.Init(0, hurtType, entityPool);
        }
        public void ShowHurtImmune(Vector3 pos)
        {
            entityPool.LoadAssetSync(hurtNumAssetPath);
            var go = entityPool.InstantiateObj(hurtNumAssetPath, pos, Quaternion.identity, transform);
            if (!go) return;
            var obj = go.GetComponent<HurtNum>();
            var hurtType = HurtNumType.Immune;
            obj.Init(0, hurtType, entityPool);
        }
        public virtual void ShowBossComing()
        {
            UIManager.Instance.OpenDialog<BossComingView, BossComingViewModel>().Forget();
        }
        
        public MonsterController GetNearestMonster(Vector3 pos)
        {
            return enemyManager.GetNearest(pos);
        }

        public MonsterController GetNearestMonster(Vector3 pos, float distance)
        {
            var tmpList = ListPool<MonsterController>.Get();
            enemyManager.NearestMonster(tmpList, pos, 1, distance);
            MonsterController nearestMonster = null;
            if (tmpList.Count > 0)
            {
                nearestMonster = tmpList[0];
            }
            ListPool<MonsterController>.Release(tmpList);
            return nearestMonster;
        }

        public MonsterController GetRandMonster()
        {
            return enemyManager.RandMonsterInScreen();
        }
        
        public List<MonsterController> GetRandMonstersInScreen(int count, bool selectBoss = true)
        {
            var monsters = ListPool<MonsterController>.Get();
            enemyManager.GetRandMonstersInScreen(monsters, count, selectBoss);
            return monsters;
        }
        /// <summary>
        /// 获取最近的count个怪物
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<MonsterController> GetNearestMonsters(Vector3 pos, int count)
        {
            var tmpList = ListPool<MonsterController>.Get();
            enemyManager.NearestMonster(tmpList, pos, count);
            return tmpList;
        }
        public MonsterController GetRandMonsterInRange(Vector3 pos, float distance)
        {
            return enemyManager.RandMonsterInRange(pos, distance);
        }
        public List<MonsterController> GetRandMonstersInRange(Vector3 pos, float distance)
        {
            var monsters = ListPool<MonsterController>.Get();
            enemyManager.RandMonstersInRange(pos,distance,monsters);
            return monsters;
        }
        public List<MonsterController> GetAllMonsterInScreen()
        {
            var monsters = ListPool<MonsterController>.Get();
            enemyManager.GetAllMonstersInScreen(monsters);
            return monsters; 
        }

        private async UniTask PlayActiveEffect2000(MonsterController monsterController)
        {
            var pos = monsterController.transform.position;
            const string effectPath = "Effects/skill_hero2000";
            var effectObj = await AssetsManager.InstantiateAsync(effectPath);
            if(effectObj != null)
            {
                effectObj.transform.position = pos;
                effectObj.transform.parent = fightPanelTrans;
                AddAutoReleaseGObj(effectObj, 2f);
                if(monsterController != null && !monsterController.CheckMonsterIsDead())
                {
                    var dmg = monsterController.MonsterData.resource.Hp;
                    monsterController.DecHp((int)dmg + 1);
                }
            }
        }
        private async UniTask PlayActiveEffect3000()
        {
            var pos = fightPanelTrans.position;
            const string effectPath = "Effects/skilEff_hero3000";
            var effectObj = await AssetsManager.InstantiateAsync(effectPath);
            if(effectObj != null)
            {
                effectObj.transform.position = pos;
                effectObj.transform.parent = fightPanelTrans;
                AddAutoReleaseGObj(effectObj, 2f);
            }
        }
        private void TakeHeroSkill2000(int heroId)
        {
            var playerAttr = playerCtrl.Player.attr;
            var enableHeroSkill = playerAttr.Calc(AttributeType.EnableHeroSkill);
            if(enableHeroSkill < 1)return;
            var killNum = Lodash.RoundToInt(playerAttr.Calc(AttributeType.KillNum));
            if(killNum <= 0)return;
            var monsterList = GetRandMonstersInScreen(killNum, selectBoss:false);
            if (monsterList.Count > 0)
            {
                FightingSoundHelper.Instance.PlayHeroActiveSkill(heroId);
            }
            monsterList.ForEach(monsterController =>
            {
                if(monsterController.CheckMonsterIsDead())return;
                PlayActiveEffect2000(monsterController).Forget();
            });
            ListPool<MonsterController>.Release(monsterList);
        }

        private void TakeHeroSkill3000(int heroId)
        {
            var playerAttr = playerCtrl.Player.attr;
            var stopTime = playerAttr.Calc(AttributeType.StopTime) * GameConst.TimeDivisor;
            if (stopTime <= 0) return;
            var maxHpDmg = playerAttr.Calc(AttributeType.MaxHpDmg);
            var monsterList = GetAllMonsterInScreen();
            if (monsterList.Count > 0)
            {
                PlayActiveEffect3000().Forget();
                FightingSoundHelper.Instance.PlayHeroActiveSkill(heroId);
            }
            monsterList.ForEach(monsterController =>
            {
                if (monsterController.CheckMonsterIsDead()) return;
                var dmgValue = 0f;
                if (maxHpDmg > 0.001f) dmgValue = monsterController.MonsterData.resource.MaxHp * maxHpDmg;
                var buff = new Buff
                {
                    id = (int)AttributeType.StopTime,
                    attrName = AttributeName.StopTime,
                    startTime = GameTime.Instance.GTime,
                    duration = stopTime,
                    interval = 1f,
                    value = dmgValue,
                    valueType = BuffValueType.Negative,
                    multi = false,
                };
                monsterController.AddStopBuff(buff);
            });
            ListPool<MonsterController>.Release(monsterList);
        }
        /// <summary>
        /// 英雄4000的大招
        /// </summary>
        /// <param name="heroId"></param>
        private void TakeHeroSkill4000(int heroId)
        {
            var playerAttr = playerCtrl.Player.attr;
            var enableHeroSkill = playerAttr.Calc(AttributeType.EnableHeroSkill);
            if (enableHeroSkill <= 0) return;
            var monsterList = GetAllMonsterInScreen();
            if (monsterList.Count <= 0) return;
            for (var i = monsterList.Count - 1; i >= 0; i--)
            {
                if (monsterList[i].Data.IsDead()) monsterList.RemoveAt(i);
            }
            if (monsterList.Count <= 0) return;
            FightingSoundHelper.Instance.PlayHeroActiveSkill(heroId);
            entityPool.LoadAssetSync(superBullet4000PathStr);
            var bullet = entityPool.InstantiateObj(superBullet4000PathStr, Vector3.zero, Quaternion.identity, fightPanelTrans);
            if (bullet == null) return;
            var bulletComp = bullet.GetComponent<PlayerSandCircle>();
            if (bulletComp == null) return;
            var sandCircleRange = playerAttr.Calc(AttributeType.SandCircleRange);
            var targetPos = GetFightLocalPos(monsterList[0].transform.position);
            bulletComp.InitWithTarget(playerCtrl.Data, sandCircleRange,entityPool);
            bulletComp.transform.localPosition = targetPos;
        }
        public void TakeHeroSkill()
        {
            var heroId = GameDataManager.Instance.HeroId;
            switch (heroId)
            {
                case 2000:
                    TakeHeroSkill2000(heroId);
                    return;
                case 3000:
                    TakeHeroSkill3000(heroId);
                    return;
                case 4000:
                    TakeHeroSkill4000(heroId);
                    return;
            }
        }

        public void ShootWeaponMaxLevel(int equipId, float dmgPercent)
        {
            var weapon = playerCtrl.WeaponSkillController.GetMaxLevelWeapon(equipId);
            if(weapon == null)return;
            playerCtrl.WeaponSkillController.SkillsMap.TryGetValue(weapon.weaponData.SkillData.id, out var skillIns);
            if (skillIns != null)
            {
                (skillIns as PlayerSkill200)?.ReHitShoot(weapon.weaponData, dmgPercent);
            }
        }
        public async void DestroyMonster(MonsterController monster, bool isClear = false)
        {
            if (FightType == (int)EStateType.StageTypeEndless && !monster.IsSplitBody)
            {
                var maxPassChapter = DataCenter.mainStageData.GetMaxPassChapter();
                GameDataManager.Instance.EndlessRewardCoinNum += GameDataManager.Instance.GetMonsterCoinNumByChapterId(maxPassChapter);
            }
            enemyManager.Remove(monster);
            await monster.ReleaseModel(isClear);
            if(monster == null) return;
            entityPool.ReleaseObj(monster.gameObject);
        }

        public async void DestroyMonsterWithDeadFx(MonsterController monster, bool isClear = false)
        {
            if (monster != null)
            {
                monster.PlayDeadFx();
            }
            DestroyMonster(monster, isClear);
        }
        
        public async void DestroyDrop(DropController drop, bool isClear = false)
        {
            dropManager.Remove(drop);
            await drop.ReleaseModel(isClear);
            if(drop == null) return;
            AssetsManager.ReleaseInstance(drop.gameObject);
            if(drop == null) return;
            Destroy(drop.gameObject);
            // entityPool.ReleaseObj(drop.gameObject);
        }
        public virtual void OnMonsterDead(MonsterType monsterType, Vector3 pos)
        {
            
        }
        public bool CheckAllMonsterIsDead()
        {
            if (spawnManager.CurrenSpawnCount < spawnManager.TotalSpawnCount) return false;
            return enemyManager.EnemyList.Count <= 0;
        }

        /// <summary>
        /// 清理所有怪物
        /// </summary>
        /// <param name="clearBoss">是否清理 boss</param>
        public void ClearAllMonster(bool clearBoss)
        {
            var monsterList = ListPool<MonsterController>.Get();
            enemyManager.GetAllMonsters(monsterList, clearBoss);
            foreach (var monster in monsterList)
            {
                DestroyMonsterWithDeadFx(monster, true);
            }
            ListPool<MonsterController>.Release(monsterList);
        }
        
        /// <summary>
        /// 重置战斗状态，用于复活
        /// </summary>
        public void ResetFightState()
        {
            GamePlayManager.ClearAllBullets();
            TimerManager.Instance.RemoveTimerByTag(GameConst.TimerTagName.Grinding);
            for (var i = enemyManager.EnemyList.Count - 1; i >= 0; i--)
            {
                DestroyMonster(enemyManager.EnemyList[i],true);
            }
        }
    }
}