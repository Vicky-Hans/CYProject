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
	public partial class SecretSeasonCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "effect")]
		public List<int> Effect { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "rule")]
		public string Rule { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "pic")]
		public string Pic { get; private set; }

		public SecretSeasonCfg()
		{
		}

		[SerializationConstructor]
		public SecretSeasonCfg(int Id, List<int> Effect, string Rule, string Pic)
		{
			this.Id = Id;
			this.Effect = Effect;
			this.Rule = Rule;
			this.Pic = Pic;
		}
	}
	public partial class SecretSeasonCfgCollection : CfgCollectionBase<SecretSeasonCfg>
	{
		private const string dataPath = "SecretSeason";
		private Dictionary<int, SecretSeasonCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretSeasonCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretSeasonCfg>();
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

			var config = await DataTableManager.LoadTableAsync<SecretSeasonCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretSeasonCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SecretSeasonCfg GetDataById(int id)
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