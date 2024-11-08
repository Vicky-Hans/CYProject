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
	public partial class HeroLevelCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "levelCost")]
		public List<List<Reward>> LevelCost { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "attrAdd")]
		public List<List<Attribute>> AttrAdd { get; private set; }

		public HeroLevelCfg()
		{
		}

		[SerializationConstructor]
		public HeroLevelCfg(int Id, List<List<Reward>> LevelCost, List<List<Attribute>> AttrAdd)
		{
			this.Id = Id;
			this.LevelCost = LevelCost;
			this.AttrAdd = AttrAdd;
		}
	}
	public partial class HeroLevelCfgCollection : CfgCollectionBase<HeroLevelCfg>
	{
		private const string dataPath = "HeroLevel";
		private Dictionary<int, HeroLevelCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroLevelCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroLevelCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroLevelCfg GetDataById(int id)
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