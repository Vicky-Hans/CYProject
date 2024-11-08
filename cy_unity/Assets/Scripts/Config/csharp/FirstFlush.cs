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
	public partial class FirstFlushCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "reward1")]
		public List<Reward> Reward1 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "reward2")]
		public List<Reward> Reward2 { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "cost")]
		public List<Reward> Cost { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "packageId")]
		public int PackageId { get; private set; }

		public FirstFlushCfg()
		{
		}

		[SerializationConstructor]
		public FirstFlushCfg(int Id, string Icon, List<Reward> Reward, List<Reward> Reward1, List<Reward> Reward2, List<Reward> Cost, int PackageId)
		{
			this.Id = Id;
			this.Icon = Icon;
			this.Reward = Reward;
			this.Reward1 = Reward1;
			this.Reward2 = Reward2;
			this.Cost = Cost;
			this.PackageId = PackageId;
		}
	}
	public partial class FirstFlushCfgCollection : CfgCollectionBase<FirstFlushCfg>
	{
		private const string dataPath = "FirstFlush";
		private Dictionary<int, FirstFlushCfg> idMapItems;
		private Dictionary<int, List<FirstFlushCfg>> packageIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<FirstFlushCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FirstFlushCfg>();
			packageIdMapItems = new Dictionary<int, List<FirstFlushCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(packageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<FirstFlushCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FirstFlushCfg>();
			packageIdMapItems = new Dictionary<int, List<FirstFlushCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(packageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public FirstFlushCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<FirstFlushCfg> GetDataByPackageId(int packageId)
		{
			Load();

			if(!packageIdMapItems.TryGetValue(packageId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}