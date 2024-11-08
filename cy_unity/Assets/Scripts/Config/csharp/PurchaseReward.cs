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
	public partial class PurchaseRewardCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "time")]
		public int Time { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "Reward")]
		public List<Reward> Reward { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		public PurchaseRewardCfg()
		{
		}

		[SerializationConstructor]
		public PurchaseRewardCfg(int Id, int Time, List<Reward> Reward, List<Reward> OptionalReward)
		{
			this.Id = Id;
			this.Time = Time;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
		}
	}
	public partial class PurchaseRewardCfgCollection : CfgCollectionBase<PurchaseRewardCfg>
	{
		private const string dataPath = "PurchaseReward";
		private Dictionary<int, PurchaseRewardCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<PurchaseRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PurchaseRewardCfg>();
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

			var config = await DataTableManager.LoadTableAsync<PurchaseRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PurchaseRewardCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public PurchaseRewardCfg GetDataById(int id)
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