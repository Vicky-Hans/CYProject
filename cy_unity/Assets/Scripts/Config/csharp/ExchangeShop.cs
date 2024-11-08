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
	public partial class ExchangeShopCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "typeId")]
		public int TypeId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "buyLimit")]
		public int BuyLimit { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "Consumption")]
		public List<Reward> Consumption { get; private set; }

		public ExchangeShopCfg()
		{
		}

		[SerializationConstructor]
		public ExchangeShopCfg(int Id, int TypeId, List<Reward> Reward, List<Reward> OptionalReward, int BuyLimit, int Num, List<Reward> Consumption)
		{
			this.Id = Id;
			this.TypeId = TypeId;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
			this.BuyLimit = BuyLimit;
			this.Num = Num;
			this.Consumption = Consumption;
		}
	}
	public partial class ExchangeShopCfgCollection : CfgCollectionBase<ExchangeShopCfg>
	{
		private const string dataPath = "ExchangeShop";
		private Dictionary<int, ExchangeShopCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ExchangeShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ExchangeShopCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ExchangeShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ExchangeShopCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ExchangeShopCfg GetDataById(int id)
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