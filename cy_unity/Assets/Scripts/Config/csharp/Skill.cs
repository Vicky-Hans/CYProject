using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using MessagePack;
using DH.Config;
using System;

using UnityEngine.Scripting;

namespace DH.Config 
{
	[Preserve]
	[DHDataTableObject]
	[MessagePackObject]
	public partial class SkillCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "heroEquipId")]
		public int HeroEquipId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "ChipQua")]
		public int ChipQua { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "equipId")]
		public int EquipId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "skillDetp")]
		public int SkillDetp { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "sort")]
		public int Sort { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "their")]
		public int Their { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "childType")]
		public int ChildType { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "conjureTime")]
		public int ConjureTime { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "target")]
		public int Target { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "tagetType")]
		public int TagetType { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "monsterAlb")]
		public List<MonsterAttribute> MonsterAlb { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "triggerGroup")]
		public string TriggerGroup { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "addTrigger")]
		public List<string> AddTrigger { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "addAttr")]
		public List<string> AddAttr { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "doubleAttr")]
		public List<SkillAttr> DoubleAttr { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "result")]
		public List<Attribute> Result { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "tigger1")]
		public Dictionary<string, int> Tigger1 { get; private set; }

		[Key(18)]
		[JsonProperty(PropertyName = "complete1")]
		public Dictionary<string, int> Complete1 { get; private set; }

		[Key(19)]
		[JsonProperty(PropertyName = "tigger2")]
		public Dictionary<string, int> Tigger2 { get; private set; }

		[Key(20)]
		[JsonProperty(PropertyName = "complete2")]
		public Dictionary<string, int> Complete2 { get; private set; }

		[Key(21)]
		[JsonProperty(PropertyName = "tigger3")]
		public Dictionary<string, int> Tigger3 { get; private set; }

		[Key(22)]
		[JsonProperty(PropertyName = "complete3")]
		public Dictionary<string, int> Complete3 { get; private set; }

		[Key(23)]
		[JsonProperty(PropertyName = "skillAct")]
		public string SkillAct { get; private set; }

		[Key(24)]
		[JsonProperty(PropertyName = "skillEffect")]
		public string SkillEffect { get; private set; }

		[Key(25)]
		[JsonProperty(PropertyName = "skillEffect1")]
		public string SkillEffect1 { get; private set; }

		[Key(26)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		public SkillCfg()
		{
		}

		[SerializationConstructor]
		public SkillCfg(int Id, int HeroEquipId, int ChipQua, int EquipId, int SkillDetp, int Sort, int Their, int ChildType, int ConjureTime, int Target, int TagetType, List<MonsterAttribute> MonsterAlb, string TriggerGroup, List<string> AddTrigger, List<string> AddAttr, List<SkillAttr> DoubleAttr, List<Attribute> Result, Dictionary<string, int> Tigger1, Dictionary<string, int> Complete1, Dictionary<string, int> Tigger2, Dictionary<string, int> Complete2, Dictionary<string, int> Tigger3, Dictionary<string, int> Complete3, string SkillAct, string SkillEffect, string SkillEffect1, string Icon)
		{
			this.Id = Id;
			this.HeroEquipId = HeroEquipId;
			this.ChipQua = ChipQua;
			this.EquipId = EquipId;
			this.SkillDetp = SkillDetp;
			this.Sort = Sort;
			this.Their = Their;
			this.ChildType = ChildType;
			this.ConjureTime = ConjureTime;
			this.Target = Target;
			this.TagetType = TagetType;
			this.MonsterAlb = MonsterAlb;
			this.TriggerGroup = TriggerGroup;
			this.AddTrigger = AddTrigger;
			this.AddAttr = AddAttr;
			this.DoubleAttr = DoubleAttr;
			this.Result = Result;
			this.Tigger1 = Tigger1;
			this.Complete1 = Complete1;
			this.Tigger2 = Tigger2;
			this.Complete2 = Complete2;
			this.Tigger3 = Tigger3;
			this.Complete3 = Complete3;
			this.SkillAct = SkillAct;
			this.SkillEffect = SkillEffect;
			this.SkillEffect1 = SkillEffect1;
			this.Icon = Icon;
		}
	}
	public partial class SkillCfgCollection : CfgCollectionBase<SkillCfg>
	{
		private const string dataPath = "Skill";
		private Dictionary<int, SkillCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SkillCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<SkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SkillCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SkillCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

	}
}