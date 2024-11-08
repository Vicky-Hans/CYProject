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
	public partial class EquipModelLanguageCfg
	{
		[Key(0)]
		[JsonProperty(PropertyName = "id")]
		public int Id { get; private set; }

		[Key(1)]
		[JsonProperty(PropertyName = "weaponName")]
		public string WeaponName { get; private set; }

		public EquipModelLanguageCfg()
		{
		}

		[SerializationConstructor]
		public EquipModelLanguageCfg(int Id, string WeaponName)
		{
			this.Id = Id;
			this.WeaponName = WeaponName;
		}
	}
	public partial class EquipModelLanguageCfgCollection : LocalizeCollectionBase<EquipModelLanguageCfg>
	{
		private const string dataPath = "strings/{0}/EquipModelLanguage";
		private Dictionary<int, EquipModelLanguageCfg> idMapItems;

		public override void Load()
		{
			if (loaded)
			{
				return;
			}

			var config = DataTableManager.LoadLocalizeTable<EquipModelLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipModelLanguageCfg>();
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

			var config = await DataTableManager.LoadLocalizeTableAsync<EquipModelLanguageCfg>(dataPath);
			dataItems = config.Data;

			idMapItems = new Dictionary<int, EquipModelLanguageCfg>();
			foreach(var item in dataItems) 
			{
				AddItem(idMapItems, item.Id, item);
			}

			loaded = true;
		}

		public EquipModelLanguageCfg GetDataById(int id)
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