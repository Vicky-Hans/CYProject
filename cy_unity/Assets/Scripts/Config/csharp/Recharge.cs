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
	public partial class RechargeCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "payId")]
		public string PayId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "packageId")]
		public int PackageId { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "price")]
		public double Price { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "priceCn")]
		public string PriceCn { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "priceStr")]
		public string PriceStr { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "priceCnStr")]
		public string PriceCnStr { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "vouchersDollar")]
		public int VouchersDollar { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "diamonds")]
		public int Diamonds { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "points")]
		public int Points { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "items")]
		public List<Reward> Items { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "eventName")]
		public string EventName { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "appsFlyerId")]
		public int AppsFlyerId { get; private set; }

		public RechargeCfg()
		{
		}

		[SerializationConstructor]
		public RechargeCfg(int Id, string PayId, string Name, int PackageId, double Price, string PriceCn, string PriceStr, string PriceCnStr, int VouchersDollar, int Diamonds, int Points, List<Reward> Items, string EventName, int AppsFlyerId)
		{
			this.Id = Id;
			this.PayId = PayId;
			this.Name = Name;
			this.PackageId = PackageId;
			this.Price = Price;
			this.PriceCn = PriceCn;
			this.PriceStr = PriceStr;
			this.PriceCnStr = PriceCnStr;
			this.VouchersDollar = VouchersDollar;
			this.Diamonds = Diamonds;
			this.Points = Points;
			this.Items = Items;
			this.EventName = EventName;
			this.AppsFlyerId = AppsFlyerId;
		}
	}
	public partial class RechargeCfgCollection : CfgCollectionBase<RechargeCfg>
	{
		private const string dataPath = "Recharge";
		private Dictionary<int, RechargeCfg> idMapItems;
		private Dictionary<string, List<RechargeCfg>> payIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<RechargeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, RechargeCfg>();
			payIdMapItems = new Dictionary<string, List<RechargeCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(payIdMapItems, item.PayId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<RechargeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, RechargeCfg>();
			payIdMapItems = new Dictionary<string, List<RechargeCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(payIdMapItems, item.PayId, item);
			}

			loaded = true;
		}

		public RechargeCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<RechargeCfg> GetDataByPayId(string payId)
		{
			Load();

			if(!payIdMapItems.TryGetValue(payId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}