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
	public partial class MonthlyVipMainCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "effectId")]
		public List<int> EffectId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "daiyReward")]
		public List<Reward> DaiyReward { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "time")]
		public int Time { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "packageId")]
		public int PackageId { get; private set; }

		public MonthlyVipMainCfg()
		{
		}

		[SerializationConstructor]
		public MonthlyVipMainCfg(int Id, string Name, List<int> EffectId, List<Reward> Reward, List<Reward> DaiyReward, int Time, int PackageId)
		{
			this.Id = Id;
			this.Name = Name;
			this.EffectId = EffectId;
			this.Reward = Reward;
			this.DaiyReward = DaiyReward;
			this.Time = Time;
			this.PackageId = PackageId;
		}
	}
	public partial class MonthlyVipMainCfgCollection : CfgCollectionBase<MonthlyVipMainCfg>
	{
		private const string dataPath = "MonthlyVipMain";
		private Dictionary<int, MonthlyVipMainCfg> idMapItems;
		private Dictionary<int, List<MonthlyVipMainCfg>> packageIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MonthlyVipMainCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonthlyVipMainCfg>();
			packageIdMapItems = new Dictionary<int, List<MonthlyVipMainCfg>>();
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

			var config = await DataTableManager.LoadTableAsync<MonthlyVipMainCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MonthlyVipMainCfg>();
			packageIdMapItems = new Dictionary<int, List<MonthlyVipMainCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(packageIdMapItems, item.PackageId, item);
			}

			loaded = true;
		}

		public MonthlyVipMainCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<MonthlyVipMainCfg> GetDataByPackageId(int packageId)
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