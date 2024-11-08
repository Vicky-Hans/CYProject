using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.Config;
using DH.Data;
using Spine.Unity;
using UnityEngine;
namespace DH.Game
{
    public partial class CharacterController : BaseMonoUnit, IDamageable
    {
        public AttackBox attackBox;
        public Transform modelParent;
        private SkeletonAnimation playerAnim;
        public PlayerHpBar hpBar;
        public GameObject inRangeEffect;
        public GameObject playerHpShieldObj;
        public WeaponSkillController WeaponSkillController;
        [HideInInspector] public PlayerHpShield playerHpShield;
        private SpineAnimator animator;
        private SpineAnimator boxAnimator;
        private PlayerHpTrigger playerHpTrigger;
        public PlayerShockWaveTrigger PlayerShockWaveTrigger;
        private PlayerUnderAttackTrigger playerUnderAttackTrigger;
        private InRangeTrigger inRangeTrigger;
        public MoveComponent MoveComponent { get; set; }
        
        public async UniTask InitStart()
        {
            await Init(Player.CreateDefault(true));
        }
        public async UniTask Init(Player player)
        {
            attackBox.ignoreTargetProtect = true;
            data = player;
            await InitPlayerModel();
            CalRoleAttr();
            var entityPool = BattleManager.Instance.fightingManagerIns.entityPool;
            WeaponSkillController = new WeaponSkillController();
            WeaponSkillController.CheckedWaveStart = true;
            WeaponSkillController.Init(data, this, entityPool);
            playerHpTrigger = new PlayerHpTrigger();
            playerHpTrigger.Init(Player);
            playerUnderAttackTrigger = new PlayerUnderAttackTrigger();
            playerUnderAttackTrigger.Init(Player);
            inRangeTrigger = new InRangeTrigger();
            inRangeTrigger.Init(Player);
            PlayerShockWaveTrigger = new PlayerShockWaveTrigger();
            PlayerShockWaveTrigger.Init(Player);
            var tmpObj = Instantiate(playerHpShieldObj, Vector3.zero, Quaternion.identity, modelParent);
            if (tmpObj != null)
            {
                tmpObj.transform.localPosition = new Vector3(0.2f, -0.7f, 0f);
                playerHpShield = tmpObj.GetComponent<PlayerHpShield>();
            }
            if (playerHpShield != null) playerHpShield.Init(this);
            attackBox.damageArgs.sender = this;
            attackBox.damageArgs.damagePoint = 0;
            GameDataManager.Instance.PhyEquipMergedGainValue = Player.GetPhyEquipMergedGain();
            GameDataManager.Instance.MagicEquipMergedGainValue = Player.GetMagicEquipMergedGain();
        }

        private async UniTask InitPlayerModel()
        {
            var heroId = GameDataManager.Instance.HeroId;
            if (heroId <= 0) heroId = 1000;
            var heroModelPath = $"Player/heroCard_{heroId}";
            var heroTrans = await AssetsManager.InstantiateWithParentAsync(heroModelPath, modelParent, false, cts.Token);
            playerAnim = heroTrans.GetComponent<SkeletonAnimation>();
            animator = playerAnim.GetComponent<SpineAnimator>();
            animator.CharacterController = this;
            boxAnimator = transform.Find("ModelParent/heroModel_box")
                .GetComponent<SpineAnimator>();
            animator.PlayAnimation(GameConst.AnimationName.Idle);
            boxAnimator.PlayAnimation(GameConst.AnimationName.Idle);
            var characterTrans = playerAnim.transform;
            characterTrans.localScale = Vector3.one * 0.5f;
            boxAnimator.transform.localScale = characterTrans.localScale;
            hpBar.PlayerMono = this;
            if (BattleManager.Instance.IsStageForest())
            {
                boxAnimator.gameObject.SetActive(false);
                MoveComponent = GetComponent<MoveComponent>();
                var mr = heroTrans.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.sortingOrder = 10;
                }
                hpBar.gameObject.SetActive(true);
            }
            else
            {
                hpBar.gameObject.SetActive(false);
                CheckRoleSkin();
            }
        }

        private void CheckRoleSkin()
        {
            var wearBox = GameDataManager.Instance.CurFightData.Attr.WearBox;
            var skeletonAnimation = boxAnimator.Animator;
            var skinName = "heroEquipAct_01";
            if (wearBox > 0)
            {
                var herCfg = ConfigCenter.HeroEquipResourceCfgColl.GetDataById(wearBox);
                if (herCfg != null)
                {
                    skinName = herCfg.Act;
                }
            }
            var skeleton = skeletonAnimation.Skeleton;
            var tmpSkin = skeleton.Data.FindSkin(skinName);
            if (tmpSkin != null)
            {
                skeleton.SetSkin(tmpSkin);
                skeleton.SetSlotsToSetupPose();
                skeletonAnimation.AnimationState.SetEmptyAnimation(0, 0);
                boxAnimator.PlayAnimation(GameConst.AnimationName.Idle);
            }
        }

        public async UniTask PlayReviveEffect()
        {
            const string effectPath = "Effects/heroEquipSkill_skill_relife";
            var effectObj = await AssetsManager.InstantiateAsync(effectPath);
            if (effectObj != null)
            {
                effectObj.transform.parent = modelParent;
                effectObj.transform.localPosition = Vector3.zero;
                BattleManager.Instance.fightingManagerIns.AddAutoReleaseGObj(effectObj, 4f);
            }
        }

        public void PlayAtk()
        {
            var atkName = GameConst.AnimationName.Atk;
            if (BattleManager.Instance.IsStageForest())
            {
                atkName = GameConst.AnimationName.WalkAtk;
            }
            animator.PlayAnimation(atkName);
            boxAnimator.PlayAnimation(GameConst.AnimationName.Atk);
        }

        public void OnWeaponChanged()
        {
            var weaponList = GameDataManager.Instance.BackpackWeaponList.ToList();
            WeaponSkillController?.OnWeaponChanged(weaponList);
        }

        private bool CanMiss()
        {
            var missRate = playerUnderAttackTrigger.MissRate;
            var missRateUp = Data.clothesSkillAttr.Calc(AttributeType.MissPro);
            missRate += missRateUp;//S级套装6闪避+10%
            missRate += Data.attr.Calc(AttributeType.Miss);//英雄闪避属性提升
            return Lodash.RandRangeFloat(0, 1) < missRate;
        }
        private bool CheckDownHit(BaseMonoUnit monoUnit)
        {
            if(monoUnit.Data == null)return false;
            var downHit = monoUnit.Data.buffMgr.GetBuffsMaxValue((int)AttributeType.DownHit);
            return Lodash.RandRangeFloat(0, 1f) < downHit;
        }

        public DamageStatus OnDamage(DamageArgs args)
        {
            // 怪物攻击是否落空
            if (CheckDownHit(args.sender))
            {
                return DamageStatus.Normal;
            }
            if (CanMiss())
            {
                CheckMissUpAtkAttr();//检测闪避触发攻击力提升
                BattleManager.Instance.fightingManagerIns.ShowHurtMiss(transform.position);
                playerHpShield.TriggerMiss();
                return DamageStatus.Normal;
            }
            if (Data.ClothesResource.ImmuneEveryCount > 0)//免疫伤害
            {
                BattleManager.Instance.fightingManagerIns.ShowHurtMiss(transform.position);
                Data.ClothesResource.ImmuneEveryCount -= 1;
                playerHpShield.TriggerImmune();
                return DamageStatus.Normal;
            }
            if (Data.buffMgr.FindBuffById((int)AttributeType.HeroHpValueLess) != null)//无敌模式中
            {
                return DamageStatus.Normal;
            }
            //密林模式下的防御药水buff：拾取后1分钟内免疫所有伤害
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret && 
                Data.buffMgr.FindBuffById((int)AttributeType.ImmuneDmg) != null)
            {
                BattleManager.Instance.fightingManagerIns.ShowHurtImmune(transform.position);
                return DamageStatus.Normal;
            }
            args.damagePoint = (int)CalcDmg(args);
            if (playerHpShield.HpShield > 0)
            {
                playerHpShield.DecHpShield((int)args.damagePoint);
            }
            else
            {
                bool isOwnArmor = Data.resource.Armor > 0;
                Data.resource.DecArmor(args.damagePoint);
                if (isOwnArmor && Data.resource.Armor <= 0)
                {
                    playerHpShield.TriggerShieldAttack();
                }
            }
            if (!data.IsDead())
            {
                playerHpTrigger?.Trigger(data.resource.Progress);
                playerHpTrigger?.TriggerInvincible(data.resource.Progress);//触发无敌模式
                PlayerShockWaveTrigger?.Trigger();
                playerUnderAttackTrigger.TriggerReHit();
                CheckBeingAttackedTriggersBleeding(args);
                CheckPoisonAttackedTriggersHpRevert(args);
            }
            BattleManager.Instance.fightingManagerIns.ShowPlayerHurtNum(args, transform.position);
            return DamageStatus.Normal;
        }

        public void DecArmor(long value)
        {
            Data.resource.DecArmor(value);
            if (!data.IsDead())
            {
                playerHpTrigger?.Trigger(data.resource.Progress);
                PlayerShockWaveTrigger?.Trigger();
            }
            BattleManager.Instance.fightingManagerIns.ShowPlayerHurtNumOnly(value, transform.position);
        }
        
        public void OnUpdate(float deltaTime)
        {
            if(data == null)return;
            Data.buffMgr.CheckValid(GameTime.Instance.GTime);
            if (WeaponSkillController is { CheckedWaveStart: false }) CheckPoisonCircleAttr();//战斗开始后检测是否有毒圈生成属性
            WeaponSkillController?.OnUpdate(deltaTime);
            inRangeTrigger?.OnUpdate(deltaTime);
            hpBar.OnUpdate(deltaTime);
        }
        
        public void AddFiringBuff(Buff buff)
        {
            FightingSoundHelper.Instance.PlayBuffBurn();
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_firing";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one * 1f;
            var buffComp = buffObj.GetComponent<BuffFiring>();
            if(buffComp == null)return;
            buffComp.IsPlayer = true;
            buffComp.InitWithTarget(this, buff, entityPool);
        }
        /// <summary>
        /// 添加无敌buff
        /// </summary>
        /// <param name="buff"></param>
        public void AddInvincibleBuff(Buff buff)
        {
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_invincible";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffInvincible>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
        }
        /// <summary>
        /// 添加无敌攻击回血属性
        /// </summary>
        /// <param name="buff"></param>
        public void AddAtkReplayBuff(Buff buff)
        {
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_atkReplay";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffAtkReplay>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, buff, entityPool);
        }
        /// <summary>
        /// 暴击药水：拾取后1分钟内所有攻击必定暴击
        /// </summary>
        /// <param name="paramValue"></param>
        public void AddCriticalStrikePotionBuff(List<int> paramValue)
        {
            if (paramValue == null || paramValue.Count < 2) return;
            var curBuff = Data.buffMgr.FindBuffById((int)AttributeType.EnableCircle);
            if (curBuff != null)
            {
                curBuff.startTime = GameTime.Instance.GTime;
                return;
            }
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_fireRed";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffFireRed>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.EnableCircle,
                attrName = AttributeName.EnableCircle,
                startTime = GameTime.Instance.GTime,
                duration = paramValue[0], multi = false,
                valueType = BuffValueType.Positive,
                value = paramValue[1]*GameConst.AttributeDivisor
            }, entityPool);
        }
        /// <summary>
        /// 攻速药水：拾取后1分钟内所有装备攻速+50%
        /// </summary>
        /// <param name="paramValue"></param>
        public void AddAttackSpeedPotionBuff(List<int> paramValue)
        {
            if (paramValue == null || paramValue.Count < 2) return;
            var curBuff = Data.buffMgr.FindBuffById((int)AttributeType.UpAtkSpd);
            if (curBuff != null)
            {
                curBuff.startTime = GameTime.Instance.GTime;
                return;
            }
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_fireYellow";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffFireYellow>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.UpAtkSpd,
                attrName = AttributeName.UpAtkSpd,
                startTime = GameTime.Instance.GTime,
                duration = paramValue[0], multi = false,
                valueType = BuffValueType.Positive,
                value = paramValue[1]*GameConst.AttributeDivisor
            }, entityPool);
        }
        /// <summary>
        /// 防御药水：拾取后1分钟内免疫所有伤害
        /// </summary>
        /// <param name="paramValue"></param>
        public void AddDefensePotionBuff(List<int> paramValue)
        {
            if (paramValue == null || paramValue.Count < 2) return;
            var curBuff = Data.buffMgr.FindBuffById((int)AttributeType.ImmuneDmg);
            if (curBuff != null)
            {
                curBuff.startTime = GameTime.Instance.GTime;
                return;
            }
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_fireShield";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffFireShield>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.ImmuneDmg,
                attrName = AttributeName.ImmuneDmg,
                startTime = GameTime.Instance.GTime,
                duration = paramValue[0], multi = false,
                valueType = BuffValueType.Positive,
                value = paramValue[1]*GameConst.AttributeDivisor
            }, entityPool);
        }
        /// <summary>
        /// 疾跑药水：拾取后1分钟内移动速度+100%
        /// </summary>
        /// <param name="paramValue"></param>
        public void AddRunFastPotionBuff(List<int> paramValue)
        {
            if (paramValue == null || paramValue.Count < 2) return;
            var curBuff = Data.buffMgr.FindBuffById((int)AttributeType.RoleSpdUp);
            if (curBuff != null)
            {
                curBuff.startTime = GameTime.Instance.GTime;
                return;
            }
            var fightManager = BattleManager.Instance.fightingManagerIns;
            var entityPool = fightManager.entityPool;
            var assetPath = $"Fighting/Buff/buff_fireBlue";
            LoadAssetSync(assetPath);
            var buffObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, transform);
            if(buffObj == null)return;
            buffObj.transform.localPosition = Vector3.up * 0.6f;
            buffObj.transform.localScale = Vector3.one;
            var buffComp = buffObj.GetComponent<BuffFireBlue>();
            if(buffComp == null)return;
            buffComp.InitWithTarget(this, new Buff
            {
                id = (int)AttributeType.RoleSpdUp,
                attrName = AttributeName.RoleSpdUp,
                startTime = GameTime.Instance.GTime,
                duration = paramValue[0], multi = false,
                valueType = BuffValueType.Positive,
                value = paramValue[1]*GameConst.AttributeDivisor
            }, entityPool);
        }
        private void OnDestroy()
        {
            WeaponSkillController?.Release();
        }
        public float CalcDmg(DamageArgs args)
        {
            var mAtk = args.damagePoint;
            var monoUnit = args.sender;
            if (monoUnit != null)
            {
                var monsterAtkDown = 0f;
                var breakArmorPro = 0f;
                monsterAtkDown = monoUnit.Data.buffMgr.GetBuffsValue((int)AttributeType.MonsterAtkDown);
                var monsterAtkBonusCount = monoUnit.MonsterData.AtkBonusCount;
                var monsterAtkBonus = monoUnit.Data.attr.Calc(AttributeType.AttackBonus);
                if (monsterAtkBonusCount > 0)
                {
                    monsterAtkBonus = monoUnit.MonsterData.AtkBonus * monsterAtkBonusCount;
                }
                if (monoUnit.MonsterData.BreakArmorCount > 0)
                {
                    breakArmorPro = monoUnit.MonsterData.BreakArmorDmg;
                    if (Data.resource.Armor <  mAtk * (1 - monsterAtkDown + monsterAtkBonus+breakArmorPro)) breakArmorPro = 0;//甲值小于当前攻击力，则叠甲不生效
                }
                var equipDownDmg = Player.ClothesResource.ReviveDownDmg;
                mAtk = Lodash.RoundToInt(mAtk * (1 - monsterAtkDown - equipDownDmg + monsterAtkBonus + breakArmorPro));
            }
            var dmg = mAtk;
            var downDmg = GetDownDmg(monoUnit);
            dmg = (long)(mAtk*(1 - downDmg));
            
            return dmg > 0 ? dmg : 0f;
        }
        /// <summary>
        /// 伤害降低
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private float GetDownDmg(BaseMonoUnit unit)
        {
            var downDmg = 0.0f;
            if (!unit.Data.IsMonster) return downDmg;
            var monsterCfg =
                ConfigCenter.MonsterModelCfgColl.GetDataById(((Monster)unit.Data).cfg.ModelId);
            if (monsterCfg == null) return downDmg;
            var weapons = WeaponSkillController.weapons;
            foreach (var weapon in weapons)
            {
                if (weapon.weaponData.EquipId != 19) continue;
                var skillData = weapon.weaponData.SkillData;
                var trigger1 = skillData.GetTriggerByNameAndComplete(AttributeName.CombatUnderAtk,
                    AttributeType.DownDmg);
                var trigger2 =
                    Data.triggerSkill.GetTriggerByNameAndComplete(AttributeName.LongUnderAtk,
                        AttributeType.DownDmg);
                if (monsterCfg.Type == 1 && trigger1 != null) //近战
                {
                    trigger1.trigger.TryGetValue("prob", out var probValue);
                    var prob = probValue * GameConst.AttributeDivisor;
                    if (Lodash.RandRangeFloat(0, 1) > prob) return downDmg;
                    downDmg = trigger1.attrMgr.Calc(AttributeType.DownDmg);
                    break;
                }

                if (monsterCfg.Type == 2 && trigger2 != null) //近战
                {
                    trigger2.trigger.TryGetValue("prob", out var probValue);
                    var prob = probValue * GameConst.AttributeDivisor;
                    if (Lodash.RandRangeFloat(0, 1) > prob) return downDmg;
                    downDmg = trigger2.attrMgr.Calc(AttributeType.DownDmg);
                    break;
                }
            }

            return downDmg;
        }
        
        /// <summary>
        /// 检测受到攻击后触发Bleedbuff效果;受到伤害时，有50%的概率使伤害来源目标获得1层流血
        /// </summary>
        /// <param name="args"></param>
        private void CheckBeingAttackedTriggersBleeding(DamageArgs args)
        {
            var bleedTrigger = Data.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderAttack, AttributeType.Bleed);
            if (bleedTrigger == null) return;
            bleedTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            //检测当前怪物是否有流血buff
            var superpositionCount = 0;
            var bleedBuffs = args.sender.Data.buffMgr.FindBuffsById((int)AttributeType.Bleed);
            if (bleedBuffs is { Count: > 0 })
            {
                foreach (var buffItem in bleedBuffs)
                {
                    if (buffItem.clothesId > 0) superpositionCount++;
                }
            }
            if (superpositionCount >= Data.clothesTriggerSkill.GetBleedMaxSuperpose()) return;
            var bleedTime = bleedTrigger.attrMgr.Calc(AttributeType.BleedTime)*GameConst.TimeDivisor;
            var bleedDmg = args.damagePoint*bleedTrigger.attrMgr.Calc(AttributeType.BleedDmg);
            if (Data.ClothesResource.BleedTimeSuperposeCount > 0)
            {
                bleedTime += Data.ClothesResource.BleedTimeSuperposeCount*Data.ClothesResource.BleedSuperposeTime;
                if (bleedTime > 1) bleedTime = 1f;
            }
            var buff = new Buff
            {
                id = (int)AttributeType.Bleed,
                attrName = AttributeName.Bleed,
                startTime = GameTime.Instance.GTime,
                duration = bleedTime,
                interval = 1,
                value = bleedDmg,
                valueType = BuffValueType.Negative,
                multi = true,
                clothesId = 1,
            };
            if (args.sender is MonsterController controller) controller.AddBleedBuff(buff);
        }
        /// <summary>
        /// 检测受到持有剧毒效果怪物的伤害时，有25%概率恢复受到伤害30%的生命
        /// </summary>
        /// <param name="args"></param>
        private void CheckPoisonAttackedTriggersHpRevert(DamageArgs args)
        {
            var poisonAttackedTrigger = Data.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.HeroUnderPoisonMonsterAttack, AttributeType.DmgRevert);
            if (poisonAttackedTrigger == null) return;
            poisonAttackedTrigger.trigger.TryGetValue(AttributeName.Prob, out var probValue);
            var prob = probValue * GameConst.AttributeDivisor;
            if(Lodash.RandRangeFloat(0, 1f) > prob) return;
            //检测当前怪物是否有剧毒buff
            var poisonBuffs = args.sender.Data.buffMgr.FindBuffsById((int)AttributeType.Poison);
            if (poisonBuffs == null || poisonBuffs.Count == 0) return;
            var hpRevert = poisonAttackedTrigger.attrMgr.Calc(AttributeType.DmgRevert);
            var hpValue = Lodash.RoundToInt(args.damagePoint*hpRevert);
            Data.resource.AddHp(hpValue);
        }
        /// <summary>
        /// 随机在地图上生成2个剧毒圈，对经过的怪物造成伤害并附带1层剧毒效果
        /// </summary>
        public void CheckPoisonCircleAttr()
        {
            //.FirstKill = true;
            var roundStartTrigger = Data.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.RoundStart, AttributeType.PoisonCircleNum);
            if (roundStartTrigger == null) return;
            var circleNum = roundStartTrigger.attrMgr.Calc(AttributeType.PoisonCircleNum);
            var circleRange = roundStartTrigger.attrMgr.Calc(AttributeType.PoisonCircleRange);
            var circleSecDmg = roundStartTrigger.attrMgr.Calc(AttributeType.PoisonCircleSecDmg);
            var circleTime = roundStartTrigger.attrMgr.Calc(AttributeType.PoisonCircleTime)*GameConst.TimeDivisor;
            circleNum += Data.clothesSkillAttr.Calc(AttributeType.PoisonCircleNum);
            circleRange += Data.clothesSkillAttr.Calc(AttributeType.PoisonCircleRange);
            circleTime += Data.clothesSkillAttr.Calc(AttributeType.PoisonCircleTime)*GameConst.TimeDivisor;
            
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var assetPath = $"Player/PlayerPoisonCircle";
            LoadAssetSync(assetPath);
            var atk = Data.attr.Calc(AttributeType.Atk);
            for (var i = 0; i < circleNum; i++)//创建毒圈
            {
                var tmpObj = InstantiateObj(assetPath, Vector3.zero, Quaternion.identity, fightingManager.fightPanelTrans.transform);
                if(tmpObj == null) continue;
                var poisonCircleRange = tmpObj.GetComponent<PlayerPoisonCircle>();
                if (poisonCircleRange == null) continue;
                var buff = new Buff
                {
                    startTime = GameTime.Instance.GTime,
                    duration = circleTime, value = atk*circleSecDmg, interval = 1
                };
                poisonCircleRange.InitWithTarget(Data,buff,circleRange,fightingManager.entityPool);
                poisonCircleRange.transform.localPosition = new Vector3(Lodash.RandRangeFloat(-3f,4.2f),Lodash.RandRangeFloat(-1,1),0);
            }
        }
        /// <summary>
        /// 检测闪避触发攻击力提升属性
        /// </summary>
        public void CheckMissUpAtkAttr()
        {
            var missingTrigger = Data.clothesTriggerSkill.GetTriggerByNameAndComplete(AttributeName.Missing, AttributeType.AttackBonus);
            if (missingTrigger == null) return;
            var attackBonus = missingTrigger.attrMgr.Calc(AttributeType.AttackBonus);
            var atkBonusMax = missingTrigger.attrMgr.Calc(AttributeType.AtkBonusMax);
            if (Data.ClothesResource.MissAttackBonus + attackBonus < atkBonusMax)
            {
                Data.ClothesResource.MissAttackBonus += attackBonus;
            }
            else
            {
                Data.ClothesResource.MissAttackBonus = atkBonusMax;
            }
        }
    }
}