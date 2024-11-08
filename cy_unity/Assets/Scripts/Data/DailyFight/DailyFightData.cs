using System.Collections.Generic;
using System.Linq;
using DH.Config;
using DH.Proto;
namespace DH.Data
{
    [ProtoWrap(typeof(DailyFight))]
    public partial class DailyFightData : BaseData
    {
        private IReadOnlyList<DailyStageProgressRewardCfg> cfgs;
        private int costNum = 5;
        public override void Init()
        {
            base.Init();
            cfgs = ConfigCenter.DailyStageProgressRewardCfgColl.DataItems;
            var costCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_02);
            if (costCfg != null && costCfg.Content.Count > 0) costNum = costCfg.Content[0];
        }
        public bool CheckIsShowRedDot()
        {
            for (var i = 0; i < cfgs.Count; i++)
            {
                if (cfgs[i].Type == 1)//日进度
                {
                    if (DailyKills >= cfgs[i].Value1 && !DailyClaim.Contains(cfgs[i].Id))
                    {
                        return true;
                    }
                }
                else if (cfgs[i].Type == 2)//周进度
                {
                    if (WeekProgress >= cfgs[i].Value2 && !WeekClaim.Contains(cfgs[i].Id))
                    {
                        return true;
                    }
                }
            }
            var ret = DataCenter.livesData.CheckItemIsEnough((int)GameConst.ItemIdCode.EnergyDrink, costNum);
            // 检查是否有奖励没领取
            return DataCenter.dailyFightData.Count > 0 && ret;
        }
    }
}