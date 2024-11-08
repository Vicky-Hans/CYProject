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
	public partial class HeroEquipSkillCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "severMade")]
		public int SeverMade { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "value")]
		public List<double> Value { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "buffId")]
		public List<int> BuffId { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "quaId")]
		public int QuaId { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "heroEquipId")]
		public int HeroEquipId { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "skillId")]
		public int SkillId { get; private set; }

		public HeroEquipSkillCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipSkillCfg(int Id, string Dec, int SeverMade, List<double> Value, List<int> BuffId, int QuaId, int HeroEquipId, int SkillId)
		{
			this.Id = Id;
			this.Dec = Dec;
			this.SeverMade = SeverMade;
			this.Value = Value;
			this.BuffId = BuffId;
			this.QuaId = QuaId;
			this.HeroEquipId = HeroEquipId;
			this.SkillId = SkillId;
		}
	}
	public partial class HeroEquipSkillCfgCollection : CfgCollectionBase<HeroEquipSkillCfg>
	{
		private const string dataPath = "HeroEquipSkill";
		private Dictionary<int, HeroEquipSkillCfg> idMapItems;
		private Dictionary<int, List<HeroEquipSkillCfg>> heroEquipIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipSkillCfg>();
			heroEquipIdMapItems = new Dictionary<int, List<HeroEquipSkillCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(heroEquipIdMapItems, item.HeroEquipId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<HeroEquipSkillCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipSkillCfg>();
			heroEquipIdMapItems = new Dictionary<int, List<HeroEquipSkillCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(heroEquipIdMapItems, item.HeroEquipId, item);
			}

			loaded = true;
		}

		public HeroEquipSkillCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<HeroEquipSkillCfg> GetDataByHeroEquipId(int heroEquipId)
		{
			Load();

			if(!heroEquipIdMapItems.TryGetValue(heroEquipId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}