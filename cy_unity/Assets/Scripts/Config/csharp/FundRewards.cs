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
	public partial class FundRewardsCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "factor")]
		public int Factor { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "Rounds")]
		public int Rounds { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "exp")]
		public int Exp { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "freeReward")]
		public List<Reward> FreeReward { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "passReward2")]
		public List<Reward> PassReward2 { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		public FundRewardsCfg()
		{
		}

		[SerializationConstructor]
		public FundRewardsCfg(int Id, int Factor, int Rounds, int Exp, List<Reward> FreeReward, List<Reward> PassReward2, int Type)
		{
			this.Id = Id;
			this.Factor = Factor;
			this.Rounds = Rounds;
			this.Exp = Exp;
			this.FreeReward = FreeReward;
			this.PassReward2 = PassReward2;
			this.Type = Type;
		}
	}
	public partial class FundRewardsCfgCollection : CfgCollectionBase<FundRewardsCfg>
	{
		private const string dataPath = "FundRewards";
		private Dictionary<int, FundRewardsCfg> idMapItems;
		private Dictionary<int, List<FundRewardsCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<FundRewardsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FundRewardsCfg>();
			typeMapItems = new Dictionary<int, List<FundRewardsCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<FundRewardsCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FundRewardsCfg>();
			typeMapItems = new Dictionary<int, List<FundRewardsCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public FundRewardsCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<FundRewardsCfg> GetDataByType(int type)
		{
			Load();

			if(!typeMapItems.TryGetValue(type, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}