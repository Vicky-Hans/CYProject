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
	public partial class EquipSkillLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "dec")]
		public string Dec { get; private set; }

		public EquipSkillLanguageCfg()
		{
		}

		[SerializationConstructor]
		public EquipSkillLanguageCfg(int Id, string Dec)
		{
			this.Id = Id;
			this.Dec = Dec;
		}
	}
	public partial class EquipSkillLanguageCfgCollection : LocalizeCollectionBase<EquipSkillLanguageCfg>
	{
		private const string dataPath = "strings/{0}/EquipSkillLanguage";
		private Dictionary<int, EquipSkillLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<EquipSkillLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSkillLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<EquipSkillLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipSkillLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipSkillLanguageCfg GetDataById(int id)
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