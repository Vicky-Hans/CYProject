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
	public partial class ActivityTaskCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "type")]
		public int Type { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "typeCondi")]
		public int TypeCondi { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "eventType")]
		public int EventType { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "eventLoad")]
		public long EventLoad { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "eventCondi")]
		public int EventCondi { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "save")]
		public int Save { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "taskList")]
		public int TaskList { get; private set; }

		[Key(8)]
		[JsonProperty(PropertyName = "reward")]
		public List<Reward> Reward { get; private set; }

		[Key(9)]
		[JsonProperty(PropertyName = "firstId")]
		public int FirstId { get; private set; }

		[Key(10)]
		[JsonProperty(PropertyName = "nextId")]
		public int NextId { get; private set; }

		public ActivityTaskCfg()
		{
		}

		[SerializationConstructor]
		public ActivityTaskCfg(int Id, int Type, int TypeCondi, int EventType, long EventLoad, int EventCondi, int Save, int TaskList, List<Reward> Reward, int FirstId, int NextId)
		{
			this.Id = Id;
			this.Type = Type;
			this.TypeCondi = TypeCondi;
			this.EventType = EventType;
			this.EventLoad = EventLoad;
			this.EventCondi = EventCondi;
			this.Save = Save;
			this.TaskList = TaskList;
			this.Reward = Reward;
			this.FirstId = FirstId;
			this.NextId = NextId;
		}
	}
	public partial class ActivityTaskCfgCollection : CfgCollectionBase<ActivityTaskCfg>
	{
		private const string dataPath = "ActivityTask";
		private Dictionary<int, ActivityTaskCfg> idMapItems;
		private Dictionary<int, List<ActivityTaskCfg>> typeMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<ActivityTaskCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityTaskCfg>();
			typeMapItems = new Dictionary<int, List<ActivityTaskCfg>>();
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

			var config = await DataTableManager.LoadTableAsync<ActivityTaskCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ActivityTaskCfg>();
			typeMapItems = new Dictionary<int, List<ActivityTaskCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(typeMapItems, item.Type, item);
			}

			loaded = true;
		}

		public ActivityTaskCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<ActivityTaskCfg> GetDataByType(int type)
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