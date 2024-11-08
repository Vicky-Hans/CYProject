using System.Collections.Generic;
using Newtonsoft.Json;
using DH.Config;
namespace DH.Config 
{
	public enum RewardType
	{
		Item = 1,
		Equip = 2,
		Exp = 3,
		Head = 4,
		Lives = 5,
		DailyPoints = 6,
		WeeklyPoints = 7,
		Skill = 8,
		WeaponSkill = 9,
		Skin = 10,
		Package = 11,
		TourMonster = 12,
		MonsterEnter = 13,
		MonsterCard = 14,
		Holy = 15,
		MagicCredit = 16,
		HeroEquip = 17,
		Secret = 18
	}
	public enum ConditionType
	{
		Boss = 1,
		Survival = 2,
		Pass = 3,
		Item = 4,
		SurplusHp = 5
	}
	public enum EquipPos
	{
		Weapon = 1,
		Console = 2,
		Hatch = 3,
		ValveChamber = 4,
		Propeller = 5,
		Engine = 6
	}
	public enum SpawnType
	{
		All = 1,
		Single = 2
	}
	public enum SkillType
	{
		Auto = 1,
		Normal = 2,
		Active = 3,
		Passive = 4
	}
	public enum MonsterType
	{
		Normal = 0,
		Elite = 1,
		Boss = 2
	}
	public enum SkillQuality
	{
		None = 1,
		Normal = 2,
		Rare = 3,
		Epic = 4,
		Legend = 5
	}
	public enum ShopType
	{
		Package = 1,
		DailyShop = 2,
		EquipChest = 3
	}
	public enum TaskType
	{
		LoginNum = 1,
		HeroEquipNum = 2,
		EquipChestOpenNum = 3,
		KillEnemyNum = 4,
		GemConsume = 5,
		CosLivesNum = 6,
		HeroEquipSNum = 7,
		WatchAdsNum = 8,
		MainlineNum = 9,
		DenseForestNum = 10,
		PatrolNum = 11,
		BingoNum = 12
	}
	public enum ElementType
	{
		Task = 1,
		Package = 2,
		Shop = 3
	}
	public enum GridType
	{
		OneNum = 1,
		LTwoNum = 2,
		HTwoNum = 3,
		LThreeNum = 4,
		HThreeNum = 5,
		LtThreeNum = 6,
		LzThreeNum = 7,
		DFourNum = 8,
		LtFourNum = 9,
		SFourNum = 10
	}
}