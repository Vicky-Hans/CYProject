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
	public partial class TriggerGiftCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "frontId")]
		public int FrontId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "buyLimit")]
		public int BuyLimit { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "Sorting")]
		public int Sorting { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "Package")]
		public int Package { get; private set; }

		public TriggerGiftCfg()
		{
		}

		[SerializationConstructor]
		public TriggerGiftCfg(int Id, int NextId, int FrontId, int Type, List<Reward> Reward, List<Reward> OptionalReward, int BuyLimit, int Sorting, int Package)
		{
			this.Id = Id;
			this.NextId = NextId;
			this.FrontId = FrontId;
			this.Type = Type;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
			this.BuyLimit = BuyLimit;
			this.Sorting = Sorting;
			this.Package = Package;
		}
	}
	public partial class TriggerGiftCfgCollection : CfgCollectionBase<TriggerGiftCfg>
	{
		private const string dataPath = "TriggerGift";
		private Dictionary<int, TriggerGiftCfg> idMapItems;
		private Dictionary<int, List<TriggerGiftCfg>> typeMapItems;
		private Dictionary<int, List<TriggerGiftCfg>> PackageMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<TriggerGiftCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TriggerGiftCfg>();
			typeMapItems = new Dictionary<int, List<TriggerGiftCfg>>();
			PackageMapItems = new Dictionary<int, List<TriggerGiftCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
				AddItem(PackageMapItems, item.Package, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<TriggerGiftCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TriggerGiftCfg>();
			typeMapItems = new Dictionary<int, List<TriggerGiftCfg>>();
			PackageMapItems = new Dictionary<int, List<TriggerGiftCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
				AddItem(PackageMapItems, item.Package, item);
			}

			loaded = true;
		}

		public TriggerGiftCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<TriggerGiftCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<TriggerGiftCfg> GetDataByPackage(int Package)
		{
			Load();

			if(!PackageMapItems.TryGetValue(Package, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}