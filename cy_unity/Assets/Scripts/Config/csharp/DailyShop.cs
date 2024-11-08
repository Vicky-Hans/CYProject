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
	public partial class DailyShopCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "typeId")]
		public int TypeId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "items")]
		public List<Reward> Items { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "limit")]
		public int Limit { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "discount")]
		public List<int> Discount { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "sequ")]
		public int Sequ { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "functionId")]
		public int FunctionId { get; private set; }

		public DailyShopCfg()
		{
		}

		[SerializationConstructor]
		public DailyShopCfg(int Id, int TypeId, List<Reward> Items, int Limit, List<int> Discount, int Sequ, int FunctionId)
		{
			this.Id = Id;
			this.TypeId = TypeId;
			this.Items = Items;
			this.Limit = Limit;
			this.Discount = Discount;
			this.Sequ = Sequ;
			this.FunctionId = FunctionId;
		}
	}
	public partial class DailyShopCfgCollection : CfgCollectionBase<DailyShopCfg>
	{
		private const string dataPath = "DailyShop";
		private Dictionary<int, DailyShopCfg> idMapItems;
		private Dictionary<int, List<DailyShopCfg>> sequMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DailyShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyShopCfg>();
			sequMapItems = new Dictionary<int, List<DailyShopCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(sequMapItems, item.Sequ, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<DailyShopCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyShopCfg>();
			sequMapItems = new Dictionary<int, List<DailyShopCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(sequMapItems, item.Sequ, item);
			}

			loaded = true;
		}

		public DailyShopCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DailyShopCfg> GetDataBySequ(int sequ)
		{
			Load();

			if(!sequMapItems.TryGetValue(sequ, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}