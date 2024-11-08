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
	public partial class DailySpecialPackageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "gemValue")]
		public int GemValue { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "gemNum")]
		public List<Reward> GemNum { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "otherRewawds")]
		public List<Reward> OtherRewawds { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "offer")]
		public string Offer { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "packageId")]
		public int PackageId { get; private set; }

		public DailySpecialPackageCfg()
		{
		}

		[SerializationConstructor]
		public DailySpecialPackageCfg(int Id, int Type, int GemValue, List<Reward> GemNum, List<Reward> OtherRewawds, List<Reward> OptionalReward, string Offer, int PackageId)
		{
			this.Id = Id;
			this.Type = Type;
			this.GemValue = GemValue;
			this.GemNum = GemNum;
			this.OtherRewawds = OtherRewawds;
			this.OptionalReward = OptionalReward;
			this.Offer = Offer;
			this.PackageId = PackageId;
		}
	}
	public partial class DailySpecialPackageCfgCollection : CfgCollectionBase<DailySpecialPackageCfg>
	{
		private const string dataPath = "DailySpecialPackage";
		private Dictionary<int, DailySpecialPackageCfg> idMapItems;
		private Dictionary<int, List<DailySpecialPackageCfg>> typeMapItems;
		private Dictionary<int, List<DailySpecialPackageCfg>> packageIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DailySpecialPackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailySpecialPackageCfg>();
			typeMapItems = new Dictionary<int, List<DailySpecialPackageCfg>>();
			packageIdMapItems = new Dictionary<int, List<DailySpecialPackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
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

			var config = await DataTableManager.LoadTableAsync<DailySpecialPackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailySpecialPackageCfg>();
			typeMapItems = new Dictionary<int, List<DailySpecialPackageCfg>>();
			packageIdMapItems = new Dictionary<int, List<DailySpecialPackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
				AddItem(packageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public DailySpecialPackageCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DailySpecialPackageCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<DailySpecialPackageCfg> GetDataByPackageId(int packageId)
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