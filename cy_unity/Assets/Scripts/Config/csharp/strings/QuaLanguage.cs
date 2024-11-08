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
	public partial class QuaLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		public QuaLanguageCfg()
		{
		}

		[SerializationConstructor]
		public QuaLanguageCfg(int Id, string Name)
		{
			this.Id = Id;
			this.Name = Name;
		}
	}
	public partial class QuaLanguageCfgCollection : LocalizeCollectionBase<QuaLanguageCfg>
	{
		private const string dataPath = "strings/{0}/QuaLanguage";
		private Dictionary<int, QuaLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<QuaLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, QuaLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<QuaLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, QuaLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public QuaLanguageCfg GetDataById(int id)
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