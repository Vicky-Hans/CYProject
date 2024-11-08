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
	public partial class DailyStageProgressRewardCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "value1")]
		public int Value1 { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "value2")]
		public int Value2 { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		public DailyStageProgressRewardCfg()
		{
		}

		[SerializationConstructor]
		public DailyStageProgressRewardCfg(int Id, int Type, string Dec, int Value1, int Value2, List<Reward> Reward)
		{
			this.Id = Id;
			this.Type = Type;
			this.Dec = Dec;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Reward = Reward;
		}
	}
	public partial class DailyStageProgressRewardCfgCollection : CfgCollectionBase<DailyStageProgressRewardCfg>
	{
		private const string dataPath = "DailyStageProgressReward";
		private Dictionary<int, DailyStageProgressRewardCfg> idMapItems;
		private Dictionary<int, List<DailyStageProgressRewardCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DailyStageProgressRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageProgressRewardCfg>();
			typeMapItems = new Dictionary<int, List<DailyStageProgressRewardCfg>>();
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

			var config = await DataTableManager.LoadTableAsync<DailyStageProgressRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageProgressRewardCfg>();
			typeMapItems = new Dictionary<int, List<DailyStageProgressRewardCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public DailyStageProgressRewardCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DailyStageProgressRewardCfg> GetDataByType(int type)
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