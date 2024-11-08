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
	public partial class PackageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "rule")]
		public int Rule { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "condition")]
		public int Condition { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "optionalReward")]
		public List<Reward> OptionalReward { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "rechargeId")]
		public int RechargeId { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "cost")]
		public List<Reward> Cost { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "value")]
		public string Value { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "num")]
		public int Num { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "buyLimit")]
		public int BuyLimit { get; private set; }

		public PackageCfg()
		{
		}

		[SerializationConstructor]
		public PackageCfg(int Id, string Icon, int Rule, int Condition, List<Reward> Reward, List<Reward> OptionalReward, int RechargeId, List<Reward> Cost, string Value, int Num, int BuyLimit)
		{
			this.Id = Id;
			this.Icon = Icon;
			this.Rule = Rule;
			this.Condition = Condition;
			this.Reward = Reward;
			this.OptionalReward = OptionalReward;
			this.RechargeId = RechargeId;
			this.Cost = Cost;
			this.Value = Value;
			this.Num = Num;
			this.BuyLimit = BuyLimit;
		}
	}
	public partial class PackageCfgCollection : CfgCollectionBase<PackageCfg>
	{
		private const string dataPath = "Package";
		private Dictionary<int, PackageCfg> idMapItems;
		private Dictionary<int, List<PackageCfg>> ruleMapItems;
		private Dictionary<int, List<PackageCfg>> rechargeIdMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<PackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PackageCfg>();
			ruleMapItems = new Dictionary<int, List<PackageCfg>>();
			rechargeIdMapItems = new Dictionary<int, List<PackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(ruleMapItems, item.Rule, item);
				AddItem(rechargeIdMapItems, item.RechargeId, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<PackageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, PackageCfg>();
			ruleMapItems = new Dictionary<int, List<PackageCfg>>();
			rechargeIdMapItems = new Dictionary<int, List<PackageCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(ruleMapItems, item.Rule, item);
				AddItem(rechargeIdMapItems, item.RechargeId, item);
			}

			loaded = true;
		}

		public PackageCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<PackageCfg> GetDataByRule(int rule)
		{
			Load();

			if(!ruleMapItems.TryGetValue(rule, out var itemList))
			{
				return null;
			}
			return itemList;
		}

		public List<PackageCfg> GetDataByRechargeId(int rechargeId)
		{
			Load();

			if(!rechargeIdMapItems.TryGetValue(rechargeId, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}