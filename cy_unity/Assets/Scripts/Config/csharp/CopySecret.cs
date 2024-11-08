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
	public partial class CopySecretCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextPass")]
		public int NextPass { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "mainId")]
		public int MainId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "battleMap")]
		public string BattleMap { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "mapSize")]
		public List<int> MapSize { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "maxWave")]
		public int MaxWave { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "copyReward1")]
		public List<Reward> CopyReward1 { get; private set; }

		public CopySecretCfg()
		{
		}

		[SerializationConstructor]
		public CopySecretCfg(int Id, int NextPass, int MainId, string BattleMap, List<int> MapSize, int MaxWave, List<Reward> CopyReward1)
		{
			this.Id = Id;
			this.NextPass = NextPass;
			this.MainId = MainId;
			this.BattleMap = BattleMap;
			this.MapSize = MapSize;
			this.MaxWave = MaxWave;
			this.CopyReward1 = CopyReward1;
		}
	}
	public partial class CopySecretCfgCollection : CfgCollectionBase<CopySecretCfg>
	{
		private const string dataPath = "CopySecret";
		private Dictionary<int, CopySecretCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<CopySecretCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopySecretCfg>();
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

			var config = await DataTableManager.LoadTableAsync<CopySecretCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, CopySecretCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public CopySecretCfg GetDataById(int id)
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