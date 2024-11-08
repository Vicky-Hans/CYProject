using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DH.Config;
using DH.Data;
using DH.Game.UI;
using DHFramework;
using UnityEngine;
namespace DH.Game
{
    public class DropManager
    {
        private List<DropController> dropList = new();
        private List<DropController> pendingList = new();
        private List<DropController> removeList = new();
        private List<Vector3> posList = new();
        private int maxCount;
        /// <summary>
        /// 所有随机掉落物品
        /// </summary>
        private List<SecretEventCfg> allRandomItems = new();
        private float lastRandomTime;
        private float curTime;
        /// <summary>
        /// 随机间隔 秒
        /// </summary>
        private float randomInterval = 30;

        private float cfgTime;

        private float distance;

        /// <summary>
        /// 
        /// </summary>
        private int totalWidth;

        private int rangeOffset = 8;

        private Dictionary<int,float> ratioDictionary = new();

        private Vector2 randomRange { get; set; }
        
        public DropManager()
        {
            var maxDefCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_22);
            if (maxDefCfg is { Content: { Count: > 0 } })
            {
                maxCount = maxDefCfg.Content[0];
            }
            var timeDefCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_23);
            if (timeDefCfg is { Content: { Count: > 0}})
            {
                cfgTime = timeDefCfg.Content[0];
                randomInterval = timeDefCfg.Content[0];
            }

            InitSecretSeasonEffect();

            var allCfg = ConfigCenter.SecretEventCfgColl.DataItems;
            allRandomItems.Clear();
            foreach (var itemCfg in allCfg)
            {
                if (itemCfg.Season == null || itemCfg.Season.Contains(DataCenter.secretData.Season))
                {
                    allRandomItems.Add(itemCfg);
                }
            }
            
            var cfg = ConfigCenter.CopySecretCfgColl.GetDataById(GameDataManager.Instance.CurChapterId);
            if (cfg == null || cfg.MapSize == null || cfg.MapSize.Count < 2)
            {
                var tempCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_04);
                randomRange = new Vector2(tempCfg.Content[0] - rangeOffset, tempCfg.Content[1] - rangeOffset) * GameConst.AttributeDivisor;
            }
            else
            {
                randomRange = new Vector2(cfg.MapSize[0] -rangeOffset, cfg.MapSize[1] - rangeOffset) * GameConst.AttributeDivisor;
            }

            randomRange = new Vector2(randomRange.x - rangeOffset, randomRange.y - rangeOffset);

            var defCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_30);
            if(defCfg !=null || defCfg.Content!= null && defCfg.Content.Count > 0)
            {
                distance = defCfg.Content[0] * GameConst.AttributeDivisor;;
            }

            foreach (var item in allRandomItems)
            {
                totalWidth += item.Weight;
            }
            ratioDictionary.Clear();
            foreach (var item in allRandomItems)
            {
                ratioDictionary.Add(item.Id, item.Weight / (float)totalWidth);
            }
        }

        /// <summary>
        ///  处理赛季特效
        /// </summary>
        private void InitSecretSeasonEffect()
        {
            var cfg = ConfigCenter.SecretSeasonCfgColl.GetDataById(DataCenter.secretData.Season);
            if(cfg == null || cfg.Effect == null || cfg.Effect.Count == 0) return;
            foreach (var effect in cfg.Effect)
            {
                var effectCfg = ConfigCenter.SecretSeasonEffectCfgColl.GetDataById(effect);
                if(effectCfg == null) continue;
                switch (effect)
                {
                    case 1:
                    {
                        randomInterval = cfgTime * (1 / (1 + effectCfg.Value[0] * GameConst.AttributeDivisor));
                        break;
                    }
                    default:
                    {
                        DHLog.Error($"没处理 秘林 赛季 效果  secretseasoneffectCfg id is {effect} ");
                        break;
                    }
                }
            }
        }

        public void Init()
        {
            // 初始化 调整，为了放置刚开始的时候生成一个 所以加一个偏移量
            lastRandomTime = curTime + 3;
            if (!DataCenter.secretData.HasArchive)
            {
                RandomDropItem(maxCount,Vector3.zero, true);
            }
        }


        public List<DropController> DropList => dropList;
        
        private float GetCapsuleDis(Vector3 pos, CapsuleCollider2D capsule)
        {
            return Lodash.DistanceToCapsule(pos, capsule);
        }

        public void Update(float deltaTime)
        {
            UpdateDropList(deltaTime);
            CheckRandomItem(deltaTime);
        }
        private void UpdateDropList(float deltaTime)
        {
             // remove
            foreach (var unitObj in removeList)
            {
                dropList.Remove(unitObj);
            }
            removeList.Clear();
            
            foreach (var unitObj in dropList)
            {
                if (unitObj.IsNeedRemove)
                {
                    removeList.Add(unitObj);
                    continue;
                }
                unitObj.OnUpdate(deltaTime);
            }
            foreach (var unitObj in pendingList) dropList.Add(unitObj);
            pendingList.Clear();
        }
        public void Add(DropController obj)
        {
            pendingList.Add(obj);
        }

        public void Remove(DropController obj)
        {
            removeList.Add(obj);
        }
        /// <summary>
        /// 检查是否可以随机掉落物品
        /// </summary>
        /// <returns></returns>
        private bool CheckIsCanRandomItem()
        {
            return itemCount < maxCount;
        }

        private int itemCount
        {
            get
            {
                var count = 0;
                foreach (var dropItem in dropList)
                {
                    if (dropItem.DropId != 0)
                    {
                        count++;
                    }
                }
                return count;
            }
        }


        private void UpdatePosInfoList()
        {
            posList.Clear();
            foreach (var item in dropList)
            {
                posList.Add(item.transform.localPosition);
            }
        }

        /// <summary>
        ///  随机掉落物品
        /// </summary>
        /// <param name="count"></param>
        /// <param name="pos"></param>
        /// <param name="needRandom"></param>
        public async void RandomDropItem(int count, Vector3 pos, bool needRandom)
        {
            
            UpdatePosInfoList();
            for (int i = 0; i < count; i++)
            {
                await UniTask.Delay(20);
                var itemId = RandomSecretEventId();
                
                var tempPos = pos;
                if (needRandom)
                {
                    tempPos = GetRandomPos();
                }
                
                posList.Add(tempPos);
                BattleManager.Instance.fightingManagerIns.CreateDrop(0,tempPos, itemId);
            }
        }


        private Vector3 RandomPos()
        {
            return  new Vector3(
                Lodash.RandRangeFloat(-randomRange.x/2, randomRange.x/2),
                Lodash.RandRangeFloat(-randomRange.y/2, randomRange.y/2), 
                0);
        }

        private Vector3 GetRandomPos()
        {
            Vector3 ret=RandomPos();
            bool isRetry = true;
            int randomCount = 5000;
            while (isRetry && randomCount > 0)
            {
                isRetry = false;
                ret = RandomPos();
                foreach (var tempPos in posList)
                {
                    if (Vector3.Distance(ret, tempPos) < distance)
                    {
                        isRetry = true;
                        break;
                    }
                }

                randomCount--;
            }
            if(randomCount <=0)
            {
               DHLog.Warning("随机位置失败  随机数量大于随机范围");
            }
            return ret;
        }

        private int RandomSecretEventId()
        {
            var randomValue = Lodash.RandRangeFloat(0, 1f);
            var curValue = 0f;
            foreach (var item in ratioDictionary)
            {
                curValue += item.Value;
                if(randomValue <= curValue)
                {
                    return item.Key;
                }
            }
            return 0;
        }

        private void CheckRandomItem(float deltaTime)
        {
            if(!CheckIsCanRandomItem())
            {
                curTime += deltaTime;
                return;
            }
            if (curTime < lastRandomTime)
            {
                curTime += deltaTime;
                return;
            }
         
            RandomDropItem(1,Vector3.zero, true);
            lastRandomTime = curTime + randomInterval;
        }


        public void DealDropInfo(int dropId, float exp = 0f)
        {
            switch (dropId)
            {
                case 0: AddExp(exp); break;
                case 1: DealDropEvent1(); break;
                case 2: DealDropEvent2(dropId); break;
                case 3: DealDropEvent3(); break;
                case 4: DealDropEvent4(dropId); break;
                case 5: DealDropEvent5(dropId); break;
                case 6: DealDropEvent6(dropId); break;
                case 7: DealDropEvent7(); break;
                case 8: DealDropEvent8(dropId); break;
            }
        }
        private void AddExp(float curExp)
        {
            FightingSoundHelper.Instance.PlaySecretPickUpExp();
            PlayerStats.Instance.KillExp +=(int) curExp;
        }

        /// <summary>
        /// 磁铁
        /// </summary>
        private void DealDropEvent1()
        {
            // 所有的 
            foreach (var item in DropList)
            {
                if (item.DropId == 0)
                {
                    item.OnTriggerPlayer();
                }
            }
        }
        /// <summary>
        /// 医疗包
        /// </summary>
        private void DealDropEvent2(int dropId)
        {
            var secretCfg = ConfigCenter.SecretEventCfgColl.GetDataById(dropId);
            if(secretCfg == null || secretCfg.Param== null || secretCfg.Param.Count == 0) return;
            var maxHp = BattleManager.Instance.fightingManagerIns.playerCtrl.Player.resource.MaxHp;
            var addValue = maxHp * secretCfg.Param[0] * GameConst.AttributeDivisor;
            BattleManager.Instance.fightingManagerIns.playerCtrl.Player.AddHp((long)addValue);
            BattleManager.Instance.fightingManagerIns.CreatePlayerReviveEffect();

        }
        /// <summary>
        /// 炸弹
        /// </summary>
        private void DealDropEvent3()
        {
            FightingSoundHelper.Instance.PlaySecretPickUpBoom();
            List<MonsterController> screenMonsters = new();
            BattleManager.Instance.fightingManagerIns.enemyManager.GetAllMonstersInScreen(screenMonsters, false);
            foreach (var monster in screenMonsters)
            {
                monster.DecHpToDead();
            }
        }

        /// <summary>
        /// 暴击药水
        /// </summary>
        private void DealDropEvent4(int dropId)
        {
            var secretCfg = ConfigCenter.SecretEventCfgColl.GetDataById(dropId);
            if(secretCfg?.Param == null || secretCfg.Param.Count == 0) return;
            BattleManager.Instance.fightingManagerIns.playerCtrl.AddCriticalStrikePotionBuff(secretCfg.Param);
        }
        /// <summary>
        /// 攻速药水
        /// </summary>
        private void DealDropEvent5(int dropId)
        {
            var secretCfg = ConfigCenter.SecretEventCfgColl.GetDataById(dropId);
            if(secretCfg?.Param == null || secretCfg.Param.Count == 0) return;
            BattleManager.Instance.fightingManagerIns.playerCtrl.AddAttackSpeedPotionBuff(secretCfg.Param);
        }
        /// <summary>
        /// 防御药水
        /// </summary>
        private void DealDropEvent6(int dropId)
        {
            var secretCfg = ConfigCenter.SecretEventCfgColl.GetDataById(dropId);
            if(secretCfg?.Param == null || secretCfg.Param.Count == 0) return;
            BattleManager.Instance.fightingManagerIns.playerCtrl.AddDefensePotionBuff(secretCfg.Param);
        }
        /// <summary>
        /// 经验药水
        /// </summary>
        private void DealDropEvent7()
        {
            GameManager.Instance.AddTalentRequestToList(GameDataManager.Instance.Level,ESecretTalentRefreshType.SecretItemRefresh);
        }
        /// <summary>
        /// 疾跑药水
        /// </summary>
        private void DealDropEvent8(int dropId)
        {
            var secretCfg = ConfigCenter.SecretEventCfgColl.GetDataById(dropId);
            if(secretCfg?.Param == null || secretCfg.Param.Count == 0) return;
            BattleManager.Instance.fightingManagerIns.playerCtrl.AddRunFastPotionBuff(secretCfg.Param);
        }
        public void CheckAndUpdateLastRandomTime()
        {
            if (lastRandomTime > curTime)
            {
                return;
            }
            lastRandomTime = Mathf.Ceil(curTime) + randomInterval;
        }
    }
}