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
	public partial class ActivityCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "version")]
		public int Version { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "openTime")]
		public string OpenTime { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "endTime")]
		public string EndTime { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "lastDays")]
		public int LastDays { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "desc")]
		public string Desc { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "jackpotId")]
		public List<int> JackpotId { get; private set; }

		public ActivityCfg()
		{
		}

		[SerializationConstructor]
		public ActivityCfg(int Id, int Type, int Version, string Icon, string OpenTime, string EndTime, int LastDays, string Desc, List<int> JackpotId)
		{
			this.Id = Id;
			this.Type = Type;
			this.Version = Version;
			this.Icon = Icon;
			this.OpenTime = OpenTime;
			this.EndTime = EndTime;
			this.LastDays = LastDays;
			this.Desc = Desc;
			this.JackpotId = JackpotId;
		}
	}
	public partial class ActivityCfgCollection : CfgCollectionBase<ActivityCfg>
	{
		private const string dataPath = "Activity";
		private Dictionary<int, ActivityCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ActivityCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityCfg>();
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

			var config = await DataTableManager.LoadTableAsync<ActivityCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ActivityCfg GetDataById(int id)
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