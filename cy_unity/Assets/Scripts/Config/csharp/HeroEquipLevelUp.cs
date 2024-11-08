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
	public partial class HeroEquipLevelUpCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "itemNum")]
		public int ItemNum { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "coinNum")]
		public List<Reward> CoinNum { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "equipAttrAdd1")]
		public List<EquipAttr> EquipAttrAdd1 { get; private set; }

		public HeroEquipLevelUpCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipLevelUpCfg(int Id, int ItemNum, List<Reward> CoinNum, List<EquipAttr> EquipAttrAdd1)
		{
			this.Id = Id;
			this.ItemNum = ItemNum;
			this.CoinNum = CoinNum;
			this.EquipAttrAdd1 = EquipAttrAdd1;
		}
	}
	public partial class HeroEquipLevelUpCfgCollection : CfgCollectionBase<HeroEquipLevelUpCfg>
	{
		private const string dataPath = "HeroEquipLevelUp";
		private Dictionary<int, HeroEquipLevelUpCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroEquipLevelUpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipLevelUpCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroEquipLevelUpCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipLevelUpCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroEquipLevelUpCfg GetDataById(int id)
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