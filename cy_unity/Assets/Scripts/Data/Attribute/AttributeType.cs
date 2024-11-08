/// <summary>
/// 通过Attributes表自动生成
/// 不要手动修改
/// </summary>

using System.Collections.Generic;
namespace DH.Data
{
	public enum AttributeType
	{
		Hp = 1,
		Atk = 2,
		Hit = 3,
		Miss = 4,
		CritRate = 5,
		CritDmg = 6,
		ResRate = 7,
		Cd = 8,
		CdArmor = 9,
		CdRevert = 10,
		Range = 11,
		Spd = 12,
		YSpd = 13,
		HpBonus = 14,
		AttackBonus = 15,
		ArmorPer = 16,
		RevertPer = 17,
		UpArmor = 18,
		UpRevert = 19,
		UpDmg = 20,
		WeaponUpDmg = 21,
		DownDmg = 22,
		WeaponDownDmg = 23,
		UpAtk = 24,
		UpHp = 25,
		OutTime = 26,
		UpAtkSpd = 27,
		UpSpd = 28,
		DownAtkSpd = 29,
		UpCritDmg = 30,
		Num = 31,
		BulletSpeed = 32,
		Pierce = 33,
		BulletSize = 34,
		SpliAngle = 35,
		OverlayNum = 36,
		CatapultNum = 37,
		Kill = 38,
		LaunchNum = 39,
		AtkNum = 40,
		AtkNumCd = 41,
		Hurt = 42,
		HurtTime = 43,
		HurtDmg = 44,
		HurtProb = 45,
		DownSpd = 46,
		PoisonProb = 47,
		PalsyProb = 48,
		DownPoisonInterval = 49,
		Overlay = 50,
		BrulNum = 51,
		FiringDmg = 52,
		HpAdd = 53,
		AtkAdd = 54,
		PhyAtkBonus = 55,
		MagAtkBonus = 56,
		FiringDmgUp = 57,
		DarkAxeCrit = 58,
		BlizzardProb = 59,
		DownCritDmg = 60,
		FiringDmgDown = 61,
		Decelerate = 1001,
		Frozen = 1002,
		Firing = 1003,
		Poisoning = 1004,
		Palsy = 1005,
		AtkSpdGp = 1006,
		RevertAtk = 1007,
		Vertigo = 1008,
		FireDmg = 1009,
		PoisonDmg = 1010,
		ExtraDmg = 1011,
		DecelerateTime = 1012,
		FrozenTime = 1013,
		BrulTime = 1014,
		PoisoningTime = 1015,
		PalsyTime = 1016,
		AtkSpdGpTime = 1017,
		VertigoTime = 1018,
		UpAtkSpdGpTime = 1019,
		NotRevert = 1020,
		EnableCatapult = 1021,
		EnableWaterSword = 1022,
		EnableFireSword = 1023,
		EnableWindDagger = 1024,
		EnablePosionDagger = 1025,
		PoisonInterval = 1026,
		RangeDmg = 1027,
		EnableRangeDmg = 1028,
		RangeDmgDis = 1029,
		EnableRangePoison = 1030,
		RangePoisonInterval = 1031,
		RangePoisonDmg = 1032,
		RangePoisonTime = 1033,
		EnableLightBow = 1034,
		EnableDarkBow = 1035,
		KillProb = 1036,
		BowProb = 1037,
		DownAtk = 1038,
		Duration = 1039,
		EnableLaunch = 1040,
		EnableSplit = 1041,
		SplitNum = 1042,
		EnableDarkDart = 1043,
		EnableWindDart = 1044,
		LoseHp = 1045,
		UpFrozenPro = 1046,
		EnableRevert = 1047,
		DecSkillNum = 1048,
		EnableLandCap = 1049,
		EnableDarkCap = 1050,
		EanbleWindGlove = 1051,
		EanbleFireGlove = 1052,
		EnableCrystal = 1053,
		CrystalRange = 1054,
		MustCrit = 1055,
		EnableCrystalWand = 1056,
		EnableWindWand = 1057,
		EnableBlizzard = 1058,
		BlizzardRange = 1059,
		BlizzardDmg = 1060,
		BlizzardInterval = 1061,
		BlizzardTime = 1062,
		EnablePosionOverlay = 1063,
		EnablePalsyBane = 1064,
		EnableStrongBane = 1065,
		LaunchNumDmg = 1066,
		InjuryLift = 1067,
		BossExtraDmg = 1068,
		EveryEquipUpDmg = 1069,
		UpBlizzardDmg = 1070,
		FireWeaponAtk = 1071,
		FireWeaponNum = 1072,
		AboutPhAtkSpd = 1073,
		UpDarkDmg = 1074,
		AroundUpDmg = 1075,
		AboutArmor = 1076,
		PeerArmor = 1077,
		UpAdjoinAtk = 1078,
		WindLaunchNum = 1079,
		WindLaunchNumDmg = 1080,
		MonsterAtkDown = 1081,
		WindPierce = 1082,
		FireInterval = 1083,
		UpDaggerDmg = 1084,
		SmallDmg = 1085,
		Prob = 1086,
		AttackBonusRange = 1087,
		HpShield = 1088,
		OwnSplit = 1089,
		MaxHpPercent = 1090,
		MaxAtkPercent = 1091,
		AccelerateTime = 1092,
		EnableCircle = 1093,
		UpRange = 1094,
		UpBulletSize = 1095,
		Electrify = 1096,
		ElectrifyTime = 1097,
		ElectrifyDmg = 1098,
		DownCd = 1099,
		DownCdRange = 1100,
		UpHpAdd = 1101,
		UpAtkAdd = 1102,
		RotateSpd = 1103,
		KillMonster = 1104,
		KillNum = 1105,
		ReHit = 1106,
		ReHitDmg = 1107,
		WindCritRate = 1108,
		HpRevert = 1109,
		LightAtkBonus = 1110,
		StopTime = 1111,
		InRange = 1112,
		EnableRoleHpDmg = 1113,
		RoleHpDmg = 1114,
		EnableMaxHpDmg = 1115,
		MaxHpDmg = 1116,
		DmgArmor = 1120,
		DmgRevert = 1131,
		DmgRevertProb = 1132,
		EnableDarkAxe = 1133,
		EnableLandAxe = 1134,
		DarkAxeCritDmg = 1135,
		LandCircleAtkSpd = 1136,
		LandCircleCritRate = 1137,
		LandCircleBulletSize = 1138,
		EnableLandCircle = 1139,
		EnableWaterCircle = 1140,
		DownAtkSpdTime = 1141,
		Fainting = 1142,
		FaintingTime = 1143,
		FaintingProb = 1144,
		WaterDownSpd = 1145,
		WaterDownAtkSpd = 1146,
		DownHit = 1147,
		RoleHpBonus = 1148,
		RoleAttackBonus = 1149,
		UpPhAtk = 1150,
		UpMagicAtk = 1151,
		DownUnderPhAtk = 1152,
		DownUnderMagicAtk = 1153,
		MaxNum = 1154,
		MinNum = 1155,
		Repel = 1156,
		RepelMaxRange = 1157,
		RepelMinRange = 1158,
		RangeDmgDisMin = 1159,
		EnableSynthesisCrit = 1160,
		EnableCritSuperposition = 1161,
		EnableCritSame = 1162,
		EnableCritAll = 1163,
		DownPoisonDmg = 1164,
		SelfDestruct = 1165,
		SelfDestructDmg = 1166,
		EnableBleeding = 1167,
		BleedingDmg = 1168,
		BreakArmorDmg = 1169,
		BreakArmorRange = 1170,
		HeroEquipAttackBonus = 1171,
		HeroEquipHpBonus = 1172,
		UnderPhyAttack = 1173,
		UnderPhy4Attack = 1174,
		PhyAttackMonsterRange = 1175,
		PhyEquipBeUped = 1176,
		BagPhyEquip = 1177,
		PhyEquipMerged = 1178,
		KillMonsternum = 1179,
		HeroUnderAttack = 1180,
		Bleed = 1181,
		BleedTimeUpMax = 1182,
		Bleeding = 1183,
		BleedDmg = 1184,
		BleedTime = 1185,
		BleedProbUp = 1186,
		BleedNum = 1187,
		KillPro = 1188,
		KillProUpMax = 1189,
		ImmuneDmgNum = 1190,
		ImmuneDmg = 1191,
		SilverCoinsNum = 1192,
		UpDmgMax = 1193,
		UnderMagicAttack = 1194,
		Poison = 1195,
		PoisonDmgSec = 1196,
		PoisonTime = 1197,
		PoisonNum = 1198,
		RoundStart = 1199,
		PoisonCircleNum = 1200,
		PoisonCircleRange = 1201,
		PoisonCircleSecDmg = 1202,
		PoisonCircleTime = 1203,
		PassPoisonCircle = 1204,
		UnderMagic4Attack = 1205,
		ShockWaveDmg = 1206,
		ShockWaveNum = 1207,
		ShockWaveSize = 1208,
		ShockWaveSpeed = 1209,
		ShockWaveRepel = 1210,
		ShockWaveRepelPro = 1211,
		ShockWaveRepelRange = 1212,
		Reverting = 1213,
		HeroUnderPoisonMonsterAttack = 1214,
		BagMagicEquip = 1215,
		MagicEquipMerged = 1216,
		PoisonPassRange = 1217,
		PoisonPassSecDmg = 1218,
		PoisonPassTime = 1219,
		MagicEquipBeUped = 1220,
		LightningDmg = 1221,
		LightningSplitNum = 1222,
		LightningSplitDmg = 1223,
		MomsterLightning = 1224,
		HeroDead = 1225,
		EnableRelife = 1226,
		RelifeHp = 1227,
		Relife = 1228,
		HitMonsterHpValueMore = 1229,
		ExtraHp = 1230,
		RoundShieldFirstBroken = 1231,
		ShieldAttackDmg = 1232,
		ShieldRange = 1233,
		ShieldAttackSize = 1234,
		ShieldAttackSpeed = 1235,
		ShieldAttack = 1236,
		RepelPro = 1237,
		RepelRange = 1238,
		ShieldDmg = 1239,
		MissPro = 1240,
		Missing = 1241,
		AtkBonusMax = 1242,
		SameEquipAttackBonus = 1243,
		HeroHpValueMore = 1244,
		LaunchTime = 1245,
		ImmuneHurt = 1246,
		ImmuneFrozen = 1247,
		ImmuneVertigo = 1248,
		OffsetAngle = 1249,
		RoleSpdUp = 1250,
		SandCircleRange = 1251,
		SandCircleTime = 1252,
		UpRoleSpd = 1253,
		Energy = 1254,
		EnergyRateUp = 1255,
		RoleSpd = 1256,
		HeroImmune = 1257,
		HeroImmuneTime = 1258,
		AtkReplay = 1259,
		AtkReplayTime = 1260,
		HpDmg = 1261,
		ImmuneRepel = 1262,
		RevertTime = 1263,
		RevertCd = 1264,
		RevertRange = 1265,
		Stop = 1266,
		ImmunePierce = 1267,
		DashSpeed = 1268,
		DashNum = 1269,
		DashDistance = 1270,
		UpUnderPhAtk = 1271,
		ImmuneDmgTime = 1272,
		EnableStay = 1273,
		StayTime = 1274,
		StayDmg = 1275,
		StayDmgInterval = 1276,
		BulletSizeMax = 1277,
		EnableThump = 1278,
		ThumpDmg = 1279,
		ThumpProb = 1280,
		EnableAllThunder = 1281,
		AllThunderDmg = 1282,
		PalsyWandProb = 1283,
		ThunderWandCrit = 1284,
		PalsyWandStop = 1285,
		PalsyWandStopTime = 1286,
		AllThunderProb = 1287,
		Enwind = 1288,
		EnwindTime = 1289,
		EnwindDmg = 1290,
		EnwindProb = 1291,
		EnableDeathEnwind = 1292,
		DeathEnwindDmg = 1293,
		DeathEnwindProb = 1294,
		HpShieldRange = 1295,
		ImmuneStop = 1296,
		KillTarget = 2001,
		CritTime = 2002,
		HitFrozen = 2003,
		HitDecelerate = 2004,
		HitFire = 2005,
		AsHpValueTh = 2006,
		HitTarget = 2007,
		ReleaseSkill = 2008,
		KillPoisonTarget = 2009,
		HitSameTarget = 2010,
		HitPoisoningTarget = 2011,
		HitTarget1 = 2012,
		BowHitTarget = 2013,
		Launch = 2014,
		HitHurt = 2015,
		UseBlizzard = 2016,
		WhenPoisoning = 2017,
		HitBoss = 2018,
		EveryEquip = 2019,
		SmallHitTarget = 2020,
		SkillNum = 2021,
		HitFrozenOrDec = 2022,
		CrystalHit = 2023,
		HitTargetLittle = 2024,
		DarkBowHit = 2025,
		Dead = 2026,
		DebutTime = 2027,
		MagneticHit = 2028,
		EnableHeroSkill = 2029,
		UnderAttack = 2030,
		WaterCircleHit = 2031,
		WaterCircleHitSlow = 2032,
		WaterCircleHitFrozen = 2033,
		CombatUnderAtk = 2034,
		LongUnderAtk = 2035,
		CollisionTime = 2036,
		PassSandCircle = 2037,
		HitCircleTarget = 2038,
		HeroHpValueLess = 2039,
		MonsterHpValueTh = 2040,
		HitFirstTarget = 2041,
		PalsyWandHit = 2042,
		ThunderWandHit = 2043,
		PalsyWandStopEnd = 2044,
		RangeDmgHit = 2045,
		UnderDmg = 2046,
		UpExp = 5001,
	}

	public static class AttributeName
	{
		public const string Hp = "hp";
		public const string Atk = "atk";
		public const string Hit = "hit";
		public const string Miss = "miss";
		public const string CritRate = "critRate";
		public const string CritDmg = "critDmg";
		public const string ResRate = "resRate";
		public const string Cd = "cd";
		public const string CdArmor = "cdArmor";
		public const string CdRevert = "cdRevert";
		public const string Range = "range";
		public const string Spd = "spd";
		public const string YSpd = "ySpd";
		public const string HpBonus = "hpBonus";
		public const string AttackBonus = "attackBonus";
		public const string ArmorPer = "armorPer";
		public const string RevertPer = "revertPer";
		public const string UpArmor = "upArmor";
		public const string UpRevert = "upRevert";
		public const string UpDmg = "upDmg";
		public const string WeaponUpDmg = "weaponUpDmg";
		public const string DownDmg = "downDmg";
		public const string WeaponDownDmg = "weaponDownDmg";
		public const string UpAtk = "upAtk";
		public const string UpHp = "upHp";
		public const string OutTime = "outTime";
		public const string UpAtkSpd = "upAtkSpd";
		public const string UpSpd = "upSpd";
		public const string DownAtkSpd = "downAtkSpd";
		public const string UpCritDmg = "upCritDmg";
		public const string Num = "num";
		public const string BulletSpeed = "bulletSpeed";
		public const string Pierce = "pierce";
		public const string BulletSize = "bulletSize";
		public const string SpliAngle = "spliAngle";
		public const string OverlayNum = "overlayNum";
		public const string CatapultNum = "catapultNum";
		public const string Kill = "kill";
		public const string LaunchNum = "launchNum";
		public const string AtkNum = "atkNum";
		public const string AtkNumCd = "atkNumCd";
		public const string Hurt = "hurt";
		public const string HurtTime = "hurtTime";
		public const string HurtDmg = "hurtDmg";
		public const string HurtProb = "hurtProb";
		public const string DownSpd = "downSpd";
		public const string PoisonProb = "poisonProb";
		public const string PalsyProb = "palsyProb";
		public const string DownPoisonInterval = "downPoisonInterval";
		public const string Overlay = "overlay";
		public const string BrulNum = "brulNum";
		public const string FiringDmg = "firingDmg";
		public const string HpAdd = "HpAdd";
		public const string AtkAdd = "AtkAdd";
		public const string PhyAtkBonus = "PhyAtkBonus";
		public const string MagAtkBonus = "MagAtkBonus";
		public const string FiringDmgUp = "firingDmgUp";
		public const string DarkAxeCrit = "darkAxeCrit";
		public const string BlizzardProb = "blizzardProb";
		public const string DownCritDmg = "downCritDmg";
		public const string FiringDmgDown = "firingDmgDown";
		public const string Decelerate = "decelerate";
		public const string Frozen = "frozen";
		public const string Firing = "firing";
		public const string Poisoning = "poisoning";
		public const string Palsy = "palsy";
		public const string AtkSpdGp = "atkSpdGp";
		public const string RevertAtk = "revertAtk";
		public const string Vertigo = "vertigo";
		public const string FireDmg = "fireDmg";
		public const string PoisonDmg = "poisonDmg";
		public const string ExtraDmg = "extraDmg";
		public const string DecelerateTime = "decelerateTime";
		public const string FrozenTime = "frozenTime";
		public const string BrulTime = "brulTime";
		public const string PoisoningTime = "poisoningTime";
		public const string PalsyTime = "palsyTime";
		public const string AtkSpdGpTime = "atkSpdGpTime";
		public const string VertigoTime = "vertigoTime";
		public const string UpAtkSpdGpTime = "upAtkSpdGpTime";
		public const string NotRevert = "notRevert";
		public const string EnableCatapult = "enableCatapult";
		public const string EnableWaterSword = "enableWaterSword";
		public const string EnableFireSword = "enableFireSword";
		public const string EnableWindDagger = "enableWindDagger";
		public const string EnablePosionDagger = "enablePosionDagger";
		public const string PoisonInterval = "poisonInterval";
		public const string RangeDmg = "rangeDmg";
		public const string EnableRangeDmg = "enableRangeDmg";
		public const string RangeDmgDis = "rangeDmgDis";
		public const string EnableRangePoison = "enableRangePoison";
		public const string RangePoisonInterval = "rangePoisonInterval";
		public const string RangePoisonDmg = "rangePoisonDmg";
		public const string RangePoisonTime = "rangePoisonTime";
		public const string EnableLightBow = "enableLightBow";
		public const string EnableDarkBow = "enableDarkBow";
		public const string KillProb = "killProb";
		public const string BowProb = "bowProb";
		public const string DownAtk = "downAtk";
		public const string Duration = "duration";
		public const string EnableLaunch = "enableLaunch";
		public const string EnableSplit = "enableSplit";
		public const string SplitNum = "splitNum";
		public const string EnableDarkDart = "enableDarkDart";
		public const string EnableWindDart = "enableWindDart";
		public const string LoseHp = "loseHp";
		public const string UpFrozenPro = "upFrozenPro";
		public const string EnableRevert = "enableRevert";
		public const string DecSkillNum = "decSkillNum";
		public const string EnableLandCap = "enableLandCap";
		public const string EnableDarkCap = "enableDarkCap";
		public const string EanbleWindGlove = "eanbleWindGlove";
		public const string EanbleFireGlove = "eanbleFireGlove";
		public const string EnableCrystal = "enableCrystal";
		public const string CrystalRange = "crystalRange";
		public const string MustCrit = "mustCrit";
		public const string EnableCrystalWand = "enableCrystalWand";
		public const string EnableWindWand = "enableWindWand";
		public const string EnableBlizzard = "enableBlizzard";
		public const string BlizzardRange = "blizzardRange";
		public const string BlizzardDmg = "blizzardDmg";
		public const string BlizzardInterval = "blizzardInterval";
		public const string BlizzardTime = "blizzardTime";
		public const string EnablePosionOverlay = "enablePosionOverlay";
		public const string EnablePalsyBane = "enablePalsyBane";
		public const string EnableStrongBane = "enableStrongBane";
		public const string LaunchNumDmg = "launchNumDmg";
		public const string InjuryLift = "InjuryLift";
		public const string BossExtraDmg = "bossExtraDmg";
		public const string EveryEquipUpDmg = "everyEquipUpDmg";
		public const string UpBlizzardDmg = "upBlizzardDmg";
		public const string FireWeaponAtk = "fireWeaponAtk";
		public const string FireWeaponNum = "fireWeaponNum";
		public const string AboutPhAtkSpd = "aboutPhAtkSpd";
		public const string UpDarkDmg = "upDarkDmg";
		public const string AroundUpDmg = "aroundUpDmg";
		public const string AboutArmor = "aboutArmor";
		public const string PeerArmor = "peerArmor";
		public const string UpAdjoinAtk = "upAdjoinAtk";
		public const string WindLaunchNum = "windLaunchNum";
		public const string WindLaunchNumDmg = "windLaunchNumDmg";
		public const string MonsterAtkDown = "monsterAtkDown";
		public const string WindPierce = "windPierce";
		public const string FireInterval = "fireInterval";
		public const string UpDaggerDmg = "upDaggerDmg";
		public const string SmallDmg = "smallDmg";
		public const string Prob = "prob";
		public const string AttackBonusRange = "attackBonusRange";
		public const string HpShield = "hpShield";
		public const string OwnSplit = "ownSplit";
		public const string MaxHpPercent = "maxHpPercent";
		public const string MaxAtkPercent = "maxAtkPercent";
		public const string AccelerateTime = "accelerateTime";
		public const string EnableCircle = "enableCircle";
		public const string UpRange = "upRange";
		public const string UpBulletSize = "upBulletSize";
		public const string Electrify = "electrify";
		public const string ElectrifyTime = "electrifyTime";
		public const string ElectrifyDmg = "electrifyDmg";
		public const string DownCd = "downCd";
		public const string DownCdRange = "downCdRange";
		public const string UpHpAdd = "upHpAdd";
		public const string UpAtkAdd = "upAtkAdd";
		public const string RotateSpd = "rotateSpd";
		public const string KillMonster = "killMonster";
		public const string KillNum = "killNum";
		public const string ReHit = "reHit";
		public const string ReHitDmg = "reHitDmg";
		public const string WindCritRate = "WindCritRate";
		public const string HpRevert = "hpRevert";
		public const string LightAtkBonus = "lightAtkBonus";
		public const string StopTime = "stopTime";
		public const string InRange = "inRange";
		public const string EnableRoleHpDmg = "enableRoleHpDmg";
		public const string RoleHpDmg = "roleHpDmg";
		public const string EnableMaxHpDmg = "enableMaxHpDmg";
		public const string MaxHpDmg = "maxHpDmg";
		public const string DmgArmor = "dmgArmor";
		public const string DmgRevert = "dmgRevert";
		public const string DmgRevertProb = "dmgRevertProb";
		public const string EnableDarkAxe = "enableDarkAxe";
		public const string EnableLandAxe = "enableLandAxe";
		public const string DarkAxeCritDmg = "darkAxeCritDmg";
		public const string LandCircleAtkSpd = "landCircleAtkSpd";
		public const string LandCircleCritRate = "landCircleCritRate";
		public const string LandCircleBulletSize = "landCircleBulletSize";
		public const string EnableLandCircle = "enableLandCircle";
		public const string EnableWaterCircle = "enableWaterCircle";
		public const string DownAtkSpdTime = "downAtkSpdTime";
		public const string Fainting = "fainting";
		public const string FaintingTime = "faintingTime";
		public const string FaintingProb = "faintingProb";
		public const string WaterDownSpd = "waterDownSpd";
		public const string WaterDownAtkSpd = "waterDownAtkSpd";
		public const string DownHit = "downHit";
		public const string RoleHpBonus = "roleHpBonus";
		public const string RoleAttackBonus = "roleAttackBonus";
		public const string UpPhAtk = "upPhAtk";
		public const string UpMagicAtk = "upMagicAtk";
		public const string DownUnderPhAtk = "downUnderPhAtk";
		public const string DownUnderMagicAtk = "downUnderMagicAtk";
		public const string MaxNum = "maxNum";
		public const string MinNum = "minNum";
		public const string Repel = "repel";
		public const string RepelMaxRange = "repelMaxRange";
		public const string RepelMinRange = "repelMinRange";
		public const string RangeDmgDisMin = "rangeDmgDisMin";
		public const string EnableSynthesisCrit = "enableSynthesisCrit";
		public const string EnableCritSuperposition = "enableCritSuperposition";
		public const string EnableCritSame = "enableCritSame";
		public const string EnableCritAll = "enableCritAll";
		public const string DownPoisonDmg = "downPoisonDmg";
		public const string SelfDestruct = "selfDestruct";
		public const string SelfDestructDmg = "selfDestructDmg";
		public const string EnableBleeding = "enableBleeding";
		public const string BleedingDmg = "bleedingDmg";
		public const string BreakArmorDmg = "breakArmorDmg";
		public const string BreakArmorRange = "breakArmorRange";
		public const string HeroEquipAttackBonus = "heroEquipAttackBonus";
		public const string HeroEquipHpBonus = "heroEquipHpBonus";
		public const string UnderPhyAttack = "underPhyAttack";
		public const string UnderPhy4Attack = "underPhy4Attack";
		public const string PhyAttackMonsterRange = "phyAttackMonsterRange";
		public const string PhyEquipBeUped = "phyEquipBeUped";
		public const string BagPhyEquip = "bagPhyEquip";
		public const string PhyEquipMerged = "phyEquipMerged";
		public const string KillMonsternum = "killMonsternum";
		public const string HeroUnderAttack = "heroUnderAttack";
		public const string Bleed = "bleed";
		public const string BleedTimeUpMax = "bleedTimeUpMax";
		public const string Bleeding = "bleeding";
		public const string BleedDmg = "bleedDmg";
		public const string BleedTime = "bleedTime";
		public const string BleedProbUp = "bleedProbUp";
		public const string BleedNum = "bleedNum";
		public const string KillPro = "killPro";
		public const string KillProUpMax = "killProUpMax";
		public const string ImmuneDmgNum = "immuneDmgNum";
		public const string ImmuneDmg = "immuneDmg";
		public const string SilverCoinsNum = "silverCoinsNum";
		public const string UpDmgMax = "upDmgMax";
		public const string UnderMagicAttack = "underMagicAttack";
		public const string Poison = "poison";
		public const string PoisonDmgSec = "poisonDmgSec";
		public const string PoisonTime = "poisonTime";
		public const string PoisonNum = "poisonNum";
		public const string RoundStart = "roundStart";
		public const string PoisonCircleNum = "poisonCircleNum";
		public const string PoisonCircleRange = "poisonCircleRange";
		public const string PoisonCircleSecDmg = "poisonCircleSecDmg";
		public const string PoisonCircleTime = "poisonCircleTime";
		public const string PassPoisonCircle = "passPoisonCircle";
		public const string UnderMagic4Attack = "underMagic4Attack";
		public const string ShockWaveDmg = "shockWaveDmg";
		public const string ShockWaveNum = "shockWaveNum";
		public const string ShockWaveSize = "shockWaveSize";
		public const string ShockWaveSpeed = "shockWaveSpeed";
		public const string ShockWaveRepel = "shockWaveRepel";
		public const string ShockWaveRepelPro = "shockWaveRepelPro";
		public const string ShockWaveRepelRange = "shockWaveRepelRange";
		public const string Reverting = "reverting";
		public const string HeroUnderPoisonMonsterAttack = "heroUnderPoisonMonsterAttack";
		public const string BagMagicEquip = "bagMagicEquip";
		public const string MagicEquipMerged = "magicEquipMerged";
		public const string PoisonPassRange = "poisonPassRange";
		public const string PoisonPassSecDmg = "poisonPassSecDmg";
		public const string PoisonPassTime = "poisonPassTime";
		public const string MagicEquipBeUped = "magicEquipBeUped";
		public const string LightningDmg = "lightningDmg";
		public const string LightningSplitNum = "lightningSplitNum";
		public const string LightningSplitDmg = "lightningSplitDmg";
		public const string MomsterLightning = "momsterLightning";
		public const string HeroDead = "heroDead";
		public const string EnableRelife = "enableRelife";
		public const string RelifeHp = "relifeHp";
		public const string Relife = "relife";
		public const string HitMonsterHpValueMore = "hitMonsterHpValueMore";
		public const string ExtraHp = "extraHp";
		public const string RoundShieldFirstBroken = "roundShieldFirstBroken";
		public const string ShieldAttackDmg = "shieldAttackDmg";
		public const string ShieldRange = "shieldRange";
		public const string ShieldAttackSize = "shieldAttackSize";
		public const string ShieldAttackSpeed = "shieldAttackSpeed";
		public const string ShieldAttack = "shieldAttack";
		public const string RepelPro = "repelPro";
		public const string RepelRange = "repelRange";
		public const string ShieldDmg = "shieldDmg";
		public const string MissPro = "missPro";
		public const string Missing = "missing";
		public const string AtkBonusMax = "atkBonusMax";
		public const string SameEquipAttackBonus = "sameEquipAttackBonus";
		public const string HeroHpValueMore = "heroHpValueMore";
		public const string LaunchTime = "launchTime";
		public const string ImmuneHurt = "immuneHurt";
		public const string ImmuneFrozen = "immuneFrozen";
		public const string ImmuneVertigo = "immuneVertigo";
		public const string OffsetAngle = "offsetAngle";
		public const string RoleSpdUp = "roleSpdUp";
		public const string SandCircleRange = "sandCircleRange";
		public const string SandCircleTime = "sandCircleTime";
		public const string UpRoleSpd = "upRoleSpd";
		public const string Energy = "energy";
		public const string EnergyRateUp = "energyRateUp";
		public const string RoleSpd = "roleSpd";
		public const string HeroImmune = "heroImmune";
		public const string HeroImmuneTime = "heroImmuneTime";
		public const string AtkReplay = "atkReplay";
		public const string AtkReplayTime = "atkReplayTime";
		public const string HpDmg = "hpDmg";
		public const string ImmuneRepel = "immuneRepel";
		public const string RevertTime = "revertTime";
		public const string RevertCd = "revertCd";
		public const string RevertRange = "revertRange";
		public const string Stop = "stop";
		public const string ImmunePierce = "immunePierce";
		public const string DashSpeed = "dashSpeed";
		public const string DashNum = "dashNum";
		public const string DashDistance = "dashDistance";
		public const string UpUnderPhAtk = "upUnderPhAtk";
		public const string ImmuneDmgTime = "immuneDmgTime";
		public const string EnableStay = "enableStay";
		public const string StayTime = "stayTime";
		public const string StayDmg = "stayDmg";
		public const string StayDmgInterval = "stayDmgInterval";
		public const string BulletSizeMax = "bulletSizeMax";
		public const string EnableThump = "enableThump";
		public const string ThumpDmg = "thumpDmg";
		public const string ThumpProb = "thumpProb";
		public const string EnableAllThunder = "enableAllThunder";
		public const string AllThunderDmg = "allThunderDmg";
		public const string PalsyWandProb = "palsyWandProb";
		public const string ThunderWandCrit = "thunderWandCrit";
		public const string PalsyWandStop = "palsyWandStop";
		public const string PalsyWandStopTime = "palsyWandStopTime";
		public const string AllThunderProb = "allThunderProb";
		public const string Enwind = "enwind";
		public const string EnwindTime = "enwindTime";
		public const string EnwindDmg = "enwindDmg";
		public const string EnwindProb = "enwindProb";
		public const string EnableDeathEnwind = "enableDeathEnwind";
		public const string DeathEnwindDmg = "deathEnwindDmg";
		public const string DeathEnwindProb = "deathEnwindProb";
		public const string HpShieldRange = "hpShieldRange";
		public const string ImmuneStop = "immuneStop";
		public const string KillTarget = "killTarget";
		public const string CritTime = "critTime";
		public const string HitFrozen = "hitFrozen";
		public const string HitDecelerate = "hitDecelerate";
		public const string HitFire = "hitFire";
		public const string AsHpValueTh = "asHpValueTh";
		public const string HitTarget = "hitTarget";
		public const string ReleaseSkill = "releaseSkill";
		public const string KillPoisonTarget = "killPoisonTarget";
		public const string HitSameTarget = "hitSameTarget";
		public const string HitPoisoningTarget = "hitPoisoningTarget";
		public const string HitTarget1 = "hitTarget1";
		public const string BowHitTarget = "bowHitTarget";
		public const string Launch = "launch";
		public const string HitHurt = "hitHurt";
		public const string UseBlizzard = "useBlizzard";
		public const string WhenPoisoning = "whenPoisoning";
		public const string HitBoss = "hitBoss";
		public const string EveryEquip = "everyEquip";
		public const string SmallHitTarget = "smallHitTarget";
		public const string SkillNum = "skillNum";
		public const string HitFrozenOrDec = "hitFrozenOrDec";
		public const string CrystalHit = "crystalHit";
		public const string HitTargetLittle = "hitTargetLittle";
		public const string DarkBowHit = "darkBowHit";
		public const string Dead = "dead";
		public const string DebutTime = "debutTime";
		public const string MagneticHit = "magneticHit";
		public const string EnableHeroSkill = "enableHeroSkill";
		public const string UnderAttack = "underAttack";
		public const string WaterCircleHit = "waterCircleHit";
		public const string WaterCircleHitSlow = "waterCircleHitSlow";
		public const string WaterCircleHitFrozen = "waterCircleHitFrozen";
		public const string CombatUnderAtk = "combatUnderAtk";
		public const string LongUnderAtk = "longUnderAtk";
		public const string CollisionTime = "collisionTime";
		public const string PassSandCircle = "passSandCircle";
		public const string HitCircleTarget = "hitCircleTarget";
		public const string HeroHpValueLess = "heroHpValueLess";
		public const string MonsterHpValueTh = "monsterHpValueTh";
		public const string HitFirstTarget = "hitFirstTarget";
		public const string PalsyWandHit = "palsyWandHit";
		public const string ThunderWandHit = "thunderWandHit";
		public const string PalsyWandStopEnd = "palsyWandStopEnd";
		public const string RangeDmgHit = "rangeDmgHit";
		public const string UnderDmg = "underDmg";
		public const string UpExp = "upExp";

		public static readonly Dictionary<long,string> AttrIdToName = new ()
		{
			{1,Hp},
			{2,Atk},
			{3,Hit},
			{4,Miss},
			{5,CritRate},
			{6,CritDmg},
			{7,ResRate},
			{8,Cd},
			{9,CdArmor},
			{10,CdRevert},
			{11,Range},
			{12,Spd},
			{13,YSpd},
			{14,HpBonus},
			{15,AttackBonus},
			{16,ArmorPer},
			{17,RevertPer},
			{18,UpArmor},
			{19,UpRevert},
			{20,UpDmg},
			{21,WeaponUpDmg},
			{22,DownDmg},
			{23,WeaponDownDmg},
			{24,UpAtk},
			{25,UpHp},
			{26,OutTime},
			{27,UpAtkSpd},
			{28,UpSpd},
			{29,DownAtkSpd},
			{30,UpCritDmg},
			{31,Num},
			{32,BulletSpeed},
			{33,Pierce},
			{34,BulletSize},
			{35,SpliAngle},
			{36,OverlayNum},
			{37,CatapultNum},
			{38,Kill},
			{39,LaunchNum},
			{40,AtkNum},
			{41,AtkNumCd},
			{42,Hurt},
			{43,HurtTime},
			{44,HurtDmg},
			{45,HurtProb},
			{46,DownSpd},
			{47,PoisonProb},
			{48,PalsyProb},
			{49,DownPoisonInterval},
			{50,Overlay},
			{51,BrulNum},
			{52,FiringDmg},
			{53,HpAdd},
			{54,AtkAdd},
			{55,PhyAtkBonus},
			{56,MagAtkBonus},
			{57,FiringDmgUp},
			{58,DarkAxeCrit},
			{59,BlizzardProb},
			{60,DownCritDmg},
			{61,FiringDmgDown},
			{1001,Decelerate},
			{1002,Frozen},
			{1003,Firing},
			{1004,Poisoning},
			{1005,Palsy},
			{1006,AtkSpdGp},
			{1007,RevertAtk},
			{1008,Vertigo},
			{1009,FireDmg},
			{1010,PoisonDmg},
			{1011,ExtraDmg},
			{1012,DecelerateTime},
			{1013,FrozenTime},
			{1014,BrulTime},
			{1015,PoisoningTime},
			{1016,PalsyTime},
			{1017,AtkSpdGpTime},
			{1018,VertigoTime},
			{1019,UpAtkSpdGpTime},
			{1020,NotRevert},
			{1021,EnableCatapult},
			{1022,EnableWaterSword},
			{1023,EnableFireSword},
			{1024,EnableWindDagger},
			{1025,EnablePosionDagger},
			{1026,PoisonInterval},
			{1027,RangeDmg},
			{1028,EnableRangeDmg},
			{1029,RangeDmgDis},
			{1030,EnableRangePoison},
			{1031,RangePoisonInterval},
			{1032,RangePoisonDmg},
			{1033,RangePoisonTime},
			{1034,EnableLightBow},
			{1035,EnableDarkBow},
			{1036,KillProb},
			{1037,BowProb},
			{1038,DownAtk},
			{1039,Duration},
			{1040,EnableLaunch},
			{1041,EnableSplit},
			{1042,SplitNum},
			{1043,EnableDarkDart},
			{1044,EnableWindDart},
			{1045,LoseHp},
			{1046,UpFrozenPro},
			{1047,EnableRevert},
			{1048,DecSkillNum},
			{1049,EnableLandCap},
			{1050,EnableDarkCap},
			{1051,EanbleWindGlove},
			{1052,EanbleFireGlove},
			{1053,EnableCrystal},
			{1054,CrystalRange},
			{1055,MustCrit},
			{1056,EnableCrystalWand},
			{1057,EnableWindWand},
			{1058,EnableBlizzard},
			{1059,BlizzardRange},
			{1060,BlizzardDmg},
			{1061,BlizzardInterval},
			{1062,BlizzardTime},
			{1063,EnablePosionOverlay},
			{1064,EnablePalsyBane},
			{1065,EnableStrongBane},
			{1066,LaunchNumDmg},
			{1067,InjuryLift},
			{1068,BossExtraDmg},
			{1069,EveryEquipUpDmg},
			{1070,UpBlizzardDmg},
			{1071,FireWeaponAtk},
			{1072,FireWeaponNum},
			{1073,AboutPhAtkSpd},
			{1074,UpDarkDmg},
			{1075,AroundUpDmg},
			{1076,AboutArmor},
			{1077,PeerArmor},
			{1078,UpAdjoinAtk},
			{1079,WindLaunchNum},
			{1080,WindLaunchNumDmg},
			{1081,MonsterAtkDown},
			{1082,WindPierce},
			{1083,FireInterval},
			{1084,UpDaggerDmg},
			{1085,SmallDmg},
			{1086,Prob},
			{1087,AttackBonusRange},
			{1088,HpShield},
			{1089,OwnSplit},
			{1090,MaxHpPercent},
			{1091,MaxAtkPercent},
			{1092,AccelerateTime},
			{1093,EnableCircle},
			{1094,UpRange},
			{1095,UpBulletSize},
			{1096,Electrify},
			{1097,ElectrifyTime},
			{1098,ElectrifyDmg},
			{1099,DownCd},
			{1100,DownCdRange},
			{1101,UpHpAdd},
			{1102,UpAtkAdd},
			{1103,RotateSpd},
			{1104,KillMonster},
			{1105,KillNum},
			{1106,ReHit},
			{1107,ReHitDmg},
			{1108,WindCritRate},
			{1109,HpRevert},
			{1110,LightAtkBonus},
			{1111,StopTime},
			{1112,InRange},
			{1113,EnableRoleHpDmg},
			{1114,RoleHpDmg},
			{1115,EnableMaxHpDmg},
			{1116,MaxHpDmg},
			{1120,DmgArmor},
			{1131,DmgRevert},
			{1132,DmgRevertProb},
			{1133,EnableDarkAxe},
			{1134,EnableLandAxe},
			{1135,DarkAxeCritDmg},
			{1136,LandCircleAtkSpd},
			{1137,LandCircleCritRate},
			{1138,LandCircleBulletSize},
			{1139,EnableLandCircle},
			{1140,EnableWaterCircle},
			{1141,DownAtkSpdTime},
			{1142,Fainting},
			{1143,FaintingTime},
			{1144,FaintingProb},
			{1145,WaterDownSpd},
			{1146,WaterDownAtkSpd},
			{1147,DownHit},
			{1148,RoleHpBonus},
			{1149,RoleAttackBonus},
			{1150,UpPhAtk},
			{1151,UpMagicAtk},
			{1152,DownUnderPhAtk},
			{1153,DownUnderMagicAtk},
			{1154,MaxNum},
			{1155,MinNum},
			{1156,Repel},
			{1157,RepelMaxRange},
			{1158,RepelMinRange},
			{1159,RangeDmgDisMin},
			{1160,EnableSynthesisCrit},
			{1161,EnableCritSuperposition},
			{1162,EnableCritSame},
			{1163,EnableCritAll},
			{1164,DownPoisonDmg},
			{1165,SelfDestruct},
			{1166,SelfDestructDmg},
			{1167,EnableBleeding},
			{1168,BleedingDmg},
			{1169,BreakArmorDmg},
			{1170,BreakArmorRange},
			{1171,HeroEquipAttackBonus},
			{1172,HeroEquipHpBonus},
			{1173,UnderPhyAttack},
			{1174,UnderPhy4Attack},
			{1175,PhyAttackMonsterRange},
			{1176,PhyEquipBeUped},
			{1177,BagPhyEquip},
			{1178,PhyEquipMerged},
			{1179,KillMonsternum},
			{1180,HeroUnderAttack},
			{1181,Bleed},
			{1182,BleedTimeUpMax},
			{1183,Bleeding},
			{1184,BleedDmg},
			{1185,BleedTime},
			{1186,BleedProbUp},
			{1187,BleedNum},
			{1188,KillPro},
			{1189,KillProUpMax},
			{1190,ImmuneDmgNum},
			{1191,ImmuneDmg},
			{1192,SilverCoinsNum},
			{1193,UpDmgMax},
			{1194,UnderMagicAttack},
			{1195,Poison},
			{1196,PoisonDmgSec},
			{1197,PoisonTime},
			{1198,PoisonNum},
			{1199,RoundStart},
			{1200,PoisonCircleNum},
			{1201,PoisonCircleRange},
			{1202,PoisonCircleSecDmg},
			{1203,PoisonCircleTime},
			{1204,PassPoisonCircle},
			{1205,UnderMagic4Attack},
			{1206,ShockWaveDmg},
			{1207,ShockWaveNum},
			{1208,ShockWaveSize},
			{1209,ShockWaveSpeed},
			{1210,ShockWaveRepel},
			{1211,ShockWaveRepelPro},
			{1212,ShockWaveRepelRange},
			{1213,Reverting},
			{1214,HeroUnderPoisonMonsterAttack},
			{1215,BagMagicEquip},
			{1216,MagicEquipMerged},
			{1217,PoisonPassRange},
			{1218,PoisonPassSecDmg},
			{1219,PoisonPassTime},
			{1220,MagicEquipBeUped},
			{1221,LightningDmg},
			{1222,LightningSplitNum},
			{1223,LightningSplitDmg},
			{1224,MomsterLightning},
			{1225,HeroDead},
			{1226,EnableRelife},
			{1227,RelifeHp},
			{1228,Relife},
			{1229,HitMonsterHpValueMore},
			{1230,ExtraHp},
			{1231,RoundShieldFirstBroken},
			{1232,ShieldAttackDmg},
			{1233,ShieldRange},
			{1234,ShieldAttackSize},
			{1235,ShieldAttackSpeed},
			{1236,ShieldAttack},
			{1237,RepelPro},
			{1238,RepelRange},
			{1239,ShieldDmg},
			{1240,MissPro},
			{1241,Missing},
			{1242,AtkBonusMax},
			{1243,SameEquipAttackBonus},
			{1244,HeroHpValueMore},
			{1245,LaunchTime},
			{1246,ImmuneHurt},
			{1247,ImmuneFrozen},
			{1248,ImmuneVertigo},
			{1249,OffsetAngle},
			{1250,RoleSpdUp},
			{1251,SandCircleRange},
			{1252,SandCircleTime},
			{1253,UpRoleSpd},
			{1254,Energy},
			{1255,EnergyRateUp},
			{1256,RoleSpd},
			{1257,HeroImmune},
			{1258,HeroImmuneTime},
			{1259,AtkReplay},
			{1260,AtkReplayTime},
			{1261,HpDmg},
			{1262,ImmuneRepel},
			{1263,RevertTime},
			{1264,RevertCd},
			{1265,RevertRange},
			{1266,Stop},
			{1267,ImmunePierce},
			{1268,DashSpeed},
			{1269,DashNum},
			{1270,DashDistance},
			{1271,UpUnderPhAtk},
			{1272,ImmuneDmgTime},
			{1273,EnableStay},
			{1274,StayTime},
			{1275,StayDmg},
			{1276,StayDmgInterval},
			{1277,BulletSizeMax},
			{1278,EnableThump},
			{1279,ThumpDmg},
			{1280,ThumpProb},
			{1281,EnableAllThunder},
			{1282,AllThunderDmg},
			{1283,PalsyWandProb},
			{1284,ThunderWandCrit},
			{1285,PalsyWandStop},
			{1286,PalsyWandStopTime},
			{1287,AllThunderProb},
			{1288,Enwind},
			{1289,EnwindTime},
			{1290,EnwindDmg},
			{1291,EnwindProb},
			{1292,EnableDeathEnwind},
			{1293,DeathEnwindDmg},
			{1294,DeathEnwindProb},
			{1295,HpShieldRange},
			{1296,ImmuneStop},
			{2001,KillTarget},
			{2002,CritTime},
			{2003,HitFrozen},
			{2004,HitDecelerate},
			{2005,HitFire},
			{2006,AsHpValueTh},
			{2007,HitTarget},
			{2008,ReleaseSkill},
			{2009,KillPoisonTarget},
			{2010,HitSameTarget},
			{2011,HitPoisoningTarget},
			{2012,HitTarget1},
			{2013,BowHitTarget},
			{2014,Launch},
			{2015,HitHurt},
			{2016,UseBlizzard},
			{2017,WhenPoisoning},
			{2018,HitBoss},
			{2019,EveryEquip},
			{2020,SmallHitTarget},
			{2021,SkillNum},
			{2022,HitFrozenOrDec},
			{2023,CrystalHit},
			{2024,HitTargetLittle},
			{2025,DarkBowHit},
			{2026,Dead},
			{2027,DebutTime},
			{2028,MagneticHit},
			{2029,EnableHeroSkill},
			{2030,UnderAttack},
			{2031,WaterCircleHit},
			{2032,WaterCircleHitSlow},
			{2033,WaterCircleHitFrozen},
			{2034,CombatUnderAtk},
			{2035,LongUnderAtk},
			{2036,CollisionTime},
			{2037,PassSandCircle},
			{2038,HitCircleTarget},
			{2039,HeroHpValueLess},
			{2040,MonsterHpValueTh},
			{2041,HitFirstTarget},
			{2042,PalsyWandHit},
			{2043,ThunderWandHit},
			{2044,PalsyWandStopEnd},
			{2045,RangeDmgHit},
			{2046,UnderDmg},
			{5001,UpExp},
		};

		public static readonly Dictionary<string,long> AttrNameToId = new ()
		{
			{Hp,1},
			{Atk,2},
			{Hit,3},
			{Miss,4},
			{CritRate,5},
			{CritDmg,6},
			{ResRate,7},
			{Cd,8},
			{CdArmor,9},
			{CdRevert,10},
			{Range,11},
			{Spd,12},
			{YSpd,13},
			{HpBonus,14},
			{AttackBonus,15},
			{ArmorPer,16},
			{RevertPer,17},
			{UpArmor,18},
			{UpRevert,19},
			{UpDmg,20},
			{WeaponUpDmg,21},
			{DownDmg,22},
			{WeaponDownDmg,23},
			{UpAtk,24},
			{UpHp,25},
			{OutTime,26},
			{UpAtkSpd,27},
			{UpSpd,28},
			{DownAtkSpd,29},
			{UpCritDmg,30},
			{Num,31},
			{BulletSpeed,32},
			{Pierce,33},
			{BulletSize,34},
			{SpliAngle,35},
			{OverlayNum,36},
			{CatapultNum,37},
			{Kill,38},
			{LaunchNum,39},
			{AtkNum,40},
			{AtkNumCd,41},
			{Hurt,42},
			{HurtTime,43},
			{HurtDmg,44},
			{HurtProb,45},
			{DownSpd,46},
			{PoisonProb,47},
			{PalsyProb,48},
			{DownPoisonInterval,49},
			{Overlay,50},
			{BrulNum,51},
			{FiringDmg,52},
			{HpAdd,53},
			{AtkAdd,54},
			{PhyAtkBonus,55},
			{MagAtkBonus,56},
			{FiringDmgUp,57},
			{DarkAxeCrit,58},
			{BlizzardProb,59},
			{DownCritDmg,60},
			{FiringDmgDown,61},
			{Decelerate,1001},
			{Frozen,1002},
			{Firing,1003},
			{Poisoning,1004},
			{Palsy,1005},
			{AtkSpdGp,1006},
			{RevertAtk,1007},
			{Vertigo,1008},
			{FireDmg,1009},
			{PoisonDmg,1010},
			{ExtraDmg,1011},
			{DecelerateTime,1012},
			{FrozenTime,1013},
			{BrulTime,1014},
			{PoisoningTime,1015},
			{PalsyTime,1016},
			{AtkSpdGpTime,1017},
			{VertigoTime,1018},
			{UpAtkSpdGpTime,1019},
			{NotRevert,1020},
			{EnableCatapult,1021},
			{EnableWaterSword,1022},
			{EnableFireSword,1023},
			{EnableWindDagger,1024},
			{EnablePosionDagger,1025},
			{PoisonInterval,1026},
			{RangeDmg,1027},
			{EnableRangeDmg,1028},
			{RangeDmgDis,1029},
			{EnableRangePoison,1030},
			{RangePoisonInterval,1031},
			{RangePoisonDmg,1032},
			{RangePoisonTime,1033},
			{EnableLightBow,1034},
			{EnableDarkBow,1035},
			{KillProb,1036},
			{BowProb,1037},
			{DownAtk,1038},
			{Duration,1039},
			{EnableLaunch,1040},
			{EnableSplit,1041},
			{SplitNum,1042},
			{EnableDarkDart,1043},
			{EnableWindDart,1044},
			{LoseHp,1045},
			{UpFrozenPro,1046},
			{EnableRevert,1047},
			{DecSkillNum,1048},
			{EnableLandCap,1049},
			{EnableDarkCap,1050},
			{EanbleWindGlove,1051},
			{EanbleFireGlove,1052},
			{EnableCrystal,1053},
			{CrystalRange,1054},
			{MustCrit,1055},
			{EnableCrystalWand,1056},
			{EnableWindWand,1057},
			{EnableBlizzard,1058},
			{BlizzardRange,1059},
			{BlizzardDmg,1060},
			{BlizzardInterval,1061},
			{BlizzardTime,1062},
			{EnablePosionOverlay,1063},
			{EnablePalsyBane,1064},
			{EnableStrongBane,1065},
			{LaunchNumDmg,1066},
			{InjuryLift,1067},
			{BossExtraDmg,1068},
			{EveryEquipUpDmg,1069},
			{UpBlizzardDmg,1070},
			{FireWeaponAtk,1071},
			{FireWeaponNum,1072},
			{AboutPhAtkSpd,1073},
			{UpDarkDmg,1074},
			{AroundUpDmg,1075},
			{AboutArmor,1076},
			{PeerArmor,1077},
			{UpAdjoinAtk,1078},
			{WindLaunchNum,1079},
			{WindLaunchNumDmg,1080},
			{MonsterAtkDown,1081},
			{WindPierce,1082},
			{FireInterval,1083},
			{UpDaggerDmg,1084},
			{SmallDmg,1085},
			{Prob,1086},
			{AttackBonusRange,1087},
			{HpShield,1088},
			{OwnSplit,1089},
			{MaxHpPercent,1090},
			{MaxAtkPercent,1091},
			{AccelerateTime,1092},
			{EnableCircle,1093},
			{UpRange,1094},
			{UpBulletSize,1095},
			{Electrify,1096},
			{ElectrifyTime,1097},
			{ElectrifyDmg,1098},
			{DownCd,1099},
			{DownCdRange,1100},
			{UpHpAdd,1101},
			{UpAtkAdd,1102},
			{RotateSpd,1103},
			{KillMonster,1104},
			{KillNum,1105},
			{ReHit,1106},
			{ReHitDmg,1107},
			{WindCritRate,1108},
			{HpRevert,1109},
			{LightAtkBonus,1110},
			{StopTime,1111},
			{InRange,1112},
			{EnableRoleHpDmg,1113},
			{RoleHpDmg,1114},
			{EnableMaxHpDmg,1115},
			{MaxHpDmg,1116},
			{DmgArmor,1120},
			{DmgRevert,1131},
			{DmgRevertProb,1132},
			{EnableDarkAxe,1133},
			{EnableLandAxe,1134},
			{DarkAxeCritDmg,1135},
			{LandCircleAtkSpd,1136},
			{LandCircleCritRate,1137},
			{LandCircleBulletSize,1138},
			{EnableLandCircle,1139},
			{EnableWaterCircle,1140},
			{DownAtkSpdTime,1141},
			{Fainting,1142},
			{FaintingTime,1143},
			{FaintingProb,1144},
			{WaterDownSpd,1145},
			{WaterDownAtkSpd,1146},
			{DownHit,1147},
			{RoleHpBonus,1148},
			{RoleAttackBonus,1149},
			{UpPhAtk,1150},
			{UpMagicAtk,1151},
			{DownUnderPhAtk,1152},
			{DownUnderMagicAtk,1153},
			{MaxNum,1154},
			{MinNum,1155},
			{Repel,1156},
			{RepelMaxRange,1157},
			{RepelMinRange,1158},
			{RangeDmgDisMin,1159},
			{EnableSynthesisCrit,1160},
			{EnableCritSuperposition,1161},
			{EnableCritSame,1162},
			{EnableCritAll,1163},
			{DownPoisonDmg,1164},
			{SelfDestruct,1165},
			{SelfDestructDmg,1166},
			{EnableBleeding,1167},
			{BleedingDmg,1168},
			{BreakArmorDmg,1169},
			{BreakArmorRange,1170},
			{HeroEquipAttackBonus,1171},
			{HeroEquipHpBonus,1172},
			{UnderPhyAttack,1173},
			{UnderPhy4Attack,1174},
			{PhyAttackMonsterRange,1175},
			{PhyEquipBeUped,1176},
			{BagPhyEquip,1177},
			{PhyEquipMerged,1178},
			{KillMonsternum,1179},
			{HeroUnderAttack,1180},
			{Bleed,1181},
			{BleedTimeUpMax,1182},
			{Bleeding,1183},
			{BleedDmg,1184},
			{BleedTime,1185},
			{BleedProbUp,1186},
			{BleedNum,1187},
			{KillPro,1188},
			{KillProUpMax,1189},
			{ImmuneDmgNum,1190},
			{ImmuneDmg,1191},
			{SilverCoinsNum,1192},
			{UpDmgMax,1193},
			{UnderMagicAttack,1194},
			{Poison,1195},
			{PoisonDmgSec,1196},
			{PoisonTime,1197},
			{PoisonNum,1198},
			{RoundStart,1199},
			{PoisonCircleNum,1200},
			{PoisonCircleRange,1201},
			{PoisonCircleSecDmg,1202},
			{PoisonCircleTime,1203},
			{PassPoisonCircle,1204},
			{UnderMagic4Attack,1205},
			{ShockWaveDmg,1206},
			{ShockWaveNum,1207},
			{ShockWaveSize,1208},
			{ShockWaveSpeed,1209},
			{ShockWaveRepel,1210},
			{ShockWaveRepelPro,1211},
			{ShockWaveRepelRange,1212},
			{Reverting,1213},
			{HeroUnderPoisonMonsterAttack,1214},
			{BagMagicEquip,1215},
			{MagicEquipMerged,1216},
			{PoisonPassRange,1217},
			{PoisonPassSecDmg,1218},
			{PoisonPassTime,1219},
			{MagicEquipBeUped,1220},
			{LightningDmg,1221},
			{LightningSplitNum,1222},
			{LightningSplitDmg,1223},
			{MomsterLightning,1224},
			{HeroDead,1225},
			{EnableRelife,1226},
			{RelifeHp,1227},
			{Relife,1228},
			{HitMonsterHpValueMore,1229},
			{ExtraHp,1230},
			{RoundShieldFirstBroken,1231},
			{ShieldAttackDmg,1232},
			{ShieldRange,1233},
			{ShieldAttackSize,1234},
			{ShieldAttackSpeed,1235},
			{ShieldAttack,1236},
			{RepelPro,1237},
			{RepelRange,1238},
			{ShieldDmg,1239},
			{MissPro,1240},
			{Missing,1241},
			{AtkBonusMax,1242},
			{SameEquipAttackBonus,1243},
			{HeroHpValueMore,1244},
			{LaunchTime,1245},
			{ImmuneHurt,1246},
			{ImmuneFrozen,1247},
			{ImmuneVertigo,1248},
			{OffsetAngle,1249},
			{RoleSpdUp,1250},
			{SandCircleRange,1251},
			{SandCircleTime,1252},
			{UpRoleSpd,1253},
			{Energy,1254},
			{EnergyRateUp,1255},
			{RoleSpd,1256},
			{HeroImmune,1257},
			{HeroImmuneTime,1258},
			{AtkReplay,1259},
			{AtkReplayTime,1260},
			{HpDmg,1261},
			{ImmuneRepel,1262},
			{RevertTime,1263},
			{RevertCd,1264},
			{RevertRange,1265},
			{Stop,1266},
			{ImmunePierce,1267},
			{DashSpeed,1268},
			{DashNum,1269},
			{DashDistance,1270},
			{UpUnderPhAtk,1271},
			{ImmuneDmgTime,1272},
			{EnableStay,1273},
			{StayTime,1274},
			{StayDmg,1275},
			{StayDmgInterval,1276},
			{BulletSizeMax,1277},
			{EnableThump,1278},
			{ThumpDmg,1279},
			{ThumpProb,1280},
			{EnableAllThunder,1281},
			{AllThunderDmg,1282},
			{PalsyWandProb,1283},
			{ThunderWandCrit,1284},
			{PalsyWandStop,1285},
			{PalsyWandStopTime,1286},
			{AllThunderProb,1287},
			{Enwind,1288},
			{EnwindTime,1289},
			{EnwindDmg,1290},
			{EnwindProb,1291},
			{EnableDeathEnwind,1292},
			{DeathEnwindDmg,1293},
			{DeathEnwindProb,1294},
			{HpShieldRange,1295},
			{ImmuneStop,1296},
			{KillTarget,2001},
			{CritTime,2002},
			{HitFrozen,2003},
			{HitDecelerate,2004},
			{HitFire,2005},
			{AsHpValueTh,2006},
			{HitTarget,2007},
			{ReleaseSkill,2008},
			{KillPoisonTarget,2009},
			{HitSameTarget,2010},
			{HitPoisoningTarget,2011},
			{HitTarget1,2012},
			{BowHitTarget,2013},
			{Launch,2014},
			{HitHurt,2015},
			{UseBlizzard,2016},
			{WhenPoisoning,2017},
			{HitBoss,2018},
			{EveryEquip,2019},
			{SmallHitTarget,2020},
			{SkillNum,2021},
			{HitFrozenOrDec,2022},
			{CrystalHit,2023},
			{HitTargetLittle,2024},
			{DarkBowHit,2025},
			{Dead,2026},
			{DebutTime,2027},
			{MagneticHit,2028},
			{EnableHeroSkill,2029},
			{UnderAttack,2030},
			{WaterCircleHit,2031},
			{WaterCircleHitSlow,2032},
			{WaterCircleHitFrozen,2033},
			{CombatUnderAtk,2034},
			{LongUnderAtk,2035},
			{CollisionTime,2036},
			{PassSandCircle,2037},
			{HitCircleTarget,2038},
			{HeroHpValueLess,2039},
			{MonsterHpValueTh,2040},
			{HitFirstTarget,2041},
			{PalsyWandHit,2042},
			{ThunderWandHit,2043},
			{PalsyWandStopEnd,2044},
			{RangeDmgHit,2045},
			{UnderDmg,2046},
			{UpExp,5001},
		};
	}
}
