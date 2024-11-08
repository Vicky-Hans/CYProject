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
	public partial class ActivityRankingCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "ranking")]
		public List<int> Ranking { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "Reward")]
		public List<Reward> Reward { get; private set; }

		public ActivityRankingCfg()
		{
		}

		[SerializationConstructor]
		public ActivityRankingCfg(int Id, List<int> Ranking, List<Reward> Reward)
		{
			this.Id = Id;
			this.Ranking = Ranking;
			this.Reward = Reward;
		}
	}
	public partial class ActivityRankingCfgCollection : CfgCollectionBase<ActivityRankingCfg>
	{
		private const string dataPath = "ActivityRanking";
		private Dictionary<int, ActivityRankingCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ActivityRankingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityRankingCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ActivityRankingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityRankingCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ActivityRankingCfg GetDataById(int id)
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