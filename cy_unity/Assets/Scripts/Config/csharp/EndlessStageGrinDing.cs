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
	public partial class EndlessStageGrinDingCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "waveId")]
		public int WaveId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "arrivalTime")]
		public int ArrivalTime { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "durationTime")]
		public int DurationTime { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "incident")]
		public int Incident { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "grinDing")]
		public List<TourGrinDing> GrinDing { get; private set; }

		public EndlessStageGrinDingCfg()
		{
		}

		[SerializationConstructor]
		public EndlessStageGrinDingCfg(int Id, int NextId, int WaveId, int ArrivalTime, int DurationTime, int Incident, List<TourGrinDing> GrinDing)
		{
			this.Id = Id;
			this.NextId = NextId;
			this.WaveId = WaveId;
			this.ArrivalTime = ArrivalTime;
			this.DurationTime = DurationTime;
			this.Incident = Incident;
			this.GrinDing = GrinDing;
		}
	}
	public partial class EndlessStageGrinDingCfgCollection : CfgCollectionBase<EndlessStageGrinDingCfg>
	{
		private const string dataPath = "EndlessStageGrinDing";
		private Dictionary<int, EndlessStageGrinDingCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<EndlessStageGrinDingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EndlessStageGrinDingCfg>();
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

			var config = await DataTableManager.LoadTableAsync<EndlessStageGrinDingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EndlessStageGrinDingCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EndlessStageGrinDingCfg GetDataById(int id)
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