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
	public partial class ItemCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "typeValue")]
		public int TypeValue { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "typeValue1")]
		public int TypeValue1 { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "quality")]
		public int Quality { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "show")]
		public int Show { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "sale")]
		public int Sale { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "coinPrice")]
		public double CoinPrice { get; private set; }

		[Key(11)]
		[JsonProperty(PropertyName = "gemPrice")]
		public double GemPrice { get; private set; }

		[Key(12)]
		[JsonProperty(PropertyName = "jackpotId")]
		public List<int> JackpotId { get; private set; }

		[Key(13)]
		[JsonProperty(PropertyName = "Number")]
		public int Number { get; private set; }

		[Key(14)]
		[JsonProperty(PropertyName = "obtain")]
		public List<int> Obtain { get; private set; }

		[Key(15)]
		[JsonProperty(PropertyName = "overlay")]
		public int Overlay { get; private set; }

		[Key(16)]
		[JsonProperty(PropertyName = "usage")]
		public int Usage { get; private set; }

		[Key(17)]
		[JsonProperty(PropertyName = "minIcon")]
		public string MinIcon { get; private set; }

		public ItemCfg()
		{
		}

		[SerializationConstructor]
		public ItemCfg(int Id, string Name, string Dec, string Icon, int Type, int TypeValue, int TypeValue1, int Quality, int Show, int Sale, double CoinPrice, double GemPrice, List<int> JackpotId, int Number, List<int> Obtain, int Overlay, int Usage, string MinIcon)
		{
			this.Id = Id;
			this.Name = Name;
			this.Dec = Dec;
			this.Icon = Icon;
			this.Type = Type;
			this.TypeValue = TypeValue;
			this.TypeValue1 = TypeValue1;
			this.Quality = Quality;
			this.Show = Show;
			this.Sale = Sale;
			this.CoinPrice = CoinPrice;
			this.GemPrice = GemPrice;
			this.JackpotId = JackpotId;
			this.Number = Number;
			this.Obtain = Obtain;
			this.Overlay = Overlay;
			this.Usage = Usage;
			this.MinIcon = MinIcon;
		}
	}
	public partial class ItemCfgCollection : CfgCollectionBase<ItemCfg>
	{
		private const string dataPath = "Item";
		private Dictionary<int, ItemCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ItemCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ItemCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ItemCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ItemCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ItemCfg GetDataById(int id)
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