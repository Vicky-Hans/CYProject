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
	public partial class DailyStageGrindingCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "nowWave")]
		public int NowWave { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "time")]
		public int Time { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "gringTime")]
		public int GringTime { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "incident")]
		public int Incident { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "hpCoefficient")]
		public int HpCoefficient { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "atkCoefficient")]
		public int AtkCoefficient { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "summon")]
		public List<Grinding> Summon { get; private set; }

		public DailyStageGrindingCfg()
		{
		}

		[SerializationConstructor]
		public DailyStageGrindingCfg(int Id, int NextId, int NowWave, int Time, int GringTime, int Incident, int HpCoefficient, int AtkCoefficient, List<Grinding> Summon)
		{
			this.Id = Id;
			this.NextId = NextId;
			this.NowWave = NowWave;
			this.Time = Time;
			this.GringTime = GringTime;
			this.Incident = Incident;
			this.HpCoefficient = HpCoefficient;
			this.AtkCoefficient = AtkCoefficient;
			this.Summon = Summon;
		}
	}
	public partial class DailyStageGrindingCfgCollection : CfgCollectionBase<DailyStageGrindingCfg>
	{
		private const string dataPath = "DailyStageGrinding";
		private Dictionary<int, DailyStageGrindingCfg> idMapItems;
		private Dictionary<int, List<DailyStageGrindingCfg>> nowWaveMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DailyStageGrindingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageGrindingCfg>();
			nowWaveMapItems = new Dictionary<int, List<DailyStageGrindingCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nowWaveMapItems, item.NowWave, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<DailyStageGrindingCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageGrindingCfg>();
			nowWaveMapItems = new Dictionary<int, List<DailyStageGrindingCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nowWaveMapItems, item.NowWave, item);
			}

			loaded = true;
		}

		public DailyStageGrindingCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DailyStageGrindingCfg> GetDataByNowWave(int nowWave)
		{
			Load();

			if(!nowWaveMapItems.TryGetValue(nowWave, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}