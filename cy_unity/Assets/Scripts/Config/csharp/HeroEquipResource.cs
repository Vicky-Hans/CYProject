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
	public partial class HeroEquipResourceCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "partId")]
		public int PartId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "rareType")]
		public int RareType { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "coinPrice")]
		public List<double> CoinPrice { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "gemPrice")]
		public List<double> GemPrice { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "picture")]
		public string Picture { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "act")]
		public string Act { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "heroEquipType")]
		public int HeroEquipType { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "quaType")]
		public int QuaType { get; private set; }

		public HeroEquipResourceCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipResourceCfg(int Id, int PartId, int RareType, string Icon, List<double> CoinPrice, List<double> GemPrice, string Picture, string Act, int HeroEquipType, int QuaType)
		{
			this.Id = Id;
			this.PartId = PartId;
			this.RareType = RareType;
			this.Icon = Icon;
			this.CoinPrice = CoinPrice;
			this.GemPrice = GemPrice;
			this.Picture = Picture;
			this.Act = Act;
			this.HeroEquipType = HeroEquipType;
			this.QuaType = QuaType;
		}
	}
	public partial class HeroEquipResourceCfgCollection : CfgCollectionBase<HeroEquipResourceCfg>
	{
		private const string dataPath = "HeroEquipResource";
		private Dictionary<int, HeroEquipResourceCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipResourceCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipResourceCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroEquipResourceCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipResourceCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroEquipResourceCfg GetDataById(int id)
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