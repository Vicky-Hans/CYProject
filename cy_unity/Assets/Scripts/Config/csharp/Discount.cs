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
	public partial class DiscountCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "discount")]
		public int Discount { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "tag")]
		public string Tag { get; private set; }

		public DiscountCfg()
		{
		}

		[SerializationConstructor]
		public DiscountCfg(int Id, int Discount, string Tag)
		{
			this.Id = Id;
			this.Discount = Discount;
			this.Tag = Tag;
		}
	}
	public partial class DiscountCfgCollection : CfgCollectionBase<DiscountCfg>
	{
		private const string dataPath = "Discount";
		private Dictionary<int, DiscountCfg> idMapItems;
		private Dictionary<int, List<DiscountCfg>> discountMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DiscountCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DiscountCfg>();
			discountMapItems = new Dictionary<int, List<DiscountCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(discountMapItems, item.Discount, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<DiscountCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DiscountCfg>();
			discountMapItems = new Dictionary<int, List<DiscountCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(discountMapItems, item.Discount, item);
			}

			loaded = true;
		}

		public DiscountCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DiscountCfg> GetDataByDiscount(int discount)
		{
			Load();

			if(!discountMapItems.TryGetValue(discount, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}