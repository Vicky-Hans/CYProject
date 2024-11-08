using DH.Config;
using DH.Data;
using Pathfinding.DHRVO;
using UnityEngine;

namespace DH.Game
{
    public partial class MonsterController
    {
        private Agent rvoAgent;
        private Vector3 moveDirection;
        private Vector2 interpolatedVelocity;
        private Vector2 interpolatedRotation;
        private Transform selfTransform;
        private float verticalDirection = 1f;
        private float littleSpeedFactor = 0f;
        private float bossSpeedFactor = 0f;

        public Agent RvoAgent => rvoAgent;

        public Vector3 GetMoveSpeed()
        {
            var monsterPos = trans.position;
            var oSpeed = data.attr.Calc(AttributeType.Spd);
            var downSpd = data.buffMgr.GetBuffsMaxValue((int)AttributeType.Decelerate);
            var upSpd = data.buffMgr.GetBuffsMaxValue((int)AttributeType.UpSpd);
            upSpd += data.attr.Calc(AttributeType.UpSpd);
            downSpd += fightingBaseManager.playerCtrl.Data.attr.Calc(AttributeType.DownSpd); // 天赋
            downSpd += data.attr.Calc(AttributeType.DownSpd);
            downSpd = downSpd > 1f ? 1f : downSpd;
            var targetSpd = (1 - downSpd + upSpd);
            if(targetSpd < 0.1f)targetSpd = 0.1f;
            oSpeed *= targetSpd;
            var spdFactor = data.SpdFactor;
            // 到了汇聚线
            if (monsterPos.x - target.x <= fightingBaseManager.MonsterTargetDis)
            {
                return (target - monsterPos).normalized * oSpeed * spdFactor;
            }
            var oYSpeed = data.attr.Calc(AttributeType.YSpd);
            oYSpeed = oYSpeed > 0f && verticalDirection > 0f ? oYSpeed : -oYSpeed;
            var speed = new Vector3(-oSpeed, oYSpeed) * spdFactor;
            return speed;
        }

        private float GetSecretSpeedFactor()
        {
            if (!BattleManager.Instance.IsStageForest())
            {
                return 1f;
            }
            return MonsterData.cfg.MonsterType == (int)MonsterType.Boss ? GetBossSpeedFactor() : GetLittleSpeedFactor();
        }
        
        private float GetLittleSpeedFactor()
        {
            if(littleSpeedFactor > 0.01f)
                return littleSpeedFactor;
            var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_16);
            littleSpeedFactor = defineCfg.Content[0] * GameConst.AttributeDivisor;
            return littleSpeedFactor;
        }

        private float GetBossSpeedFactor()
        {
            if(bossSpeedFactor > 0.01f)
                return bossSpeedFactor;
            var defineCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_17);
            bossSpeedFactor = defineCfg.Content[0] * GameConst.AttributeDivisor;
            return bossSpeedFactor;
        }

        public Vector3 GetForestMoveSpeed()
        {
            var oSpeed = data.attr.Calc(AttributeType.Spd);
            oSpeed *= GetSecretSpeedFactor();
            var downSpd = data.buffMgr.GetBuffsMaxValue((int)AttributeType.Decelerate);
            var upSpd = data.buffMgr.GetBuffsMaxValue((int)AttributeType.UpSpd);
            upSpd += data.attr.Calc(AttributeType.UpSpd);
            downSpd += fightingBaseManager.playerCtrl.Data.attr.Calc(AttributeType.DownSpd); // 天赋
            downSpd += data.attr.Calc(AttributeType.DownSpd);
            downSpd = downSpd > 1f ? 1f : downSpd;
            oSpeed *= (1-downSpd + upSpd);
            var spdFactor = data.SpdFactor;
            var monsterPos = trans.position;
            return (TargetTrans.position - monsterPos).normalized * oSpeed * spdFactor;
        }

        public bool IsFarSkillType()
        {
            return MonsterData.cfg.SkillType == 1;
        }

        public bool IsAtTargetPos()
        {
            if (BattleManager.Instance.IsStageForest())
            {
                return ((Vector2)(transform.position - TargetTrans.position)).magnitude <
                       MonsterData.modelCfg.AtkDistance * GameConst.AttributeDivisor;
            }
            if(IsFarSkillType())
                return IsAtTargetPosX();
            var pos = trans.position;
            return (pos - target).magnitude < 0.2f;
        }

        public bool IsAtTargetPosX()
        {
            var pos = trans.position;
            var offset = pos.x - target.x;
            return offset is < 0.15f and > -0.15f;
        }
        
        private void FixedUpdate()
        {
            if(!BattleManager.Instance.IsStageForest())return;
            if (Rgb2d != null)
            {
                Rgb2d.velocity = Vector2.zero;
            }
        }

        public virtual void Move(float deltaTime)
        {
            if (data == null) return;
            if(data.resource.IsDead())return;
            if(SkillTaking)return;
            var pos = trans.position;
            // 牵引
            if (PullTime > 0)
            {
                PullTime -= deltaTime;
                var pullDirection = PullPos - pos;
                var tmpSpd = pullDirection.normalized * PullSpd;
                pos += tmpSpd * deltaTime;
                trans.position = pos;
                return;
            }
            if (IsStucked()) return;
            if(IsAtTargetPos())return;
            var speed = BattleManager.Instance.IsStageForest()? GetForestMoveSpeed() : GetMoveSpeed();
            pos += speed * deltaTime;
            trans.position = pos;
            CheckVerticalDirection();
            CheckClear(pos);
        }

        private void CheckVerticalDirection()
        {
            var dataPos = Data.position;
            if (dataPos.y >= fightingBaseManager.FightPanelSize.y / 2f)
            {
                verticalDirection = -1f;
            }
            else if (dataPos.y <= -fightingBaseManager.FightPanelSize.y / 2f)
            {
                verticalDirection = 1f;
            }
        }

        private void CheckClear(Vector3 pos)
        {
            if(!BattleManager.Instance.IsStageForest())return;
            if(MonsterData.cfg.MonsterType == (int)MonsterType.Boss)return;
            var distance = BattleManager.Instance.MapFightingManager.ClearMonsterDistance;
            var playerPos = BattleManager.Instance.fightingManagerIns.playerCtrl.transform.position;
            if ((playerPos - pos).magnitude < distance)return;
            BattleManager.Instance.fightingManagerIns.DestroyMonster(this, true);
        }
    }
}