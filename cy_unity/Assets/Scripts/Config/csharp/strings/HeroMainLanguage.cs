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
	public partial class HeroMainLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "name")]
		public string Name { get; private set; }

		public HeroMainLanguageCfg()
		{
		}

		[SerializationConstructor]
		public HeroMainLanguageCfg(int Id, string Name)
		{
			this.Id = Id;
			this.Name = Name;
		}
	}
	public partial class HeroMainLanguageCfgCollection : LocalizeCollectionBase<HeroMainLanguageCfg>
	{
		private const string dataPath = "strings/{0}/HeroMainLanguage";
		private Dictionary<int, HeroMainLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<HeroMainLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroMainLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<HeroMainLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroMainLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroMainLanguageCfg GetDataById(int id)
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