using DH.Data;
using UnityEngine;

namespace DH.Game
{
    public partial class LittleEnemySkill : MonoBehaviour
    {
        private float AttackCd = 1f;
        private float time;
        private readonly Collider2D[] cacheCollider = new Collider2D[1];
        private CircleCollider2D collider2D;
        private MonsterController monsterController;
        private bool isInit = false;

        public void Init()
        {
            collider2D = GetComponent<CircleCollider2D>();
            monsterController = GetComponent<MonsterController>();
            if(monsterController == null)return;
            // var tmpCd = monsterController.Data.attr.Calc(AttributeType.Cd) * GameConst.TimeDivisor;
            // AttackCd = tmpCd > 0 ? tmpCd : AttackCd;
            UpdateAtkCd();
            isInit = true;
        }

        private void UpdateAtkCd()
        {
            var tmpCd = monsterController.Data.attr.Calc(AttributeType.Cd) * GameConst.TimeDivisor;
            var downAtkSpd = monsterController.Data.buffMgr.GetBuffsMaxValue((int)AttributeType.DownAtkSpd);
            var upAtkSpd = monsterController.Data.attr.Calc(AttributeType.UpAtkSpd);
            tmpCd /= (1 - downAtkSpd + upAtkSpd);
            
            var downCdCount = monsterController.MonsterData.DownCdCount;
            if (downCdCount > 0)
            {
                var downCd = monsterController.MonsterData.DownCd;
                tmpCd *= (1 - downCd);
            }
            AttackCd = tmpCd > 0 ? tmpCd : 0f;
        }

        public  void OnUpdate(float deltaTime)
        {
            if (!isInit)return;
            
            if(monsterController == null)return;
            if(monsterController.Data.IsDead())return;
            if(monsterController.Data.IsStucked())return;
            if(monsterController.SkillTaking)return; //技能行动中不计 cd
            time += deltaTime;
            if (BattleManager.Instance.IsStageForest() && monsterController.MonsterData.IsBoss())
            {// 密林 boss 不需要停止
            }
            else if(!monsterController.IsAtTargetPos())return;
            UpdateAtkCd();
            if (time > AttackCd)
            {
                time = 0;
                DoAttack();
            }
        }

        private async void DoAttack()
        {
            var atkId = GetAtkId();
            if (atkId > 0)
            {
                DoSkillAttack(atkId);
                return;
            }
            monsterController.PlayAnimation(GameConst.AnimationName.Atk);
            var pos = transform.position;
            var radius = collider2D.radius * 2f;
            var layerMask = LayerMask.GetMask("Player");
            var count = Physics2D.OverlapCircleNonAlloc(pos, radius, cacheCollider, layerMask);
            if (count > 0)
            {
                FightingSoundHelper.Instance.PlayMonsterNearSkillSnd();
                var dmgArgs = monsterController.attackBox.damageArgs.Clone();
                BattleManager.Instance.fightingManagerIns.playerCtrl.OnDamage(dmgArgs);
            }
        }

        private void DoSkillAttack(int atkId)
        {
            if(monsterController == null)return;
            if (monsterController.SkillController.SkillsMap.TryGetValue(atkId, out var skill))
            {
                var actName = skill.SkillData.Cfg.SkillAct;
                if (!string.IsNullOrEmpty(actName))
                {
                    monsterController.PlayAnimation(actName);
                }
                FightingSoundHelper.Instance.PlayMonsterFarSkillSnd();
                skill.TakeSkill();
            }
        }

        public int GetAtkId()
        {
            if(monsterController == null)return 0;
            var skills = monsterController.MonsterData.cfg.AtkId;
            if(skills == null || skills.Count <= 0)return 0;
            if (BattleManager.Instance.IsStageForest())
            {
                return skills[Lodash.RandRange(0, skills.Count)].Skill;
            }
            return skills[0].Skill;
        }
        
    }
}