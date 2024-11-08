using DH.Config;
using DH.Data;
using DHFramework;
using UnityEngine;

namespace DH.Game
{
    public partial class CharacterController
    {
        public bool IsMoving { get; set; }
        private float roleSpd = 2;

        public float RoleSpd
        {
            get
            {
                if(!BattleManager.Instance.IsStageForest()) return roleSpd;
                //spd
                var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_01);
                roleSpd = cfg.Content[0] + data.attr.Calc(AttributeType.RoleSpd);
                roleSpd *= GameConst.AttributeDivisor;
                var spdUp = data.attr.Calc(AttributeType.RoleSpdUp);
                var upSpdBuff = data.buffMgr.FindBuffById((int)AttributeType.UpSpd);
                var curRoleSpdUp = Data.buffMgr.FindBuffById((int)AttributeType.RoleSpdUp);
                if (upSpdBuff != null) spdUp += upSpdBuff.value;
                if (curRoleSpdUp != null) spdUp += curRoleSpdUp.value;
                roleSpd += roleSpd * spdUp;
                return roleSpd;
            }
            set => roleSpd = value;
        }
        private MoveComponent moveComponent;

        public void FlipX(Vector2 direction)
        {
            if (animator == null) return;
            switch (direction.x)
            {
                case < 0:
                    animator.FlipX(true);
                    break;
                case > 0:
                    animator.FlipX(false);
                    break;
            }
        }
        /// <summary>
        /// 获取角色模型朝向
        /// </summary>
        /// <returns></returns>
        public int GetPlayerOrientation()
        {
            return Lodash.RoundToInt(animator.Animator.skeleton.ScaleX);
        }
        public void PlayWalk()
        {
            if(animator == null)return;
            animator.PlayAnimation(GameConst.AnimationName.Walk);
        }

        public void PlayIdle()
        {
            if(animator == null)return;
            animator.PlayAnimation(GameConst.AnimationName.Idle);
        }

        public void CalRoleAttr()
        {
            if(!BattleManager.Instance.IsStageForest())return;
            //spd
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_01);
            roleSpd = cfg.Content[0] * GameConst.AttributeDivisor;
            var spdUp = data.attr.Calc(AttributeType.RoleSpdUp);
            roleSpd += roleSpd * spdUp;
        }
    }
}