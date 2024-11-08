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
	public partial class TalentCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "des")]
		public string Des { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "talentId")]
		public int TalentId { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "param")]
		public List<int> Param { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "target")]
		public int Target { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "equipSkillId")]
		public int EquipSkillId { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "value")]
		public List<string> Value { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "equipModelId")]
		public int EquipModelId { get; private set; }

		public TalentCfg()
		{
		}

		[SerializationConstructor]
		public TalentCfg(int Id, int Quality, string Des, int Type, int TalentId, List<int> Param, int Target, string Icon, int EquipSkillId, List<string> Value, int EquipModelId)
		{
			this.Id = Id;
			this.Quality = Quality;
			this.Des = Des;
			this.Type = Type;
			this.TalentId = TalentId;
			this.Param = Param;
			this.Target = Target;
			this.Icon = Icon;
			this.EquipSkillId = EquipSkillId;
			this.Value = Value;
			this.EquipModelId = EquipModelId;
		}
	}
	public partial class TalentCfgCollection : CfgCollectionBase<TalentCfg>
	{
		private const string dataPath = "Talent";
		private Dictionary<int, TalentCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<TalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TalentCfg>();
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

			var config = await DataTableManager.LoadTableAsync<TalentCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TalentCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public TalentCfg GetDataById(int id)
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