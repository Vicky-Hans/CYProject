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
	public partial class FunctionOpenLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		public FunctionOpenLanguageCfg()
		{
		}

		[SerializationConstructor]
		public FunctionOpenLanguageCfg(int Id, string Name)
		{
			this.Id = Id;
			this.Name = Name;
		}
	}
	public partial class FunctionOpenLanguageCfgCollection : LocalizeCollectionBase<FunctionOpenLanguageCfg>
	{
		private const string dataPath = "strings/{0}/FunctionOpenLanguage";
		private Dictionary<int, FunctionOpenLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<FunctionOpenLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionOpenLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<FunctionOpenLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, FunctionOpenLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public FunctionOpenLanguageCfg GetDataById(int id)
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