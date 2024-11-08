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
	public partial class HeroEquipSkillLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		public HeroEquipSkillLanguageCfg()
		{
		}

		[SerializationConstructor]
		public HeroEquipSkillLanguageCfg(int Id, string Dec)
		{
			this.Id = Id;
			this.Dec = Dec;
		}
	}
	public partial class HeroEquipSkillLanguageCfgCollection : LocalizeCollectionBase<HeroEquipSkillLanguageCfg>
	{
		private const string dataPath = "strings/{0}/HeroEquipSkillLanguage";
		private Dictionary<int, HeroEquipSkillLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<HeroEquipSkillLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipSkillLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<HeroEquipSkillLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, HeroEquipSkillLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public HeroEquipSkillLanguageCfg GetDataById(int id)
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