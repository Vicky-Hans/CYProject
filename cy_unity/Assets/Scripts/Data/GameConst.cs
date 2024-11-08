using System;
using System.Collections.Generic;
using DH.Config;
using DH.Proto;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace DH.Data
{
    public static class GameConst
    {
        public const float PixelPerUnit = 100f;
        public const float TileWidth = 108f;
        public const float TileHeight = 108f;
        public const float BackTileWidth = 216f;
        public const float BackTileHeight = 192f;
        public static readonly Vector3 SceneSeparator = new Vector3(0, 4.86f + 2.2f, 0);
        public const int TenThousand = 10000;
        public const float AttributeDivisor = 0.0001f;
        public const float TimeDivisor = 0.001f;
        /// <summary>
        /// 秘林 给赛季提示的 时间
        /// </summary>
        public const long SecretShowTipsTime = 60;

        /// <summary>
        ///  最大的武器个数
        /// </summary>
        public const int MaxWeaponCount = 5;

        /// <summary>
        /// 百分比
        /// </summary>
        public const float Percentage = 0.01f;

        public const int GuideDelayTriggerTime = 500;
        public const string DiscordUrl = "https://discord.gg/2dH47qAyFX";
        public const string MailUrl = "mailto:hecs@droidhang.com";
        /// <summary>
        /// 秘林的调查问卷链接
        /// </summary>
        public const string SecretSurveysUrl = "https://docs.google.com/forms/d/e/1FAIpQLSfYHnJCIdiBc1e5ztlE9xVgUoZOxAeiGapwG5E9hPGpk6LvUQ/viewform?usp=pp_url";

        /// <summary>
        /// 护卫舰的模型偏移
        /// </summary>
        public const int FrigateModelOffset = 10000;

        /// <summary>
        ///  boss 转盘容错
        /// </summary>
        public const int BossRewardFailover = 50;

        /// <summary>
        /// 游戏的默认速度
        /// </summary>
        public const float TimeDefaultScale = 1.0f;

        /// <summary>
        /// 审核商店状态
        /// </summary>
        public const bool IsIosAuditState = false;

        /// <summary>
        /// 进游戏次数 
        /// </summary>
        public const string EnterGameCount = "enter_game_count";

        public const string LuckDrawKey = "luck_draw_skip_ani";
        public const string MagicDrawKey = "magic_draw_skip_ani";
        public const string SecretBattleSpeedKey = "secret_battle_speed_key";
        /// <summary>
        /// 赛季祝福弹窗
        /// </summary>
        public const string SecretSeasonPopUpKey = "secret_season_pop_up";

        public enum EHangupOp
        {
            Time = 1,
            Hp,
            Ad,
        }

        public enum EHeadCfgType
        {
            Head = 1,
            Frame,
        }

        public class EventCode
        {
            public const string GuideTrigger = "guide_trigger";
            public const string GuideComplete = "guide_complete";
            public const string OnMainUiPageMoveEnd = "on_main_ui_page_move_end";
            public const string OnChapterListMoveEnd = "on_chapter_list_move_end";
            public const string OnEvolutionListChooseCell = "on_evolution_list_choose_cell";
            public const string OnEvolutionUpgrade = "on_evolution_upgrade";
            public const string OnSkillUpgrade = "on_skill_upgrade";
            public const string OnTriggerSkillEffect = "on_trigger_skill_effect";
        }

        /// <summary>
        /// 本地数据的key
        /// </summary>
        public class UserInfoCode
        {
            public const string EffectState = "effect_state";
            public const string MusicState = "music_state";
        }

        public enum ItemIdCode
        {
            Money = 1,

            /// <summary>
            /// 新晶
            /// </summary>
            Stone = 2,

            /// <summary>
            /// 能量饮料
            /// </summary>
            EnergyDrink = 3,

            /// <summary>
            ///   游戏中里面的 货币 （银币）
            /// </summary>
            GameCoin = 4,

            /// <summary>
            /// 经验值
            /// </summary>
            Exp = 5,

            /// <summary>
            /// 英雄养成材料
            /// </summary>
            HeroUpRes = 6,

            /// <summary>
            /// 免广告卷
            /// </summary>
            AdFreeVouche = 7,

            /// <summary>
            /// 源货币
            /// </summary>
            Yuan = 8,

            /// <summary>
            /// 秘林积分
            /// </summary>
            SecretScore = 10,

            /// <summary>
            /// 用于武器研究的核心材料
            /// </summary>
            WeaponCore = 11,

            /// <summary>
            ///周活
            /// </summary>

            WeeklyPoint = 12,

            /// <summary>
            /// 星核
            /// </summary>
            XingHe = 13,

            //返利点
            RebatePoint = 14,

            // 深红结晶
            DeepRedCrystal = 15,

            // 竞标赛兑换商店
            ChampionExchange = 16,

            //扭蛋币
            EggCoin = 17,

            //扭蛋红心
            EggRedHeart = 18,
            
            //冰果币
            BinGoCoin = 19,
            //冰果点数
            BinGoPoint = 20,
            
            /// <summary>
            /// 钛金矿
            /// </summary>
            Titannium = 100,

            /// <summary>
            /// 能量块
            /// </summary>
            ActionBall = 101,

            /// <summary>
            /// 铀矿
            /// </summary>
            Uranium = 102,

            /// <summary>
            /// 水晶
            /// </summary>
            Crystal = 120,

            //武器碎片
            WeaponFragment = 201,

            /// <summary>
            /// 幸运抽奖的钥匙 
            /// </summary>
            LuckyDrawKey = 310,

            /// <summary>
            /// 次元结晶 
            /// </summary>
            Dimension = 311,

            /// <summary>
            /// 次元抽奖卷
            /// </summary>
            DimensionDraw = 312,

            /// <summary>
            /// 盲盒消耗券
            /// </summary>
            blindBoxMoney = 313,

            /// <summary>
            /// 盲盒纪念券
            /// </summary>
            blindBoxCommemorate = 314,

            /// <summary>
            /// 低级结晶
            /// </summary>
            lowCrystallized = 315,

            /// <summary>
            /// 高级结晶
            /// </summary>
            HighCrystallized = 316,

            /// <summary>
            /// 快速收益体验 
            /// </summary>
            QuickGet = 350,

            /// <summary>
            /// 幸运抽奖的再来一次
            /// </summary>
            LuckyRePlay = 1000,



        }

        public enum ETreasureId
        {
            /// <summary>
            /// 折扣卡
            /// </summary>
            TreasureDisCount = 1,

            /// <summary>
            /// 至尊黑卡
            /// </summary>
            TreasureBlackCard = 2,
        }

        public enum ItemType
        {
            Currency = 1,
            Paper,
            Stone,
            EquipFragment = 2,
            EquipFragmentRandom=3,
            HeroEquipUpLevel=6,
            HeroEquipMerge=7,
            HeroEquipRandom=8,
            ResourcePackage = 14,
            SkinFragment = 17,
        }

        public enum ENumType
        {
            /// <summary>
            /// 数值
            /// </summary>
            NumberTypeValue = 0,

            /// <summary>
            /// 百分比
            /// </summary>
            PercentTypeValue = 1,
            Time = 3
        }

        public enum EShowNumType
        {
            /// <summary>
            /// 显示数值
            /// </summary>
            ShowNumTypeValue = 0,

            /// <summary>
            /// 显示百分比
            /// </summary>
            ShowNumTypePercent = 1,

            /// <summary>
            /// 显示时间 显示 秒
            /// </summary>
            ShowNumTypeTime = 3,
        }

        public enum EEvolutionType
        {
            /// <summary>
            /// 战斗天赋
            /// </summary>
            EvolutionTypeFight = 1,

            /// <summary>
            /// 挖矿天赋
            /// </summary>
            EvolutionTypeMining = 2,

            /// <summary>
            /// 特殊天赋
            /// </summary>
            EvolutionTypeSpecial = 3
        }
        /// <summary>
        /// 对应QuaCfg配置表Id
        /// </summary>
        public enum QuaType
        {
            White = 1,
            Green = 2,
            Blue = 3,
            Purple = 4,
            Orange = 5,
            Red = 6,
            Rainbow = 7,
        }

        public class EquipAttr
        {
            public const string atk = "atk";
            public const string hp = "hp";
            public const string hpBouns = "hpBouns";
            public const string attackBouns = "attackBouns";

            /// <summary>
            /// 武器倍率
            /// </summary>
            public const string hurtRate = "hurtRate";

            /// <summary>
            /// 最大数
            /// </summary>
            public const string NumMax = "numMax";

            /// <summary>
            /// 当前数
            /// </summary>
            public const string Num = "num";
        }

        public class MapAniName
        {
            public const string Enter = "chuchang";
            public const string Idle = "daiji";
        }

        public class AnimationName
        {
            public const string Walk = "walk";
            public const string Idle = "idle";
            public const string Idle1 = "idle1";
            public const string Idle2 = "idle2";
            public const string Atk = "atk";
            public const string WalkAtk = "walk_atk";
            public const string Attack = "attack";
            public const string Skill1 = "skill1";
            public const string Skill2 = "skill2";
            public const string Dead = "dead";
            public const string End = "end";
            public const string Enter = "chuchang";
            public const string Hurt = "hurt";
        }

        public class TimerTagName
        {
            public const string Grinding = "grinding";
            public const string Monster = "monster";
            public const string GameScene = "gameScene";
            public const string Weapon = "weapon";
            public const string PauseTask = "PauseTask";
            public const string LoopTimer = "LoopTimer"; //无尽循环计时器
            public const string AutoReleaseObj = "AutoReleaseObj"; //自动计时释放
        }

    }

    public enum EquipAtkType
    {
        Physic = 1,
        Magic = 2,
        Defender = 3,
        Money = 4
    }

    public enum EquipAttrType
    {
        Water = 1,
        Fire = 2,
        Light = 3,
        None = 4,
        Dark = 5,
        Earth = 6,
        Wind = 7,
    }

    public enum ItemState
    {
        Free,
        Selected,
        Lock,
        Mask,
    }

    public interface IItemState
    {
        public ItemState State { get; }
    }

    public enum EShopCfgId
    {
        Chapter = 1,
        Daily,
        Box,
        Diamond,
        Gold,
        Yuan
    }

    public enum ETabType
    {
        /// <summary>
        /// 商店
        /// </summary>
        TabTypeShop = 0,

        /// <summary>
        /// 主角
        /// </summary>
        Role,

        /// <summary>
        /// 主线
        /// </summary>
        TabTypeMainStage,

        /// <summary>
        /// 装备
        /// </summary>
        TabTypeEquip,

        /// <summary>
        /// 活动
        /// </summary>
        TabTypeActivity,
        //
        // /// <summary>
        // ///  武器研究
        // /// </summary>
        // TabTypeSearch
    }

    public enum EStateType
    {
        StageTypeNone = 0,

        /// <summary>
        /// 主线
        /// </summary>
        StageTypeMainStage = 1,

        /// <summary>
        /// 每日挑战
        /// </summary>
        StageTypeChallenge = 2,

        //无尽模式
        StageTypeEndless = 3,

        //模拟作战模式
        StageTypeSimulate = 4,

        //密林
        StageTypeSecret = 5,
    }

    public enum EShopCostType
    {
        CountAd = -2,
        Ad = -1,
        Free = 0,
        Diamond = 1,
        Gold = 2,
    }

    public enum EShopGoldCostType
    {
        Diamond = -3, //钻石商品
        AdCount = -2, //首次免费，后一次看广告；
        Ad = -1, //广告商品
    }

    public enum EShopItemState
    {
        Free,
        AdCountFree,
        Cost,
        Finish,
        Ad,
        MonthCardFree,
    }

    public enum ETaskPageType
    {
        /// <summary>
        /// 所有任务
        /// </summary>
        TaskPageTypeNone = 0,

        /// <summary>
        ///  签到
        /// </summary>
        TaskPageTypeSign = 1,

        /// <summary>
        /// 日常
        /// </summary>
        TaskPageTypeDaily = 2,

        /// <summary>
        /// 周常
        /// </summary>
        TaskPageTypeWeekly = 3,

        /// <summary>
        /// 成就
        /// </summary>
        TaskPageTypeAchieve = 4,
    }

    public enum ETaskType
    {
        // /// <summary>
        // ///  开服签到
        // /// </summary>
        // TaskTypeNewSign = -1,
        //
        // /// <summary>
        // ///  签到
        // /// </summary>
        // TaskTypeDailySign = 1,
        //
        // /// <summary>
        // /// 日常
        // /// </summary>
        // TaskTypeDailyTask = 2,
        //
        // /// <summary>
        // /// 周常
        // /// </summary>
        // TaskTypeWeeklyTask = 3,
        //
        // /// <summary>
        // /// 成就
        // /// </summary>
        // TaskTypeAchieveTask = 4,
        //
        // /// <summary>
        // /// 累计签到 宝箱
        // /// </summary>
        // TaskTypeSignBox = 5,
        //
        // /// <summary>
        // /// 日常宝箱
        // /// </summary>
        // TaskTypeDailyBox = 6,
        //
        // /// <summary>
        // /// 周常宝箱
        // /// </summary>
        // TaskTypeWeeklyBox = 7,
        //
        // /// <summary>
        // /// 幸运抽奖 的 每日任务
        // /// </summary>
        // TaskTypeLuckyDaily = 8,
        //
        // ///
        // /// 幸运抽奖 的累计任务
        // ///
        // TaskTypeLuckyBox = 9,
        //
        // /// <summary>
        // /// 新手任务
        // /// </summary>
        // TaskTypeNewbieTask = 10,
        //
        // /// <summary>
        // /// 盲盒任务
        // /// </summary>
        // TaskTypeBlindBox = 11,
        //
        //
        // /// <summary>
        // /// 新手冲刺活动任务
        // /// </summary>
        // NewcomerSprintEvent = 12,

        //魔法学院任务
        CollegeTask = 1,
        //扭蛋
        LuckEgg = 2,
        //魔法宾果
        MagicBingo = 3,
    }

    public enum ESignOp
    {
        /// <summary>
        /// 1-七天循环奖励
        /// </summary>
        SignOpNormal = 1,

        /// <summary>
        /// 2-累积宝箱领取
        /// </summary>
        SignOpBox = 2
    }

    public enum ETaskOpType
    {
        //1-日常任务领取，2-日常任务宝箱领取，3-周常任务领取，4-周常宝箱领取，5-成就任务领取
        /// <summary>
        /// 日常任务领取
        /// </summary>
        TaskOpDailyTaskNormal = 1,

        /// <summary>
        /// 日常任务宝箱领取
        /// </summary>
        TaskOpDailyTaskBox = 2,

        /// <summary>
        /// 周常任务领取
        /// </summary>
        TaskOpWeeklyTaskNormal = 3,

        /// <summary>
        /// 周常宝箱领取
        /// </summary>
        TaskOpWeeklyTaskBox = 4,

        /// <summary>
        /// 成就任务领取
        /// </summary>
        TaskOpAchieveTask = 5,

        /// <summary>
        /// 新手任务的领取
        /// </summary>
        TaskOpNewbieTask = 6
    }

    public enum EBoxState
    {
        Close,
        Open,
        Wait
    }

    public enum EGameWeaponType
    {
        // chenjiahui：
        // 0：保底技能
        // 1：武器默认技能
        // 2：武器被动技能（局外等级解锁）
        // 3：武器战斗技能（战斗内随机抽取）
        /// <summary>
        /// 保底技能
        /// </summary>
        WeaponTypeMin = 0,

        /// <summary>
        /// 武器默认技能
        /// </summary>
        WeaponTypeWeapon = 1,

        /// <summary>
        /// 武器被动技能（局外等级解锁）
        /// </summary>
        WeaponTypePass = 2,
    }

    /// <summary>
    /// 武器系别
    /// </summary>
    public enum EWeaponFactor
    {
        Ice = 1,
        Fire = 2,
        Water = 3,
        Light = 4,
        Psychic = 5,
        Dark = 6,
        Electric = 7,
    }

    public enum DmgType
    {
        Normal,
        Small,
        Boom,
        SmallBoom,
        FireBoom,
        Zone,
        SmallRebel,
        Continue,
        Plain,
        Range,
        Ignore,
    }

    public enum SkillHurtType
    {
        Normal,
        Crit,
        Immune
    }

    public enum SkillShapeType
    {
        Circle,
        Rectangle,
        FullScreen,
        None
    }

    public enum EMonsterMoveType
    {
        Normal = 0,
        Down = 1,
        Up = 2,
        Left = 3,
        Right = 4,
    }
    
    public enum EStageRewardType
    {
        College = 3,
    }

    public class WeaponNameId
    {
        public const int Weapon201 = 1;
        public const int Weapon301 = 2;
        public const int Weapon401 = 3;
        public const int Weapon501 = 4;
        public const int Weapon601 = 5;
        public const int Weapon701 = 6;
        public const int Weapon801 = 7;
        public const int Weapon901 = 8;
        public const int Weapon1001 = 9;
        public const int Weapon1101 = 10;
        public const int Weapon1201 = 11;
        public const int Weapon1301 = 12;
    }

    public class SaveLocalKey
    {
        /// <summary>
        /// 等级基金是否弹出
        /// </summary>
        public const string NewbieLevelPrePopUp = "newbie_pre_level_popup_value";

        public const string NewbieLevelVersionPopUp = "newbie_pre_level_vesion_value";

        /// <summary>
        /// 水晶基金是否弹出
        /// </summary>
        public const string NewbieStonePrePopUp = "newbie_pre_stone_popup_value";

        public const string NewbieStoneVersionPopUp = "newbie_pre_level_vesion_value";

        /// <summary>
        /// 新手礼包是否弹出
        /// </summary>
        public const string NewbiePrePopUp = "newbie_activity_popup_value";
    }

    public class AdKey
    {
        public const string MainStageGameEndDouble = "main_stage_game_end_double";
        public const string RandomSkill = "random_skill";
        public const string LuckyDraw = "lucky_draw";
        public const string ChampionExchange = "chanpion_exchange";
    }

    public enum EAdType
    {
        Main = 0,
        MainFight = 1,
        MiningFight = 2,
        Else = 3,
    }

    public class DhHexColor
    {
        public const string Red = "#FF0000";
        public const string White = "#e7f0ff";
        public const string WhiteWIthAlpha = "#FFFFFFFF";
        public const string WhiteWIthAlphaLow = "#FFFFFF00";
        public const string Black = "#000000";
        public const string BlackWIthAlpha = "#000000FF";
        public const string BlackWIthAlphaLow = "#00000000";
        public const string Gray = "#444444";
        public const string GrayBtnTextColor = "#a5a5a5";
        public const string GreenBtnTextColor = "#017651";
        public const string BlueBtnGoOnTextColor = "#036382";
        public const string BlueBtnConfirmTextColor = "#9cdaea";
        public const string YellowBtnTextColor = "#a85d26";
        public const string RedBtnTextColor = "#ffd4d6";
        public const string PurpleBtnTextColor = "#a996ff";
    }

    public enum EMailReadState
    {
        /// <summary>
        /// 邮件未读
        /// </summary>
        MailStateUnRead = 0,

        /// <summary>
        /// 邮件已读
        /// </summary>
        MailStateRead = 1,
    }

    public enum EMailRewardState
    {
        /// <summary>
        /// 邮件没领取
        /// </summary>
        MailRewardStateUnClaim = 0,

        /// <summary>
        /// 邮件已领取
        /// </summary>
        MailRewardStateClaim = 1,
    }

    public enum EMailType
    {
        /// <summary>
        /// 补偿邮件
        /// </summary>
        MailTypeCompensate = 1,

        /// <summary>
        /// 活动邮件
        /// </summary>
        MailTypeActivity = 2,

        /// <summary>
        /// 通知邮件
        /// </summary>
        MailTypeNotify = 3,

        /// <summary>
        /// 工会邮件
        /// </summary>
        MailTypeGuild = 4,

        /// <summary>
        /// 超链接邮件
        /// </summary>
        MailTypeLink = 5,

        /// <summary>
        /// 购买延迟
        /// </summary>
        MailTypeShop = 6
    }

    public enum ShopChestId
    {
        Cloth = 5,
    }

    public enum EMailFavoriteState
    {
        /// <summary>
        /// 邮件收藏状态：未收藏
        /// </summary>
        MailFavoriteStateUnFavorite = 0,

        /// <summary>
        /// 邮件收藏状态：已收藏
        /// </summary>
        MailFavoriteStateFavorite = 1,
    }

    public class MailRewards
    {
        public int type;
        public int id;
        public long count;
        public List<long> param;

        public MailRewards(int type, int id, long count, List<long> param = null)
        {
            this.type = type;
            this.id = id;
            this.count = count;
            this.param = param;
        }

        public MailRewards()
        {
        }
    }

    public enum ETimeFormatType
    {
        // 默认时间格式 h:m:s
        Default = 0,

        // 默认时间格式 h小时:m分钟:s秒
        DefaultWithUnit,

        /// <summary>
        /// 时间格式 h:m 两位 
        /// </summary>
        TimeFormatTypeHourMinute,

        /// <summary>
        /// 时间格式 h:m 两位 
        /// </summary>
        TimeFormatTypeHourMinuteWithUnit,

        /// <summary>
        /// 时间格式 时间精确到一位， 不到一小时 显示 刚刚 到了一小时 显示 x小时前 大于一天显示 x天前
        /// </summary>
        TimeFormatTypeMail,

        /// <summary>
        /// 大于1天显示天 小于一天显示时分
        /// </summary>
        TimeFormatMonthCard,

        /// <summary>
        ///  锦标赛的时间 格式  1天 24小时 24分钟  小于一天 24小时24分钟24秒
        /// </summary>
        TimeFormatChampion,

        /// <summary>
        /// 时间格式 m:s 两位 
        /// </summary>
        TimeFormatTypeMinuteWithUnit,
    }

    public enum ESpecialSkillId
    {
        /// <summary>
        /// 战斗开始时，立即进行1次奖励选择
        /// </summary>
        SpecialId_100195 = 100195,

        /// <summary>
        /// 在学习 4 武器个数 上限
        /// </summary>
        SpecialId_100296 = 100196,
    }

    public enum DiscountShopType
    {
        Daily,
        Week,
        Month,
    }

    public enum ERankAreaType
    {
        /// <summary>
        /// 全球
        /// </summary>
        RankAreaTypeGlobal = 1,

        /// <summary>
        /// 区域
        /// </summary>
        RankAreaTypeLocal = 2,
    }

    public enum EFunctionOpenType
    {

        /// <summary>
        /// 主线
        /// </summary>
        FunctionTypeMainStage = 1,

        /// <summary>
        /// 扫荡
        /// </summary>
        FunctioonTypeFarm = 2,

        /// <summary>
        /// 装备
        /// </summary>
        FunctionTypeEquip = 3,

        /// <summary>
        ///  商店
        /// </summary>
        FunctionTypeShop = 4,

        /// <summary>
        /// 排行榜
        /// </summary>
        FunctionTypeRank = 5,
        /// <summary>
        /// 无尽关卡
        /// </summary>
        FunctionEndless = 6,

        /// <summary>
        /// 英雄功能
        /// </summary>
        FunctionRole = 7,

        /// <summary>
        /// 每日挑战
        /// </summary>
        FunctionDailyFight = 8,
        /// <summary>
        /// 章节基金
        /// </summary>
        FunctionChapterFund = 10,
        
        FunctionClothes = 11,//服饰
        /// <summary>
        /// 秘境
        /// </summary>
        FunctionSecret = 12,
        
        /// <summary>
        /// 0元购
        /// </summary>
        ActivityFreeBuy = 13,

        /// <summary>
        /// 月卡
        /// </summary>
        MonthCard = 101,

        /// <summary>
        /// 新手礼包
        /// </summary>
        FunctionTypeNewPackage = 104,

        /// <summary>
        /// 特惠通行证
        /// </summary>
        FunctionTypePassports = 105,

        /// <summary>
        /// 每日特惠
        /// </summary>
        DailySpecialOffers = 106,

        /// <summary>
        /// 触发礼包特殊商店
        /// </summary>
        TriggerGiftShop = 107,

        /// <summary>
        /// 幸运转盘
        /// </summary>
        LuckyDraw = 108,

        /// <summary>
        /// 砖石通行证
        /// </summary>
        FunctionTypePassportsStone = 109,

        /// <summary>
        /// 心愿
        /// </summary>
        Wish = 110,

        /// <summary>
        /// 魔法祈愿
        /// </summary>
        MagicDraw=111,
        

        
        //魔法学院
        College = 112,
        
        /// <summary>
        /// 魔法祈愿
        /// </summary>
        LuckTravel=113,

        /// <summary>
        /// 魔法时代
        /// </summary>
        MagicEra = 114,
        
        /// <summary>
        /// 幸运扭蛋
        /// </summary>
        LuckyEgg = 115,
        
        /// <summary>
        /// 金秋特惠
        /// </summary>
        AutumnSpecialOffer = 116,
        
        /// <summary>
        /// 魔法宾果
        /// </summary>
        MagicBingo = 117,
        
        /// <summary>
        /// 挑战
        /// </summary>
        FunctionTypeChallengeStage,
        
        /// <summary>
        /// 邀请 / 分享
        /// </summary>
        InviterAndShare = 664,

        //好评引导
        GuidePositiveReviews = 665,

        //兑换码功能
        RedeemCode = 666,

    }

    public enum EActivityType
    {
        /// <summary>
        /// 幸运转盘
        /// </summary>
        ActivityTypeLuckyDraw = 1,
        
              
        /// <summary>
        /// 幸运扭蛋
        /// </summary>
        ActivityLuckEgg = 6,
        /// <summary>
        /// 魔法宾果
        /// </summary>
        MagicBingo = 9,
        
        /// <summary>
        /// 秘林 问卷调查
        /// </summary>
        ActivityTypeSecretSurveys = 10,
      
    }

    public enum EPackageType
    {
        Chapter = 1,
        Crystal = 2,
        Diamond = 3,
        LevelFund = 5,
        CrystalFund = 6,
        Newbie = 7,
        MonthCard = 8,
        Daily = 9,
        Week = 10,
        Month = 11,
        // Yuan = 12,
        CollegeShop = 12,
        FreeBuy = 15,
        AutumnSpecial = 17,
        MagicBingo = 18,
    }

    public enum ELuckyPageBtnType
    {
        /// <summary>
        /// 转盘
        /// </summary>
        PageBtnTypePrizeWheel = 0,

        /// <summary>
        /// 任务
        /// </summary>
        PageBtnTypeTask = 1,
    }

    public enum EquipChestId
    {
        Clothes = 5,
    }

    public class CommonItemTipsData
    {
        private Reward reward;
        private Vector3 pos;
        private Vector3 offset;
        private Action callback;

        public Reward Reward => reward;
        public Vector3 Pos => pos;
        public Vector3 Offset => offset;
        public Action Callback => callback;

        public CommonItemTipsData(Reward reward, Vector3 pos, Vector3 offset,
            Action callback = null)
        {
            this.reward = reward;
            this.pos = pos;
            this.offset = offset;
            this.callback = callback;
        }

        public CommonItemTipsData(Resource reward, Vector3 pos, Vector3 offset,
            Action callback = null)
        {
            this.reward = new Reward((RewardType)reward.Type, reward.Id, reward.Count);
            this.pos = pos;
            this.offset = offset;
            this.callback = callback;
        }

        public CommonItemTipsData(RandomReward reward, Vector3 pos, Vector3 offset,
            Action callback = null)
        {
            this.reward = new Reward(reward.Type, reward.Id, reward.Count);
            this.pos = pos;
            this.offset = offset;
            this.callback = callback;
        }

        public CommonItemTipsData(ResourceData reward, Vector3 pos, Vector3 offset,
            Action callback = null)
        {
            this.reward = new Reward((RewardType)reward.Type, reward.Id, reward.Count);
            this.pos = pos;
            this.offset = offset;
            this.callback = callback;
        }

        public CommonItemTipsData(MailRewards reward, Vector3 pos, Vector3 offset,
            Action callback = null)
        {
            this.reward = new Reward((RewardType)reward.type, reward.id, reward.count);
            this.pos = pos;
            this.offset = offset;
            this.callback = callback;
        }
    }

    public class MainStageIconActionName
    {
        public const string Enter = "chuchang";
        public const string Normal = "daiji";
    }

    public class CommonRuleData
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsShowBg { get; set; }

        /// <summary>
        ///  背景图片路径
        /// </summary>
        public string bgPath { get; set; }

        public CommonRuleData(string title, string content, bool isShowBg)
        {
            Title = title;
            Content = content;
            IsShowBg = isShowBg;
        }
    }

    public class TimerTag
    {
        /// <summary>
        ///  主线 boss 死亡后的 转盘奖励
        /// </summary>
        public const string BossToPrizeWheel = "boss_to_prizewheel";
    }

    //挖矿科技类型  1011.冒险者初始移动速度{0},初始挖矿速度{1}；1012.冒险者移动速度增加%{0}；1013.冒险者挖矿速度增加%{0}；1021.可以实时检测到角色{0}格范围内的资源点和事件
    public enum MiningTechnologyType
    {
        AddInit = 1011, //科技属性初始化值
        AddMoveSp = 1012, //移动速度增加
        AddDigSp = 1013, //挖矿速度增加
        CheckResRange = 1021, //检测范围
    }

    //礼包类型
    public enum EPackageID
    {
        College = 47, //学院基金
        ZeroBuy = 66,//0元购
        LuckEgg = 69,//扭蛋基金
    }


    public enum ERankPlayerInfoType
    {
        /// <summary>
        ///  正常的玩家信息界面
        /// </summary>
        RankPlayInfoTypeNormal,

        /// <summary>
        /// 锦标赛的玩家信息界面
        /// </summary>
        RankPlayInfoTypeChampion,
    }

    public enum ERandomSkillType
    {
        ERandomSkillTypeNormal = 0,

        /// <summary>
        /// 转盘
        /// </summary>
        ERandomSkillTypeWheel,

        /// <summary>
        ///  无尽模式的随机
        /// </summary>
        ERandomSkillTypeEndLess,
    }

    public enum EUiEffectTipType
    {
        /// <summary>
        /// 上下动
        /// </summary>
        UiEffectTipTyPeUpAndDown = 0,

        /// <summary>
        /// 左右动
        /// </summary>
        UiEffectTipTyPeLeftAndRight,
    }

    /// <summary>
    /// 段位动画的icon
    /// </summary>
    public enum ETierAniType
    {
        TierAniAll = 0,
        TierAniEnter,
        TierAniIdle
    }

    public enum EEvolutionTalentState
    {
        EvolutionStateLock,
        EvolutionStateUnLock,
        EvolutionStateMaxLevel,
    }

    //护卫舰商店
    public enum ShopFrigateType
    {
        Low = 1,
        High,
    }

    //护卫舰商店抽奖类型 1-广告免费，2-砖石，3-道具, 4,砖石十连抽，5-道具十连抽
    public enum ShopFrigateLotteryType
    {
        Adv = 1,
        Stone,
        Item,
        TenStone,
        TenItem
    }

    /// <summary>
    /// 活动通用的按钮类型
    /// </summary>
    public enum ECellTabBtnType
    {
        CellTabBntTypeSimulateBattle = 10,
        CellTabBntTypeSimulateRank = 11
    }

    /// <summary>
    /// 新手活动按钮类型
    /// </summary>
    public enum NewcomerSprintTabBtnType
    {
        CellTabBntTypeTask = 1,
        CellTabBntTypeGift = 2,
    }

    public enum ETalentType
    {
        /// <summary>
        /// 所有天赋
        /// </summary>
        TalentTypeNone = 0,

        /// <summary>
        /// 武器天赋
        /// </summary>
        TalentTypeEquip = 1,

        /// <summary>
        /// 效果天赋
        /// </summary>
        TalentTypeEffect = 2,
        /// <summary>
        /// 武器
        /// </summary>
        TalentWeapon = 3,
        
    }

    public enum ERefreshType
    {
        /// <summary>
        /// 不能刷新
        /// </summary>
        RefreshTypeNone = -1,

        /// <summary>
        /// 免费刷新
        /// </summary>
        RefreshTypeFree = 1,

        /// <summary>
        /// 广告刷新
        /// </summary>
        RefreshTypeAd = 2,

        /// <summary>
        /// 钱刷新
        /// </summary>
        RefreshTypeMoney = 3,
    }

    public enum MainStageInfoNodeRightButType
    {
        FuncEnum,//功能按钮-设置 邮件
        MonthCard,//月卡
        Newbie,//新手礼包
        Rank,//排行榜
        TriggerGift,//触发礼包
        Mail,//邮件
        RechargePoint,//计费点
        Passports,//通行证
        DailySpecialOffers,//每日特惠
        SubscribeGift,//关注有礼
        LuckyDraw,//幸运转盘
        MagicDraw,//魔力转盘
        SecretSurveys, //秘林 问卷调查
        College,//魔法学院
        LuckTravel,//幸运之旅
        ActivityFreeBuy,//0元购
        MagicEr,//魔法时代
        LuckyEgg,//幸运扭蛋
        AutumnSpecialOffer,//幸运扭蛋
        BoosterPack,//助力礼包
        Invited,//邀请
        MagicBingo,//魔法宾果
    }

    public enum ERankType
    {
        /// <summary>
        /// 主线排行榜
        /// </summary>
        RankItemMainStage = 1,

        // /// <summary>
        // /// 无尽关卡
        // /// </summary>
        RankItemEndless = 2,
        
        /// <summary>
        /// 秘林排行榜
        /// </summary>
        RankItemSecret = 4,
    }

    public enum ERankPageType
    {
        /// <summary>
        /// 全球排行榜
        /// </summary>
        MainStageRankTypeGlobal = 1,

        /// <summary>
        /// 区域排行榜
        /// </summary>
        MainStageRankTypeLocal = 2
    }

    public enum MonthCardEffectType
    {
        /// <summary>
        /// 扫荡收益+5%（金币/经验）
        /// </summary>
        SweepingGains = 1001,

        /// <summary>
        /// 首次刷新额外获取1把武器
        /// </summary>
        GetExtraWeaponOn1stRefresh = 1002,

        /// <summary>
        /// 开启战斗加速功能
        /// </summary>
        BattleAcceleration = 1003,

        /// <summary>
        /// 快速巡逻X3
        /// </summary>
        PatrolNumsx3 = 1004,

        /// <summary>
        /// 体力上限+20
        /// </summary>
        Stamina20 = 1005,

        /// <summary>
        /// 巡逻收益+15%
        /// </summary>
        PatrolGains15 = 1006,

        /// <summary>
        /// 免看广告得奖励
        /// </summary>
        AdRreeReward = 1007,
        
        /// <summary>
        /// 专属头像
        /// </summary>
        DedicatedAvatar = 1008,
        
        /// <summary>
        /// 金色昵称
        /// </summary>
        GoldenNickname = 1009,
        
        /// <summary>
        /// 永久免广告
        /// </summary>
        ADFreeForever = 1010,
        /// <summary>
        /// 巡逻收益保存时间+10
        /// </summary>
        PatrolTimes = 1011,
    }

    public enum EPassportRewardType
    {
        /// <summary>
        /// 免费
        /// </summary>
        PassportTypeFree,

        /// <summary>
        /// vip
        /// </summary>
        PassportTypeVip
    }

    public enum ESpecialTalentId
    {
        /// <summary>
        /// 随机1件装备合成等级提升1级
        /// </summary>
        RandomEquipUpgrade = 104
    }

    public enum EPassPortType
    {
        /// <summary>
        /// 特惠  通行证
        /// </summary>
        PassportTypeDiscount = 1,

        /// <summary>
        /// 砖石 通行证
        /// </summary>
        PassportTypeStone = 2,
        /// <summary>
        /// 章节 通行证
        /// </summary>
        PassportTypeChapter = 3
    }

    public enum EHeadShowType
    {
        /// <summary>
        /// 静态
        /// </summary>
        HeadShowTypeStatic = 1,

        /// <summary>
        ///  动态
        /// </summary>
        HeadShowTypeDynamic = 2
    }

    public enum ELuckDrawOpType
    {
        /// <summary>
        /// 1-广告抽奖 
        /// </summary>
        LuckDrawOpAd = 1,

        /// <summary>
        /// 2-道具单抽 
        /// </summary>
        LuckDrawOpOneTimes = 2,

        /// <summary>
        /// 3-道具五连抽
        /// </summary>
        LuckDrawOpFiveTimes = 3,
    }

    /// <summary>
    /// 心愿类型
    /// </summary>
    public enum EWishType
    {
        /// <summary>
        /// 装备
        /// </summary>
        WishTypeEquip = 1,

        /// <summary>
        /// 圣物
        /// </summary>
        WishTypeHoly
    }

    public enum EDrawProgressIndex
    {
        DrawProgressStart = 1,
        DrawProgressCenter,
        DrawProgressEnd,
    }

    public enum EDrawState
    {
        DrawLucky = 1,
        DrawMagic = 2
    }

    public enum ESubTitleBtnState
    {
        /// <summary>
        /// 未解锁
        /// </summary>
        Lock = 0,

        /// <summary>
        /// 解锁未选中
        /// </summary>
        Normal,

        /// <summary>
        /// 解锁选中
        /// </summary>
        Choose
    }

    public enum EActivityShowType
    {
        /// <summary>
        /// 每日挑战
        /// </summary>
        DailyChallenge = 0,

        /// <summary>
        /// 无尽模式
        /// </summary>
        Endless,

        /// <summary>
        /// 秘林探险
        /// </summary>
        Secret
    }

    /// <summary>
    /// 秘林的天赋选择
    /// </summary>
    public enum ESecretTalentRefreshType
    {
        /// <summary>
        /// 初始天赋
        /// </summary>
        Init = 1,
        /// <summary>
        /// 升级天赋
        /// </summary>
        Upgrade,
        /// <summary>
        /// 广告刷新天赋
        /// </summary>
        AdRefresh,
        /// <summary>
        /// 密林道具天赋刷新
        /// </summary>
        SecretItemRefresh
    }

    public enum ActivityStageType
    {
        LuckDraw = 1,//幸运抽奖
        College = 3,//魔法学院
        LuckTravel = 4,//幸运之旅
        LuckEgg = 5,//扭蛋
        MagicBingo = 6,//魔法宾果
        TriggerGiftProgress = 10,//神秘商店累充天数奖励
        
    }
    public enum ExchangeShopType
    {
        LuckEgg = 1,
        MagicBingo = 2,
    }
    public enum ActivityTaskType
    {
        College = 1,//魔法学院
        LuckEgg = 2,//扭蛋
    }
    public enum ActivityFund
    {
        Hyperspace=1,
        College=2,
        LuckEgg=3,
    }
    public enum EBoosterPackState
    {
        /// <summary>
        /// 未锁住
        /// </summary>
        Lock,
        /// <summary>
        /// 可以购买
        /// </summary>
        CanOp,
        /// <summary>
        /// 售空
        /// </summary>
        SoldOut
    }
    

}