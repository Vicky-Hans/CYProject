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
	public partial class DailyStageBuffCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public int Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "rejectId")]
		public int RejectId { get; private set; }

		[Key(3)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		[Key(4)]
		[JsonProperty(PropertyName = "value")]
		public List<int> Value { get; private set; }

		[Key(5)]
		[JsonProperty(PropertyName = "value2")]
		public List<int> Value2 { get; private set; }

		[Key(6)]
		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; private set; }

		[Key(7)]
		[JsonProperty(PropertyName = "skillId")]
		public int SkillId { get; private set; }

		public DailyStageBuffCfg()
		{
		}

		[SerializationConstructor]
		public DailyStageBuffCfg(int Id, int Name, int RejectId, string Dec, List<int> Value, List<int> Value2, string Icon, int SkillId)
		{
			this.Id = Id;
			this.Name = Name;
			this.RejectId = RejectId;
			this.Dec = Dec;
			this.Value = Value;
			this.Value2 = Value2;
			this.Icon = Icon;
			this.SkillId = SkillId;
		}
	}
	public partial class DailyStageBuffCfgCollection : CfgCollectionBase<DailyStageBuffCfg>
	{
		private const string dataPath = "DailyStageBuff";
		private Dictionary<int, DailyStageBuffCfg> idMapItems;
		private Dictionary<int, List<DailyStageBuffCfg>> nameMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadTable<DailyStageBuffCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageBuffCfg>();
			nameMapItems = new Dictionary<int, List<DailyStageBuffCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nameMapItems, item.Name, item);
			}

			loaded = true;
		}

		public override async UniTask LoadAsync()
		{
			if (loaded)
			{
				return;
			}

			var config = await DataTableManager.LoadTableAsync<DailyStageBuffCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageBuffCfg>();
			nameMapItems = new Dictionary<int, List<DailyStageBuffCfg>>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
				AddItem(nameMapItems, item.Name, item);
			}

			loaded = true;
		}

		public DailyStageBuffCfg GetDataById(int id)
		{
			Load();

			if(!idMapItems.TryGetValue(id, out var item))
			{
				return null;
			}
			return item;
		}

		public List<DailyStageBuffCfg> GetDataByName(int name)
		{
			Load();

			if(!nameMapItems.TryGetValue(name, out var itemList))
			{
				return null;
			}
			return itemList;
		}

	}
}