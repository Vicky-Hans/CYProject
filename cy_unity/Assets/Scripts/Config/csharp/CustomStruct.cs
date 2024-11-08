using System.Collections.Generic;
using Newtonsoft.Json;
using DH.Config;
using UnityEngine.Scripting;
using MessagePack;
namespace DH.Config 
{
	[Preserve]
	[MessagePackObject]
	public partial class Reward
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public RewardType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "count")]
		public long Count { get; private set; }

		public Reward()
		{
		}

		[SerializationConstructor]
		public Reward(RewardType Type, int Id, long Count)
		{
			this.Type = Type;
			this.Id = Id;
			this.Count = Count;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class Date
	{
		[Key(0)]
		[JsonProperty(PropertyName = "year")]
		public long Year { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "month")]
		public long Month { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "day")]
		public long Day { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "hour")]
		public long Hour { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "minute")]
		public long Minute { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "second")]
		public long Second { get; private set; }

		public Date()
		{
		}

		[SerializationConstructor]
		public Date(long Year, long Month, long Day, long Hour, long Minute, long Second)
		{
			this.Year = Year;
			this.Month = Month;
			this.Day = Day;
			this.Hour = Hour;
			this.Minute = Minute;
			this.Second = Second;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class CYTime
	{
		[Key(0)]
		[JsonProperty(PropertyName = "hour")]
		public long Hour { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "minute")]
		public long Minute { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "second")]
		public long Second { get; private set; }

		public CYTime()
		{
		}

		[SerializationConstructor]
		public CYTime(long Hour, long Minute, long Second)
		{
			this.Hour = Hour;
			this.Minute = Minute;
			this.Second = Second;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class Grinding
	{
		[Key(0)]
		[JsonProperty(PropertyName = "frequency")]
		public int Frequency { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "repetition")]
		public int Repetition { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		public Grinding()
		{
		}

		[SerializationConstructor]
		public Grinding(int Frequency, int Num, int Repetition, int Id, int Type)
		{
			this.Frequency = Frequency;
			this.Num = Num;
			this.Repetition = Repetition;
			this.Id = Id;
			this.Type = Type;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class TourGrinDing
	{
		[Key(0)]
		[JsonProperty(PropertyName = "frequency")]
		public int Frequency { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "repetition")]
		public int Repetition { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "monsterId")]
		public int MonsterId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "enterTypeJackpot")]
		public int EnterTypeJackpot { get; private set; }

		public TourGrinDing()
		{
		}

		[SerializationConstructor]
		public TourGrinDing(int Frequency, int Num, int Repetition, int MonsterId, int EnterTypeJackpot)
		{
			this.Frequency = Frequency;
			this.Num = Num;
			this.Repetition = Repetition;
			this.MonsterId = MonsterId;
			this.EnterTypeJackpot = EnterTypeJackpot;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class Attribute
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public string Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public Attribute()
		{
		}

		[SerializationConstructor]
		public Attribute(string Type, int Value)
		{
			this.Type = Type;
			this.Value = Value;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class AttributeSection
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public string Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "start")]
		public int Start { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "end")]
		public int End { get; private set; }

		public AttributeSection()
		{
		}

		[SerializationConstructor]
		public AttributeSection(string Type, int Start, int End)
		{
			this.Type = Type;
			this.Start = Start;
			this.End = End;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class ShopContent
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public ShopType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		public ShopContent()
		{
		}

		[SerializationConstructor]
		public ShopContent(ShopType Type, int Id)
		{
			this.Type = Type;
			this.Id = Id;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class RandomReward
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public RewardType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "count")]
		public int Count { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		public RandomReward()
		{
		}

		[SerializationConstructor]
		public RandomReward(RewardType Type, int Id, int Count, int Weight)
		{
			this.Type = Type;
			this.Id = Id;
			this.Count = Count;
			this.Weight = Weight;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class RandomRewardMax
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public RewardType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "count")]
		public int Count { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		public RandomRewardMax()
		{
		}

		[SerializationConstructor]
		public RandomRewardMax(RewardType Type, int Id, int Count, int Weight)
		{
			this.Type = Type;
			this.Id = Id;
			this.Count = Count;
			this.Weight = Weight;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class MonsterSkill
	{
		[Key(0)]
		[JsonProperty(PropertyName = "skill")]
		public int Skill { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "time")]
		public int Time { get; private set; }

		public MonsterSkill()
		{
		}

		[SerializationConstructor]
		public MonsterSkill(int Skill, int Time)
		{
			this.Skill = Skill;
			this.Time = Time;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class MonsterAttribute
	{
		[Key(0)]
		[JsonProperty(PropertyName = "monster")]
		public int Monster { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		public MonsterAttribute()
		{
		}

		[SerializationConstructor]
		public MonsterAttribute(int Monster, int Num)
		{
			this.Monster = Monster;
			this.Num = Num;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class MonsterCoord
	{
		[Key(0)]
		[JsonProperty(PropertyName = "abscissa")]
		public int Abscissa { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "vertical")]
		public int Vertical { get; private set; }

		public MonsterCoord()
		{
		}

		[SerializationConstructor]
		public MonsterCoord(int Abscissa, int Vertical)
		{
			this.Abscissa = Abscissa;
			this.Vertical = Vertical;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class Element
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public ElementType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		public Element()
		{
		}

		[SerializationConstructor]
		public Element(ElementType Type, int Id)
		{
			this.Type = Type;
			this.Id = Id;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class WaveReward
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public RewardType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "count")]
		public int Count { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "wave")]
		public int Wave { get; private set; }

		public WaveReward()
		{
		}

		[SerializationConstructor]
		public WaveReward(RewardType Type, int Id, int Count, int Wave)
		{
			this.Type = Type;
			this.Id = Id;
			this.Count = Count;
			this.Wave = Wave;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class WeaponAttr
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public string Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public WeaponAttr()
		{
		}

		[SerializationConstructor]
		public WeaponAttr(string Type, int Value)
		{
			this.Type = Type;
			this.Value = Value;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class WeaponAttrAdd
	{
		[Key(0)]
		[JsonProperty(PropertyName = "content")]
		public int Content { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public string Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public WeaponAttrAdd()
		{
		}

		[SerializationConstructor]
		public WeaponAttrAdd(int Content, string Type, int Value)
		{
			this.Content = Content;
			this.Type = Type;
			this.Value = Value;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class SkillAttr
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "atr")]
		public Attribute Atr { get; private set; }

		public SkillAttr()
		{
		}

		[SerializationConstructor]
		public SkillAttr(int Id, Attribute Atr)
		{
			this.Id = Id;
			this.Atr = Atr;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class Condition
	{
		[Key(0)]
		[JsonProperty(PropertyName = "type")]
		public ConditionType Type { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "count")]
		public long Count { get; private set; }

		public Condition()
		{
		}

		[SerializationConstructor]
		public Condition(ConditionType Type, int Id, long Count)
		{
			this.Type = Type;
			this.Id = Id;
			this.Count = Count;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class EquipAttr
	{
		[Key(0)]
		[JsonProperty(PropertyName = "partId")]
		public int PartId { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "attrType")]
		public string AttrType { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "value")]
		public int Value { get; private set; }

		public EquipAttr()
		{
		}

		[SerializationConstructor]
		public EquipAttr(int PartId, string AttrType, int Value)
		{
			this.PartId = PartId;
			this.AttrType = AttrType;
			this.Value = Value;
		}
	}

	[Preserve]
	[MessagePackObject]
	public partial class SecretGrinDing
	{
		[Key(0)]
		[JsonProperty(PropertyName = "frequency")]
		public int Frequency { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "repetition")]
		public int Repetition { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "monsterId")]
		public int MonsterId { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "enterTypeJackpot")]
		public int EnterTypeJackpot { get; private set; }

		public SecretGrinDing()
		{
		}

		[SerializationConstructor]
		public SecretGrinDing(int Frequency, int Num, int Repetition, int Type, int MonsterId, int EnterTypeJackpot)
		{
			this.Frequency = Frequency;
			this.Num = Num;
			this.Repetition = Repetition;
			this.Type = Type;
			this.MonsterId = MonsterId;
			this.EnterTypeJackpot = EnterTypeJackpot;
		}
	}

}