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
	public partial class JackpotLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "itemTypeName")]
		public string ItemTypeName { get; private set; }

		public JackpotLanguageCfg()
		{
		}

		[SerializationConstructor]
		public JackpotLanguageCfg(int Id, string ItemTypeName)
		{
			this.Id = Id;
			this.ItemTypeName = ItemTypeName;
		}
	}
	public partial class JackpotLanguageCfgCollection : LocalizeCollectionBase<JackpotLanguageCfg>
	{
		private const string dataPath = "strings/{0}/JackpotLanguage";
		private Dictionary<int, JackpotLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<JackpotLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, JackpotLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<JackpotLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, JackpotLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public JackpotLanguageCfg GetDataById(int id)
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