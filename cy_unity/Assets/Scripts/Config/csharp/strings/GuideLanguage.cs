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
	public partial class GuideLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "desc")]
		public string Desc { get; private set; }

		public GuideLanguageCfg()
		{
		}

		[SerializationConstructor]
		public GuideLanguageCfg(int Id, string Desc)
		{
			this.Id = Id;
			this.Desc = Desc;
		}
	}
	public partial class GuideLanguageCfgCollection : LocalizeCollectionBase<GuideLanguageCfg>
	{
		private const string dataPath = "strings/{0}/GuideLanguage";
		private Dictionary<int, GuideLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<GuideLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, GuideLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<GuideLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, GuideLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public GuideLanguageCfg GetDataById(int id)
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