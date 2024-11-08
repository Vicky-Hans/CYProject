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
	public partial class TriggerGiftTypeCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "trigger")]
		public int Trigger { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "triggerNum")]
		public List<int> TriggerNum { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "duration")]
		public int Duration { get; private set; }

		public TriggerGiftTypeCfg()
		{
		}

		[SerializationConstructor]
		public TriggerGiftTypeCfg(int Id, int Trigger, List<int> TriggerNum, int Duration)
		{
			this.Id = Id;
			this.Trigger = Trigger;
			this.TriggerNum = TriggerNum;
			this.Duration = Duration;
		}
	}
	public partial class TriggerGiftTypeCfgCollection : CfgCollectionBase<TriggerGiftTypeCfg>
	{
		private const string dataPath = "TriggerGiftType";
		private Dictionary<int, TriggerGiftTypeCfg> idMapItems;
		private Dictionary<int, List<TriggerGiftTypeCfg>> triggerMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<TriggerGiftTypeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TriggerGiftTypeCfg>();
			triggerMapItems = new Dictionary<int, List<TriggerGiftTypeCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(triggerMapItems, item.Trigger, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<TriggerGiftTypeCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TriggerGiftTypeCfg>();
			triggerMapItems = new Dictionary<int, List<TriggerGiftTypeCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(triggerMapItems, item.Trigger, item);
			}

			loaded = true;
		}

		public TriggerGiftTypeCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<TriggerGiftTypeCfg> GetDataByTrigger(int trigger)
		{
			Load();

			if(!triggerMapItems.TryGetValue(trigger, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}