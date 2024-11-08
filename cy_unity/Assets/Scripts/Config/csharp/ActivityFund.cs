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
	public partial class ActivityFundCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "factor")]
		public int Factor { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "freeReward")]
		public List<Reward> FreeReward { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "passReward2")]
		public List<Reward> PassReward2 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		public ActivityFundCfg()
		{
		}

		[SerializationConstructor]
		public ActivityFundCfg(int Id, int Factor, List<Reward> FreeReward, List<Reward> PassReward2, int Type)
		{
			this.Id = Id;
			this.Factor = Factor;
			this.FreeReward = FreeReward;
			this.PassReward2 = PassReward2;
			this.Type = Type;
		}
	}
	public partial class ActivityFundCfgCollection : CfgCollectionBase<ActivityFundCfg>
	{
		private const string dataPath = "ActivityFund";
		private Dictionary<int, ActivityFundCfg> idMapItems;
		private Dictionary<int, List<ActivityFundCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ActivityFundCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityFundCfg>();
			typeMapItems = new Dictionary<int, List<ActivityFundCfg>>();
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

			var config = await DataTableManager.LoadTableAsync<ActivityFundCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityFundCfg>();
			typeMapItems = new Dictionary<int, List<ActivityFundCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public ActivityFundCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<ActivityFundCfg> GetDataByType(int type)
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