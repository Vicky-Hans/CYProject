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
	public partial class EquipChestCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "item")]
		public List<Reward> Item { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "jackpotId")]
		public List<int> JackpotId { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "itemId")]
		public int ItemId { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "purchase1")]
		public int Purchase1 { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "purchase2")]
		public int Purchase2 { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "exp")]
		public int Exp { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "tCount")]
		public List<int> TCount { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "tQua")]
		public List<int> TQua { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "gmJackpotId")]
		public List<int> GmJackpotId { get; private set; }

		public EquipChestCfg()
		{
		}

		[SerializationConstructor]
		public EquipChestCfg(int Id, int Type, string Name, string Dec, List<Reward> Item, List<int> JackpotId, string Icon, int ItemId, int Purchase1, int Purchase2, int Exp, List<int> TCount, List<int> TQua, List<int> GmJackpotId)
		{
			this.Id = Id;
			this.Type = Type;
			this.Name = Name;
			this.Dec = Dec;
			this.Item = Item;
			this.JackpotId = JackpotId;
			this.Icon = Icon;
			this.ItemId = ItemId;
			this.Purchase1 = Purchase1;
			this.Purchase2 = Purchase2;
			this.Exp = Exp;
			this.TCount = TCount;
			this.TQua = TQua;
			this.GmJackpotId = GmJackpotId;
		}
	}
	public partial class EquipChestCfgCollection : CfgCollectionBase<EquipChestCfg>
	{
		private const string dataPath = "EquipChest";
		private Dictionary<int, EquipChestCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EquipChestCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipChestCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EquipChestCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipChestCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipChestCfg GetDataById(int id)
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