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
	public partial class HolyLvCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "holyCost")]
		public int HolyCost { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "coinCost")]
		public List<Reward> CoinCost { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "attrAdd")]
		public List<WeaponAttrAdd> AttrAdd { get; private set; }

		public HolyLvCfg()
		{
		}

		[SerializationConstructor]
		public HolyLvCfg(int Id, int HolyCost, List<Reward> CoinCost, List<WeaponAttrAdd> AttrAdd)
		{
			this.Id = Id;
			this.HolyCost = HolyCost;
			this.CoinCost = CoinCost;
			this.AttrAdd = AttrAdd;
		}
	}
	public partial class HolyLvCfgCollection : CfgCollectionBase<HolyLvCfg>
	{
		private const string dataPath = "HolyLv";
		private Dictionary<int, HolyLvCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HolyLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolyLvCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HolyLvCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HolyLvCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HolyLvCfg GetDataById(int id)
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