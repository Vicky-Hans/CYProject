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
	public partial class SecretSeasonEffectCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "value")]
		public List<int> Value { get; private set; }

		public SecretSeasonEffectCfg()
		{
		}

		[SerializationConstructor]
		public SecretSeasonEffectCfg(int Id, List<int> Value)
		{
			this.Id = Id;
			this.Value = Value;
		}
	}
	public partial class SecretSeasonEffectCfgCollection : CfgCollectionBase<SecretSeasonEffectCfg>
	{
		private const string dataPath = "SecretSeasonEffect";
		private Dictionary<int, SecretSeasonEffectCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<SecretSeasonEffectCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretSeasonEffectCfg>();
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

			var config = await DataTableManager.LoadTableAsync<SecretSeasonEffectCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, SecretSeasonEffectCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public SecretSeasonEffectCfg GetDataById(int id)
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