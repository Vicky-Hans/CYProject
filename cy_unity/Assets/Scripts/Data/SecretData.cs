using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using DH.UIFramework;

namespace DH.Data
{
    [ProtoWrap(typeof(Secret))]
    public partial class SecretData : BaseData
    {
        [AutoNotify] private int beginStage;
        public override void Init()
        {
            base.Init();
        }
        protected override void ClearData()
        {
            StageInfo.Clear();
            HasArchive = false;
            base.ClearData();
        }

        public List<int> GetSecretRewardList()
        {
            var ret = new List<int>();
            var awardsCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_10);
            if (awardsCfg != null && awardsCfg.Content.Count > 0)
            {
                ret = awardsCfg.Content;
            }
            return ret;
        }

        /// <summary>
        /// 获取关卡状态
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public EBoxState GetBoxStateByChapterId(int chapterId)
        {
            if (chapterId > CurrStage)
            {
                return EBoxState.Close;
            }

            if (!StageInfo.TryGetValue(chapterId, out SecretStage stageInfo))
            {
                return EBoxState.Close;
            }

            if (!stageInfo.Pass)
            {
                return EBoxState.Close;
            }

            if (stageInfo.Claim)
            {
                return EBoxState.Open;
            }

            return EBoxState.Wait;
        }
        
        /// <summary>
        /// 更新关卡信息
        /// </summary>
        /// <param name="chapterId"></param>
        /// <param name="pass"></param>
        /// <param name="claim"></param>
        public void UpdateStageInfo(int chapterId, bool pass, bool claim)
        {
            
            if (!StageInfo.TryGetValue(chapterId, out SecretStage stageInfo))
            {
                stageInfo = new SecretStage();
                StageInfo[chapterId] = stageInfo;
            }
            stageInfo.Pass = pass;
            stageInfo.Claim = claim;
        }

        public bool IsCanClaimReward(int chapterId)
        {
            if (!StageInfo.TryGetValue(chapterId, out SecretStage stageInfo))
            {
                return false;
            }
            return stageInfo.Claim;
        }

        public SecretStage GetStageInfo(int stageId)
        {
            return StageInfo.TryGetValue(stageId, out var stage) ? stage : null;
        }
        
        public void ChangeSecretScore(int value, bool isAdd)
        {
            if (isAdd)
            {
                Score += value; 
            }
            else
            {
                Score -= value;
            }


        }
    }
}