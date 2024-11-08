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
	public partial class ProPictureLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "description")]
		public string Description { get; private set; }

		public ProPictureLanguageCfg()
		{
		}

		[SerializationConstructor]
		public ProPictureLanguageCfg(int Id, string Description)
		{
			this.Id = Id;
			this.Description = Description;
		}
	}
	public partial class ProPictureLanguageCfgCollection : LocalizeCollectionBase<ProPictureLanguageCfg>
	{
		private const string dataPath = "strings/{0}/ProPictureLanguage";
		private Dictionary<int, ProPictureLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<ProPictureLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProPictureLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<ProPictureLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, ProPictureLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public ProPictureLanguageCfg GetDataById(int id)
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