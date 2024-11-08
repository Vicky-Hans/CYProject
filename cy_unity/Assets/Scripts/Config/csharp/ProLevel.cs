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
	public partial class ProLevelCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "level")]
		public int Level { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "exp")]
		public int Exp { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "total_exp_1")]
		public int Total_exp_1 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "items")]
		public List<Reward> Items { get; private set; }

		public ProLevelCfg()
		{
		}

		[SerializationConstructor]
		public ProLevelCfg(int Id, int Level, int Exp, int Total_exp_1, List<Reward> Items)
		{
			this.Id = Id;
			this.Level = Level;
			this.Exp = Exp;
			this.Total_exp_1 = Total_exp_1;
			this.Items = Items;
		}
	}
	public partial class ProLevelCfgCollection : CfgCollectionBase<ProLevelCfg>
	{
		private const string dataPath = "ProLevel";
		private Dictionary<int, ProLevelCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ProLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProLevelCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ProLevelCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProLevelCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ProLevelCfg GetDataById(int id)
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