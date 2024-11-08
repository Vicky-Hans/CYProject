using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DHFramework;
using Pathfinding.DHRVO;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Pool;

namespace DH.Game
{
    public enum DeadType
    {
        Plain = 0,
        Suicide,
        Beheaded
    }
    
    public partial class MonsterController : BaseMonoUnit, IDamageable
    {
        [AssetPath] public string modelPath;
        [AssetPath] public string deadFxPath;
        [AssetPath] public string suicideFxPath;
        [AssetPath] public string beheadedFxPath;
        public float deadFxTime;
        public AttackBox attackBox;
        
        private Vector3 target;
        private Transform trans;
        private IMonsterAnimator mainAnimator;
        private LittleEnemySkill littleEnemySkill;
        private EnemyHpBar enemyHpBar;
        private IMonsterVertexColor monsterVertexColor;
        private readonly List<BaseBuff> buffList = new();

        private bool assetReady;
        protected GameObject model;
        protected SpriteRenderer spriteRenderer;


        private Transform buffNodeTrans;
        private Transform vertigoBuffNodeTrans;

        private FightingBaseManager fightingBaseManager;
        private MonsterHpShield monsterHpShield;
        private MonsterSelfDestruct monsterSelfDestruct;
        private MonsterLowHpRecovery lowHpRecovery;  // 特性 静止恢复血量
        // boss 是否处于进场行走中
        public bool EnterMoving { get; set; }
        public EMonsterMoveType MoveType { get; set; }
        public int MonsterEntry { get; set; }
        
        public bool IsSplitBody { get; set; } // 是否是分身，分身不提供exp，不继承怪物特性
        public Transform TargetTrans { get; set; }

        public Rigidbody2D Rgb2d { get; private set; }
        
        public SpineAnimator SpineAnimator => mainAnimator as SpineAnimator;

        public void SetTarget(Vector3 t)
        {
            target = t;
        }
        // 被牵引的目标位置
        public Vector3 PullPos { get; set; }
        // 被牵引的时间, 0表示没有牵引
        public float PullTime { get; set; }
        // 牵引速度
        public float PullSpd { get; set; }
        // 技能行动中
        public bool SkillTaking { get; set; }
        public void Init(MonsterCfg unit, Vector3 pos, Agent agent, AssetPoolEntity assetPool)
        {
            fightingBaseManager = BattleManager.Instance.fightingManagerIns;
            GetComponent<Collider2D>().enabled = true;
            trans = transform;
            data = new Monster(unit, isSplitBody:IsSplitBody);
            skillController.Init(data, this, assetPool);
            SetTarget(pos);
            SetupView();
            buffNodeTrans = transform.Find("buffNode");
            vertigoBuffNodeTrans = transform.Find("vertigoBuffNode");
            attackBox.ignoreTargetProtect = true;
            attackBox.damageArgs.sender = this;
            attackBox.damageArgs.damagePoint = (long)data.attr.Calc(AttributeType.Atk);
            attackBox.ignoreAttack = !BattleManager.Instance.IsStageForest();
            // FixAttackBoxSize();
            TargetTrans = fightingBaseManager.playerCtrl.transform;
            // feature hpShield
            var hpShieldTrigger = Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.DebutTime, AttributeType.HpShield);
            if (hpShieldTrigger != null)
            {
                AddHpShieldObj(hpShieldTrigger);
            }
            // feature debut speed
            var debutUpSpdTrigger = Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.DebutTime, AttributeType.UpSpd);
            if (debutUpSpdTrigger != null)
            {
                var accelerateTime = debutUpSpdTrigger.attrMgr.Calc(AttributeType.AccelerateTime) * GameConst.TimeDivisor;
                var upSpd = debutUpSpdTrigger.attrMgr.Calc(AttributeType.UpSpd);
                var buff = new Buff
                {
                    id = (int)AttributeType.UpSpd,
                    attrName = AttributeName.UpSpd,
                    startTime = GameTime.Instance.GTime,
                    duration = accelerateTime,
                    valueType = BuffValueType.Positive,
                    value = upSpd,
                    multi = false
                };
                Data.buffMgr.AddBuff(buff);
            }
            // feature lowHpRecovery
            lowHpRecovery = new MonsterLowHpRecovery();
            lowHpRecovery.Init(this);
            //是否带有自毁属性
            var selfDestructTrigger = Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.CollisionTime, AttributeType.SelfDestruct);
            if (selfDestructTrigger != null) AddSelfDestruct(selfDestructTrigger);
            // feature atkBonusRange
            if (MonsterData.attr.Calc(AttributeType.AttackBonusRange) > 0)
            {
                AddAtkBonusRange();
            }
            if (MonsterData.attr.Calc(AttributeType.DownCdRange) > 0)
            {
                AddDownCdRange();
            }
            if (MonsterData.attr.Calc(AttributeType.BreakArmorRange) > 0)
            {
                AddBreakArmorRange();
            }
            if (Data.attr.Calc(AttributeType.ImmuneStop) > 0)//免疫定身特性
            {
                Data.buffMgr.AddBuff(new Buff
                {
                    id = (int)AttributeType.ImmuneStop,
                    attrName = AttributeName.ImmuneStop,
                    startTime = GameTime.Instance.GTime,
                    duration = 3600*24f,
                    interval = 999f,
                    value = 0f,
                    valueType = BuffValueType.Positive,
                    multi = false,
                });
            }
        }

        public Vector3 GetHurtPos()
        {
            return buffNodeTrans.position;
        }
        public virtual void OnUpdate(float deltaTime)
        {
            if(MonsterData.IsDead())return;
            if (IsStucked())
            {
                mainAnimator?.Pause();
            }
            else
            {
                mainAnimator?.Resume();
            }
           
            Move(deltaTime);
            CheckFlipX();
            
            // 记录当前位置
            Data.position = transform.position;
            if(littleEnemySkill != null)
            {
                littleEnemySkill.OnUpdate(deltaTime);
            }
            if (MonsterData == null || MonsterData.IsDead()) return;
            skillController.OnUpdate(deltaTime);
            Data.buffMgr.CheckValid(GameTime.Instance.GTime);
            lowHpRecovery?.OnUpdate(deltaTime);
        }
        protected override void OnAssetsLoaded()
        {
            assetReady = true;
            SetupView();
        }
        /// <summary>
        /// 资源可能已经加载完成，防止时序导致MonsterData为空，需要在Init时尝试加载模型
        /// </summary>
        private void SetupView()
        {
            if (!assetReady || model || data == null) return;
            if (cts == null || cts.IsCancellationRequested) return;
            var modelPoint = transform.Find("Model");
            model = InstantiateObj(modelPath, modelPoint, false);
            if(MonsterData.cfg.MonsterType == (int)MonsterType.Normal)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
            mainAnimator = model.GetComponent<IMonsterAnimator>();
            if (mainAnimator is SpineAnimator spineAnimator)
            {
                spineAnimator.Animator.UpdateMode = UpdateMode.Nothing;
            }
            littleEnemySkill = GetComponent<LittleEnemySkill>();
            if(littleEnemySkill != null) littleEnemySkill.Init();
            if (MonsterData.cfg.MonsterType == (int)MonsterType.Boss)
            {
                enemyHpBar = GetComponentInChildren<EnemyHpBar>(true);
                if (enemyHpBar != null) enemyHpBar.gameObject.SetActive(true);
                if (BattleManager.Instance.IsStageForest()) ShowHpBar(false);
            }
            monsterVertexColor = GetComponentInChildren<IMonsterVertexColor>();
            CheckFlipX();
            PlayWalk();
            ProcessForestRgb2d();
        }
        private void ProcessForestRgb2d()
        {
            if (!BattleManager.Instance.IsStageForest()) return;
            var mCollider = GetComponent<Collider2D>();
            mCollider.isTrigger = false;
            Rgb2d = GetComponent<Rigidbody2D>();
            Rgb2d.gravityScale = 0;
            Rgb2d.mass = 1f;
            Rgb2d.freezeRotation = true;
            Rgb2d.bodyType = RigidbodyType2D.Dynamic;
            Rgb2d.sharedMaterial = BattleManager.Instance.MapFightingManager.monsterPhyMat;
        }
        public bool HasHpShieldObj()
        {
            return monsterHpShield != null && monsterHpShield.HpShield > 0;
        }
        private void AddHpShieldObj(SkillTrigger trigger)
        {
            if(trigger == null)return;
            if(MonsterData.IsDead())return;
            if (monsterHpShield == null)
            {
                var hpShieldPrefab = fightingBaseManager.MonsterHpShieldPrefab;
                var hpShieldObj = Instantiate(hpShieldPrefab, Vector3.zero, Quaternion.identity, transform);
                if(hpShieldObj == null)return;
                monsterHpShield = hpShieldObj.GetComponent<MonsterHpShield>();
                monsterHpShield.transform.localScale = Vector3.one *(GetFrozenScale()+0.5f);
                monsterHpShield.transform.localPosition = GetFrozenPos();;
            }
            monsterHpShield.Init(this, trigger);
        }
        private void AddAtkBonusRange()
        {
            if(MonsterData.IsDead())return;
            var atkBonusRangePrefab = fightingBaseManager.MonsterAtkBonusRangePrefab;
            var atkBonusObj = Instantiate(atkBonusRangePrefab, Vector3.zero, Quaternion.identity, transform);
            if(atkBonusObj == null)return;
            var monsterAtkBonusRange = atkBonusObj.GetComponent<MonsterAtkBonusRange>();
            monsterAtkBonusRange.transform.localPosition = Vector3.zero;
            monsterAtkBonusRange.Init(this);
        }
        private void AddDownCdRange()
        {
            if(MonsterData.IsDead())return;
            var downCdRangePrefab = fightingBaseManager.MonsterDownCdRangePrefab;
            var tmpObj = Instantiate(downCdRangePrefab, Vector3.zero, Quaternion.identity, transform);
            if(tmpObj == null)return;
            var monsterDownCdRange = tmpObj.GetComponent<MonsterDownCdRange>();
            monsterDownCdRange.transform.localPosition = Vector3.zero;
            monsterDownCdRange.Init(this);
        }
        /// <summary>
        /// 添加自毁特性
        /// </summary>
        private void AddSelfDestruct(SkillTrigger trigger)
        {
            if(trigger == null || MonsterData.IsDead())return;
            var selfDestructPrefab = fightingBaseManager.MonsterSelfDestructPrefab;
            var selfDestructObj = Instantiate(selfDestructPrefab, Vector3.zero, Quaternion.identity, transform);
            if(selfDestructObj == null)return;
            monsterSelfDestruct = selfDestructObj.GetComponent<MonsterSelfDestruct>();
            monsterSelfDestruct.transform.localPosition = Vector3.zero;
            monsterSelfDestruct.Init(this, trigger);
        }
        /// <summary>
        /// 增加破甲特性
        /// </summary>
        private void AddBreakArmorRange()
        {
            if(MonsterData.IsDead())return;
            var breakArmorPrefab = fightingBaseManager.MonsterBreakArmorPrefab;
            var breakArmorObj = Instantiate(breakArmorPrefab, Vector3.zero, Quaternion.identity, transform);
            if(breakArmorObj == null) return;
            var monsterBreakArmorRange = breakArmorObj.GetComponent<MonsterBreakArmor>();
            monsterBreakArmorRange.transform.localPosition = Vector3.zero;
            monsterBreakArmorRange.Init(this);
        }
        /// <summary>
        /// 死亡时为周围恢复生命
        /// </summary>
        private void CheckUnderDeadRevert()
        {
            var trigger = Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.Dead, AttributeType.HpRevert);
            if(trigger == null) return;
            if(!Data.CheckProb(trigger)) return;
            var pos = trans.position;
            var hpRevert = trigger.attrMgr.Calc(AttributeType.HpRevert);
            var revertRange = trigger.attrMgr.Calc(AttributeType.RevertRange);
            var maxHp = Data.resource.MaxHp;
            var revertHp = hpRevert * maxHp;
            PlayUnderDeadRevertFx(pos, revertRange);
            var layerMask = LayerMask.GetMask("Enemy");
            var count = Physics2D.OverlapCircleNonAlloc(pos, revertRange, PhysicsUtility.CacheCollider, layerMask);
            for (var i = 0; i < count; i++)
            {
                var tmpTarget = PhysicsUtility.CacheCollider[i];
                var monster = tmpTarget.GetComponent<MonsterController>();
                if(monster == null)continue;
                if(monster.Data.IsDead())return;
                monster.HealDelta((int)revertHp);
            }
        }
        /// <summary>
        /// 静止回血特效
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="range"></param>
        public void PlayStopRevertFx(float duration, float range = 1f)
        {
            if(CheckMonsterIsDead())return;
            var entityPool = BattleManager.Instance.fightingManagerIns.entityPool;
            var assetPath = "Player/MonsterStopRevert";
            entityPool.LoadAssetSync(assetPath);
            var obj = entityPool.InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, trans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range;
            BattleManager.Instance.fightingManagerIns.AddAutoReleaseGObj(obj, duration);
        }
        /// <summary>
        /// 播放死亡范围恢复特效
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="range"></param>
        public void PlayUnderDeadRevertFx(Vector3 pos, float range)
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightingManager.entityPool;
            var assetPath = "Player/MonsterRangeRevert";
            entityPool.LoadAssetSync(assetPath);
            var obj = entityPool.InstantiateObj(assetPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj) return;
            obj.transform.localScale = Vector3.one * range;
            fightingManager.AddAutoReleaseGObj(obj, 2);
        }
        private void PlayWalk()
        {
            mainAnimator?.PlayWalk();
        }
        
        private void PlayDead(bool isClear = false)
        {
            // 分裂检查
            if(isClear){}
            else
            {
                CheckSplit();
                CheckUnderDeadRevert();
            }
            if (MonsterData.cfg.MonsterType == (int)MonsterType.Boss)
            {
                monsterVertexColor?.PlayDead();
            }
            else
            {
                mainAnimator?.PlayDead();
            }
        }
        
        private void CheckSplit()
        {
            if(IsSplitBody)return;
            var splitTrigger = Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.Dead, AttributeType.OwnSplit);
            if(splitTrigger == null)return;
            if(!Data.CheckProb(splitTrigger))return;
            var splitNum = Lodash.RoundToInt(splitTrigger.attrMgr.Calc(AttributeType.OwnSplit));
            if(splitNum <= 0)return;
            var splitPos = transform.position;
            var maxHpPercent = splitTrigger.attrMgr.Calc(AttributeType.MaxHpPercent);
            var maxAtkPercent = splitTrigger.attrMgr.Calc(AttributeType.MaxAtkPercent);
            for (var i = 0; i < splitNum; i++)
            {
                var mPos = splitPos + new Vector3(Lodash.RandRangeFloat(0f, 0.6f),Lodash.RandRangeFloat(-0.5f, 0.5f), 0);
                mPos = ClampPos(mPos);
                var monster = fightingBaseManager.CloneMonster(this);
                monster.ShowHpBar(false);
                UpdateHpAndAtk(maxHpPercent, maxAtkPercent, this, monster);
                monster.transform.position = mPos;
            }
        }
        /// <summary>
        /// 更新攻击力
        /// </summary>
        /// <param name="atk"></param>
        private void UpdateAtk(long atk)
        {
            MonsterData.ReInitAtk(atk);
            attackBox.damageArgs.damagePoint = atk;
        }
        /// <summary>
        /// 分裂、召唤的怪 血量攻击更新
        /// </summary>
        /// <param name="hpPercent"></param>
        /// <param name="atkPercent"></param>
        /// <param name="originMonster"></param>
        /// <param name="newMonster"></param>
        public void UpdateHpAndAtk(float hpPercent, float atkPercent, MonsterController originMonster, MonsterController newMonster)
        {
            if(originMonster== null || newMonster == null)return;
            if(hpPercent > 0)
            {
                var mMaxHp = originMonster.MonsterData.resource.MaxHp;
                mMaxHp = Lodash.RoundToInt(mMaxHp * hpPercent);
                newMonster.MonsterData.ReInitHp(mMaxHp);
            }
            if (!(atkPercent > 0)) return;
            var tmpAtk = originMonster.MonsterData.attr.Calc(AttributeType.Atk);
            tmpAtk *= atkPercent;
            newMonster.UpdateAtk(Lodash.RoundToInt(tmpAtk));
        }
        public void PlayAnimation(string aniName)
        {
            mainAnimator?.PlayAnimation(aniName);
        }
        public void CheckFlipX()
        {
            if(TargetTrans == null)
            {
                mainAnimator.FlipX(0 > transform.position.x);
                return;
            }
            mainAnimator?.FlipX(TargetTrans.position.x > transform.position.x);
        }
        /// <summary>
        /// 释放对象，方便使用对象池进行管理
        /// </summary>
        public async UniTask ReleaseModel(bool isClear = false)
        {
            foreach (var item in buffList)
            {
                if (!item || item.Recycled) continue;
                item.Recycle();
            }
            buffList.Clear();
            
            if (enemyHpBar != null)
            {
                enemyHpBar.gameObject.SetActive(false);
            }
            var mCollider = GetComponent<Collider2D>();
            if(mCollider!=null)
            {
                mCollider.enabled = false;
            }
            PlayDead(isClear);
            if (model)
            {
                ReleaseObj(model);
                model = null;
            }
            skillController.Release();
            ReleaseAssets();
        }
        public void PlayDeadFx(DeadType deadType = DeadType.Plain)
        {
            var fxPath = deadFxPath;
            if (deadType == DeadType.Beheaded)
            {
                fxPath = beheadedFxPath;
            }
            else if (deadType == DeadType.Suicide)
            {
                fxPath = suicideFxPath;
            }
            var fxScale = MonsterData.cfg.MonsterType == (int)MonsterType.Boss ? 2f : 1f;
            var pos = trans.position;
            var fx = InstantiateObj(fxPath, pos, Quaternion.identity, trans.parent);
            if (fx == null) return;
            fx.transform.localScale = Vector3.one * fxScale;
            BattleManager.Instance.fightingManagerIns.AddAutoReleaseUnit(fx, deadFxTime, this);
        }
        private void PlayHurt(int weaponModelId)
        {
            FightingSoundHelper.Instance.PlayWeaponHit(weaponModelId);
            monsterVertexColor?.PlayHurt();
        }
        
        public void DecHpValue(long value, int weaponModelId = 0)
        {
            if (HasHpShieldObj())
            {
                monsterHpShield.DecHp((int)value);
                return;
            }
            data.resource.DecHp(value);
            lowHpRecovery?.CheckHpRecoveryTrigger();//检测怪物受到伤害后有概率恢复其攻击力100%的生命值
            lowHpRecovery?.TriggerHpPercent();
            UpdateHpBar();
            if(weaponModelId > 0)
            {
                PlayerStats.Instance.AddHurt(weaponModelId, value);
            }
        }
        public DamageStatus OnDamage(DamageArgs args)
        {
            var status = DamageStatus.None;
            var isSuicide = false;
            if (data.resource.IsDead())
            {
                args.skillData.KillNum++;
                return DamageStatus.Ignore;
            }
            // 免伤
            if (MonsterData.ImmuneDmg())
            {
                BattleManager.Instance.fightingManagerIns.ShowHurtImmune(transform.position);
                return status |= DamageStatus.Normal;
            }
            var canIgnoreDownDmg = false;
            PlayHurt(args.weaponModelId);
            if (args.skillId != 0 && args.skillData != null)
            {
                if (args.beheaded)
                {
                    args.damagePoint = data.resource.Hp;
                }
                else if (args.clothesId == 0)
                {
                    var hurtResult = args.skillData.CalcDmg(data, args.weaponSkill, args.dmgType, canIgnoreDownDmg:canIgnoreDownDmg);
                    if (args.dmgPercent > 0)
                    {
                        hurtResult.dmg *= args.dmgPercent;
                    }
                    args.damagePoint = (long)hurtResult.dmg;
                    args.isCrit = hurtResult.hurtType == SkillHurtType.Crit;
                }
            }
            else
            {
                if (MonsterData.cfg.AtkType == 1) // 自爆
                {
                    isSuicide = true;
                    args.damagePoint = MonsterData.resource.Hp;
                }
            }
            DecHpValue(args.damagePoint, args.weaponModelId);
            if (args.skillId != 0) // 技能伤害才显示
                BattleManager.Instance.fightingManagerIns.ShowHurtNum(args, transform.position);
            if (data.resource.Hp <= 0)
            {
                if (args is { skillData: not null }) args.skillData.KillNum++;
                var deadType = DeadType.Plain;
                if (args.beheaded)
                {
                    deadType = DeadType.Beheaded;
                }
                else if (isSuicide)
                {
                    deadType = DeadType.Suicide;
                }
                ProcessDead(deadType);
                CheckMonsterHpShield();//检测怪物死亡后有概率为周围怪物生成最大生命值50%的护盾
                args.skillData?.CheckKillMonsterAttrTrigger(PlayerStats.Instance.KillCount);
                if (args.skillData != null) CheckAtkTriggerPoisonCircleAttr(args);
                status |= DamageStatus.Dead;
            }
            if (args.skillData != null)
            {
                CheckSClothes910063Trigger(args);//检测处于背包的武器攻击敌人时有15%概率携带连锁闪电，对攻击目标及周围的3命敌人造成电击伤害
                CheckPlayerInvincibleAttr(args);//检测玩家无敌属性
            }
            if(args.skillData != null && args.weaponSkill != null && data.resource.Hp > 0)
            {
                if (args.weaponSkill.equipCfg.AtkType == (int)EquipAtkType.Physic)
                {
                    CheckBleedAttr(args);
                }
                else if (args.weaponSkill.equipCfg.AtkType == (int)EquipAtkType.Magic)
                {
                    CheckMagicAtkTriggerPoisonAttr(args);
                }
                if (args.weaponSkill.WeaponModelId == 1405) CheckPalsyWandStop(args);//凤凰魔杖；凤凰魔杖定身属性检测
                CheckMagic910058SkillAttr(args);//对怪物造成伤害时有概率使怪物获得剧毒路径
                CheckSClothes910083Trigger(args);//检测攻击有5%概率附带当前护盾值15%的伤害
            }
            return status |= DamageStatus.Normal;
        }
        public void DecHp(int dmg, int weaponModelId = 0)
        {
            if (data.resource.IsDead()) return;
            DecHpValue(dmg, weaponModelId);
            BattleManager.Instance.fightingManagerIns.ShowHurtNumOnly(dmg, transform.position);
            if (data.resource.Hp <= 0)
            {
                ProcessDead();
            }
        }
        /// <summary>
        /// 怪物直接死亡
        /// </summary>
        public void DecHpToDead()
        {
            if (data.resource.IsDead()) return;
            var dmg = data.resource.Hp;
            if (HasHpShieldObj())
            {
                monsterHpShield.DecHp(monsterHpShield.HpShield);
                dmg+=monsterHpShield.HpShield;
            }
            data.resource.DecHp(data.resource.Hp);
            lowHpRecovery?.TriggerHpPercent();
            
            BattleManager.Instance.fightingManagerIns.ShowHurtNumOnly((int)dmg, transform.position);
            if (data.resource.Hp <= 0) ProcessDead();
        }

        public void HealPercent(float percent)
        {
            var maxHp = data.resource.MaxHp;
            var tmpDelta = (int)(maxHp * percent);
            HealDelta(tmpDelta);
        }
        
        public void HealDelta(int delta)
        {
            data.resource.AddHp(delta);
            UpdateHpBar();
            var pos = transform.position;
            BattleManager.Instance.fightingManagerIns.ShowMonsterRecoverNum(pos, delta);
        }
        
        public void ProcessDead(DeadType deadType = DeadType.Plain)
        {
            AddExp();
            AddKillEnergy();
            MonsterData.DeadPoisonCount = MonsterData.PoisonCount;
            var dropCoin = (fightingBaseManager as MainFightingManager)?.GetDropGoldCoin()??0;
            GameDataManager.Instance.GameCoin += dropCoin;
            UIEffectManager.Instance.PlayerGameCoinAction(transform.position, dropCoin).Forget();
            if (MonsterData.cfg.MonsterType == (int)MonsterType.Boss)
            {
                if(!IsSplitBody)
                {
                    BattleManager.Instance.fightingManagerIns.BossCount--;
                }
            }
            if (deadType != DeadType.Suicide)
            {
                if (GameDataManager.Instance.CurStageType == EStateType.StageTypeEndless 
                    || GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
                {
                    if (!IsSplitBody) PlayerStats.Instance.KillCount++;
                }
                else
                {
                    PlayerStats.Instance.KillCount++;
                }
            }
            PlayDeadFx(deadType);
            BattleManager.Instance.fightingManagerIns.OnMonsterDead((MonsterType)MonsterData.cfg.MonsterType, transform.position);
            BattleManager.Instance.fightingManagerIns.DestroyMonster(this);
            if (!BattleManager.Instance.IsStageForest()) return;
            if (data is Monster monster && monster.cfg.MonsterType == (int)MonsterType.Boss)
            {
                BattleManager.Instance.fightingManagerIns.dropManager.RandomDropItem(1, transform.position,false);
            }
        }
        private void AddExp()
        {
            if(IsSplitBody)return;
            var exp = MonsterData.cfg.KillExp;
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeChallenge)
            {
                var newExp = GameDataManager.Instance.GetMonsterKillExpByMonsterId(MonsterData.cfg.Id);
                if (newExp > 0) exp = newExp;
            }
            var upExp = fightingBaseManager.playerCtrl.Data.attr.Calc(AttributeType.UpExp);
            exp = (int)(exp * (1+upExp));
            if (BattleManager.Instance.IsStageForest())
            {
                // 这里掉落经验 
                BattleManager.Instance.fightingManagerIns.CreateDrop(exp, transform.position);
            }
            else
            {
                PlayerStats.Instance.KillExp += exp;
            }
        }
        private void AddKillEnergy()
        {
            if(IsSplitBody)return;
            var energy = MonsterData.cfg.KillEnergy;
            energy += Lodash.RoundToInt(energy*GetUpEnergyRecovery());//能量提升率
            GameDataManager.Instance.AddHeroActiveEnergy(energy);
        }
        private void UpdateHpBar()
        {
            if(enemyHpBar == null)return;
            enemyHpBar.SetPercent(data.resource.Progress);
        }
        private void ShowHpBar(bool show)
        {
            if(enemyHpBar == null)return;
            enemyHpBar.gameObject.SetActive(show);
        }
        /// <summary>
        /// 被击退
        /// </summary>
        /// <param name="distance"></param>
        public void Repeled(float distance)
        {
            if(MonsterData.ImmuneRepel())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var shipPos = fightManager.playerCtrl.transform.position;
            var transPos = transform.position;
            var direction = transPos  - shipPos;
            if (MonsterData.cfg.SkillType == 1 && Mathf.Abs(direction.y) > 0) direction.x = 0;
            var delta = new Vector3((direction.normalized * distance).x,0,0);//只处理X轴方向（怪物从右到左的，Y轴就没必要处理了）
            transPos += delta;
            transPos = ClampPos(transPos);
            transform.position = transPos;
        }
        /// <summary>
        /// 限定横坐标在屏幕内
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Vector3 ClampPos(Vector3 pos)
        {
            return BattleManager.Instance.fightingManagerIns.ClampPos(pos);
        }
        private float GetFrozenScale()
        {
            var mType = MonsterData.cfg.MonsterType;
            return mType switch
            {
                (int)MonsterType.Normal => 1f,
                (int)MonsterType.Elite => 1f,
                (int)MonsterType.Boss => 1f,
                _ => 1f
            };
        }
        public Vector3 GetFrozenPos()
        {
            if (buffNodeTrans)
            {
                var wPos = buffNodeTrans.position;
                var lPos = transform.InverseTransformPoint(wPos);
                return lPos;
            }
            var modelId = MonsterData.cfg.ModelId;
            return modelId is 521 or 522 or 523 or 524 ? new Vector3(0f, 1f, 0f) : Vector3.zero;
        }
        private float GetFiringScale()
        {
            return GetFrozenScale();
        }
        private float GetVertigoScale()
        {
            var mType = MonsterData.cfg.MonsterType;
            return mType switch
            {
                (int)MonsterType.Normal => 1f,
                (int)MonsterType.Elite => 1f,
                (int)MonsterType.Boss => 1f,
                _ => 1f
            };
        }
        private Vector3 GetVertigoPos()
        {
            if (vertigoBuffNodeTrans)
            {
                var wPos = vertigoBuffNodeTrans.position;
                var lPos = transform.InverseTransformPoint(wPos);
                return lPos;
            }
            var mType = MonsterData.cfg.MonsterType;
            return mType switch
            {
                (int)MonsterType.Normal => new Vector3(0f, 0.5f, 0),
                (int)MonsterType.Elite => new Vector3(0f, 0.9f, 0),
                (int)MonsterType.Boss => new Vector3(0f, 2f, 0),
                _ => new Vector3(0f, 0.5f, 0)
            };
        }
        /// <summary>
        /// add冰冻buff
        /// </summary>
        /// <param name="buff"></param>
        public void AddFrozenBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            if(MonsterData.ImmuneFrozen())return;
            FightingSoundHelper.Instance.PlayBuffFrozen();
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_frozen";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFrozenScale();
            var buffComp = buffObj.GetComponent<BuffFrozen>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        
        /// <summary>
        /// add重伤buff
        /// </summary>
        /// <param name="buff"></param>
        public void AddHurtBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            if(MonsterData.ImmuneHurt())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_hurt";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFrozenScale();
            var buffComp = buffObj.GetComponent<BuffHurt>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        public void AddFiringBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            FightingSoundHelper.Instance.PlayBuffBurn();
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_firing";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffFiring>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }

        public void AddPoisonBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_poison";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffPoison>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }

        public void AddElectrifyBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_electrify";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffElectrify>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        /// <summary>
        /// add减速buff
        /// </summary>
        /// <param name="buff"></param>
        public void AddDecelerateBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_decelerate";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffDecelerate>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }

        /// <summary>
        /// buff stop
        /// </summary>
        /// <param name="buff"></param>
        public void AddStopBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_stop";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffStop>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        /// <summary>
        /// 藤曼缠绕束缚buff 
        /// </summary>
        /// <param name="buff"></param>
        public void AddEnwindBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_enwind";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffEnWind>();
            if(buffComp == null)return;
            DHLog.Warning("输出老魔杖的缠绕时间："+buff.duration);
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        public void AddImmuneDmgBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            MonsterData.buffMgr.AddBuff(buff);
        }

        public void AddFaintingBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_fainting";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffFainting>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        /// <summary>
        /// add晕眩buff
        /// </summary>
        /// <param name="buff"></param>
        public void AddVertigoBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            if(MonsterData.ImmuneVertigo())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_vertigo";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetVertigoPos();
            buffObj.transform.localScale = Vector3.one * GetVertigoScale();
            var buffComp = buffObj.GetComponent<BuffVertigo>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        
        public void AddPalsyBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_palsy";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetVertigoPos();
            buffObj.transform.localScale = Vector3.one * GetVertigoScale();
            var buffComp = buffObj.GetComponent<BuffPalsy>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        /// <summary>
        /// 获取玩家能量恢复提升
        /// </summary>
        /// <returns></returns>
        private float GetUpEnergyRecovery()
        {
            var upValue = fightingBaseManager.playerCtrl.Data.attr.Calc(AttributeType.EnergyRateUp);
            return upValue;
        }
        /// <summary>
        /// 检测玩家无敌属性
        /// </summary>
        /// <param name="args"></param>
        private void CheckPlayerInvincibleAttr(DamageArgs args)
        {
            if (args.skillData?.owner.buffMgr.FindBuffById((int)AttributeType.HeroHpValueLess) == null) return;
            if (args.skillData.owner.buffMgr.FindBuffById((int)AttributeType.AtkReplay) != null)
            {
                var buff = args.skillData.owner.buffMgr.FindBuffById((int)AttributeType.AtkReplay);
                var newHp = args.skillData.owner.resource.MaxHp * buff.value;
                args.skillData.owner.resource.AddHp(Lodash.RoundToInt(newHp));
                BattleManager.Instance.fightingManagerIns.ShowPlayerRecoverNum(Lodash.RoundToInt(newHp));
            }
            else
            {
                var heroInvincibleTrigger = args.skillData.owner.triggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroHpValueLess,AttributeType.HeroImmune);
                if (heroInvincibleTrigger == null) return;
                var atkReplay = heroInvincibleTrigger.attrMgr.Calc(AttributeType.AtkReplay);
                var atkReplayTime = heroInvincibleTrigger.attrMgr.Calc(AttributeType.AtkReplayTime)*GameConst.TimeDivisor;
                if (!(atkReplayTime > 0)) return;
                var newHp = args.skillData.owner.resource.MaxHp * atkReplay;
                args.skillData.owner.resource.AddHp(Lodash.RoundToInt(newHp));
                BattleManager.Instance.fightingManagerIns.ShowPlayerRecoverNum(Lodash.RoundToInt(newHp));
                BattleManager.Instance.fightingManagerIns.playerCtrl.AddAtkReplayBuff(new Buff
                {
                    id = (int)AttributeType.AtkReplay,
                    attrName = AttributeName.AtkReplay,
                    startTime = GameTime.Instance.GTime,
                    duration = atkReplayTime,multi = false,
                    valueType = BuffValueType.Positive,
                    value = atkReplay
                });
            }
        }
        /// <summary>
        /// 麻痹雷杖定身触发检测
        /// </summary>
        /// <param name="args"></param>
        private void CheckPalsyWandStop(DamageArgs args)
        {
            if (MonsterData.ImmuneStop()) return;
            var palsyWandStopTrigger = args.skillData?.GetTriggerByNameAndComplete(AttributeName.PalsyWandHit,AttributeType.PalsyWandStop);
            if (palsyWandStopTrigger == null) return;
            palsyWandStopTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue* GameConst.AttributeDivisor+args.weaponSkill.SkillData.attrMgr.Calc(AttributeType.PalsyWandProb);
            if (Lodash.RandRangeFloat(0, 1f) > prob) return;
            var bleedBuffs = Data.buffMgr.FindBuffsById((int)AttributeType.StopTime);
            if (bleedBuffs is { Count: > 0 }) return;
            var palsyWandStopTime = palsyWandStopTrigger.attrMgr.Calc(AttributeType.PalsyWandStopTime)*GameConst.TimeDivisor;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var assetPath = $"Fighting/Buff/buff_palsyWandStop";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null) return;
            buffObj.transform.localPosition = GetFrozenPos();
            buffObj.transform.localScale = Vector3.one * GetFiringScale();
            var buffComp = buffObj.GetComponent<BuffPalsyWandStop>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.StopTime,
                attrName = AttributeName.StopTime,
                startTime = GameTime.Instance.GTime,
                duration = palsyWandStopTime,value = 0,
                interval = 0, skillData = args.skillData,
                multi = false, valueType = BuffValueType.Negative
            }, fightManager.entityPool);
            buffList.Add(buffComp);
        }
        /// <summary>
        /// 检测怪物死亡后有概率为周围怪物生成最大生命值50%的护盾
        /// </summary>
        private void CheckMonsterHpShield()
        {
            var hpShieldTrigger = MonsterData.triggerSkill.GetTriggerByNameAndComplete(AttributeName.Dead,
                AttributeType.HpShield);
            if (hpShieldTrigger == null) return;
            hpShieldTrigger.trigger.TryGetValue("prob", out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if (Lodash.RandRangeFloat(0, 1) > prob) return;
            var hpShieldRange = hpShieldTrigger.attrMgr.Calc(AttributeType.HpShieldRange);
            var monsterList = BattleManager.Instance.fightingManagerIns.GetRandMonstersInRange(transform.position,hpShieldRange);
            if (monsterList.Count == 0) 
            {
                ListPool<MonsterController>.Release(monsterList);
                return;
            }
            foreach (var monsterCtrl in monsterList)
            {
                if (monsterCtrl == null || monsterCtrl.CheckMonsterIsDead()) continue;
                if (monsterCtrl.HasHpShieldObj() || monsterCtrl.MonsterData == MonsterData) continue;
                monsterCtrl.AddHpShieldObj(hpShieldTrigger);
            }
            ListPool<MonsterController>.Release(monsterList);
        }
        #region 物理服饰技能逻辑相关
         /// <summary>
        /// 检测物理攻击时触发的流血特性
        /// </summary>
        private void CheckBleedAttr(DamageArgs args)
        {
            var bleedTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderPhyAttack, AttributeType.Bleed);
            var phyEquipBeUpTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.PhyEquipBeUped, AttributeType.Bleed);
            var underPhy4AttackTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderPhy4Attack, AttributeType.Bleed);
            var phyAttackMonsterRangeTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.PhyAttackMonsterRange, AttributeType.Bleed);
            if (bleedTrigger != null) CheckUnderPhyAttackBleedTrigger(args,bleedTrigger);//检测物理攻击时触发的流血特性
            if (phyEquipBeUpTrigger != null) CheckPhyEquipBeUpBleedTrigger(args,phyEquipBeUpTrigger);//物理武器受到其他武器加成时,攻击有2%概率给敌人添加1层流血效果
            if (underPhy4AttackTrigger != null) CheckUnderPhy4AttackBleedTrigger(args,underPhy4AttackTrigger);//物理武器合成到4阶后，攻击时有2%概率给敌人添加1层流血效果
            if (phyAttackMonsterRangeTrigger != null) CheckPhyAttackMonsterRangeBleedTrigger(args,phyAttackMonsterRangeTrigger);//对距离小于5米的怪物造成物理伤害时，有50%概率附带1层流血效果
        }
        /// <summary>
        /// 检测物理攻击时触发的流血特性
        /// </summary>
        /// <param name="args"></param>
        /// <param name="bleedTrigger"></param>
        private void CheckUnderPhyAttackBleedTrigger(DamageArgs args,SkillTrigger bleedTrigger)
        {
            bleedTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            //检测当前怪物是否有流血buff
            var superpositionCount = CheckHaveBleedBuff();
            if (superpositionCount >= args.skillData.owner.clothesTriggerSkill.GetBleedMaxSuperpose()) return;
            var bleedTime = bleedTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
            var bleedDmg = args.damagePoint * bleedTrigger.attrMgr.Calc(AttributeType.BleedDmg);
            if (args.skillData.owner.ClothesResource.BleedTimeSuperposeCount > 0)
            {
                bleedTime += args.skillData.owner.ClothesResource.BleedTimeSuperposeCount*args.skillData.owner.ClothesResource.BleedSuperposeTime;
                if (bleedTime > 1) bleedTime = 1f;
            }
            CreateBleedBuffAndAdd(bleedTime,bleedDmg);
        }
        /// <summary>
        /// 物理武器受到其他武器加成时,攻击有2%概率给敌人添加1层流血效果，触发概率可叠加
        /// </summary>
        /// <param name="args"></param>
        /// <param name="bleedTrigger"></param>
        private void CheckPhyEquipBeUpBleedTrigger(DamageArgs args, SkillTrigger bleedTrigger)
        {
            bleedTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            if (!GameDataManager.Instance.CheckPhyWeaponAttrAffected(args.weaponModelId)) return;
            //检测当前怪物是否有流血buff
            var superpositionCount = CheckHaveBleedBuff();
            if (superpositionCount >= args.skillData.owner.clothesTriggerSkill.GetBleedMaxSuperpose()) return;
            var bleedTime = bleedTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
            var bleedDmg = args.damagePoint * bleedTrigger.attrMgr.Calc(AttributeType.BleedDmg);
            CreateBleedBuffAndAdd(bleedTime,bleedDmg);
        }
        /// <summary>
        /// 物理武器合成到4阶后，攻击时有2%概率给敌人添加1层流血效果，触发概率可叠加
        /// </summary>
        /// <param name="args"></param>
        /// <param name="bleedTrigger"></param>
        private void CheckUnderPhy4AttackBleedTrigger(DamageArgs args, SkillTrigger bleedTrigger)
        {
            bleedTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            if (args.weaponSkill.backData.WeaponLev != 4) return;
            //检测当前怪物是否有流血buff
            var superpositionCount = CheckHaveBleedBuff();
            if (superpositionCount >= args.skillData.owner.clothesTriggerSkill.GetBleedMaxSuperpose()) return;
            var bleedTime = bleedTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
            var bleedDmg = args.damagePoint * bleedTrigger.attrMgr.Calc(AttributeType.BleedDmg);
            CreateBleedBuffAndAdd(bleedTime,bleedDmg);
        }
        /// <summary>
        /// 对距离小于5米的怪物造成物理伤害时，有50%概率附带1层流血效果
        /// </summary>
        /// <param name="args"></param>
        /// <param name="bleedTrigger"></param>
        private void CheckPhyAttackMonsterRangeBleedTrigger(DamageArgs args,SkillTrigger bleedTrigger)
        {
            bleedTrigger.trigger.TryGetValue(AttributeName.Prob, out var tmpProbValue);
            var prob = tmpProbValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            bleedTrigger.trigger.TryGetValue(AttributeName.PhyAttackMonsterRange, out var atkRangeValue);
            var atkRange = atkRangeValue * GameConst.AttributeDivisor;
            if ((trans.position - fightingBaseManager.playerCtrl.transform.position).sqrMagnitude > atkRange * atkRange) return;
            //检测当前怪物是否有流血buff
            var superpositionCount = CheckHaveBleedBuff();
            if (superpositionCount >= args.skillData.owner.clothesTriggerSkill.GetBleedMaxSuperpose()) return;
            var bleedTime = bleedTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
            var bleedDmg = args.damagePoint * bleedTrigger.attrMgr.Calc(AttributeType.BleedDmg);
            CreateBleedBuffAndAdd(bleedTime,bleedDmg);
        }
        /// <summary>
        /// 检测当前怪物是否有流血buff
        /// </summary>
        /// <returns></returns>
        private int CheckHaveBleedBuff()
        {
            var superpositionCount = 0;//叠加层数
            var bleedBuffs = Data.buffMgr.FindBuffsById((int)AttributeType.Bleed);
            if (bleedBuffs == null || bleedBuffs.Count == 0) return 0;
            foreach (var buffItem in bleedBuffs)
            {
                if (buffItem.clothesId > 0) superpositionCount++;
            }
            return superpositionCount;
        }
        /// <summary>
        /// 创建并添加流血buff
        /// </summary>
        /// <param name="bleedTime"></param>
        /// <param name="bleedDmg"></param>
        private void CreateBleedBuffAndAdd(float bleedTime,float bleedDmg)
        {
            AddBleedBuff(new Buff
            {
                id = (int)AttributeType.Bleed,
                attrName = AttributeName.Bleed,
                startTime = GameTime.Instance.GTime,
                duration = bleedTime,interval = 1,value = bleedDmg,
                valueType = BuffValueType.Negative,multi = true, clothesId = 1,
            });
        }
        /// <summary>
        /// 添加BleedBuff特效
        /// </summary>
        /// <param name="buff"></param>
        public void AddBleedBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_bleed";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetVertigoPos();
            buffObj.transform.localScale = Vector3.one * GetVertigoScale();
            var buffComp = buffObj.GetComponent<BuffBleed>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        #endregion
        #region 魔法服饰技能相关M
        /// <summary>
        /// 检测魔法攻击时触发的中毒特性
        /// </summary>
        private void CheckMagicAtkTriggerPoisonAttr(DamageArgs args)
        {
            var underMagicAttackTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderMagicAttack, AttributeType.Poison);
            var magicEquipBeUpTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.MagicEquipBeUped, AttributeType.Poison);
            var underMagic4AttackTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.UnderMagic4Attack, AttributeType.Poison);
            if (underMagicAttackTrigger != null) CheckUnderMagicAttackPoisonTrigger(args,underMagicAttackTrigger);//检测魔法武器攻击附带1层剧毒效果
            if (magicEquipBeUpTrigger != null) CheckMagicEquipBeUpPoisonTrigger(args,magicEquipBeUpTrigger);//魔法武器受到其他武器加成时攻击有2%概率给敌人添加1层剧毒效果
            if (underMagic4AttackTrigger != null) CheckUnderMagic4AttackPoisonTrigger(args,underMagic4AttackTrigger);//魔法武器合成达到4阶后，攻击时有2%概率给敌人添加1层剧毒效果
        }
        /// <summary>
        /// 测魔法武器攻击附带1层剧毒效果
        /// </summary>
        /// <param name="args"></param>
        /// <param name="poisonTrigger"></param>
        private void CheckUnderMagicAttackPoisonTrigger(DamageArgs args, SkillTrigger poisonTrigger)
        {
            poisonTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            CreatePoisonBuffAndAdd(args,poisonTrigger);
        }
        /// <summary>
        /// 魔法武器受到其他武器加成时攻击有2%概率给敌人添加1层剧毒效果
        /// </summary>
        /// <param name="args"></param>
        /// <param name="poisonTrigger"></param>
        private void CheckMagicEquipBeUpPoisonTrigger(DamageArgs args, SkillTrigger poisonTrigger)
        {
            poisonTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            if (!GameDataManager.Instance.CheckMagicWeaponAttrAffected(args.weaponModelId)) return;
            CreatePoisonBuffAndAdd(args,poisonTrigger);
        }
        /// <summary>
        /// 魔法武器合成达到4阶后，攻击时有2%概率给敌人添加1层剧毒效果
        /// </summary>
        /// <param name="args"></param>
        /// <param name="poisonTrigger"></param>
        private void CheckUnderMagic4AttackPoisonTrigger(DamageArgs args, SkillTrigger poisonTrigger)
        {
            poisonTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            if (args.weaponSkill.backData.WeaponLev != 4) return;
            CreatePoisonBuffAndAdd(args,poisonTrigger);
        }
        /// <summary>
        /// 检测怪物死亡后触发毒圈属性
        /// </summary>
        /// <param name="args"></param>
        private void CheckAtkTriggerPoisonCircleAttr(DamageArgs args)
        {
            var poisonCircleTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.KillMonster, AttributeType.PoisonCircleRange);
            if (poisonCircleTrigger == null) return;
            poisonCircleTrigger.trigger.TryGetValue(AttributeName.Prob, out var poisonCircleValue);
            if (Lodash.RandRangeFloat(0, 1f) > poisonCircleValue * GameConst.AttributeDivisor) return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var assetPath = $"Player/PlayerPoisonCircle";
            LoadAssetSync(assetPath);
            var tmpObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, fightManager.fightPanelTrans.transform);
            if(tmpObj == null) return;
            var poisonCircleRange = tmpObj.GetComponent<PlayerPoisonCircle>();
            if (poisonCircleRange == null) return;
            var circleRange = poisonCircleTrigger.attrMgr.Calc(AttributeType.PoisonCircleRange);
            var circleSecDmg = poisonCircleTrigger.attrMgr.Calc(AttributeType.PoisonCircleSecDmg);
            var circleTime = poisonCircleTrigger.attrMgr.Calc(AttributeType.PoisonCircleTime)*GameConst.TimeDivisor;
            var atk = args.skillData.owner.attr.Calc(AttributeType.Atk);
            var buff = new Buff
            {
                startTime = GameTime.Instance.GTime,duration = circleTime,
                interval = 1,value = atk*circleSecDmg
            };
            poisonCircleRange.InitWithTarget(args.skillData.owner,buff,circleRange,fightManager.entityPool);
            poisonCircleRange.transform.localPosition = fightManager.GetFightLocalPos(transform.position);
        }
        /// <summary>
        /// 检测对怪物造成伤害时有概率使怪物获得剧毒路径
        /// </summary>
        /// <param name="args"></param>
        private void CheckMagic910058SkillAttr(DamageArgs args)
        {
            var poisonPassTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.PoisonPassRange);
            if (poisonPassTrigger == null) return;
            poisonPassTrigger.trigger.TryGetValue(AttributeName.Prob, out var poisonPassPropValue);
            if (Lodash.RandRangeFloat(0, 1f) > poisonPassPropValue * GameConst.AttributeDivisor) return;
            var poisonPassBuffs = Data.buffMgr.FindBuffsById((int)AttributeType.PoisonPassRange);
            if (poisonPassBuffs is { Count: > 0 }) return;//不重复添加
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            //添加buff作标记
            var assetPath = $"Fighting/Buff/buff_poisonPath";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null) return;
            buffObj.transform.localPosition = GetVertigoPos();
            buffObj.transform.localScale = Vector3.one * GetVertigoScale();
            var buffComp = buffObj.GetComponent<BuffPoisonPath>();
            if(buffComp == null)return;
            var poisonPassTime = poisonPassTrigger.attrMgr.Calc(AttributeType.PoisonPassTime)*GameConst.TimeDivisor;
            poisonPassTime += args.skillData.owner.clothesSkillAttr.Calc(AttributeType.PoisonPassTime)*GameConst.TimeDivisor;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.PoisonPassRange,
                attrName = AttributeName.PoisonPassRange,
                startTime = GameTime.Instance.GTime,
                duration = poisonPassTime,interval = 0,
                value = 0,multi = true, clothesId = 1,
                valueType = BuffValueType.Negative,
            }, fightingManager.entityPool);
            buffComp.curDamageArgs = args.Clone();
            buffList.Add(buffComp);
            Data.buffMgr.AddBuff(new Buff
            {
                id = (int)AttributeType.PoisonPassSecDmg,
                attrName = AttributeName.PoisonPassSecDmg,
                startTime = GameTime.Instance.GTime,
                duration = poisonPassTime, interval = 1,
                value = 0,
                valueType = BuffValueType.Negative,
                multi = true, clothesId = 2,
            });
        }
        /// <summary>
        /// 创建并添加剧毒buff
        /// </summary>
        /// <param name="args"></param>
        /// <param name="poisonTrigger"></param>
        private void CreatePoisonBuffAndAdd(DamageArgs args, SkillTrigger poisonTrigger)
        {
            //检测当前怪物是否有剧毒buff
            var superpositionCount = CheckHavePoisonBuff();
            if (superpositionCount >= args.skillData.owner.clothesTriggerSkill.GetPoisonMaxSuperpose()) return;
            var poisonTime = poisonTrigger.attrMgr.Calc(AttributeType.PoisonTime)*GameConst.TimeDivisor;
            var poisonDmgProp = poisonTrigger.attrMgr.Calc(AttributeType.PoisonDmgSec);
            var poisonDmg = args.damagePoint* poisonDmgProp;
            AddPoisonBuff(new Buff
            {
                id = (int)AttributeType.Poison,
                attrName = AttributeName.Poison,
                startTime = GameTime.Instance.GTime,
                duration = poisonTime, interval = 1,value = poisonDmg,
                valueType = BuffValueType.Negative,multi = true,clothesId = 2,
            });
        }
        /// <summary>
        /// 检测当前怪物是否有中毒buff
        /// </summary>
        /// <returns></returns>
        private int CheckHavePoisonBuff()
        {
            var superpositionCount = 0;//叠加层数
            var poisonBuffs = Data.buffMgr.FindBuffsById((int)AttributeType.Poison);
            if (poisonBuffs == null || poisonBuffs.Count == 0) return 0;
            foreach (var buffItem in poisonBuffs)
            {
                if (buffItem.clothesId > 0) superpositionCount++;
            }
            return superpositionCount;
        }
        #endregion
        #region S套装技能相关
        /// <summary>
        /// 攻击有5%概率附带当前护盾值15%的伤害
        /// </summary>
        /// <param name="args"></param>
        private void CheckSClothes910083Trigger(DamageArgs args)
        {
            var shieldDmgTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.ShieldDmg);
            if (shieldDmgTrigger == null) return;
            var curHpShield = fightingBaseManager.playerCtrl.Data.resource.Armor;
            if (curHpShield <= 0) return;
            shieldDmgTrigger.trigger.TryGetValue(AttributeName.Prob, out var shieldPropValue);
            if (Lodash.RandRangeFloat(0, 1f) > shieldPropValue * GameConst.AttributeDivisor) return;
            DecHp(Lodash.RoundToInt(curHpShield*shieldDmgTrigger.attrMgr.Calc(AttributeType.ShieldDmg)));
        }
        private string tmpBulletPath = $"Fighting/Weapon/skill_clothes_bullet_910063";
        /// <summary>
        /// 处于背包的武器攻击敌人时有15%概率携带连锁闪电，对攻击目标及周围的3命敌人造成电击伤害。
        /// </summary>
        /// <param name="args"></param>
        private void CheckSClothes910063Trigger(DamageArgs args)
        {
            var lightningDmgTrigger = args.skillData.owner.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HitTarget, AttributeType.LightningDmg);
            if (lightningDmgTrigger == null) return;
            lightningDmgTrigger.trigger.TryGetValue(AttributeName.Prob, out var lightningPropValue);
            if (Lodash.RandRangeFloat(0, 1f) > lightningPropValue * GameConst.AttributeDivisor) return;
            DecHp(Lodash.RoundToInt(args.damagePoint*lightningDmgTrigger.attrMgr.Calc(AttributeType.LightningDmg)));//额外伤害
            var targetNum = lightningDmgTrigger.attrMgr.Calc(AttributeType.LightningSplitNum);//分裂个数
            var targetDmg = lightningDmgTrigger.attrMgr.Calc(AttributeType.LightningSplitDmg);//电击伤害
            //fightingBaseManager.entityPool.
                LoadAssetSync(tmpBulletPath);
            var targets = fightingBaseManager.GetNearestMonsters(transform.position,Lodash.RoundToInt(targetNum));
            if (targets.Count == 0) return;
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].CheckMonsterIsDead()) continue;
                var bleedBuffs = targets[i].Data.buffMgr.FindBuffsById((int)AttributeType.Palsy);
                if (bleedBuffs is { Count: > 0 }) continue;
                var bullet = //fightingBaseManager.entityPool.
                    InstantiateObj(tmpBulletPath, transform.position, Quaternion.identity, fightingBaseManager.fightPanelTrans);
                var bulletComp = bullet.GetComponent<PlayerClothesBullet910063>();
                var dmgArgs = new DamageArgs
                {
                    damagePoint = Lodash.RoundToInt(args.damagePoint*targetDmg),
                    effect = DamageArgs.SkillFactionEffect(SkillFaction.None),
                    sender = fightingBaseManager.playerCtrl,
                    skillData = args.skillData.owner.clothesTriggerSkill,
                    dmgType = DmgType.Normal
                };
                bulletComp.TargetMonster = targets[i];
                bulletComp.InitWithTarget(dmgArgs, GetConfigArgs, transform.position, targets[i].transform.position, fightingBaseManager.entityPool);
            }
        }
        public float GetConfigArgs(string key)
        {
            return 0;
        }
        /// <summary>
        /// 创建服饰麻痹buff特效
        /// </summary>
        /// <param name="buff"></param>
        public void AddClothesPalsyBuff(Buff buff)
        {
            if(CheckMonsterIsDead())return;
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_clothesPalsy";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = GetVertigoPos();
            buffObj.transform.localScale = Vector3.one * GetVertigoScale();
            var buffComp = buffObj.GetComponent<BuffClothesPalsy>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
            buffList.Add(buffComp);
        }
        #endregion
        /// <summary>
        /// 检查怪物是否死掉
        /// </summary>
        /// <returns></returns>
        public bool CheckMonsterIsDead()
        {
            return data?.resource == null || data.resource.IsDead();
        }
    }
}