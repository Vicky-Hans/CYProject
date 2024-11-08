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
	public partial class MainGrinding100Cfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "customId")]
		public int CustomId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "nowWave")]
		public int NowWave { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "time")]
		public int Time { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "gringTime")]
		public int GringTime { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "incident")]
		public int Incident { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "hpCoefficient")]
		public int HpCoefficient { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "atkCoefficient")]
		public int AtkCoefficient { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "summon")]
		public List<Grinding> Summon { get; private set; }

		public MainGrinding100Cfg()
		{
		}

		[SerializationConstructor]
		public MainGrinding100Cfg(int Id, int NextId, int CustomId, int NowWave, int Time, int GringTime, int Incident, int HpCoefficient, int AtkCoefficient, List<Grinding> Summon)
		{
			this.Id = Id;
			this.NextId = NextId;
			this.CustomId = CustomId;
			this.NowWave = NowWave;
			this.Time = Time;
			this.GringTime = GringTime;
			this.Incident = Incident;
			this.HpCoefficient = HpCoefficient;
			this.AtkCoefficient = AtkCoefficient;
			this.Summon = Summon;
		}
	}
	public partial class MainGrinding100CfgCollection : CfgCollectionBase<MainGrinding100Cfg>
	{
		private const string dataPath = "MainGrinding100";
		private Dictionary<int, MainGrinding100Cfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<MainGrinding100Cfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MainGrinding100Cfg>();
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

			var config = await DataTableManager.LoadTableAsync<MainGrinding100Cfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, MainGrinding100Cfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public MainGrinding100Cfg GetDataById(int id)
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