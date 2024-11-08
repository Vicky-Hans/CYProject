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
	public partial class AgeMagicPackageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "day")]
		public int Day { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "firstId")]
		public int FirstId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "payName")]
		public string PayName { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "PackageId")]
		public int PackageId { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "value")]
		public string Value { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "buyLimit")]
		public int BuyLimit { get; private set; }

		public AgeMagicPackageCfg()
		{
		}

		[SerializationConstructor]
		public AgeMagicPackageCfg(int Id, int Day, int FirstId, string PayName, List<Reward> Reward, List<Reward> OptionalReward, int PackageId, string Value, int Num, int BuyLimit)
		{
			this.Id = Id;
			this.Day = Day;
			this.FirstId = FirstId;
			this.PayName = PayName;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
			this.PackageId = PackageId;
			this.Value = Value;
			this.Num = Num;
			this.BuyLimit = BuyLimit;
		}
	}
	public partial class AgeMagicPackageCfgCollection : CfgCollectionBase<AgeMagicPackageCfg>
	{
		private const string dataPath = "AgeMagicPackage";
		private Dictionary<int, AgeMagicPackageCfg> idMapItems;
		private Dictionary<int, List<AgeMagicPackageCfg>> PackageIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<AgeMagicPackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, AgeMagicPackageCfg>();
			PackageIdMapItems = new Dictionary<int, List<AgeMagicPackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(PackageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<AgeMagicPackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, AgeMagicPackageCfg>();
			PackageIdMapItems = new Dictionary<int, List<AgeMagicPackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(PackageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public AgeMagicPackageCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<AgeMagicPackageCfg> GetDataByPackageId(int PackageId)
		{
			Load();

			if(!PackageIdMapItems.TryGetValue(PackageId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}