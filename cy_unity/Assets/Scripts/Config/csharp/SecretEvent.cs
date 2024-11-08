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
	public partial class SecretEventCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "des")]
		public string Des { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "talentId")]
		public int TalentId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "param")]
		public List<int> Param { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "weight")]
		public int Weight { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "season")]
		public List<int> Season { get; private set; }

		public SecretEventCfg()
		{
		}

		[SerializationConstructor]
		public SecretEventCfg(int Id, string Des, int Type, int TalentId, List<int> Param, string Icon, int Weight, List<int> Season)
		{
			this.Id = Id;
			this.Des = Des;
			this.Type = Type;
			this.TalentId = TalentId;
			this.Param = Param;
			this.Icon = Icon;
			this.Weight = Weight;
			this.Season = Season;
		}
	}
	public partial class SecretEventCfgCollection : CfgCollectionBase<SecretEventCfg>
	{
		private const string dataPath = "SecretEvent";
		private Dictionary<int, SecretEventCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretEventCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretEventCfg>();
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

			var config = await DataTableManager.LoadTableAsync<SecretEventCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretEventCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SecretEventCfg GetDataById(int id)
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