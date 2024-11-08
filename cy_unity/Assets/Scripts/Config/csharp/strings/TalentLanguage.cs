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
	public partial class TalentLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "des")]
		public string Des { get; private set; }

		public TalentLanguageCfg()
		{
		}

		[SerializationConstructor]
		public TalentLanguageCfg(int Id, string Des)
		{
			this.Id = Id;
			this.Des = Des;
		}
	}
	public partial class TalentLanguageCfgCollection : LocalizeCollectionBase<TalentLanguageCfg>
	{
		private const string dataPath = "strings/{0}/TalentLanguage";
		private Dictionary<int, TalentLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<TalentLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TalentLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<TalentLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, TalentLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public TalentLanguageCfg GetDataById(int id)
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