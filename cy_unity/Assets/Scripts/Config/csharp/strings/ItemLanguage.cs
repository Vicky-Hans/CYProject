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
	public partial class ItemLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		[Key(2)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		public ItemLanguageCfg()
		{
		}

		[SerializationConstructor]
		public ItemLanguageCfg(int Id, string Name, string Dec)
		{
			this.Id = Id;
			this.Name = Name;
			this.Dec = Dec;
		}
	}
	public partial class ItemLanguageCfgCollection : LocalizeCollectionBase<ItemLanguageCfg>
	{
		private const string dataPath = "strings/{0}/ItemLanguage";
		private Dictionary<int, ItemLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<ItemLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ItemLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<ItemLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ItemLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ItemLanguageCfg GetDataById(int id)
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