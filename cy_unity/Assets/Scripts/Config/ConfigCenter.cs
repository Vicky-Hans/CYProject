/// <summary>
/// 通过配置表自动生成
/// 不要手动修改
/// </summary>

using System.Collections.Generic;
using DH.Config;

namespace DH.Config
{
    public static class ConfigCenter
    {
        public static List<ICfgCollectionBase> Configs = new List<ICfgCollectionBase>();

        public static ActivityCfgCollection ActivityCfgColl;
        public static ActivityFundCfgCollection ActivityFundCfgColl;
        public static ActivityRankingCfgCollection ActivityRankingCfgColl;
        public static ActivityTaskCfgCollection ActivityTaskCfgColl;
        public static ActivityTaskLanguageCfgCollection ActivityTaskLanguageCfgColl;
        public static AgeMagicPackageCfgCollection AgeMagicPackageCfgColl;
        public static AttributesCfgCollection AttributesCfgColl;
        public static AttributesLanguageCfgCollection AttributesLanguageCfgColl;
        public static CopyCfgCollection CopyCfgColl;
        public static CopyEquipCfgCollection CopyEquipCfgColl;
        public static CopyEquipWeightsCfgCollection CopyEquipWeightsCfgColl;
        public static CopyLanguageCfgCollection CopyLanguageCfgColl;
        public static CopyLevelCfgCollection CopyLevelCfgColl;
        public static CopySecretCfgCollection CopySecretCfgColl;
        public static CopyTalentCfgCollection CopyTalentCfgColl;
        public static DailyShopCfgCollection DailyShopCfgColl;
        public static DailySpecialPackageCfgCollection DailySpecialPackageCfgColl;
        public static DailyStageBuffCfgCollection DailyStageBuffCfgColl;
        public static DailyStageBuffLanguageCfgCollection DailyStageBuffLanguageCfgColl;
        public static DailyStageGrindingCfgCollection DailyStageGrindingCfgColl;
        public static DailyStageProgressRewardCfgCollection DailyStageProgressRewardCfgColl;
        public static DailyStageProgressRewardLanguageCfgCollection DailyStageProgressRewardLanguageCfgColl;
        public static DefinesCfgCollection DefinesCfgColl;
        public static DiscountCfgCollection DiscountCfgColl;
        public static EndlessStageGrinDingCfgCollection EndlessStageGrinDingCfgColl;
        public static EndlessStageMonsterLvCfgCollection EndlessStageMonsterLvCfgColl;
        public static EquipCfgCollection EquipCfgColl;
        public static EquipAttrCfgCollection EquipAttrCfgColl;
        public static EquipChestCfgCollection EquipChestCfgColl;
        public static EquipChestLanguageCfgCollection EquipChestLanguageCfgColl;
        public static EquipChestLvCfgCollection EquipChestLvCfgColl;
        public static EquipGridCfgCollection EquipGridCfgColl;
        public static EquipLvCfgCollection EquipLvCfgColl;
        public static EquipModelCfgCollection EquipModelCfgColl;
        public static EquipModelLanguageCfgCollection EquipModelLanguageCfgColl;
        public static EquipSkillCfgCollection EquipSkillCfgColl;
        public static EquipSkillLanguageCfgCollection EquipSkillLanguageCfgColl;
        public static EquipSlotsCfgCollection EquipSlotsCfgColl;
        public static ErrMsgCfgCollection ErrMsgCfgColl;
        public static ExchangeShopCfgCollection ExchangeShopCfgColl;
        public static FirstFlushCfgCollection FirstFlushCfgColl;
        public static FunctionJumpCfgCollection FunctionJumpCfgColl;
        public static FunctionJumpLanguageCfgCollection FunctionJumpLanguageCfgColl;
        public static FunctionOpenCfgCollection FunctionOpenCfgColl;
        public static FunctionOpenLanguageCfgCollection FunctionOpenLanguageCfgColl;
        public static FundRewardsCfgCollection FundRewardsCfgColl;
        public static GiftCodeCfgCollection GiftCodeCfgColl;
        public static GlobalLanguageCfgCollection GlobalLanguageCfgColl;
        public static GuideCfgCollection GuideCfgColl;
        public static GuideLanguageCfgCollection GuideLanguageCfgColl;
        public static HeroEquipBuffLanguageCfgCollection HeroEquipBuffLanguageCfgColl;
        public static HeroEquipLevelUpCfgCollection HeroEquipLevelUpCfgColl;
        public static HeroEquipPartCfgCollection HeroEquipPartCfgColl;
        public static HeroEquipPartLanguageCfgCollection HeroEquipPartLanguageCfgColl;
        public static HeroEquipQuaUpCfgCollection HeroEquipQuaUpCfgColl;
        public static HeroEquipRareCfgCollection HeroEquipRareCfgColl;
        public static HeroEquipResourceCfgCollection HeroEquipResourceCfgColl;
        public static HeroEquipResourceLanguageCfgCollection HeroEquipResourceLanguageCfgColl;
        public static HeroEquipSkillCfgCollection HeroEquipSkillCfgColl;
        public static HeroEquipSkillLanguageCfgCollection HeroEquipSkillLanguageCfgColl;
        public static HeroLevelCfgCollection HeroLevelCfgColl;
        public static HeroMainCfgCollection HeroMainCfgColl;
        public static HeroMainLanguageCfgCollection HeroMainLanguageCfgColl;
        public static HeroSkillCfgCollection HeroSkillCfgColl;
        public static HeroSkillLanguageCfgCollection HeroSkillLanguageCfgColl;
        public static HeroStarCfgCollection HeroStarCfgColl;
        public static HolyCfgCollection HolyCfgColl;
        public static HolyLvCfgCollection HolyLvCfgColl;
        public static HolySkillCfgCollection HolySkillCfgColl;
        public static ItemCfgCollection ItemCfgColl;
        public static ItemLanguageCfgCollection ItemLanguageCfgColl;
        public static JackpotCfgCollection JackpotCfgColl;
        public static JackpotLanguageCfgCollection JackpotLanguageCfgColl;
        public static MailCfgCollection MailCfgColl;
        public static MailLanguageCfgCollection MailLanguageCfgColl;
        public static MainGrindingCfgCollection MainGrindingCfgColl;
        public static MainGrinding100CfgCollection MainGrinding100CfgColl;
        public static MainGrinding150CfgCollection MainGrinding150CfgColl;
        public static MainGrinding200CfgCollection MainGrinding200CfgColl;
        public static MainGrinding50CfgCollection MainGrinding50CfgColl;
        public static MonsterCfgCollection MonsterCfgColl;
        public static MonsterEnterCfgCollection MonsterEnterCfgColl;
        public static MonsterModelCfgCollection MonsterModelCfgColl;
        public static MonthlyVipEffectCfgCollection MonthlyVipEffectCfgColl;
        public static MonthlyVipEffectLanguageCfgCollection MonthlyVipEffectLanguageCfgColl;
        public static MonthlyVipMainCfgCollection MonthlyVipMainCfgColl;
        public static MonthlyVipMainLanguageCfgCollection MonthlyVipMainLanguageCfgColl;
        public static PackageCfgCollection PackageCfgColl;
        public static PackageLanguageCfgCollection PackageLanguageCfgColl;
        public static PrayJackpotCfgCollection PrayJackpotCfgColl;
        public static ProLevelCfgCollection ProLevelCfgColl;
        public static ProPictureCfgCollection ProPictureCfgColl;
        public static ProPictureLanguageCfgCollection ProPictureLanguageCfgColl;
        public static PurchaseRewardCfgCollection PurchaseRewardCfgColl;
        public static QuaCfgCollection QuaCfgColl;
        public static QuaLanguageCfgCollection QuaLanguageCfgColl;
        public static RechargeCfgCollection RechargeCfgColl;
        public static SecretCopyLevelCfgCollection SecretCopyLevelCfgColl;
        public static SecretCopyTalentCfgCollection SecretCopyTalentCfgColl;
        public static SecretEventCfgCollection SecretEventCfgColl;
        public static SecretGrinDingCfgCollection SecretGrinDingCfgColl;
        public static SecretSeasonCfgCollection SecretSeasonCfgColl;
        public static SecretSeasonEffectCfgCollection SecretSeasonEffectCfgColl;
        public static SecretStageMonsterLvCfgCollection SecretStageMonsterLvCfgColl;
        public static ShareRewardProgressCfgCollection ShareRewardProgressCfgColl;
        public static ShopCfgCollection ShopCfgColl;
        public static ShopLanguageCfgCollection ShopLanguageCfgColl;
        public static ShopTabCfgCollection ShopTabCfgColl;
        public static ShopTabLanguageCfgCollection ShopTabLanguageCfgColl;
        public static SkillCfgCollection SkillCfgColl;
        public static StageRewardCfgCollection StageRewardCfgColl;
        public static TalentCfgCollection TalentCfgColl;
        public static TalentLanguageCfgCollection TalentLanguageCfgColl;
        public static TriggerGiftCfgCollection TriggerGiftCfgColl;
        public static TriggerGiftLanguageCfgCollection TriggerGiftLanguageCfgColl;
        public static TriggerGiftTypeCfgCollection TriggerGiftTypeCfgColl;
        public static WaveRewardCfgCollection WaveRewardCfgColl;

        public static void InitConfigs()
        {
            ActivityCfgColl = new ActivityCfgCollection();
            Configs.Add(ActivityCfgColl);

            ActivityFundCfgColl = new ActivityFundCfgCollection();
            Configs.Add(ActivityFundCfgColl);

            ActivityRankingCfgColl = new ActivityRankingCfgCollection();
            Configs.Add(ActivityRankingCfgColl);

            ActivityTaskCfgColl = new ActivityTaskCfgCollection();
            Configs.Add(ActivityTaskCfgColl);

            ActivityTaskLanguageCfgColl = new ActivityTaskLanguageCfgCollection();
            Configs.Add(ActivityTaskLanguageCfgColl);

            AgeMagicPackageCfgColl = new AgeMagicPackageCfgCollection();
            Configs.Add(AgeMagicPackageCfgColl);

            AttributesCfgColl = new AttributesCfgCollection();
            Configs.Add(AttributesCfgColl);

            AttributesLanguageCfgColl = new AttributesLanguageCfgCollection();
            Configs.Add(AttributesLanguageCfgColl);

            CopyCfgColl = new CopyCfgCollection();
            Configs.Add(CopyCfgColl);

            CopyEquipCfgColl = new CopyEquipCfgCollection();
            Configs.Add(CopyEquipCfgColl);

            CopyEquipWeightsCfgColl = new CopyEquipWeightsCfgCollection();
            Configs.Add(CopyEquipWeightsCfgColl);

            CopyLanguageCfgColl = new CopyLanguageCfgCollection();
            Configs.Add(CopyLanguageCfgColl);

            CopyLevelCfgColl = new CopyLevelCfgCollection();
            Configs.Add(CopyLevelCfgColl);

            CopySecretCfgColl = new CopySecretCfgCollection();
            Configs.Add(CopySecretCfgColl);

            CopyTalentCfgColl = new CopyTalentCfgCollection();
            Configs.Add(CopyTalentCfgColl);

            DailyShopCfgColl = new DailyShopCfgCollection();
            Configs.Add(DailyShopCfgColl);

            DailySpecialPackageCfgColl = new DailySpecialPackageCfgCollection();
            Configs.Add(DailySpecialPackageCfgColl);

            DailyStageBuffCfgColl = new DailyStageBuffCfgCollection();
            Configs.Add(DailyStageBuffCfgColl);

            DailyStageBuffLanguageCfgColl = new DailyStageBuffLanguageCfgCollection();
            Configs.Add(DailyStageBuffLanguageCfgColl);

            DailyStageGrindingCfgColl = new DailyStageGrindingCfgCollection();
            Configs.Add(DailyStageGrindingCfgColl);

            DailyStageProgressRewardCfgColl = new DailyStageProgressRewardCfgCollection();
            Configs.Add(DailyStageProgressRewardCfgColl);

            DailyStageProgressRewardLanguageCfgColl = new DailyStageProgressRewardLanguageCfgCollection();
            Configs.Add(DailyStageProgressRewardLanguageCfgColl);

            DefinesCfgColl = new DefinesCfgCollection();
            Configs.Add(DefinesCfgColl);

            DiscountCfgColl = new DiscountCfgCollection();
            Configs.Add(DiscountCfgColl);

            EndlessStageGrinDingCfgColl = new EndlessStageGrinDingCfgCollection();
            Configs.Add(EndlessStageGrinDingCfgColl);

            EndlessStageMonsterLvCfgColl = new EndlessStageMonsterLvCfgCollection();
            Configs.Add(EndlessStageMonsterLvCfgColl);

            EquipCfgColl = new EquipCfgCollection();
            Configs.Add(EquipCfgColl);

            EquipAttrCfgColl = new EquipAttrCfgCollection();
            Configs.Add(EquipAttrCfgColl);

            EquipChestCfgColl = new EquipChestCfgCollection();
            Configs.Add(EquipChestCfgColl);

            EquipChestLanguageCfgColl = new EquipChestLanguageCfgCollection();
            Configs.Add(EquipChestLanguageCfgColl);

            EquipChestLvCfgColl = new EquipChestLvCfgCollection();
            Configs.Add(EquipChestLvCfgColl);

            EquipGridCfgColl = new EquipGridCfgCollection();
            Configs.Add(EquipGridCfgColl);

            EquipLvCfgColl = new EquipLvCfgCollection();
            Configs.Add(EquipLvCfgColl);

            EquipModelCfgColl = new EquipModelCfgCollection();
            Configs.Add(EquipModelCfgColl);

            EquipModelLanguageCfgColl = new EquipModelLanguageCfgCollection();
            Configs.Add(EquipModelLanguageCfgColl);

            EquipSkillCfgColl = new EquipSkillCfgCollection();
            Configs.Add(EquipSkillCfgColl);

            EquipSkillLanguageCfgColl = new EquipSkillLanguageCfgCollection();
            Configs.Add(EquipSkillLanguageCfgColl);

            EquipSlotsCfgColl = new EquipSlotsCfgCollection();
            Configs.Add(EquipSlotsCfgColl);

            ErrMsgCfgColl = new ErrMsgCfgCollection();
            Configs.Add(ErrMsgCfgColl);

            ExchangeShopCfgColl = new ExchangeShopCfgCollection();
            Configs.Add(ExchangeShopCfgColl);

            FirstFlushCfgColl = new FirstFlushCfgCollection();
            Configs.Add(FirstFlushCfgColl);

            FunctionJumpCfgColl = new FunctionJumpCfgCollection();
            Configs.Add(FunctionJumpCfgColl);

            FunctionJumpLanguageCfgColl = new FunctionJumpLanguageCfgCollection();
            Configs.Add(FunctionJumpLanguageCfgColl);

            FunctionOpenCfgColl = new FunctionOpenCfgCollection();
            Configs.Add(FunctionOpenCfgColl);

            FunctionOpenLanguageCfgColl = new FunctionOpenLanguageCfgCollection();
            Configs.Add(FunctionOpenLanguageCfgColl);

            FundRewardsCfgColl = new FundRewardsCfgCollection();
            Configs.Add(FundRewardsCfgColl);

            GiftCodeCfgColl = new GiftCodeCfgCollection();
            Configs.Add(GiftCodeCfgColl);

            GlobalLanguageCfgColl = new GlobalLanguageCfgCollection();
            Configs.Add(GlobalLanguageCfgColl);

            GuideCfgColl = new GuideCfgCollection();
            Configs.Add(GuideCfgColl);

            GuideLanguageCfgColl = new GuideLanguageCfgCollection();
            Configs.Add(GuideLanguageCfgColl);

            HeroEquipBuffLanguageCfgColl = new HeroEquipBuffLanguageCfgCollection();
            Configs.Add(HeroEquipBuffLanguageCfgColl);

            HeroEquipLevelUpCfgColl = new HeroEquipLevelUpCfgCollection();
            Configs.Add(HeroEquipLevelUpCfgColl);

            HeroEquipPartCfgColl = new HeroEquipPartCfgCollection();
            Configs.Add(HeroEquipPartCfgColl);

            HeroEquipPartLanguageCfgColl = new HeroEquipPartLanguageCfgCollection();
            Configs.Add(HeroEquipPartLanguageCfgColl);

            HeroEquipQuaUpCfgColl = new HeroEquipQuaUpCfgCollection();
            Configs.Add(HeroEquipQuaUpCfgColl);

            HeroEquipRareCfgColl = new HeroEquipRareCfgCollection();
            Configs.Add(HeroEquipRareCfgColl);

            HeroEquipResourceCfgColl = new HeroEquipResourceCfgCollection();
            Configs.Add(HeroEquipResourceCfgColl);

            HeroEquipResourceLanguageCfgColl = new HeroEquipResourceLanguageCfgCollection();
            Configs.Add(HeroEquipResourceLanguageCfgColl);

            HeroEquipSkillCfgColl = new HeroEquipSkillCfgCollection();
            Configs.Add(HeroEquipSkillCfgColl);

            HeroEquipSkillLanguageCfgColl = new HeroEquipSkillLanguageCfgCollection();
            Configs.Add(HeroEquipSkillLanguageCfgColl);

            HeroLevelCfgColl = new HeroLevelCfgCollection();
            Configs.Add(HeroLevelCfgColl);

            HeroMainCfgColl = new HeroMainCfgCollection();
            Configs.Add(HeroMainCfgColl);

            HeroMainLanguageCfgColl = new HeroMainLanguageCfgCollection();
            Configs.Add(HeroMainLanguageCfgColl);

            HeroSkillCfgColl = new HeroSkillCfgCollection();
            Configs.Add(HeroSkillCfgColl);

            HeroSkillLanguageCfgColl = new HeroSkillLanguageCfgCollection();
            Configs.Add(HeroSkillLanguageCfgColl);

            HeroStarCfgColl = new HeroStarCfgCollection();
            Configs.Add(HeroStarCfgColl);

            HolyCfgColl = new HolyCfgCollection();
            Configs.Add(HolyCfgColl);

            HolyLvCfgColl = new HolyLvCfgCollection();
            Configs.Add(HolyLvCfgColl);

            HolySkillCfgColl = new HolySkillCfgCollection();
            Configs.Add(HolySkillCfgColl);

            ItemCfgColl = new ItemCfgCollection();
            Configs.Add(ItemCfgColl);

            ItemLanguageCfgColl = new ItemLanguageCfgCollection();
            Configs.Add(ItemLanguageCfgColl);

            JackpotCfgColl = new JackpotCfgCollection();
            Configs.Add(JackpotCfgColl);

            JackpotLanguageCfgColl = new JackpotLanguageCfgCollection();
            Configs.Add(JackpotLanguageCfgColl);

            MailCfgColl = new MailCfgCollection();
            Configs.Add(MailCfgColl);

            MailLanguageCfgColl = new MailLanguageCfgCollection();
            Configs.Add(MailLanguageCfgColl);

            MainGrindingCfgColl = new MainGrindingCfgCollection();
            Configs.Add(MainGrindingCfgColl);

            MainGrinding100CfgColl = new MainGrinding100CfgCollection();
            Configs.Add(MainGrinding100CfgColl);

            MainGrinding150CfgColl = new MainGrinding150CfgCollection();
            Configs.Add(MainGrinding150CfgColl);

            MainGrinding200CfgColl = new MainGrinding200CfgCollection();
            Configs.Add(MainGrinding200CfgColl);

            MainGrinding50CfgColl = new MainGrinding50CfgCollection();
            Configs.Add(MainGrinding50CfgColl);

            MonsterCfgColl = new MonsterCfgCollection();
            Configs.Add(MonsterCfgColl);

            MonsterEnterCfgColl = new MonsterEnterCfgCollection();
            Configs.Add(MonsterEnterCfgColl);

            MonsterModelCfgColl = new MonsterModelCfgCollection();
            Configs.Add(MonsterModelCfgColl);

            MonthlyVipEffectCfgColl = new MonthlyVipEffectCfgCollection();
            Configs.Add(MonthlyVipEffectCfgColl);

            MonthlyVipEffectLanguageCfgColl = new MonthlyVipEffectLanguageCfgCollection();
            Configs.Add(MonthlyVipEffectLanguageCfgColl);

            MonthlyVipMainCfgColl = new MonthlyVipMainCfgCollection();
            Configs.Add(MonthlyVipMainCfgColl);

            MonthlyVipMainLanguageCfgColl = new MonthlyVipMainLanguageCfgCollection();
            Configs.Add(MonthlyVipMainLanguageCfgColl);

            PackageCfgColl = new PackageCfgCollection();
            Configs.Add(PackageCfgColl);

            PackageLanguageCfgColl = new PackageLanguageCfgCollection();
            Configs.Add(PackageLanguageCfgColl);

            PrayJackpotCfgColl = new PrayJackpotCfgCollection();
            Configs.Add(PrayJackpotCfgColl);

            ProLevelCfgColl = new ProLevelCfgCollection();
            Configs.Add(ProLevelCfgColl);

            ProPictureCfgColl = new ProPictureCfgCollection();
            Configs.Add(ProPictureCfgColl);

            ProPictureLanguageCfgColl = new ProPictureLanguageCfgCollection();
            Configs.Add(ProPictureLanguageCfgColl);

            PurchaseRewardCfgColl = new PurchaseRewardCfgCollection();
            Configs.Add(PurchaseRewardCfgColl);

            QuaCfgColl = new QuaCfgCollection();
            Configs.Add(QuaCfgColl);

            QuaLanguageCfgColl = new QuaLanguageCfgCollection();
            Configs.Add(QuaLanguageCfgColl);

            RechargeCfgColl = new RechargeCfgCollection();
            Configs.Add(RechargeCfgColl);

            SecretCopyLevelCfgColl = new SecretCopyLevelCfgCollection();
            Configs.Add(SecretCopyLevelCfgColl);

            SecretCopyTalentCfgColl = new SecretCopyTalentCfgCollection();
            Configs.Add(SecretCopyTalentCfgColl);

            SecretEventCfgColl = new SecretEventCfgCollection();
            Configs.Add(SecretEventCfgColl);

            SecretGrinDingCfgColl = new SecretGrinDingCfgCollection();
            Configs.Add(SecretGrinDingCfgColl);

            SecretSeasonCfgColl = new SecretSeasonCfgCollection();
            Configs.Add(SecretSeasonCfgColl);

            SecretSeasonEffectCfgColl = new SecretSeasonEffectCfgCollection();
            Configs.Add(SecretSeasonEffectCfgColl);

            SecretStageMonsterLvCfgColl = new SecretStageMonsterLvCfgCollection();
            Configs.Add(SecretStageMonsterLvCfgColl);

            ShareRewardProgressCfgColl = new ShareRewardProgressCfgCollection();
            Configs.Add(ShareRewardProgressCfgColl);

            ShopCfgColl = new ShopCfgCollection();
            Configs.Add(ShopCfgColl);

            ShopLanguageCfgColl = new ShopLanguageCfgCollection();
            Configs.Add(ShopLanguageCfgColl);

            ShopTabCfgColl = new ShopTabCfgCollection();
            Configs.Add(ShopTabCfgColl);

            ShopTabLanguageCfgColl = new ShopTabLanguageCfgCollection();
            Configs.Add(ShopTabLanguageCfgColl);

            SkillCfgColl = new SkillCfgCollection();
            Configs.Add(SkillCfgColl);

            StageRewardCfgColl = new StageRewardCfgCollection();
            Configs.Add(StageRewardCfgColl);

            TalentCfgColl = new TalentCfgCollection();
            Configs.Add(TalentCfgColl);

            TalentLanguageCfgColl = new TalentLanguageCfgCollection();
            Configs.Add(TalentLanguageCfgColl);

            TriggerGiftCfgColl = new TriggerGiftCfgCollection();
            Configs.Add(TriggerGiftCfgColl);

            TriggerGiftLanguageCfgColl = new TriggerGiftLanguageCfgCollection();
            Configs.Add(TriggerGiftLanguageCfgColl);

            TriggerGiftTypeCfgColl = new TriggerGiftTypeCfgCollection();
            Configs.Add(TriggerGiftTypeCfgColl);

            WaveRewardCfgColl = new WaveRewardCfgCollection();
            Configs.Add(WaveRewardCfgColl);

        }

        public static void Clear()
        {
            ActivityCfgColl = null;
            ActivityFundCfgColl = null;
            ActivityRankingCfgColl = null;
            ActivityTaskCfgColl = null;
            ActivityTaskLanguageCfgColl = null;
            AgeMagicPackageCfgColl = null;
            AttributesCfgColl = null;
            AttributesLanguageCfgColl = null;
            CopyCfgColl = null;
            CopyEquipCfgColl = null;
            CopyEquipWeightsCfgColl = null;
            CopyLanguageCfgColl = null;
            CopyLevelCfgColl = null;
            CopySecretCfgColl = null;
            CopyTalentCfgColl = null;
            DailyShopCfgColl = null;
            DailySpecialPackageCfgColl = null;
            DailyStageBuffCfgColl = null;
            DailyStageBuffLanguageCfgColl = null;
            DailyStageGrindingCfgColl = null;
            DailyStageProgressRewardCfgColl = null;
            DailyStageProgressRewardLanguageCfgColl = null;
            DefinesCfgColl = null;
            DiscountCfgColl = null;
            EndlessStageGrinDingCfgColl = null;
            EndlessStageMonsterLvCfgColl = null;
            EquipCfgColl = null;
            EquipAttrCfgColl = null;
            EquipChestCfgColl = null;
            EquipChestLanguageCfgColl = null;
            EquipChestLvCfgColl = null;
            EquipGridCfgColl = null;
            EquipLvCfgColl = null;
            EquipModelCfgColl = null;
            EquipModelLanguageCfgColl = null;
            EquipSkillCfgColl = null;
            EquipSkillLanguageCfgColl = null;
            EquipSlotsCfgColl = null;
            ErrMsgCfgColl = null;
            ExchangeShopCfgColl = null;
            FirstFlushCfgColl = null;
            FunctionJumpCfgColl = null;
            FunctionJumpLanguageCfgColl = null;
            FunctionOpenCfgColl = null;
            FunctionOpenLanguageCfgColl = null;
            FundRewardsCfgColl = null;
            GiftCodeCfgColl = null;
            GlobalLanguageCfgColl = null;
            GuideCfgColl = null;
            GuideLanguageCfgColl = null;
            HeroEquipBuffLanguageCfgColl = null;
            HeroEquipLevelUpCfgColl = null;
            HeroEquipPartCfgColl = null;
            HeroEquipPartLanguageCfgColl = null;
            HeroEquipQuaUpCfgColl = null;
            HeroEquipRareCfgColl = null;
            HeroEquipResourceCfgColl = null;
            HeroEquipResourceLanguageCfgColl = null;
            HeroEquipSkillCfgColl = null;
            HeroEquipSkillLanguageCfgColl = null;
            HeroLevelCfgColl = null;
            HeroMainCfgColl = null;
            HeroMainLanguageCfgColl = null;
            HeroSkillCfgColl = null;
            HeroSkillLanguageCfgColl = null;
            HeroStarCfgColl = null;
            HolyCfgColl = null;
            HolyLvCfgColl = null;
            HolySkillCfgColl = null;
            ItemCfgColl = null;
            ItemLanguageCfgColl = null;
            JackpotCfgColl = null;
            JackpotLanguageCfgColl = null;
            MailCfgColl = null;
            MailLanguageCfgColl = null;
            MainGrindingCfgColl = null;
            MainGrinding100CfgColl = null;
            MainGrinding150CfgColl = null;
            MainGrinding200CfgColl = null;
            MainGrinding50CfgColl = null;
            MonsterCfgColl = null;
            MonsterEnterCfgColl = null;
            MonsterModelCfgColl = null;
            MonthlyVipEffectCfgColl = null;
            MonthlyVipEffectLanguageCfgColl = null;
            MonthlyVipMainCfgColl = null;
            MonthlyVipMainLanguageCfgColl = null;
            PackageCfgColl = null;
            PackageLanguageCfgColl = null;
            PrayJackpotCfgColl = null;
            ProLevelCfgColl = null;
            ProPictureCfgColl = null;
            ProPictureLanguageCfgColl = null;
            PurchaseRewardCfgColl = null;
            QuaCfgColl = null;
            QuaLanguageCfgColl = null;
            RechargeCfgColl = null;
            SecretCopyLevelCfgColl = null;
            SecretCopyTalentCfgColl = null;
            SecretEventCfgColl = null;
            SecretGrinDingCfgColl = null;
            SecretSeasonCfgColl = null;
            SecretSeasonEffectCfgColl = null;
            SecretStageMonsterLvCfgColl = null;
            ShareRewardProgressCfgColl = null;
            ShopCfgColl = null;
            ShopLanguageCfgColl = null;
            ShopTabCfgColl = null;
            ShopTabLanguageCfgColl = null;
            SkillCfgColl = null;
            StageRewardCfgColl = null;
            TalentCfgColl = null;
            TalentLanguageCfgColl = null;
            TriggerGiftCfgColl = null;
            TriggerGiftLanguageCfgColl = null;
            TriggerGiftTypeCfgColl = null;
            WaveRewardCfgColl = null;

            Configs.Clear();
        }

        public static string GetGlobalString(string id, params object[] par)
        {
            if (GlobalLanguageCfgColl == null)
                return id;
            var cfg = GlobalLanguageCfgColl.GetDataById(id);
            if (cfg == null)
                return id;
            string output = cfg.Name;
            if (par == null || par.Length == 0)
            {
                return output;
            }
            else
            {
                return string.Format(output, par);
            }
        }
    }
}
