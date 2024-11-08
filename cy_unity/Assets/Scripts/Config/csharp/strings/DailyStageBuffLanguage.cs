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
	public partial class DailyStageBuffLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		public DailyStageBuffLanguageCfg()
		{
		}

		[SerializationConstructor]
		public DailyStageBuffLanguageCfg(int Id, string Dec)
		{
			this.Id = Id;
			this.Dec = Dec;
		}
	}
	public partial class DailyStageBuffLanguageCfgCollection : LocalizeCollectionBase<DailyStageBuffLanguageCfg>
	{
		private const string dataPath = "strings/{0}/DailyStageBuffLanguage";
		private Dictionary<int, DailyStageBuffLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<DailyStageBuffLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageBuffLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<DailyStageBuffLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, DailyStageBuffLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public DailyStageBuffLanguageCfg GetDataById(int id)
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