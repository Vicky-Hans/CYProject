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
	public partial class GlobalLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public string Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		public GlobalLanguageCfg()
		{
		}

		[SerializationConstructor]
		public GlobalLanguageCfg(string Id, string Name)
		{
			this.Id = Id;
			this.Name = Name;
		}
	}
	public partial class GlobalLanguageCfgCollection : LocalizeCollectionBase<GlobalLanguageCfg>
	{
		private const string dataPath = "strings/{0}/GlobalLanguage";
		private Dictionary<string, GlobalLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<GlobalLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<string, GlobalLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<GlobalLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<string, GlobalLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public GlobalLanguageCfg GetDataById(string id)
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