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
	public partial class HeroStarCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "starCost")]
		public int StarCost { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "levelLimitAdd")]
		public int LevelLimitAdd { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "attrAdd")]
		public List<List<Attribute>> AttrAdd { get; private set; }

		public HeroStarCfg()
		{
		}

		[SerializationConstructor]
		public HeroStarCfg(int Id, int StarCost, int LevelLimitAdd, List<List<Attribute>> AttrAdd)
		{
			this.Id = Id;
			this.StarCost = StarCost;
			this.LevelLimitAdd = LevelLimitAdd;
			this.AttrAdd = AttrAdd;
		}
	}
	public partial class HeroStarCfgCollection : CfgCollectionBase<HeroStarCfg>
	{
		private const string dataPath = "HeroStar";
		private Dictionary<int, HeroStarCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<HeroStarCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroStarCfg>();
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

			var config = await DataTableManager.LoadTableAsync<HeroStarCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroStarCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroStarCfg GetDataById(int id)
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