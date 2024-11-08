using System;
using Cysharp.Threading.Tasks;
using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class BossSkill51001Bullet : BaseBullet
    {
        [AssetPath] public string boomPath;
        public AttackBox attackBox;
        private Skill skillData;
        public MonsterController MonsterController { get; set; }
        private int dashNum;
        private float dashSpeed;
        private float dashDistance;
        private int dashCount;
        private float dashTime;
        private float dashTimer;
        private bool dashing;
        // public BossSkill51001 SkillIns { get; set; }
        private Vector3 vSpd;
        private MapFightingManager mapFightingManager;
        private Transform monsterTransform;
        
        public override void InitWithTarget(DamageArgs damageArgs, Func<string, float> paramGetter,
            Vector3 startPosition, Vector3 targetPosition, IPool<GameObject> pool)
        {
            base.InitWithTarget(damageArgs, paramGetter, startPosition, targetPosition, pool);
            mapFightingManager = BattleManager.Instance.MapFightingManager;
            monsterTransform = MonsterController.transform;
            skillData = damageArgs.skillData;
            // cacheTransform.localScale = Vector3.one * bulletSize;
            attackBox.ResetData();
            attackBox.damageArgs = damageArgs;
            attackBox.Damage += AttackBoxOnDamage;
            StartDash();
        }

        private void AttackBoxOnDamage(DamageArgs arg1, IDamageable arg2, DamageStatus arg3)
        {
            PlayBoom();
        }
        
        private void PlayBoom()
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var pos = transform.position;
            var obj = InstantiateObj(boomPath, pos, Quaternion.identity, fightingManager.fightPanelTrans);
            if (!obj)
            {
                return;
            }
            fightingManager.AddAutoReleaseUnit(obj, 2, this);
        }

        private void StartDash()
        {
            dashNum = Lodash.RoundToInt(skillData.attrMgr.Calc(AttributeType.DashNum));
            dashSpeed = skillData.attrMgr.Calc(AttributeType.DashSpeed);
            dashDistance = skillData.attrMgr.Calc(AttributeType.DashDistance);
            dashTime = dashDistance / dashSpeed;
            dashCount = 0;
            dashing = false;
            Dash();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            if(Recycled)return;
            if (MonsterController == null || MonsterController.CheckMonsterIsDead())
            {
                Recycle();
                return;
            }
            transform.position = monsterTransform.position;
            if(!dashing)return;
            // MonsterController.Rgb2d.velocity = vSpd;
            var position = monsterTransform.position;
            position += vSpd * deltaTime;
            position = ClampPosition(position);
            monsterTransform.position = position;
            dashTimer += deltaTime;
            if (dashTimer >= dashTime)
            {
                StopDash().Forget();
            }
        }

        private Vector3 ClampPosition(Vector3 pos)
        {
            var xMin = mapFightingManager.BossBornPos.x - mapFightingManager.BossAreaSize.x / 2f;
            var xMax = mapFightingManager.BossBornPos.x + mapFightingManager.BossAreaSize.x / 2f;
            var yMin = mapFightingManager.BossBornPos.y - mapFightingManager.BossAreaSize.y / 2f;
            var yMax = mapFightingManager.BossBornPos.y + mapFightingManager.BossAreaSize.y / 2f;
            if (pos.x < xMin)
            {
                pos.x = xMin;
            }

            if (pos.x > xMax)
            {
                pos.x = xMax;
            }

            if (pos.y < yMin)
            { 
                pos.y = yMin;              
            }

            if (pos.y > yMax)
            {
                pos.y = yMax;
            }
            return pos;
        }
        
        private void Dash()
        {
            if(MonsterController == null || MonsterController.CheckMonsterIsDead())
            {
                Recycle();
                return;
            }
            // play prepare animation
            MonsterController.SpineAnimator.PlaySpecAnimation("skill1_1");
            MonsterController.SpineAnimator.AddSpecAnimation("skill1_2", false);
            attackBox.EnableAttack();
            MonsterController.SkillTaking = true;
            dashing = true;
            // play dash animation
            var target = GetTarget();
            var direction = (target.transform.position - MonsterController.transform.position).normalized;
            vSpd = direction * dashSpeed;
            // play end animation
        }

        private async UniTaskVoid StopDash()
        {
            dashing = false;
            vSpd = Vector3.zero;
            // MonsterController.Rgb2d.velocity = vSpd;
            dashTimer = 0f;
            if (MonsterController == null || MonsterController.CheckMonsterIsDead())
            {
                Recycle();
                return;
            }
            MonsterController.SpineAnimator.PlaySpecAnimation("skill1_3");
            MonsterController.SpineAnimator.AddSpecAnimation(GameConst.AnimationName.Walk, true);
            MonsterController.SkillTaking = false;
            attackBox.DisableAttack();
            dashCount++;
            if (dashCount >= dashNum)
            {
                Recycle();
                return;
            }
            if (!await PauseTask.Delay(0.2f))
            {
                return;
            }
            Dash();
        }
        
        private CharacterController GetTarget()
        {
            var fightingManager = BattleManager.Instance.fightingManagerIns;
            var target = fightingManager.playerCtrl;
            return target;
        }
    }
}