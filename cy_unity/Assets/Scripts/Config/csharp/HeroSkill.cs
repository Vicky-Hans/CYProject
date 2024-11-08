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
	public partial class HeroSkillCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "skillType")]
		public int SkillType { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "lvAstrict")]
		public int LvAstrict { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "heroId")]
		public int HeroId { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "heroAttr")]
		public int HeroAttr { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "effect")]
		public int Effect { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "severMade")]
		public int SeverMade { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "value")]
		public List<double> Value { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "equipModelId")]
		public List<int> EquipModelId { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "TalentId")]
		public List<int> TalentId { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "skillId")]
		public int SkillId { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "energy")]
		public int Energy { get; private set; }

		public HeroSkillCfg()
		{
		}

		[SerializationConstructor]
		public HeroSkillCfg(int Id, int SkillType, string Icon, int LvAstrict, int HeroId, int HeroAttr, int Effect, int SeverMade, List<double> Value, List<int> EquipModelId, List<int> TalentId, int SkillId, int Energy)
		{
			this.Id = Id;
			this.SkillType = SkillType;
			this.Icon = Icon;
			this.LvAstrict = LvAstrict;
			this.HeroId = HeroId;
			this.HeroAttr = HeroAttr;
			this.Effect = Effect;
			this.SeverMade = SeverMade;
			this.Value = Value;
			this.EquipModelId = EquipModelId;
			this.TalentId = TalentId;
			this.SkillId = SkillId;
			this.Energy = Energy;
		}
	}
	public partial class HeroSkillCfgCollection : CfgCollectionBase<HeroSkillCfg>
	{
		private const string dataPath = "HeroSkill";
		private Dictionary<int, HeroSkillCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroSkillCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroSkillCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroSkillCfg GetDataById(int id)
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