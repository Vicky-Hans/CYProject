using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DH.Config;
using DH.UIFramework;
using DH.Proto;
using DH.UIFramework.Observables;
using DHFramework;
using Google.Protobuf.Collections;
using UnityEngine;
using UnityEngine.Pool;
using EquipAttr = DH.Proto.EquipAttr;

namespace DH.Data
{

    public enum EGameState
    {
        None,
        /// <summary>
        /// 开始
        /// </summary>
        Begin,
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        ///  失败
        /// </summary>
        Fail,
    }

    public partial class GameDataManager : ObservableSingleton<GameDataManager>
    {
        /// <summary>
        /// 仅用于战斗中给武器分配唯一id,已分配id的武器不要重复分配
        /// 每场战斗开始时，需要重置，从1开始
        /// </summary>
        private static long weaponUid = 1;
        [AutoNotify] private int gMaxRow; //背包最大行数
        [AutoNotify] private int gMaxColumn; //背包最大列数
        [AutoNotify] private int totalMergeNum; //棋盘总的合成次数
        [AutoNotify] private int physicsMergeNum; //棋盘物理装备合成次数
        [AutoNotify] private int magicMergeNum; //棋盘魔法装备合成次数
        [AutoNotify] private Rect gridBorderRect; //格子边界
        [AutoNotify] private long mergeWeaponUid; //最新合成武器的uid
        [AutoNotify] private List<List<int>> gridData; //背包数据（来自服务端）
        [AutoNotify] private ObservableList<RandomWeaponData> randomWeaponDataList; //随机武器库数据（来自服务器）
        [AutoNotify] private IBaseFightData curFightData; //当前战斗数据
        /// <summary>
        /// 合成武器的uid
        /// </summary>
        [AutoNotify] private List<long> mergeWeaponList = new();
        /// <summary>
        /// 游戏时间
        /// </summary>
        [AutoNotify] private int curGameDuration;
        [AutoNotify] private bool showBossInfo;
        
        public void SetFightData(IBaseFightData fightData)
        {
            CurFightData = fightData;
        }
        public EStateType CurStageType{ get; set; }
        /// <summary>
        /// 当前章节的id
        /// </summary>
        public int CurChapterId { get; set; }

        /// <summary>
        /// 当前心愿的个数
        /// </summary>
        [AutoNotify] private int curWishCount;
        private bool waveEnd = true;
        public bool WaveEnd
        {
            get => waveEnd;
            set
            {
                Set(ref waveEnd, value);
                if (curFightData.Attr.HeroActive != null)
                {
                    IsCanUseHeroActiveSkill =!WaveEnd && curFightData.Attr.HeroActive.Energy >= maxHeroActiveEnergyValue;
                }
                else
                {
                    IsCanUseHeroActiveSkill = !WaveEnd;
                }
            }
        }

        [AutoNotify] private EGameState curGameState;
        [AutoNotify] private float expProgressValue;
        [AutoNotify] private int randomRefreshCount; //随机库刷新的次数统计
        [AutoNotify] private ObservableList<BackpackWeaponData> backpackWeaponList=new(); //背包武器数据（供其他系统使用）
        [AutoNotify] private int endlessTotalSpawnNum; //无尽关卡总波次
        [AutoNotify] private int endlessStartID; //无尽关卡怪物波次起始ID

        [AutoNotify] private bool isArchiveEnter; //是否为存档模式进入
        /// <summary>
        /// 是否可以使用主动技能
        /// </summary>
        [AutoNotify] private bool isCanUseHeroActiveSkill;
        public int PhyEquipMergedGainValue { get; set; }//物理武器合成增益数值
        public int MagicEquipMergedGainValue { get; set; }//魔法武器合成增益数值
        /// <summary>
        ///  主动技能的能量最大值
        /// </summary>
        private int maxHeroActiveEnergyValue;
        /// <summary>
        /// 游戏中的货币 （银币）
        /// </summary>
        public int GameCoin
        {
            get =>curFightData.Stage.Money;
            set
            {
                DataCenter.itemsData.GameCoin = value;
                if (curFightData is { Stage: not null })
                {
                    curFightData.Stage.Money = value;
                }
            }
        }
        /// <summary>
        /// 当前天赋选择的次数
        /// </summary>
        public int ChooseTalentCount
        {
            get=>curFightData.Stage.TalentRefreshLv;
            set =>curFightData.Stage.TalentRefreshLv = value;
        }
        /// <summary>
        ///  获取可以选择的天赋次数
        /// </summary>
        /// <returns></returns>
        public int GetCanChooseTalentCount()
        {
            var chooseTalent = GetChooseTalent(ETalentType.TalentTypeNone);
            // 没随机
            if (UnOpTalents.Count == 0 && chooseTalent.Count == 0)
            {
                return Level -1;
            }
            // 随机了
            return Level - ChooseTalentCount - 1;
        }
        /// <summary>
        /// 刷新出来没有操作的天赋
        /// </summary>
        public List<int> UnOpTalents
        {
            get=>curFightData.Stage.Talents.ToList();
            set
            {
                curFightData.Stage.Talents.Clear();
                foreach (var id in value)
                {
                    curFightData.Stage.Talents.Add(id);
                }
            }
        }

        public void ClearUnOpTalent()
        {
            curFightData.Stage.Talents.Clear();
        }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get=>curFightData.Stage.Lv;
            set => curFightData.Stage.Lv = value;
        }

        public int Kills =>curFightData.Stage.Kills;
        public int Exp
        {
            get=>curFightData.Stage.Exp;

            set=>curFightData.Stage.Exp = value;
        }

        /// <summary>
        /// 初始天赋剩余次数，进入战斗时默认选n次装备天赋，不选完无法战斗；
        /// </summary>
        public int TalentBeginRefresh
        {
            get=>curFightData.Stage.TalentBeginRefresh;
            set=>curFightData.Stage.TalentBeginRefresh = value;
        }

        /// <summary>
        /// 刷新装备需要的钱
        /// </summary>
        public int EquipRefreshMoney
        {
            get=>curFightData.Stage.EquipRefreshMoney; 
            set=>curFightData.Stage.EquipRefreshMoney = value;
        }

        /// <summary>
        /// 装备广告刷新的总次数
        /// </summary>
        public int EquipAdRefreshTotalCount
        {
            get=>curFightData.Stage.EquipAdRefresh;
            set=>curFightData.Stage.EquipAdRefresh = value;
        }

        /// <summary>
        /// 装备免费刷新的次数
        /// </summary>
        public int EquipFreeRefreshCount
        {
            get=>curFightData.Stage.EquipWaveRefresh;
            set=>curFightData.Stage.EquipWaveRefresh = value;
        }

        public MapField<string,int> RoleAttr=>curFightData.Attr.RoleAttr;

        public RepeatedField<int> GlobalSkills=>curFightData.Attr.GlobalSkills;

        public MapField<int,EquipAttr> EquipsAttr=>curFightData.Attr.Equips;

        public MapField<long, long> HurtStat=>curFightData.Stage.HurtStat;

        public FightAttr Attr=>curFightData.Attr;
        public MapField<string,int> StageAttr=>curFightData.Stage.Attr;
        public long Uid=>curFightData.Uid;

        public long Seed=>curFightData.Seed; 

        public long SurvivalTime=>curFightData.Stage.SurvivalTime;

        public int EquipWaveRefresh
        {
            get => curFightData.Stage.EquipWaveRefresh;
            set => curFightData.Stage.EquipWaveRefresh = value;
        }

        public int EquipWaveAdRefresh
        {
            get=>curFightData.Stage.EquipWaveAdRefresh;
            set=>curFightData.Stage.EquipWaveAdRefresh = value;
        }

        public int killedCount
        {
            get=>curFightData.Stage.Kills;
            set=> curFightData.Stage.Kills = value;
        }

        public long Hp
        {
            get
            {
                if (curFightData.Stage.Attr.ContainsKey(AttributeName.Hp))
                {
                    return curFightData.Stage.Attr[AttributeName.Hp];
                }
                
                return curFightData.Attr.RoleAttr.GetValueOrDefault(AttributeName.Hp, 100);
                
            }
        }

        public long MaxHp
        {
            get
            {
                if (curFightData.Stage.Attr.ContainsKey("maxHp"))
                {
                    return curFightData.Stage.Attr["maxHp"];
                }
                
                return curFightData.Attr.RoleAttr.GetValueOrDefault(AttributeName.Hp, 100);
            }
        }

        public long Armor
        {
            get
            {
                if (curFightData.Stage.Attr.TryGetValue("armor", out var armor))
                {
                    return armor;
                }
                return 0;
            }
        }

        public int TridentKillNum
        {
            get
            {
                curFightData.Stage.Attr.TryGetValue("tridentKillNum",
                    out var killNum);
                return killNum;
            }
        }
        public Dictionary<long, long> SkillHurtDic=>curFightData.Stage.HurtStat.ToDictionary(x => x.Key, x => x.Value);
        /// <summary>
        /// 当前波次广告刷新次数
        /// </summary>
        public int EquipWaveAdRefreshCount
        {
            get=>curFightData.Stage.EquipWaveAdRefresh;
            set=>curFightData.Stage.EquipWaveAdRefresh = value;
        }

        public int Wave
        {
            get=>curFightData.Stage.Wave;

            set=>curFightData.Stage.Wave = value;   
        }

        /// <summary>
        /// 英雄主动技能信息
        /// </summary>
        public HeroActive HeroActive=>curFightData.Attr?.HeroActive;

        /// <summary>
        /// /英雄局外对战斗内刷新相关有影响的技能（heroSkill表的id）
        /// </summary>
        public RepeatedField<int> HeroEffect =>curFightData.Attr.HeroEffect;

        /// <summary>
        ///  英雄主动技能属性
        /// </summary>
        public MapField<string, int> ActiveSkillAttr=>curFightData.Attr.HeroActive.Attr;

        /// <summary>
        /// 英雄id
        /// </summary>
        public int HeroId=>curFightData.Attr.HeroActive.HeroId;
        
        /// <summary>
        ///  英雄主动技能技能效果；带触发效果的；纯属性的已经计算完毕；
        /// </summary>
        public RepeatedField<int> heroSkills=>curFightData.Attr.HeroActive.Skills;

        /// <summary>
        /// 获取/设置 当前能量值
        /// </summary>
        public int HeroActiveEnergy
        {
            get=>curFightData.Attr.HeroActive.Energy;

            set
            {
                if (curFightData.Attr?.HeroActive == null)
                {
                    return;
                }

                if (value > maxHeroActiveEnergyValue)
                {
                    curFightData.Attr.HeroActive.Energy = maxHeroActiveEnergyValue;
                }
                else
                {
                    curFightData.Attr.HeroActive.Energy = value;
                }

                IsCanUseHeroActiveSkill = !WaveEnd &&
                                          curFightData.Attr.HeroActive.Energy >=
                                          maxHeroActiveEnergyValue;
            }
        }

        /// <summary>
        /// 添加英雄主动技能能量
        /// </summary>
        /// <param name="value"></param>
        public void AddHeroActiveEnergy(int value)
        {
            if (HeroActiveEnergy + value > maxHeroActiveEnergyValue)
            {
                HeroActiveEnergy = maxHeroActiveEnergyValue;
            }
            else
            {
                HeroActiveEnergy += value;
            }
           
        }
        /// <summary>
        /// 进度值
        /// </summary>
        /// <returns></returns>
        public float HeroActiveEnergyProgress()
        {
            if (maxHeroActiveEnergyValue == 0)
            {
                return 0;
            }

            return 1 - HeroActiveEnergy / (float) maxHeroActiveEnergyValue;
        }

        /// <summary>
        /// 随机装备数据，此处有个潜规则，赋值时候一定要先赋值Holy，再赋值Equips
        /// </summary>
        public List<int> Equips
        {
            get=>curFightData.Stage.Equips.ToList();
            set
            {
                curFightData.Stage.Equips.Clear();
                foreach (var id in value)
                {
                    curFightData.Stage.Equips.Add(id);
                }
                RandomWeaponDataList.Clear();
                for (var i = 0; i < value.Count; i++)
                {
                    var tmpData = new RandomWeaponData {WeaponId = value[i],WeaponAttrType = 1};
                    RandomWeaponDataList.Add(tmpData);
                }
                RandomRefreshCount++;
            }
        }
        /// <summary>
        /// 装备进阶
        /// </summary>
        /// <param name="equipId"></param>
        /// <param name="equipModelId"></param>
        public void ChooseEquipAdvance(int equipId, int equipModelId)
        {
            if (curFightData.Stage.EquipMultiAttr.ContainsKey(equipId))
            {
                DHLog.Error("已经选择过了");
                return;
            }
            curFightData.Stage.EquipMultiAttr.Add(equipId,equipModelId);
        }
        /// <summary>
        /// 获取装备进阶的模型id
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public int GetEquipAdvanceEquipModelIdByEquipId(int equipId)
        {
            if(curFightData.Stage.EquipMultiAttr.TryGetValue(equipId,out var modelId))
            {
                return modelId;
            }
            return 0;
        }

        /// <summary>
        /// 获取当前有的心愿
        /// </summary>
        public RepeatedField<int> Wish=>curFightData.Stage.Wish;

        /// <summary>
        /// 添加心愿
        /// </summary>
        /// <param name="wishId"></param>
        public void AddWish(int wishId)
        {
            if(IsSecretFightData()) return;
            curFightData.Stage.Wish.Add(wishId);
            CurWishCount = Wish.Count;
        }
        /// <summary>
        /// 移除心愿
        /// </summary>
        /// <param name="wishId"></param>
        public void RemoveWish(int wishId)
        {
            if(IsSecretFightData()) return;
            if (!Wish.Contains(wishId))
            {
                DHLog.Error($"不存在该心愿 id is {wishId}");
                return;
            }
            // 只会移除第一个 移除没得的也不会报错
            curFightData.Stage.Wish.Remove(wishId);
            CurWishCount = Wish.Count;
        }
        /// <summary>
        /// 获取心愿类型
        /// </summary>
        /// <param name="wishId"></param>
        /// <returns></returns>
        public EWishType ParseWishType(int wishId)
        {
            var type = wishId % 10;
            return (EWishType)type;
        }
        /// <summary>
        /// 获取心愿的数据id
        /// </summary>
        /// <param name="wishId"></param>
        /// <returns></returns>
        public int ParseWishDataId(int wishId)
        {
            return wishId / 10;
        }
                /// <summary>
        /// 获取心愿id
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetWishIdByDataIdAndType(int dataId,EWishType type)
        {
            return dataId*10+(int)type;
        }

        public MapField<int,int> EquipSlots=> curFightData.Stage.EquipSlots;

#if UNITY_EDITOR
        private void PrintFightData(IBaseFightData fightData)
        {
            Type type = fightData.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(fightData, null);
                Debug.Log($"{property.Name}: {value}");
            }
        }
#endif
        
        public void Init(IBaseFightData fightData,EStateType curStateType = EStateType.StageTypeMainStage)
        {
#if UNITY_EDITOR
            DHLog.Debug("================= Print FightData Start =================");
            PrintFightData(fightData);   
            DHLog.Debug("================= Print FightData End =================");
#endif
            ResetWeaponUid();
            curFightData = fightData;
            GameCoin = fightData.Stage.Money;
            CurGameState = EGameState.Begin;
            CurWishCount = Wish?.Count ?? 0;
            waveEnd = true;
            CurGameDuration = fightData.Stage.Dur == null ? 0 : fightData.Stage.Dur;
            BackpackWeaponList.Clear();
            GMaxRow = 5;
            GMaxColumn = 7;
            GridData = new List<List<int>>(GMaxRow);
            ShowBossInfo = false;
            mergeWeaponList.Clear();
            for (var i = 0; i < GMaxRow; i++)
            {
                var rowList = new List<int>(GMaxColumn);
                rowList.AddRange(Enumerable.Repeat(0,GMaxColumn));
                GridData.Add(rowList);
            }

            if (!IsSecretFightData())
            {
                if (fightData.Stage == null) return;
                if ( fightData.Stage.EquipSlots != null)
                {
                    foreach (var gridInfo in fightData.Stage.EquipSlots)
                    {
                        if (gridInfo.Value <= 0) continue;
                        var row = gridInfo.Key/100-1;
                        var column = gridInfo.Key%100-1;
                        if (row < 0) row = 0;
                        if (column < 0) column = 0;
                        if (row >= GridData.Count) continue;
                        if (column >= GridData[row].Count) continue;
                        GridData[row][column] = gridInfo.Value;
                        if (gridInfo.Value > 1)//新增武器
                        {
                            var curWeaponId = gridInfo.Value;
                            var weaponData = CreateBackpackData(curWeaponId*10+1,1,ELocationType.Backpack);
                            if (weaponData == null) continue;
                            weaponData.RowIdx = row;
                            weaponData.ColumnIdx = column;
                            BackpackWeaponList.Add(weaponData);
                        }
                    }
                }
            }
            else
            {
                if (fightData.Stage.EquipSlots== null || fightData.Stage.EquipSlots.Count <= 0)
                {
                    if (fightData.Stage.HeroEquipTalent != null && fightData.Stage.HeroEquipTalent.Count > 0)
                    {
                        // 初始话
                        foreach (var item in fightData.Stage.HeroEquipTalent)
                        {
                            for (int i = 0; i < item.Value; i++)
                            {
                                InitBackpackWeaponListByEquipTalent(item.Key);
                            }
                        } 
                    }
                } 
                else if (fightData.Stage.EquipSlots != null && fightData.Stage.EquipSlots.Count > 0)
                {
                    foreach (var item in fightData.Stage.EquipSlots)
                    {
                        for (int i = 0; i < item.Value; i++)
                        {
                            var weaponData = CreateBackpackData(item.Key*10+1,1,ELocationType.Backpack);
                            if (weaponData == null) continue;
                            BackpackWeaponList.Add(weaponData);
                        }
                    }
                }
            }


            RandomWeaponDataList = new ObservableList<RandomWeaponData>();
            if (fightData.Stage.Equips is { Count: > 0 })//随机装备
            {
                for (var i = 0; i < fightData.Stage.Equips.Count; i++)
                {
                    var tmpData = new RandomWeaponData{ WeaponId = fightData.Stage.Equips[i],WeaponAttrType = 1};
                    RandomWeaponDataList.Add(tmpData);
                }
            }
            //根据新增的武器，再次计算格子的被占用情况
            for (var i = 0; i < BackpackWeaponList.Count; i++)
            {
                var curWeaponData = BackpackWeaponList[i];
                var startRow = curWeaponData.RowIdx;
                var startColumn = curWeaponData.ColumnIdx;
                for (var j = 0; j < curWeaponData.OccupyList.Count; j++)
                {
                    var curRow = startRow+curWeaponData.OccupyList[j].x;
                    var curColumn = startColumn+curWeaponData.OccupyList[j].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    GridData[curRow][curColumn] = curWeaponData.WeaponId;
                }
            }
            GetAreaBorder();
            maxHeroActiveEnergyValue = GetMaxHeroActiveEnergy();
        }
        private void InitBackpackWeaponListByEquipTalent(int talentId)
        {
            var secretTalentCfg = ConfigCenter.SecretCopyTalentCfgColl.GetDataById(talentId);
            if (secretTalentCfg == null)
            {
                DHLog.Error($"错误的配置 请检查 SecretCopyTalentCfg  是否存在 {talentId}");
                return;
            }

            if (secretTalentCfg.Type == 3)
            {
                var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(secretTalentCfg.Id);
                if (talentCfg == null)
                {
                    DHLog.Error($"错误的配置 请检查 TalentCfg  是否存在 {talentId}");
                    return;
                }

                var weaponData = CreateBackpackData(talentCfg.EquipModelId * 10 + 1, 1, ELocationType.Backpack);
                if (weaponData == null) return;
                AddEquipToSlots(talentCfg.EquipModelId, null);
            }
        }

        public bool IsSecretFightData()
        {
            return CurStageType == EStateType.StageTypeSecret;
        }

        private int GetMaxHeroActiveEnergy()
        {
            var heroCfg = ConfigCenter.HeroMainCfgColl.GetDataById(HeroId);
            if(heroCfg == null || heroCfg.MainSkill == 0)
            {
                return 0;
            }

            var heroSkillCfg = ConfigCenter.HeroSkillCfgColl.GetDataById(heroCfg.MainSkill);
            if (heroSkillCfg == null|| heroSkillCfg.Energy == 0)
            {
                return 0;
            }
                
            return heroSkillCfg.Energy;
        }
        /// <summary>
        ///  恢复数据
        /// </summary>
        public void OnReviveSuccess()
        {
            WaveEnd = true;
            PlayerStats.Instance.ReviveLastWaveData();
        }
        private long GetWeaponUid()
        {
            return weaponUid++;
        }
        /// <summary>
        /// 每场战斗开始的时候需要调用
        /// </summary>
        private void ResetWeaponUid()
        {
            weaponUid = 1;
        }
        /// <summary>
        /// 获取装备初始属性
        /// </summary>
        /// <param name="equipId">装备id</param>
        /// <returns></returns>
        public Dictionary<string, int> GetSkillProperty(int equipId)
        {
            Dictionary<string, int> ret = new();
            if (curFightData == null) return ret;
            if (!curFightData.Attr.Equips.ContainsKey(equipId)) return ret;
            var data = curFightData.Attr.Equips[equipId];
            foreach (var item in data.Attr)
            {
                ret.Add(item.Key, item.Value);
            }
            return ret;
            
        }
        public void ClearUnChooseTalent()
        {
            curFightData.Stage.Talents.Clear();
        }
        /// <summary>
        /// 天赋广告刷新的剩余次数
        /// </summary>
        public int TalentAdReFreshCount
        {
            get => curFightData.Stage.TalentAdRefresh;
            set => curFightData.Stage.TalentAdRefresh = value;
        }
        public bool CheckIsCanRandomTalent()
        {
            return Level - ChooseTalentCount > 1;
        }
        /// <summary>
        /// 获取所选天赋
        /// </summary>
        public MapField<int, int> GetChooseTalent(ETalentType type)
        {
            switch (type)
            {
                case ETalentType.TalentTypeNone:
                {
                    MapField<int, int> ret = new();
                    foreach (var item in curFightData.Stage.EquipTalent)
                    {
                        ret.Add(item.Key, item.Value);
                    }
                    foreach (var item in curFightData.Stage.EffectTalent)
                    {
                        ret.Add(item.Key, item.Value);
                    }

                    // if (curFightData.Stage.HeroEquipTalent != null)
                    // {
                    //     foreach (var item in curFightData.Stage.HeroEquipTalent)
                    //     {
                    //         ret.Add(item.Key, item.Value);
                    //     }
                    // }
                    return ret;
                } break;
                case ETalentType.TalentTypeEquip:
                {
                    return curFightData.Stage.EquipTalent;
                } break;
                case ETalentType.TalentTypeEffect:
                {
                    return curFightData.Stage.EffectTalent;
                } break;
                case ETalentType.TalentWeapon:
                {
                    return curFightData.Stage.HeroEquipTalent;
                } break;
            }
            return null;
        }

        public MapField<int, EquipAttr> GetHeroEquips()
        {
            return curFightData.Attr.HeroEquips;
        }

        public int GetHeReviveTimes()
        {
            return curFightData.Stage.HeReviveTimes;
        }
        
        /// <summary>
        /// 添加选中天赋
        /// </summary> int equipModelId, int qeuipId, Action<int> callback
        /// <param name="talentId"></param>
        public void AddChooseTalent(List<int> talentIds, Action<int,int,Action<int>> callback=null)
        {
            if (IsSecretFightData())
            {
                foreach (var talentId in talentIds)
                {
                    AddChooseTalent(talentId, callback);
                }
            }
            else
            {
                foreach (var talentId in talentIds)
                {
                    AddChooseTalent(talentId, callback); 
                }
            }
            NotifyGameUIWeaponRefresh();
        }
        public void AddChooseTalent(int talentId, Action<int,int,Action<int>> callback=null)
        {
            if (IsSecretFightData())
            {
                var secretTalentCfg = ConfigCenter.SecretCopyTalentCfgColl.GetDataById(talentId);
                if (secretTalentCfg == null)
                {
                    DHLog.Error($"错误的配置 请检查 SecretCopyTalentCfg  是否存在 {talentId}");
                    return;
                }

                if (secretTalentCfg.Type == 3)
                {
                    var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(secretTalentCfg.Id);
                    if (talentCfg == null)
                    {
                        DHLog.Error($"错误的配置 请检查 TalentCfg  是否存在 {talentId}");
                        return;
                    }
                    // 加武器
                    AddEquipToSlots(talentCfg.EquipModelId,callback);
                    // 加装备
                    if (!curFightData.Stage.HeroEquipTalent.TryGetValue(talentId, out int count))
                    {
                        count = 0;
                        curFightData.Stage.HeroEquipTalent.Add(talentId, count);
                    }
                    count++;
                    curFightData.Stage.HeroEquipTalent[talentId] = count;
                   
                }
                else
                {
                    var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
                    if (talentCfg.Type == 1) // 战斗天赋
                    {
                        if (!curFightData.Stage.EquipTalent.TryGetValue(talentId, out int count))
                        {
                            count = 0;
                            curFightData.Stage.EquipTalent.Add(talentId, count);
                        }
                        count++;
                        curFightData.Stage.EquipTalent[talentId] = count;
                    }
                    else
                    {
                        if (!curFightData.Stage.EffectTalent.TryGetValue(talentId, out int count))
                        {
                            count = 0;
                            curFightData.Stage.EffectTalent.Add(talentId, count);
                        }
                        count++;
                        curFightData.Stage.EffectTalent[talentId] = count;
                    }
                }
            }
            else
            {
                var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
                if (talentCfg.Type == 1) // 战斗天赋
                {
                    if (!curFightData.Stage.EquipTalent.TryGetValue(talentId, out int count))
                    {
                        count = 0;
                        curFightData.Stage.EquipTalent.Add(talentId, count);
                    }
                    count++;
                    curFightData.Stage.EquipTalent[talentId] = count;
                }
                else
                {
                    if (!curFightData.Stage.EffectTalent.TryGetValue(talentId, out int count))
                    {
                        count = 0;
                        curFightData.Stage.EffectTalent.Add(talentId, count);
                    }
                    count++;
                    curFightData.Stage.EffectTalent[talentId] = count;
                }
            }
            RaisePropertyChanged(nameof(BackpackWeaponList));
        }

        /// <summary>
        /// 根据装备Id获取装备配置
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public EquipCfg GetEquipCfgData(int equipId)
        {
            return ConfigCenter.EquipCfgColl.GetDataById(equipId);
        }
        #region 棋盘合成相关数据逻辑
        /// <summary>
        /// 新增实体格子
        /// </summary>
        /// <param name="occupyCells"></param>
        public void AddSloidBlocks(List<Vector2Int> occupyCells)
        {
            for (var i = 0; i < occupyCells.Count; i++)
            {
                var curRow = occupyCells[i].x;
                var curColumn = occupyCells[i].y;
                if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                GridData[curRow][curColumn] = 1;
            }
        }
        /// <summary>
        /// 根据Key值获取武器棋盘坐标信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Vector2Int? GetGridPosInfoByKey(int key)
        {
            return new Vector2Int(key/100-1,key%100-1);
        }
        /// <summary>
        /// 获取边界区域
        /// </summary>
        /// <returns></returns>
        public void GetAreaBorder()
        {
            var minRow = GMaxRow;
            var maxRow = 0;
            var minColumn = GMaxColumn;
            var maxColumn = 0;
            for (var i = 0; i < GMaxRow; i++) 
            {
                for (var j = 0; j < GMaxColumn; j++)
                {
                    if (GridData[i][j] <= 0) continue;
                    if (i > maxRow) maxRow = i;
                    if (i < minRow) minRow = i;
                    if (j > maxColumn) maxColumn = j;
                    if (j < minColumn) minColumn = j;
                }
            }
            GridBorderRect = new Rect {xMin = minRow+1,xMax = maxRow+1,yMin = minColumn+1,yMax = maxColumn+1};//边界
        }
        /// <summary>
        /// 获取武器数据类型
        /// </summary>
        /// <param name="equipModelId"></param>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public int GetWeaponParmIdByEquipModelIdAndStateId(int equipModelId, int stateId)
        {
            return equipModelId * 10 + stateId;
        }

        public void NotifyGameUIWeaponRefresh()
        {
            RaisePropertyChanged(nameof(BackpackWeaponList));
        }

        /// <summary>
        /// 检测双属性武器合成条件是否解锁
        /// </summary>
        /// <param name="equipId"></param>
        public bool CheckWeaponDoubleAttrUnlocked(int equipId)
        {
            var isUnlocked = false;
            var doubleLevel = 0;//双属性解锁等级
            var curEquipLevel = DataCenter.equipData.GetEquipLevel(equipId);
            var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
            if (equipCfg != null) doubleLevel = equipCfg.DoubleUnlock;
            if (curEquipLevel >= doubleLevel) isUnlocked = true;
            return isUnlocked;
        }
        
        /// <summary>
        /// 根据传入的武器Id+装备Id判断是否为最大等级武器
        /// </summary>
        /// <param name="weaponId"></param>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public bool CheckWeaponMaxLevel(int weaponId,int equipId)
        {
            var isMax = false;
            var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(equipId);
            var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponId);
            if (equipCfg != null && weaponCfg != null && weaponCfg.Class == equipCfg.MaxComposeLv) isMax = true;
            return isMax;
        }
        /// <summary>
        /// 添加武器到背包
        /// </summary>
        public void AddWeaponToBackpack(BackpackWeaponData weaponData)
        {
            var isHave = false;
            for (var i = BackpackWeaponList.Count-1; i >= 0; i--)
            {
                var curweapon = BackpackWeaponList[i];
                if (curweapon.Uid == weaponData.Uid)
                {
                    isHave = true;
                    BackpackWeaponList[i] = weaponData;
                    break;
                }
            }
            if (!isHave) BackpackWeaponList.Add(weaponData);
        }
        /// <summary>
        /// 从背包里移除武器
        /// </summary>
        public void RemoveWeaponFromBackpack(BackpackWeaponData weaponData)
        {
            for (var i = BackpackWeaponList.Count-1; i >= 0; i--)
            {
                var curWeapon = BackpackWeaponList[i];
                if (weaponData.Uid == curWeapon.Uid)
                {
                    BackpackWeaponList.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// 创建背包物品数据
        /// </summary>
        /// <param name="weaponParamId">广告类型(0=不看，1=看)</param>
        /// <param name="weaponAttrType">1~普通武器 2~圣物武器</param>
        /// <param name="localType">位置类型（背包还是随机库里）</param>
        /// <returns></returns>
        public BackpackWeaponData CreateBackpackData(int weaponParamId,int weaponAttrType,ELocationType localType)
        {
            var weaponId = weaponParamId/10;
            var adState = weaponParamId%10;
            var weaponCfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponId);
            var weaponCopyCfg = ConfigCenter.CopyEquipWeightsCfgColl.GetDataById(weaponId);
            var adType = 0;
            var weaponName = "common[common_alpha]";
            if (weaponCfg == null)
            {
                DHLog.Error("当前武器不存在：weponId = "+weaponId);
                return null;
            }
            if (weaponCopyCfg != null) adType = weaponCopyCfg.Adv;
            var weaponData = new BackpackWeaponData();
            weaponData.Uid = GetWeaponUid();
            weaponData.WeaponId = weaponId;
            weaponData.WeaponAttrType = weaponAttrType;//1~普通武器 2~圣物武器
            weaponData.WeaponParamId = weaponParamId;
            weaponData.EquipId = weaponCfg.Equip;
            weaponData.IconPath = weaponCfg.BattlePic;
            weaponData.AdIconPath = weaponName;
            weaponData.HighEffect = "";
            if (weaponCfg.HighEffect != null) weaponData.HighEffect = weaponCfg.HighEffect;
            weaponData.ShapType = (GridType)weaponCfg.GridType;//形状类型
            weaponData.NextInfo = weaponCfg.NextId;//下级武器信息
            weaponData.LocationType = localType;//位置类型（背包还是随机库里）
            weaponData.AdType = adType;//广告类型(0=不看，1=看)
            weaponData.WeaponLev = weaponCfg.Class;//武器等级
            if (adType == 1 && adState == 0)//看广告但是未看过
            {
                weaponData.IsUnlocked = false;
            }
            else
            {
                weaponData.IsUnlocked = true;
            }
            weaponData.IsUnlocked = true;//是否解锁(是否看过广告)
            weaponData.OccupyList = new List<Vector2Int>(35);//占位信息
            var shapInfo = GetWeaponShapInfo((GridType)weaponCfg.GridType);
            CalculationOccupyData((GridType)weaponCfg.GridType,ref weaponData.OccupyList);//计算占位信息
            weaponData.Width = shapInfo.x;
            weaponData.Height = shapInfo.y;
            return weaponData;
        }
        /// <summary>
        /// 创建格子数据
        /// </summary>
        /// <param name="equipCfg"></param>
        /// <param name="adType"></param>
        /// <param name="adIconPath"></param>
        /// <returns></returns>
        public GridAddData CreateGridAddData(EquipModelCfg equipCfg,int adType,string adIconPath)
        {
            var shapInfo = GetWeaponShapInfo((GridType)equipCfg.GridType);
            var gridAddData = new GridAddData();
            gridAddData.Uid = GetWeaponUid();
            gridAddData.Width = shapInfo.x;
            gridAddData.Height = shapInfo.y;
            gridAddData.GridId = equipCfg.Id;
            gridAddData.AdType = adType;
            gridAddData.IconPath = equipCfg.BattlePic;
            gridAddData.AdIconPath = adIconPath;
            gridAddData.IsUnlocked = false;
            gridAddData.ShapType = (GridType)equipCfg.GridType;
            gridAddData.LocationType = ELocationType.Godown;
            gridAddData.OccupyList = new List<Vector2Int>();
            CalculationOccupyData(gridAddData.ShapType,ref gridAddData.OccupyList);
            return gridAddData;
        }
        /// <summary>
        /// 根据武器Uid获取武器数据
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private BackpackWeaponData GetWeaponDataByUid(long uid)
        {
            for (var i = 0; i < BackpackWeaponList.Count; i++)
            {
                if (BackpackWeaponList[i].Uid == uid) return BackpackWeaponList[i];
            }
            return null;
        }
        /// <summary>
        /// 检测是否为属性增益武器
        /// </summary>
        /// <param name="equipId"></param>
        /// <returns></returns>
        public bool CheckIsAttributeGainWeapon(int equipId)
        {
            var isHave = false;
            if (curFightData is { Stage: not null } && equipId == 1 && curFightData.Stage.EquipTalent.ContainsKey(207))//水之剑
            {
                isHave = true; 
            }
            else if (curFightData is { Stage: not null } && equipId == 5)//魔法帽
            {
                if (curFightData.Stage.EquipTalent.ContainsKey(233) || curFightData.Stage.EquipTalent.ContainsKey(235) 
                    || curFightData.Stage.EquipTalent.ContainsKey(239))
                {
                    isHave = true;
                }
            }
            else if (curFightData is { Stage: not null } && equipId == 6)//手套
            {
                if (curFightData.Stage.EquipTalent.ContainsKey(241) || curFightData.Stage.EquipTalent.ContainsKey(244))
                {
                    isHave = true;
                }
            }
            else if (curFightData is { Stage: not null } && equipId == 20)//魔戒
            {
                if (curFightData.Stage.EquipTalent.ContainsKey(309) || curFightData.Stage.EquipTalent.ContainsKey(311) 
                    || curFightData.Stage.EquipTalent.ContainsKey(312))
                {
                    isHave = true;
                }
            }
            return isHave;
        }
        /// <summary>
        /// 对相邻武器属性有影响的天赋:
        /// 107 在水之剑两侧的其他水属性武器击败目标 equipId = 1;equipSkillId = 1207           talentId = 207
        /// 501 背包中与魔法帽相邻的所有魔法武器 equipId = 5; equipSkillId = 5201             talentId = 233 
        /// 503 背包中与魔法帽相邻的所有魔法武器 equipId = 5;equipSkillId = 5203              talentId = 235
        /// 506	所有暗属性的武器伤害增幅额外+32% equipId = 5;equipSkillId = 5206              talentId = 238
        /// 507 与地之魔法帽在同一行的防具都可获得叠盾效果增幅 equipId = 5;equipSkillId = 5207   talentId = 239
        /// 509	地之魔法帽	在地之魔法帽左右位置的防具叠盾效果+22%
        /// 510	暗之魔法帽	暗之魔法帽不再回血,并每次扣除当前血量2.5%,在暗之魔法帽周围的所有武器伤害+52%
        /// 601 背包中与手套相邻的所有物理武器 equipId = 6;equipSkillId = 6201                talentId = 241 
        /// 604 背包中与手套相邻的所有物理武器 equipId = 6;equipSkillId = 6204                talentId = 244
        /// 2001 魔法戒指周围的武器暴击率提升 equipId = 20;equipSkillId = 20201               talentId = 309
        /// 2003 魔法戒指对同行同列的武器均提升暴击率 equipId = 20;equipSkillId = 20203         talentId = 311
        /// 2004 魔法戒指对背包内所有武器均提升暴击率 equipId = 20;equipSkillId = 20204         talentId = 312
        /// </summary>
        public List<long> GetAttributeGainWeaponList(BackpackWeaponData weaponData,List<Vector2Int> occupyInfo)
        {
            var retUidList = new List<long>(35);
            var retList = new List<BackpackWeaponData>(35);
            var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(weaponData.WeaponId);
            var equipAttrType = cfg.AttrType;
            switch (weaponData.EquipId)
            {
                case 1://水之剑两侧的其他水属性武器击败目标
                    retList = GetWeaponSideListByOccupyInfo(weaponData.ShapType, occupyInfo);//获取武器两侧武器
                    for (var i = retList.Count-1; i >= 0; i--)
                    {
                        var tmpCfg = ConfigCenter.EquipModelCfgColl.GetDataById(retList[i].WeaponId);
                        if (tmpCfg?.AttrType == equipAttrType) continue;
                        retList.RemoveAt(i);
                    }
                    break;
                case 5:
                    if (curFightData is { Stage: not null } && curFightData.Stage.EquipTalent.ContainsKey(239)) //与地之魔法帽在同一行的防具
                    {
                        retList = GetWeaponSameRowByOccupyInfo(weaponData.ShapType, occupyInfo);
                        for (var i = retList.Count-1; i >= 0; i--)
                        {
                            var tmpCfg = GetEquipCfgData(retList[i].EquipId);
                            if (tmpCfg?.AtkType == (int)EquipAtkType.Defender) continue;
                            retList.RemoveAt(i);
                        }
                        if (curFightData is { Stage: not null } && (curFightData.Stage.EquipTalent.ContainsKey(233)
                             || curFightData.Stage.EquipTalent.ContainsKey(235))) //背包中与魔法帽相邻的所有魔法武器
                        {
                            var tmpRetList = GetWeaponTopAndDownByOccupyInfo(weaponData.ShapType, occupyInfo);
                            for (var i = tmpRetList.Count-1; i >= 0; i--)
                            {
                                var tmpCfg = GetEquipCfgData(tmpRetList[i].EquipId);
                                if (tmpCfg?.AtkType == (int)EquipAtkType.Magic)
                                {
                                    retList.Add(tmpRetList[i]);
                                }
                            }
                        }
                    }
                    else if (curFightData is { Stage: not null } && (curFightData.Stage.EquipTalent.ContainsKey(233)
                                 ||curFightData.Stage.EquipTalent.ContainsKey(235)))//背包中与魔法帽相邻的所有魔法武器
                    {
                        retList = GetWeaponNearbyByOccupyInfo(weaponData.ShapType, occupyInfo);
                        for (var i = retList.Count-1; i >= 0; i--)
                        {
                            var tmpCfg = ConfigCenter.EquipCfgColl.GetDataById(retList[i].EquipId);
                            if (tmpCfg?.AtkType == (int)EquipAtkType.Magic) continue;
                            retList.RemoveAt(i);
                        }
                    }
                    break;
                case 6://背包中与手套相邻的所有物理武器
                    if (curFightData is { Stage: not null } && (curFightData.Stage.EquipTalent.ContainsKey(241) ||curFightData.Stage.EquipTalent.ContainsKey(244)))
                    {
                        retList = GetWeaponNearbyByOccupyInfo(weaponData.ShapType, occupyInfo);
                        for (var i = retList.Count-1; i >= 0; i--)
                        {
                            var tmpCfg = ConfigCenter.EquipCfgColl.GetDataById(retList[i].EquipId);
                            if (tmpCfg?.AtkType == (int)EquipAtkType.Physic) continue;
                            retList.RemoveAt(i);
                        }
                    }
                    break;
                case 20://背包中与魔戒相邻/同行同列/所有
                    if (curFightData is { Stage: not null } && curFightData.Stage.EquipTalent.ContainsKey(312))//魔戒对背包内所有武器
                    {
                        retList = BackpackWeaponList.ToList();
                    }
                    else
                    {
                        if (curFightData is { Stage: not null } && curFightData.Stage.EquipTalent.ContainsKey(309))//魔戒周围的武器
                        {
                            retList = GetWeaponNearbyByOccupyInfo(weaponData.ShapType, occupyInfo);
                        } 
                        if (curFightData is { Stage: not null } && curFightData.Stage.EquipTalent.ContainsKey(311))//魔戒同行同列的武器
                        {
                            var tmpRetList = GetWeaponSameRowAndColumnList(weaponData.ShapType, occupyInfo);
                            foreach (var itemInfo in tmpRetList)
                            {
                                retList.Add(itemInfo);
                            }
                        }
                    }
                    for (var i = retList.Count-1; i >= 0; i--)
                    {
                        var tmpCfg = ConfigCenter.EquipCfgColl.GetDataById(retList[i].EquipId);
                        if (tmpCfg?.AtkType != (int)EquipAtkType.Physic &&
                            tmpCfg?.AtkType != (int)EquipAtkType.Magic)
                        {
                            retList.RemoveAt(i);
                        }
                    }
                    break;
            }
            for (var i = 0; i < retList.Count; i++)
            {
                retUidList.Add(retList[i].Uid);
            }
            return retUidList;
        }
        /// <summary>
        /// 根据占位信息获取一个武器两侧的武器列表 1、左右边界都不超框 2、左超框 3、右超框 4、武器中空的情况需要考虑
        /// </summary>
        /// <param name="shapType"></param>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponSideListByOccupyInfo(GridType shapType,List<Vector2Int> occupyInfo)
        {
            var retList = new List<BackpackWeaponData>();
            var curRectInfo = GetWeaponRectInfo(occupyInfo);
            curRectInfo.yMax += 1;
            curRectInfo.yMin -= 1;
            var curList = ListPool<Vector2Int>.Get();
            //1、左右边界都不超框 2、左超框 3、右超框 
            for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
            {
                if (curRectInfo.yMin >= 0)
                {
                    var leftPos = new Vector2Int{ x = (int)i,y = (int)curRectInfo.yMin};
                    curList.Add(leftPos);
                }
                if (!(curRectInfo.yMax < GMaxColumn)) continue;
                var rightPos = new Vector2Int{ x = (int)i,y = (int)curRectInfo.yMax};
                curList.Add(rightPos);
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(shapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,occupyInfo[0].x,occupyInfo[0].y,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x + occupyInfo[0].x;
                    var curColumn = occupyInfoList[i].y + occupyInfo[0].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow ||curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] <= 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            return retList;
        }
        /// <summary>
        /// 根据占位信息获取武器的上下相邻武器数据
        /// 1、上下边界都不超框 2、上超框 3、下超框 //4、武器中空的情况需要考虑
        /// </summary>
        /// <param name="shapType"></param>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponTopAndDownByOccupyInfo(GridType shapType,List<Vector2Int> occupyInfo)
        {
            var retList = new List<BackpackWeaponData>();
            var curRectInfo = GetWeaponRectInfo(occupyInfo);
            curRectInfo.xMax += 1;
            curRectInfo.xMin -= 1;
            var curList = new List<Vector2Int>(35);
            //1、上下边界都不超框 2、上超框 3、下超框 
            for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
            {
                if (curRectInfo.xMin >= 0)
                {
                    var topPos = new Vector2Int{ x = (int)curRectInfo.xMin,y = (int)i};
                    curList.Add(topPos);
                }
                if (curRectInfo.xMax < GMaxRow)
                {
                    var downPos = new Vector2Int{ x = (int)curRectInfo.xMax,y = (int)i};
                    curList.Add(downPos);
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(shapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,occupyInfo[0].x,occupyInfo[0].y,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+occupyInfo[0].x;
                    var curColumn = occupyInfoList[i].y+occupyInfo[0].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] <= 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x + curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y + curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            return retList;
        }
        /// <summary>
        /// 根据占位信息获取一个武器相邻的武器列表(上下左右)
        /// </summary>
        /// <param name="shapType"></param>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponNearbyByOccupyInfo(GridType shapType,List<Vector2Int> occupyInfo)
        {
            var retList = new List<BackpackWeaponData>();
            var leftToRightList = GetWeaponSideListByOccupyInfo(shapType, occupyInfo);
            var topToBottomList = GetWeaponTopAndDownByOccupyInfo(shapType, occupyInfo);
            if (leftToRightList.Count > 0)
            {
                for (var i = 0; i < leftToRightList.Count; i++)
                {
                    retList.Add(leftToRightList[i]);
                }
            }
            if (topToBottomList.Count > 0)
            {
                for (var i = 0; i < topToBottomList.Count; i++)
                {
                    retList.Add(topToBottomList[i]);
                }
            }
            return retList;
        }
        /// <summary>
        /// 根据占位信息获取一个武器同行的武器列表
        /// </summary>
        /// <param name="shapType"></param>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponSameRowByOccupyInfo(GridType shapType,List<Vector2Int> occupyInfo)
        {
            var retList = new List<BackpackWeaponData>();
            var curRectInfo = GetWeaponRectInfo(occupyInfo);
            var curList = ListPool<Vector2Int>.Get();
            //1、左右边界都不超框 2、左超框 3、右超框 
            curRectInfo.yMax += 1;
            curRectInfo.yMin -= 1;
            if (curRectInfo.yMin >= 0)//左边区域格子存起来
            {
                for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
                {
                    for (var j = 0; j <= curRectInfo.yMin; j++)
                    {
                        var leftPos = new Vector2Int{ x = (int)i,y = j};
                        curList.Add(leftPos);
                    }
                }
            }
            if (curRectInfo.yMax < GMaxColumn)//右边区域格子存起来
            {
                for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
                {
                    for (var j = curRectInfo.yMax; j < GMaxColumn; j++)
                    {
                        var rightPos = new Vector2Int{ x = (int)i,y = (int)j};
                        curList.Add(rightPos);
                    }
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(shapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,occupyInfo[0].x,occupyInfo[0].y,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+occupyInfo[0].x;
                    var curColumn = occupyInfoList[i].y+occupyInfo[0].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] == 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            return retList;
        }
        /// <summary>
        /// 根据占位信息获取一个武器同行同列的武器列表
        /// </summary>
        /// <param name="shapType"></param>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponSameRowAndColumnList(GridType shapType,List<Vector2Int> occupyInfo)
        {
            var retList = new List<BackpackWeaponData>();
            var curRectInfo = GetWeaponRectInfo(occupyInfo);
            var weaponUidList = ListPool<long>.Get();
            var curList = ListPool<Vector2Int>.Get();
            //1、上下边界都不超框 2、上超框 3、下超框 
            curRectInfo.xMax += 1;
            curRectInfo.xMin -= 1;
            if (curRectInfo.xMin >= 0)//上边区域格子存起来
            {
                for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
                {
                    for (var j = 0; j <= curRectInfo.xMin; j++)
                    {
                        var upPos = new Vector2Int{ x = j,y = (int)i};
                        curList.Add(upPos);
                    }
                }
            }
            if (curRectInfo.xMax < GMaxRow)//下边区域格子存起来
            {
                for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
                {
                    for (var j = curRectInfo.xMax; j < GMaxRow; j++)
                    {
                        var downPos = new Vector2Int{ x = (int)j,y = (int)i};
                        curList.Add(downPos);
                    }
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(shapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,occupyInfo[0].x,occupyInfo[0].y,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+occupyInfo[0].x;
                    var curColumn = occupyInfoList[i].y+occupyInfo[0].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] == 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            var tmpRetList = GetWeaponSameRowByOccupyInfo(shapType, occupyInfo);
            for (var i = tmpRetList.Count-1; i >= 0; i--)
            {
                if (weaponUidList.Contains(tmpRetList[i].Uid)) continue;
                retList.Add(tmpRetList[i]);
                weaponUidList.Add(tmpRetList[i].Uid);
            }
            ListPool<long>.Release(weaponUidList);
            return retList;
        }
        /// <summary>
        /// 获取一个武器两侧的 武器列表 1、左右边界都不超框 2、左超框 3、右超框 4、武器中空的情况需要考虑
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        public List<BackpackWeaponData> GetWeaponSideList(long uid, List<BackpackWeaponData> retList)
        {
            var weaponData = GetWeaponDataByUid(uid);
            if (weaponData == null) return retList;
            var curRectInfo = GetWeaponRectInfo(weaponData);
            curRectInfo.yMax += 1;
            curRectInfo.yMin -= 1;
            var curList = ListPool<Vector2Int>.Get();
            //1、左右边界都不超框 2、左超框 3、右超框 
            for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
            {
                if (curRectInfo.yMin >= 0)
                {
                    var leftPos = new Vector2Int{ x = (int)i,y = (int)curRectInfo.yMin};
                    curList.Add(leftPos);
                }
                if (curRectInfo.yMax < GMaxColumn)
                {
                    var rightPos = new Vector2Int{ x = (int)i,y = (int)curRectInfo.yMax};
                    curList.Add(rightPos);
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(weaponData.ShapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,weaponData.RowIdx,weaponData.ColumnIdx,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x + weaponData.RowIdx;
                    var curColumn = occupyInfoList[i].y + weaponData.ColumnIdx;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow ||curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] <= 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            return retList;
        }
        /// <summary>
        /// 获取一个武器相邻的武器列表(上下左右)
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        public List<BackpackWeaponData> GetWeaponNearbyList(long uid, List<BackpackWeaponData> retList)
        {
            var weaponData = GetWeaponDataByUid(uid);
            if (weaponData == null) return retList;
            var weaponUidList = ListPool<long>.Get();
            var leftAndRightWeaponList = ListPool<BackpackWeaponData>.Get();
            GetWeaponSideList(uid, leftAndRightWeaponList);
            var topAndDownWeaponList = GetWeaponTopAndDownbyUid(weaponData);
            if (leftAndRightWeaponList.Count > 0)
            {
                for (var i = 0; i < leftAndRightWeaponList.Count; i++)
                {
                    if (weaponUidList.Contains(leftAndRightWeaponList[i].Uid)) continue;
                    retList.Add(leftAndRightWeaponList[i]);
                    weaponUidList.Add(leftAndRightWeaponList[i].Uid);
                }
            }
            if (topAndDownWeaponList.Count > 0)
            {
                for (var i = 0; i < topAndDownWeaponList.Count; i++)
                {
                    if (weaponUidList.Contains(topAndDownWeaponList[i].Uid)) continue;
                    retList.Add(topAndDownWeaponList[i]);
                    weaponUidList.Add(topAndDownWeaponList[i].Uid);
                }
            }
            ListPool<long>.Release(weaponUidList);
            ListPool<BackpackWeaponData>.Release(leftAndRightWeaponList);
            return retList;
        }
        /// <summary>
        /// 获取一个武器同行同列的武器列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        public List<BackpackWeaponData> GetWeaponSameRowAndColumnList(long uid, List<BackpackWeaponData> retList)
        {
            var weaponData = GetWeaponDataByUid(uid);
            if (weaponData == null) return retList;
            var weaponUidList = ListPool<long>.Get();
            var sameRowWeaponList = ListPool<BackpackWeaponData>.Get();
            var sameColumnWeaponList = ListPool<BackpackWeaponData>.Get();
            GetWeaponSameRowList(uid, sameRowWeaponList);//同行武器
            GetWeaponSameColumnList(uid, sameColumnWeaponList);//同列武器
            if (sameRowWeaponList.Count > 0)
            {
                for (var i = 0; i < sameRowWeaponList.Count; i++)
                {
                    retList.Add(sameRowWeaponList[i]);
                }
            }
            if (sameColumnWeaponList.Count > 0)
            {
                for (var i = 0; i < sameColumnWeaponList.Count; i++)
                {
                    if (weaponUidList.Contains(sameColumnWeaponList[i].Uid)) continue;
                    retList.Add(sameColumnWeaponList[i]);
                    weaponUidList.Add(sameColumnWeaponList[i].Uid);
                }
            }
            ListPool<long>.Release(weaponUidList);
            ListPool<BackpackWeaponData>.Release(sameRowWeaponList);
            ListPool<BackpackWeaponData>.Release(sameColumnWeaponList);
            return retList;
        }
        /// <summary>
        /// 获取一个武器同行的武器列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        public List<BackpackWeaponData> GetWeaponSameRowList(long uid, List<BackpackWeaponData> retList)
        {
            var weaponData = GetWeaponDataByUid(uid);
            if (weaponData == null) return retList;
            var curRectInfo = GetWeaponRectInfo(weaponData);
            var curList = ListPool<Vector2Int>.Get();
            //1、左右边界都不超框 2、左超框 3、右超框 
            curRectInfo.yMax += 1;
            curRectInfo.yMin -= 1;
            if (curRectInfo.yMin >= 0)//左边区域格子存起来
            {
                for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
                {
                    for (var j = 0; j <= curRectInfo.yMin; j++)
                    {
                        var leftPos = new Vector2Int{ x = (int)i,y = j};
                        curList.Add(leftPos);
                    }
                }
            }
            if (curRectInfo.yMax < GMaxColumn)//右边区域格子存起来
            {
                for (var i = curRectInfo.xMin; i <= curRectInfo.xMax; i++)
                {
                    for (var j = curRectInfo.yMax; j < GMaxColumn; j++)
                    {
                        var rightPos = new Vector2Int{ x = (int)i,y = (int)j};
                        curList.Add(rightPos);
                    }
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(weaponData.ShapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,weaponData.RowIdx,weaponData.ColumnIdx,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+weaponData.RowIdx;
                    var curColumn = occupyInfoList[i].y+weaponData.ColumnIdx;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] == 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            return retList;
        }
        /// <summary>
        /// 获取一个武器同列的武器列表
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        public List<BackpackWeaponData> GetWeaponSameColumnList(long uid, List<BackpackWeaponData> retList)
        {
            var weaponData = GetWeaponDataByUid(uid);
            if (weaponData == null) return retList;
            var curRectInfo = GetWeaponRectInfo(weaponData);
            var curList = ListPool<Vector2Int>.Get();
            //1、上下边界都不超框 2、上超框 3、下超框 
            curRectInfo.xMax += 1;
            curRectInfo.xMin -= 1;
            if (curRectInfo.xMin >= 0)//上边区域格子存起来
            {
                for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
                {
                    for (var j = 0; j <= curRectInfo.xMin; j++)
                    {
                        var upPos = new Vector2Int{ x = j,y = (int)i};
                        curList.Add(upPos);
                    }
                }
            }
            if (curRectInfo.xMax < GMaxRow)//下边区域格子存起来
            {
                for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
                {
                    for (var j = curRectInfo.xMax; j < GMaxRow; j++)
                    {
                        var downPos = new Vector2Int{ x = (int)j,y = (int)i};
                        curList.Add(downPos);
                    }
                }
            }
            
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(weaponData.ShapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,weaponData.RowIdx,weaponData.ColumnIdx,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+weaponData.RowIdx;
                    var curColumn = occupyInfoList[i].y+weaponData.ColumnIdx;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] == 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x+curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y+curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            ListPool<Vector2Int>.Release(curList);
            return retList;
        }
        /// <summary>
        /// 获取武器的上下相邻武器数据
        /// 1、上下边界都不超框 2、上超框 3、下超框 4、武器中空的情况需要考虑
        /// </summary>
        /// <param name="weaponData"></param>
        /// <returns></returns>
        private List<BackpackWeaponData> GetWeaponTopAndDownbyUid(BackpackWeaponData weaponData)
        {
            var retList = new List<BackpackWeaponData>();
            if (weaponData == null) return retList;
            var curRectInfo = GetWeaponRectInfo(weaponData);
            curRectInfo.xMax += 1;
            curRectInfo.xMin -= 1;
            var curList = new List<Vector2Int>(35);
            //1、上下边界都不超框 2、上超框 3、下超框 
            for (var i = curRectInfo.yMin; i <= curRectInfo.yMax; i++)
            {
                if (curRectInfo.xMin >= 0)
                {
                    var topPos = new Vector2Int{ x = (int)curRectInfo.xMin,y = (int)i};
                    curList.Add(topPos);
                }
                if (curRectInfo.xMax < GMaxRow)
                {
                    var downPos = new Vector2Int{ x = (int)curRectInfo.xMax,y = (int)i};
                    curList.Add(downPos);
                }
            }
            //4、武器中空的情况需要考虑
            var occupyInfoList = GetHollowOccupyInfoByShap(weaponData.ShapType);
            if (occupyInfoList.Count > 0)
            {
                RemoveHollowOccupyAdjoin(occupyInfoList,weaponData.RowIdx,weaponData.ColumnIdx,curList);
                for (var i = 0; i < occupyInfoList.Count; i++) 
                {
                    var curRow = occupyInfoList[i].x+weaponData.RowIdx;
                    var curColumn = occupyInfoList[i].y+weaponData.ColumnIdx;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    var curPos = new Vector2Int{ x = curRow,y = curColumn};
                    curList.Add(curPos);
                }
            }
            for (var i = 0; i < curList.Count; i++)
            {
                var curPos = curList[i];
                if (curPos.x < 0 || curPos.x >= GMaxRow || curPos.y < 0 || curPos.y >= GMaxColumn) continue;
                if (GridData[curPos.x][curPos.y] <= 0) continue;
                for (var j = 0; j < BackpackWeaponList.Count; j++)
                {
                    var isHave = false;
                    var curWeaponData = BackpackWeaponList[j];
                    for (var k = 0; k < curWeaponData.OccupyList.Count; k++)
                    {
                        var curRow = curWeaponData.OccupyList[k].x + curWeaponData.RowIdx;
                        var curColumn = curWeaponData.OccupyList[k].y + curWeaponData.ColumnIdx;
                        if (curPos.x == curRow && curPos.y == curColumn)
                        {
                            isHave = true;
                            break;
                        }
                    }
                    if (isHave) retList.Add(curWeaponData);
                }
            }
            return retList;
        }
        /// <summary>
        /// 去除中空相邻的数据
        /// </summary>
        /// <param name="occupyInfoList"></param>
        /// <param name="rowIdx"></param>
        /// <param name="columnIdx"></param>
        /// <param name="retList"></param>
        /// <returns></returns>
        private List<Vector2Int> RemoveHollowOccupyAdjoin(List<Vector2Int> occupyInfoList,int rowIdx,int columnIdx,List<Vector2Int> retList)
        {
            var sideList = new List<Vector2Int>(4){new Vector2Int(0,1),new Vector2Int(0,-1),new Vector2Int(1,0),new Vector2Int(-1,0)};
            for (var i = 0; i < occupyInfoList.Count; i++) 
            {
                for (var j = 0; j < sideList.Count; j++)
                {
                    var curRow = occupyInfoList[i].x+rowIdx+sideList[j].x;
                    var curColumn = occupyInfoList[i].y+columnIdx+sideList[j].y;
                    if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                    for (var k = retList.Count-1; k >= 0; k--)
                    {
                        if (retList[k].x == curRow && retList[k].y == curColumn) retList.RemoveAt(k);
                    }
                }
            }
            return retList;
        }
        /// <summary>
        /// 获取武器的最小/最大行列数据
        /// </summary>
        /// <param name="weaponData"></param>
        /// <returns></returns>
        private Rect GetWeaponRectInfo(BackpackWeaponData weaponData)
        {
            var rectData = new Rect{ xMin = 0,xMax = 0,yMin = 0,yMax = 0};
            if (weaponData == null) return rectData;
            var minRow = weaponData.RowIdx;
            var maxRow = 0;
            var minColumn = weaponData.ColumnIdx;
            var maxColumn = 0;
            for (var i = 0; i < weaponData.OccupyList.Count; i++) 
            {
                var curRow = weaponData.OccupyList[i].x+weaponData.RowIdx;
                var curColumn = weaponData.OccupyList[i].y+weaponData.ColumnIdx;
                if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                if (curRow > maxRow) maxRow = curRow;
                if (curRow < minRow) minRow = curRow;
                if (curColumn > maxColumn) maxColumn = curColumn;
                if (curColumn < minColumn) minColumn = curColumn;
            }
            return new Rect{ xMin = minRow,yMin = minColumn,xMax = maxRow,yMax = maxColumn};
        }
        /// <summary>
        /// 获取武器的最小/最大行列数据
        /// </summary>
        /// <param name="occupyInfo"></param>
        /// <returns></returns>
        private Rect GetWeaponRectInfo(List<Vector2Int> occupyInfo)
        {
            var rectData = new Rect{ xMin = 0,xMax = 0,yMin = 0,yMax = 0};
            if (occupyInfo == null) return rectData;
            var minRow = GMaxRow;
            var maxRow = 0;
            var minColumn = GMaxColumn;
            var maxColumn = 0;
            for (var i = occupyInfo.Count - 1; i >= 0; i--) 
            {
                var curRow = occupyInfo[i].x;
                var curColumn = occupyInfo[i].y;
                if (curRow < 0 || curRow >= GMaxRow || curColumn < 0 || curColumn >= GMaxColumn) continue;
                if (curRow > maxRow) maxRow = curRow;
                if (curRow < minRow) minRow = curRow;
                if (curColumn > maxColumn) maxColumn = curColumn;
                if (curColumn < minColumn) minColumn = curColumn;
            }
            return new Rect{ xMin = minRow,yMin = minColumn,xMax = maxRow,yMax = maxColumn};
        }
        /// <summary>
        /// 获取特殊形状的占位信息 类型 GridType.LzThreeNum
        /// </summary>
        /// <returns></returns>
        public List<Vector2Int> GetSpecialOccupyList()
        {
            var occupyList = new List<Vector2Int>(3);
            var grid1 = new Vector2Int{ x = 0,y = 1};
            var grid2 = new Vector2Int{ x = 1,y = 0};
            var grid3 = new Vector2Int{ x = 1,y = 1};
            occupyList.Add(grid1);
            occupyList.Add(grid2);
            occupyList.Add(grid3);
            return occupyList;
        }
        /// <summary>
        /// 检测物理武器属性是否被增益
        /// 601 背包中与手套相邻的所有物理武器 equipId = 6;equipSkillId = 6201                talentId = 241 
        /// 604 背包中与手套相邻的所有物理武器 equipId = 6;equipSkillId = 6204                talentId = 244
        /// 2001 魔法戒指周围的武器暴击率提升 equipId = 20;equipSkillId = 20201               talentId = 309
        /// 2003 魔法戒指对同行同列的武器均提升暴击率 equipId = 20;equipSkillId = 20203         talentId = 311
        /// 2004 魔法戒指对背包内所有武器均提升暴击率 equipId = 20;equipSkillId = 20204         talentId = 312
        /// </summary>
        /// <param name="weaponModelId"></param>
        public bool CheckPhyWeaponAttrAffected(int weaponModelId)
        {
            if (curFightData?.Stage == null) return false;
            if (curFightData?.Stage.EquipTalent.Count == 0) return false;
            var isAffected = false;
            var wList = ListPool<BackpackWeaponData>.Get();
            var curNearUidList = ListPool<long>.Get();
            var curSameRowAndColumnList = ListPool<long>.Get();
            foreach (var weaponItem in BackpackWeaponList)
            {
                if (curFightData is { Stage: {EquipTalent:{Count: > 0}}})
                {
                    if (weaponItem.EquipId == 6 && (curFightData.Stage.EquipTalent.ContainsKey(241)||curFightData.Stage.EquipTalent.ContainsKey(244)))//手套相邻的所有物理武器
                    {
                        curNearUidList.Add(weaponItem.Uid);
                    } else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(312))//魔戒
                    {
                        isAffected = true;
                        break;
                    } else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(309))
                    {
                        curNearUidList.Add(weaponItem.Uid);
                    }
                    else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(309))
                    {
                        curSameRowAndColumnList.Add(weaponItem.Uid);
                    }
                }
            }
            if (isAffected)
            {
                ListPool<BackpackWeaponData>.Release(wList);
                ListPool<long>.Release(curNearUidList);
                ListPool<long>.Release(curSameRowAndColumnList);
                return true;
            }
            if (curNearUidList.Count > 0)
            {
                foreach (var weaponId in curNearUidList)
                {
                    GetWeaponNearbyList(weaponId, wList);//相邻武器
                }
            }
            if (curSameRowAndColumnList.Count > 0)
            {
                foreach (var weaponId in curSameRowAndColumnList)
                {
                    GetWeaponSameRowAndColumnList(weaponId, wList);//相邻武器
                }
            }
            if (wList.Count > 0)
            {
                foreach (var weaponInfo in wList)
                {
                    if (weaponInfo.WeaponId != weaponModelId) continue;
                    isAffected = true;
                    break;
                }
            }
            ListPool<BackpackWeaponData>.Release(wList);
            ListPool<long>.Release(curNearUidList);
            ListPool<long>.Release(curSameRowAndColumnList);
            return isAffected;
        }
        /// <summary>
        /// 检测魔法武器属性是否被增益
        /// 501 背包中与魔法帽相邻的所有魔法武器 equipId = 5; equipSkillId = 5201             talentId = 233 
        /// 503 背包中与魔法帽相邻的所有魔法武器 equipId = 5;equipSkillId = 5203              talentId = 235
        /// 2001 魔法戒指周围的武器暴击率提升 equipId = 20;equipSkillId = 20201               talentId = 309
        /// 2003 魔法戒指对同行同列的武器均提升暴击率 equipId = 20;equipSkillId = 20203         talentId = 311
        /// 2004 魔法戒指对背包内所有武器均提升暴击率 equipId = 20;equipSkillId = 20204         talentId = 312
        /// </summary>
        /// <param name="weaponModelId"></param>
        public bool CheckMagicWeaponAttrAffected(int weaponModelId)
        {
            if (curFightData?.Stage == null) return false;
            if (curFightData?.Stage.EquipTalent.Count == 0) return false;
            var isAffected = false;
            var wList = ListPool<BackpackWeaponData>.Get();
            var curNearUidList = ListPool<long>.Get();
            var curSameRowAndColumnList = ListPool<long>.Get();
            foreach (var weaponItem in BackpackWeaponList)
            {
                if (curFightData is { Stage: {EquipTalent:{Count: > 0}}})
                {
                    if (weaponItem.EquipId == 5 && (curFightData.Stage.EquipTalent.ContainsKey(233)||curFightData.Stage.EquipTalent.ContainsKey(235)))//手套相邻的所有物理武器
                    {
                        curNearUidList.Add(weaponItem.Uid);
                    } else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(312))//魔戒
                    {
                        isAffected = true;
                        break;
                    } else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(309))
                    {
                        curNearUidList.Add(weaponItem.Uid);
                    }
                    else if (weaponItem.EquipId == 20 && curFightData.Stage.EquipTalent.ContainsKey(309))
                    {
                        curSameRowAndColumnList.Add(weaponItem.Uid);
                    }
                }
            }
            if (isAffected)
            {
                ListPool<BackpackWeaponData>.Release(wList);
                ListPool<long>.Release(curNearUidList);
                ListPool<long>.Release(curSameRowAndColumnList);
                return true;
            }
            if (curNearUidList.Count > 0)
            {
                foreach (var weaponId in curNearUidList)
                {
                    GetWeaponNearbyList(weaponId, wList);//相邻武器
                }
            }
            if (curSameRowAndColumnList.Count > 0)
            {
                foreach (var weaponId in curSameRowAndColumnList)
                {
                    GetWeaponSameRowAndColumnList(weaponId, wList);//相邻武器
                }
            }
            if (wList.Count > 0)
            {
                foreach (var weaponInfo in wList)
                {
                    if (weaponInfo.WeaponId != weaponModelId) continue;
                    isAffected = true;
                    break;
                }
            }
            ListPool<BackpackWeaponData>.Release(wList);
            ListPool<long>.Release(curNearUidList);
            ListPool<long>.Release(curSameRowAndColumnList);
            return isAffected;
        }
        /// <summary>
        /// 根据武器形状获取武器的中空占位信息
        /// </summary>
        /// <returns></returns>
        private List<Vector2Int> GetHollowOccupyInfoByShap(GridType gridType)
        {
            var occupyInfoList = new List<Vector2Int>();
            switch (gridType)
            {
                case GridType.LtThreeNum: //L型3格（向下）sizeInfo = new Vector2Int(2,2);
                {
                    var grid1 = new Vector2Int{ x = 1,y = 1};
                    occupyInfoList.Add(grid1);
                } break;
                case GridType.LzThreeNum: //L型3格（向上）sizeInfo = new Vector2Int(2,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    occupyInfoList.Add(grid1);
                } break;
                case GridType.DFourNum://丁字4格 sizeInfo = new Vector2Int(3,2);
                {
                    var grid1 = new Vector2Int{ x = 1,y = 0};
                    var grid2 = new Vector2Int{ x = 1,y = 2};
                    occupyInfoList.Add(grid1);
                    occupyInfoList.Add(grid2);
                } break; 
                case GridType.LtFourNum://L型4格 sizeInfo = new Vector2Int(2,3);
                {
                    var grid1 = new Vector2Int{ x = 1,y = 1};
                    var grid2 = new Vector2Int{ x = 2,y = 1};
                    occupyInfoList.Add(grid1);
                    occupyInfoList.Add(grid2);
                } break;
            }
            return occupyInfoList;
        }
        /// <summary>
        /// 根据形状类型获取当前武器的占格子宽高
        /// </summary>
        /// <param name="gridType"></param>
        /// <returns></returns>
        private Vector2Int GetWeaponShapInfo(GridType gridType)
        {
            var sizeInfo = Vector2Int.one;
            switch (gridType)
            {
                case GridType.LtThreeNum: //L型3格（向下）
                case GridType.LzThreeNum: //L型3格（向上）
                case GridType.SFourNum://4方格(220,220)
                {
                    sizeInfo = new Vector2Int(2,2);
                } break; 
                case GridType.LTwoNum://横排2格(220,110)
                {
                    sizeInfo = new Vector2Int(2,1);
                } break; 
                case GridType.HTwoNum://竖排2格(110,220)
                {
                    sizeInfo = new Vector2Int(1,2);
                } break; 
                case GridType.LThreeNum://横排3格(330,110)
                {
                    sizeInfo = new Vector2Int(3,1);
                } break; 
                case GridType.HThreeNum://竖排3格(110,330)
                {
                    sizeInfo = new Vector2Int(1,3);
                } break; 
                case GridType.DFourNum://丁字4格(330,220)
                {
                    sizeInfo = new Vector2Int(3,2);
                } break; 
                case GridType.LtFourNum://L型4格(220,330)
                {
                    sizeInfo = new Vector2Int(2,3);
                } break;
                case GridType.OneNum:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gridType), gridType, null);
            }
            return sizeInfo;
        }
        /// <summary>
        /// 根据形状计算武器的占位信息
        /// </summary>
        /// <param name="gridType"></param>
        /// <param name="occupyList"></param>
        private void CalculationOccupyData(GridType gridType,ref List<Vector2Int> occupyList)
        {
            switch (gridType)
            {
                case GridType.LTwoNum://横排2格 sizeInfo = new Vector2Int(2,1);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                } break; 
                case GridType.HTwoNum://竖排2格 sizeInfo = new Vector2Int(1,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 1,y = 0};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                } break; 
                case GridType.LThreeNum://横排3格 sizeInfo = new Vector2Int(3,1);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    var grid3 = new Vector2Int{ x = 0,y = 2};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                } break; 
                case GridType.HThreeNum://竖排3格 sizeInfo = new Vector2Int(1,3);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 1,y = 0};
                    var grid3 = new Vector2Int{ x = 2,y = 0};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                } break; 
                case GridType.LtThreeNum: //L型3格（向下）sizeInfo = new Vector2Int(2,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    var grid3 = new Vector2Int{ x = 1,y = 0};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                } break;
                case GridType.LzThreeNum: //L型3格（向上）sizeInfo = new Vector2Int(2,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 1,y = -1};
                    var grid3 = new Vector2Int{ x = 1,y = 0};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                } break;
                case GridType.DFourNum://丁字4格 sizeInfo = new Vector2Int(3,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    var grid3 = new Vector2Int{ x = 0,y = 2};
                    var grid4 = new Vector2Int{ x = 1,y = 1};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                    occupyList.Add(grid4);
                } break; 
                case GridType.LtFourNum://L型4格 sizeInfo = new Vector2Int(2,3);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    var grid3 = new Vector2Int{ x = 1,y = 0};
                    var grid4 = new Vector2Int{ x = 2,y = 0};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                    occupyList.Add(grid4);
                } break;
                case GridType.SFourNum://4方格 sizeInfo = new Vector2Int(2,2);
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    var grid2 = new Vector2Int{ x = 0,y = 1};
                    var grid3 = new Vector2Int{ x = 1,y = 0};
                    var grid4 = new Vector2Int{ x = 1,y = 1};
                    occupyList.Add(grid1);
                    occupyList.Add(grid2);
                    occupyList.Add(grid3);
                    occupyList.Add(grid4);
                } break;
                case GridType.OneNum:
                default:
                {
                    var grid1 = new Vector2Int{ x = 0,y = 0};
                    occupyList.Add(grid1);
                } break;
            }
        }
        #endregion
        #region 无尽关卡
        [AutoNotify] private int endlessRewardCoinNum; //无尽关卡金币奖励数量
        private readonly Dictionary<int, List<long>> monsterBaseAttr = new(5); //怪物基础属性
        public Dictionary<int, int> RandomMonsterList = new (5);//每轮随机出来的怪物信息
        private readonly Dictionary<int, int> monsterTypeDic = new(4){{101,0},{102,1},{103,2},{104,3},{105,4}};
        public Dictionary<int, Vector2> MonsterBaseAttrList = new (5);//怪物基础属性列表（基础攻击力+血量），随波次更新值
        /// <summary>
        /// 初始化无尽关卡怪物基础属性值
        /// </summary>
        public void InitEndlessBaseAttr(int curChapterIdx)
        {
            monsterBaseAttr.Clear();
            monsterBaseAttr.Add((int)AttributeType.Hp,new List<long>(5){1,1,1,1,1});
            monsterBaseAttr.Add((int)AttributeType.Atk,new List<long>(5){1,1,1,1,1});
            var lvCfg = ConfigCenter.MonsterCfgColl.GetDataByStageId(curChapterIdx);
            if (lvCfg == null || lvCfg.Count == 0) return;
            for (var i = 0; i < lvCfg.Count; i++)
            {
                var attributesList = lvCfg[i].MonsterPro;
                for (var j = 0; j < attributesList.Count; j++)
                {
                    var type = AttributeHelper.GetAttributeTypeByName(attributesList[j].Type);
                    if (!monsterBaseAttr.ContainsKey((int)type)) continue;
                    monsterBaseAttr[(int)type][i] = attributesList[j].Value;
                }
            }
        }
        /// <summary>
        /// 获取所有武器天赋Id（只包含单选的武器）
        /// 先从天赋表找到对应的EquipSkillId，然后再从EquipSkill表里找到对应的EquipId,
        /// 然后再从Equip表的EquipSkillId字段里查找是否包含对应的EquipSkillId
        /// </summary>
        public List<int> GetEndlessWeaponTalentList()
        {
            var talentIdList = new List<int>(20);
            var equipMultiList = new List<int>(20);//已完成选择的武器技能列表
            if (curFightData.Stage.EquipMultiAttr.Count > 0)
            {
                foreach (var equipInfo in curFightData.Stage.EquipMultiAttr)
                {
                    var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipInfo.Value);
                    if (equipModelCfg == null) continue;
                    var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(equipModelCfg.Effect);
                    if (equipSkillCfg == null) continue;
                    equipMultiList.Add(equipSkillCfg.Id);
                }
            }
            var talentCfgs = ConfigCenter.TalentCfgColl.DataItems;
            for (var i = 0; i < talentCfgs.Count; i++)
            {
                if (talentCfgs[i].EquipSkillId <= 0) continue;
                var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(talentCfgs[i].EquipSkillId);
                if (equipSkillCfg == null) continue;
                var equipCfg = ConfigCenter.EquipCfgColl.GetDataById(equipSkillCfg.EquipId);
                if (equipCfg?.EquipSkillId == null || equipCfg.EquipSkillId.Count == 0) continue;
                if (equipCfg.EquipSkillId.Contains(talentCfgs[i].EquipSkillId))
                {
                    talentIdList.Add(talentCfgs[i].Id);
                }
                else if (equipMultiList.Count > 0 && equipMultiList.Contains(talentCfgs[i].EquipSkillId))
                {
                    talentIdList.Add(talentCfgs[i].Id);
                }
            }
            return talentIdList;
        }
        /// <summary>
        /// 根据当前最大通关章节获取奖励
        /// </summary>
        /// <param name="chapterId"></param>
        /// <returns></returns>
        public int GetMonsterCoinNumByChapterId(int chapterId)
        {
            var coinNum = 0;
            var cfg = ConfigCenter.CopyCfgColl.GetDataById(chapterId);
            if (cfg == null) return coinNum;
            if (cfg.Coin.Count > 0) coinNum = cfg.Coin[0];
            return coinNum;
        }
        /// <summary>
        /// 获取无尽关卡模式下的武器天赋
        /// </summary>
        /// <param name="equipModelId"></param>
        public int GetEndlessTalentId(int equipModelId)
        {
            var talentId = 0;
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
            if (equipModelCfg == null) return talentId;
            var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(equipModelCfg.Effect);
            if (equipSkillCfg == null) return talentId;
            var talentCfgs = ConfigCenter.TalentCfgColl.DataItems;
            for (var i = 0; i < talentCfgs.Count; i++)
            {
                if (talentCfgs[i].EquipSkillId <= 0) continue;
                if (talentCfgs[i].EquipSkillId != equipSkillCfg.Id) continue;
                talentId = talentCfgs[i].Id;
                break;
            }
            return talentId;
        }
        /// <summary>
        /// 根据属性类型以及怪物索引获取基础属性值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="monsterIdx"></param>
        /// <returns></returns>
        public long GetMonsterBaseAttr(AttributeType type,int monsterIdx)
        {
            var value = 1;
            if (!monsterBaseAttr.ContainsKey((int)type)) return value;
            if (monsterBaseAttr[(int)type].Count < monsterIdx) return value;
            return monsterBaseAttr[(int)type][monsterIdx];
        }
        /// <summary>
        /// 根据天赋Id获取装备Id
        /// </summary>
        /// <param name="talentId"></param>
        /// <returns></returns>
        public int GetEquipIdByTalentId(int talentId)
        {
            var equipId = 0;
            var talentCfg = ConfigCenter.TalentCfgColl.GetDataById(talentId);
            if (talentCfg == null) return equipId;
            var equipSkillCfg = ConfigCenter.EquipSkillCfgColl.GetDataById(talentCfg.EquipSkillId);
            if (equipSkillCfg == null) return equipId;
            equipId = equipSkillCfg.EquipId;
            return equipId;
        }
        /// <summary>
        /// 根据怪物Id获取对应的基础属性
        /// </summary>
        /// <param name="monsterId"></param>
        /// <returns></returns>
        public Vector2 GetEndlessMonsterBaseAttr(int monsterId)
        {
            var baseAttr = Vector2.one;
            if (MonsterBaseAttrList.TryGetValue(monsterId,out var baseInfo))
            {
                baseAttr = MonsterBaseAttrList[monsterId];
            }
            return baseAttr;
        }
        /// <summary>
        /// 根据怪物ID+波次刷新当前怪物的属性加成
        /// 小怪1模型=101;小怪2模型=102;小怪3模型=103 boss1=104 boss2 = 105
        /// </summary>
        /// <param name="monsterTypeID"></param>
        /// <param name="curSpawnNum"></param>
        public void UpdateMonsterBaseAttrBySpawnNum(int monsterTypeID,int curSpawnNum)
        {
            float hpValue = 1;
            float atkValue = 1;
            EndlessStageMonsterLvCfg cfg = null;
            var tmpHpCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_06);
            var tmpAtkCfg1 = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_07);
            if (monsterTypeID is 104 or 105)
            {
                tmpHpCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_08);
                tmpAtkCfg1 = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.endless_09);
            }
            else
            {
                cfg = GetAtkAndHpFactor(curSpawnNum);
            }
            var hpAttrFactor = tmpHpCfg.Content[0] * GameConst.AttributeDivisor;
            var atkAttrFactor = tmpAtkCfg1.Content[0] * GameConst.AttributeDivisor;
            var baseHpValue = GetMonsterBaseAttr(AttributeType.Hp, monsterTypeDic[monsterTypeID]);
            var baseAtkValue = GetMonsterBaseAttr(AttributeType.Atk, monsterTypeDic[monsterTypeID]);
            //根据波次刷新怪物基础属性
            if (monsterTypeID is 104 or 105)
            {
                //hp = (当前关卡血量*(1+(波次/K)); k=全局系数 2024/8/1新公式
                hpValue = baseHpValue*(1 + curSpawnNum / hpAttrFactor);
                //atk = (当前关卡攻击力*当前波次^K; k=全局系数
                atkValue = baseAtkValue*Mathf.Pow(curSpawnNum, atkAttrFactor);
            }
            else if (cfg != null)
            {
                //hp = (当前关卡血量+当前波次^K)*N //k=全局系数，N为波次系数（TournamentStageMonsterLvCfgColl）
                var tmpAtkValue = Mathf.Pow(curSpawnNum, atkAttrFactor);
                var tmpHpValue = Mathf.Pow(curSpawnNum, hpAttrFactor);
                atkValue = (baseAtkValue + tmpAtkValue)*(cfg.AtkAdd * GameConst.AttributeDivisor);
                hpValue = (baseHpValue + tmpHpValue)*(cfg.HpAdd * GameConst.AttributeDivisor);
            }
            if (!RandomMonsterList.TryGetValue(monsterTypeID, out var monsterId)) return;
            if (MonsterBaseAttrList.TryGetValue(monsterId,out var _))
            {
                MonsterBaseAttrList[monsterId] = new Vector2(hpValue,atkValue);
            }
            else
            {
                MonsterBaseAttrList.Add(monsterId,new Vector2(hpValue,atkValue));
            }
        }
        /// <summary>
        /// 根据当前关卡ID+波次获取攻击力/血量的加成因子
        /// </summary>
        /// <param name="spawnNum"></param>
        /// <returns></returns>
        private EndlessStageMonsterLvCfg GetAtkAndHpFactor(int spawnNum)
        {
            var tmpCfgList = ConfigCenter.EndlessStageMonsterLvCfgColl.DataItems;
            if (tmpCfgList == null || tmpCfgList.Count == 0) return null;
            EndlessStageMonsterLvCfg cfg = null;
            for (var i = 0; i < tmpCfgList.Count; i++)
            {
                if (spawnNum < tmpCfgList[i].WaveId[0] || spawnNum > tmpCfgList[i].WaveId[1]) continue;
                cfg = tmpCfgList[i];
                break;
            }
            return cfg;
        }
        #endregion

        #region 密林
        public Dictionary<int, int> RandomMonsterTypeList = new (5);//每轮随机出来的怪物类型信息
        /// <summary>
        /// 根据怪物ID+波次刷新当前怪物的属性加成
        /// 小怪1模型=101;小怪2模型=102;小怪3模型=103 boss1=104 boss2 = 105
        /// </summary>
        /// <param name="monsterType"></param>
        /// <param name="monsterTypeID"></param>
        /// <param name="curSecretWave"></param>
        public void UpdateSecretMonsterBaseAttrBySpawnNum(int monsterType,int monsterTypeID,int curSecretWave)
        {
            float hpValue = 1;
            float atkValue = 1;
            SecretStageMonsterLvCfg cfg = null;
            var tmpHpCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_06);
            var tmpAtkCfg1 = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_07);
            if ((monsterType+1) is 4 or 5)
            {
                tmpHpCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_08);
                tmpAtkCfg1 = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.Secret_09);
            }
            else
            {
                cfg = GetSecretAtkAndHpFactor(curSecretWave);
            }
            var hpAttrFactor = tmpHpCfg.Content[0] * GameConst.AttributeDivisor;
            var atkAttrFactor = tmpAtkCfg1.Content[0] * GameConst.AttributeDivisor;
            var baseHpValue = GetMonsterBaseAttr(AttributeType.Hp, monsterType);
            var baseAtkValue = GetMonsterBaseAttr(AttributeType.Atk, monsterType);
            //根据波次刷新怪物基础属性
            if ((monsterType+1) is 4 or 5)
            {
                //hp = (当前关卡血量*(1+(波次/K)); k=全局系数 2024/8/1新公式
                hpValue = baseHpValue*(1 + curSecretWave / hpAttrFactor);
                //atk = (当前关卡攻击力*当前波次^K; k=全局系数
                atkValue = baseAtkValue*Mathf.Pow(curSecretWave, atkAttrFactor);
            }
            else if (cfg != null)
            {
                //hp = (当前关卡血量+当前波次^K)*N //k=全局系数，N为波次系数（TournamentStageMonsterLvCfgColl）
                var tmpAtkValue = Mathf.Pow(curSecretWave, atkAttrFactor);
                var tmpHpValue = Mathf.Pow(curSecretWave, hpAttrFactor);
                atkValue = (baseAtkValue + tmpAtkValue)*(cfg.AtkAdd * GameConst.AttributeDivisor);
                hpValue = (baseHpValue + tmpHpValue)*(cfg.HpAdd * GameConst.AttributeDivisor);
            }
            if (!RandomMonsterList.TryGetValue(monsterTypeID, out var monsterId)) return;
            if (MonsterBaseAttrList.TryGetValue(monsterId,out var _))
            {
                MonsterBaseAttrList[monsterId] = new Vector2(hpValue,atkValue);
            }
            else
            {
                MonsterBaseAttrList.Add(monsterId,new Vector2(hpValue,atkValue));
            }
        }
        /// <summary>
        /// 获取当前波次下的bossId
        /// </summary>
        /// <returns></returns>
        public int GetCurWaveBossId()
        {
            if (RandomMonsterTypeList.ContainsKey(4)) return RandomMonsterTypeList.GetValueOrDefault(4, 0);
            return RandomMonsterTypeList.ContainsKey(5) ? RandomMonsterTypeList.GetValueOrDefault(5, 0) : 0;
        }
        /// <summary>
        /// 根据当前关卡ID+波次获取攻击力/血量的加成因子
        /// </summary>
        /// <param name="secretWave"></param>
        /// <returns></returns>
        private SecretStageMonsterLvCfg GetSecretAtkAndHpFactor(int secretWave)
        {
            var tmpCfgList = ConfigCenter.SecretStageMonsterLvCfgColl.DataItems;
            if (tmpCfgList == null || tmpCfgList.Count == 0) return null;
            SecretStageMonsterLvCfg cfg = null;
            for (var i = 0; i < tmpCfgList.Count; i++)
            {
                if (secretWave < tmpCfgList[i].WaveId[0] || secretWave > tmpCfgList[i].WaveId[1]) continue;
                cfg = tmpCfgList[i];
                break;
            }
            return cfg;
        }
        #endregion
        #region 每日挑战
        /// <summary>
        /// 根据怪物类型获取怪物Id
        /// </summary>
        /// <param name="monsterType"></param>
        public int GetMonsterIdByType(int monsterType)
        {
            var monsterId = 101;
            DataCenter.dailyFightData.Mons.TryGetValue(monsterType, out monsterId);
            return monsterId;
        }
        /// <summary>
        /// 根据怪物Id获取对应的击杀经验
        /// </summary>
        /// <param name="monsterId"></param>
        /// <returns></returns>
        public int GetMonsterKillExpByMonsterId(int monsterId)
        {
            var newExp = 0;
            var cfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_10);
            if (cfg == null || cfg.Content.Count == 0) return newExp;
            var monsterCfgList = ConfigCenter.MonsterCfgColl.GetDataByStageId(cfg.Content[0]);
            if (monsterCfgList == null || monsterCfgList.Count == 0) return newExp;
            var monsterIdx = 1;
            foreach (var monsterInfo in DataCenter.dailyFightData.Mons)
            {
                if (monsterInfo.Value == monsterId)
                {
                    monsterIdx = monsterInfo.Key*-1;
                    break;
                }
            }
            if (monsterIdx-1 >= monsterCfgList.Count) return newExp;
            newExp = monsterCfgList[monsterIdx - 1].KillExp;
            return newExp;
        }
        /// <summary>
        /// 根据怪物Id获取对应的基础血量
        /// </summary>
        /// <param name="monsterId"></param>
        /// <returns></returns>
        public int GetMonsterHpByMonsterId(int monsterId)
        {
            var newHp = 0;
            var offsetNum = 0;
            var offsetCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_11);
            if (offsetCfg != null && offsetCfg.Content.Count > 0) offsetNum = offsetCfg.Content[0];
            var maxPassLevel = DataCenter.mainStageData.GetMaxPassChapter();
            maxPassLevel -= offsetNum;
            if (maxPassLevel < 0) maxPassLevel = 1;
            var monsterCfgList = ConfigCenter.MonsterCfgColl.GetDataByStageId(maxPassLevel);
            if (monsterCfgList == null || monsterCfgList.Count == 0) return newHp;
            var monsterIdx = 1;
            foreach (var monsterInfo in DataCenter.dailyFightData.Mons)
            {
                if (monsterInfo.Value == monsterId)
                {
                    monsterIdx = monsterInfo.Key*-1;
                    break;
                }
            }
            if (monsterIdx-1 >= monsterCfgList.Count) return newHp;
            var attributesList = monsterCfgList[monsterIdx - 1].MonsterPro;
            for (var j = 0; j < attributesList.Count; j++)
            {
                var type = AttributeHelper.GetAttributeTypeByName(attributesList[j].Type);
                if (type != AttributeType.Hp) continue;
                newHp = attributesList[j].Value;
            }
            return newHp;
        }
        /// <summary>
        /// 根据怪物Id获取对应的基础攻击力
        /// </summary>
        /// <param name="monsterId"></param>
        /// <returns></returns>
        public int GetMonsterAtkByMonsterId(int monsterId)
        {
            var newAtk = 0;
            var offsetNum = 0;
            var offsetCfg = ConfigCenter.DefinesCfgColl.GetDataById((int)DefineCfgId.dailyStage_11);
            if (offsetCfg != null && offsetCfg.Content.Count > 0) offsetNum = offsetCfg.Content[0];
            var maxPassLevel = DataCenter.mainStageData.GetMaxPassChapter();
            maxPassLevel -= offsetNum;
            var monsterCfgList = ConfigCenter.MonsterCfgColl.GetDataByStageId(maxPassLevel);
            if (monsterCfgList == null || monsterCfgList.Count == 0) return newAtk;
            var monsterIdx = 1;
            foreach (var monsterInfo in DataCenter.dailyFightData.Mons)
            {
                if (monsterInfo.Value == monsterId)
                {
                    monsterIdx = monsterInfo.Key*-1;
                    break;
                }
            }
            if (monsterIdx-1 >= monsterCfgList.Count) return newAtk;
            var attributesList = monsterCfgList[monsterIdx - 1].MonsterPro;
            for (var j = 0; j < attributesList.Count; j++)
            {
                var type = AttributeHelper.GetAttributeTypeByName(attributesList[j].Type);
                if (type != AttributeType.Atk) continue;
                newAtk = attributesList[j].Value;
            }
            return newAtk;
        }
        #endregion

        #region 秘林合成

        private int GetEquipMergeIndex(int originalIndex)
        {
            int canMergeIndex = -1;
            var originalModelId = CurFightData.Stage.EquipSlots[originalIndex];
            if (originalModelId == -1) return -1;
            for (int i = 0; i < CurFightData.Stage.EquipSlots.Count; i++)
            {
                var model = CurFightData.Stage.EquipSlots[i];
                if (i != originalIndex && originalModelId == model)
                {
                    canMergeIndex = i;
                }
                if (canMergeIndex != -1)
                {
                    break;
                }
            }
            return canMergeIndex;
        }

        /// <summary>
        /// 检查是否需要合成
        /// </summary>
        public bool IsCanNeedMerge
        {
            get
            {
                if (IsSecretFightData())
                {
                    foreach (var item in EquipSlots)
                    {
                        if(item.Value < 2)continue;
                        var modelId = item.Key;
                        var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(modelId);
                        if (!CheckWeaponMaxLevel(modelId, equipModelCfg.Equip))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return false;
            }
        }

        public bool CheckEquipIsShowTips(int equipModelId)
        {
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
            if(equipModelCfg ==null)return false;

            bool ret = false;
            foreach (var item in EquipSlots)
            {
                var cfg = ConfigCenter.EquipModelCfgColl.GetDataById(item.Key);
                if (cfg.Equip == equipModelCfg.Equip)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public bool CheckEquipCanMerge(int equipModelId)
        {
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(equipModelId);
            if(!EquipSlots.ContainsKey(equipModelId)) return false;
            
            if (EquipSlots.TryGetValue(equipModelId, out int count))
            {
                if (count < 1)
                {
                    return false;
                }
            }
            if(equipModelCfg ==null)return false;
            // 已经达到最高级
            if (CheckWeaponMaxLevel(equipModelId, equipModelCfg.Equip))
            {
                return false;
            }
            // 不能合成
            if (equipModelCfg.NextId == null || equipModelCfg.NextId.Count == 0)
            {
                return false;
            }

            if (equipModelCfg.NextId.Count > 1 && !CheckWeaponDoubleAttrUnlocked(equipModelCfg.Equip))
            { 
                return false;
            }

            return true;
        }

        private bool CheckEquipCanMerge(KeyValuePair<int,int> mergerModel, Action<int, int, Action<int>> callback)
        {
           if(mergerModel.Value < 2) return false;
            var originalModelId = mergerModel.Key;
            
            var equipModelCfg = ConfigCenter.EquipModelCfgColl.GetDataById(originalModelId);
            // 已经达到最高级
            var ret = CheckEquipCanMerge(originalModelId);
            if(! ret) return false;
            
            if (equipModelCfg.NextId.Count == 1)
            {
                // 移除背包数据
                RemoveBackPackDataInfo(originalModelId);
                RemoveBackPackDataInfo(originalModelId);
                // 直接合成
                AddEquipModelToBackList(equipModelCfg.NextId[0], true);
                
                CurFightData.Stage.EquipSlots[mergerModel.Key] -= 1;
                CurFightData.Stage.EquipSlots[mergerModel.Key] -=1;
                AddEquipSlots(equipModelCfg.NextId[0]);
                CheckEquipCanMerge(callback);
                return true;
            }
            
            // 最高级 合成  这里检查 是否弹出合成 
            var advanceId = GetEquipAdvanceEquipModelIdByEquipId(equipModelCfg.Equip);
            if (advanceId != 0)
            {
                // 移除背包数据
                RemoveBackPackDataInfo(originalModelId);
                RemoveBackPackDataInfo(originalModelId);
                
                // 直接合成
                AddEquipModelToBackList(advanceId, true);
                CurFightData.Stage.EquipSlots[mergerModel.Key] -= 1;
                CurFightData.Stage.EquipSlots[mergerModel.Key] -= 1;
                AddEquipSlots(advanceId);
                // 这里需要自动合并所有同类型的
                CheckEquipCanMerge(callback);
                
                return true;
            }
            
            callback(originalModelId, equipModelCfg.Equip, (modelId) =>
            {
                // 移除背包数据
                RemoveBackPackDataInfo(originalModelId);
                RemoveBackPackDataInfo(originalModelId);
                
                // 直接合成
                AddEquipModelToBackList(modelId, true);
                
                CurFightData.Stage.EquipSlots[mergerModel.Key] -= 1;
                CurFightData.Stage.EquipSlots[mergerModel.Key] -= 1;
                AddEquipSlots(modelId);
                
                CheckEquipCanMerge(callback);
            });
            return true;
        }
        /// <summary>
        ///  天赋选择完后合成
        /// </summary>
        /// <param name="callback"></param>
        public void CheckEquipCanMerge(Action<int, int, Action<int>> callback)
        {
            var newEquipSlots = new Dictionary<int,int>();
            foreach (var item in CurFightData.Stage.EquipSlots)
            {
                if(item.Value <=0) continue;
                newEquipSlots.Add(item.Key, item.Value);
            }
            CurFightData.Stage.EquipSlots.Clear();
            foreach (var item in newEquipSlots)
            {
                AddEquipSlots(item.Key, item.Value);
            }

            foreach (var item in EquipSlots)
            {
             
                if (CheckEquipCanMerge(item, callback))
                {
                    break;
                }
            }
            
            NotifyGameUIWeaponRefresh();
        }
        public void AddEquipSlots(int modelId,int addCount =1)
        {
            if(!CurFightData.Stage.EquipSlots.TryGetValue(modelId, out int count))
            {
                count = 0;
                CurFightData.Stage.EquipSlots[modelId] = count;
            }
            count+=addCount;
            CurFightData.Stage.EquipSlots[modelId] = count;
        }
        private void AddEquipToSlots(int equipModelId, Action<int,int,Action<int>> callback)
        {
            AddEquipModelToBackList(equipModelId);
            // 添加
            AddEquipSlots(equipModelId);
        }

        private void AddEquipModelToBackList(int equipModelId, bool isMerge = false)
        {
            int weaponParmId = GetWeaponParmIdByEquipModelIdAndStateId(equipModelId, 0);
            BackpackWeaponData backData = CreateBackpackData(weaponParmId, 1, ELocationType.Backpack);
            AddWeaponToBackpack(backData);
            if (isMerge)
            {
                MergeWeaponList.Add(backData.Uid);
            }
        }
        private void RemoveBackPackDataInfo(int equipModelId)
        {
            var backpackDatas = BackpackWeaponList;
            BackpackWeaponData removeData = null;
            foreach (var item in backpackDatas)
            {
                if (item.WeaponId == equipModelId)
                {
                    removeData = item;
                    break;
                }
            }

            if (removeData != null)
            {
               RemoveWeaponFromBackpack(removeData);
            }
        }
        
        #endregion
    }
}
