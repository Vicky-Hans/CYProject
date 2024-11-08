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
	public partial class StageRewardCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "level")]
		public int Level { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "Reward")]
		public List<Reward> Reward { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		public StageRewardCfg()
		{
		}

		[SerializationConstructor]
		public StageRewardCfg(int Id, int Type, int Level, List<Reward> Reward, List<Reward> OptionalReward)
		{
			this.Id = Id;
			this.Type = Type;
			this.Level = Level;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
		}
	}
	public partial class StageRewardCfgCollection : CfgCollectionBase<StageRewardCfg>
	{
		private const string dataPath = "StageReward";
		private Dictionary<int, StageRewardCfg> idMapItems;
		private Dictionary<int, List<StageRewardCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<StageRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, StageRewardCfg>();
			typeMapItems = new Dictionary<int, List<StageRewardCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<StageRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, StageRewardCfg>();
			typeMapItems = new Dictionary<int, List<StageRewardCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public StageRewardCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<StageRewardCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}