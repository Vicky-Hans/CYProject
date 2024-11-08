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
	public partial class WaveRewardCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "wave")]
		public int Wave { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "waveReward")]
		public int WaveReward { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "prob")]
		public int Prob { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "drop")]
		public int Drop { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "maxDrop")]
		public int MaxDrop { get; private set; }

		public WaveRewardCfg()
		{
		}

		[SerializationConstructor]
		public WaveRewardCfg(int Id, int Wave, int WaveReward, int Prob, int Drop, int MaxDrop)
		{
			this.Id = Id;
			this.Wave = Wave;
			this.WaveReward = WaveReward;
			this.Prob = Prob;
			this.Drop = Drop;
			this.MaxDrop = MaxDrop;
		}
	}
	public partial class WaveRewardCfgCollection : CfgCollectionBase<WaveRewardCfg>
	{
		private const string dataPath = "WaveReward";
		private Dictionary<int, WaveRewardCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<WaveRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, WaveRewardCfg>();
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

			var config = await DataTableManager.LoadTableAsync<WaveRewardCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, WaveRewardCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public WaveRewardCfg GetDataById(int id)
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